using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class RebalancedRepose
    {
        private enum PendingMode
        {
            None,
            HalfwayToBest,
            Best
        }

        private sealed class ScopedBodyCatalogState
        {
            internal object Balance;
            internal object OriginalCatalog;
            internal object ProjectedCatalog;
        }

        private static ManualLogSource _log;
        private static PendingMode _pendingMode;
        private static MethodInfo _findBuffById;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type dropType = R.AnyType("FlowCanvas.Nodes.Flow_DropBody");
            if (dropType == null) throw new MissingMemberException("FlowCanvas.Nodes.Flow_DropBody");

            Type[] callbackTypes = dropType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(type => type.GetFields(R.Inst).Any(field => field.FieldType == dropType))
                .ToArray();
            if (callbackTypes.Length != 1)
                throw new InvalidOperationException("Expected one Flow_DropBody execution display class, found " + callbackTypes.Length + ".");

            MethodInfo[] callbackMethods = callbackTypes[0].GetMethods(R.Inst)
                .Where(method => method.Name.IndexOf("<RegisterPorts>b__", StringComparison.Ordinal) >= 0)
                .Where(method => method.GetParameters().Length == 1)
                .ToArray();
            if (callbackMethods.Length != 1)
                throw new InvalidOperationException("Expected one Flow_DropBody execution callback, found " + callbackMethods.Length + ".");

            Type gameSave = R.GameType("GameSave");
            MethodInfo generateBody = R.Method(
                gameSave,
                "GenerateBody",
                false,
                new[] { typeof(int), typeof(int), typeof(int), typeof(int) });
            if (generateBody == null)
                throw new MissingMethodException("GameSave.GenerateBody(int,int,int,int)");

            ResolveBuffApi();

            R.PatchHooks(
                harmonyId + ".repose.callback",
                typeof(RebalancedRepose),
                callbackMethods[0],
                nameof(DropBodyCallbackPrefix),
                nameof(DropBodyCallbackPostfix),
                nameof(DropBodyCallbackFinalizer));
            R.PatchHooks(
                harmonyId + ".repose.generate",
                typeof(RebalancedRepose),
                generateBody,
                nameof(GenerateBodyPrefix),
                null,
                nameof(GenerateBodyFinalizer));
        }

        private static void DropBodyCallbackPrefix(object __instance)
        {
            _pendingMode = PendingMode.None;
            if (!RebalancedRuntimeState.IsReady) return;

            try
            {
                object node = FindCapturedDropNode(__instance);
                if (!IsOrdinaryDonkeyDeliveryNode(node)) return;
                if (!HasLiveBuff("buff_skull")) return;

                int tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ReposeTierParam);
                if (tier == 2)
                    _pendingMode = PendingMode.HalfwayToBest;
                else if (tier == 3)
                    _pendingMode = PendingMode.Best;
            }
            catch (Exception ex)
            {
                _pendingMode = PendingMode.None;
                LogRuntimeError("Repose delivery predicate failed closed", ex);
            }
        }

        private static void GenerateBodyPrefix(
            ref int tier_min,
            ref int tier_max,
            ref ScopedBodyCatalogState __state)
        {
            __state = null;
            if (!RebalancedRuntimeState.IsReady)
            {
                _pendingMode = PendingMode.None;
                return;
            }

            PendingMode mode = _pendingMode;
            _pendingMode = PendingMode.None;
            if (mode == PendingMode.None) return;

            try
            {
                if (!HasLiveBuff("buff_skull")) return;
                if (tier_max < tier_min) return;

                bool forceBest = mode == PendingMode.Best ||
                                 (mode == PendingMode.HalfwayToBest && UnityEngine.Random.Range(0, 2) == 0);
                if (!forceBest) return;

                int bestExistingTier;
                if (!CorpseTierSemantics.TryGetHighestExistingOrdinaryTier(tier_min, tier_max, out bestExistingTier))
                    return;

                // Preserve the accepted 0.2.14 reliability behavior first.
                // The incoming range already includes the stock Repose +1 tier.
                tier_min = bestExistingTier;

                // Gold promises the best visible body, not merely the best hidden tier.
                // Scope the host catalog to tied maximum-skull candidates only for
                // this one native GenerateBody call; vanilla RNG and body creation remain authoritative.
                if (mode == PendingMode.Best)
                    ProjectGoldBodyCatalog(bestExistingTier, tier_max, ref __state);
            }
            catch (Exception ex)
            {
                RestoreBodyCatalog(__state);
                __state = null;
                LogRuntimeError(
                    "Repose Gold maximum-body projection failed closed; accepted best-tier behavior remains active for this delivery",
                    ex);
            }
        }

        private static Exception GenerateBodyFinalizer(
            Exception __exception,
            ScopedBodyCatalogState __state)
        {
            RestoreBodyCatalog(__state);
            return __exception;
        }

        private static void ProjectGoldBodyCatalog(
            int bestTier,
            int tierMax,
            ref ScopedBodyCatalogState state)
        {
            int maxScore;
            if (!CorpseTierSemantics.TryGetMaximumSkullScoreAtTier(bestTier, out maxScore))
                throw new InvalidOperationException(
                    "Could not derive maximum corpse skull score for tier " + bestTier + ".");

            object balance = R.GetStatic(R.GameType("GameBalance"), "me");
            if (balance == null)
                throw new InvalidOperationException("GameBalance.me unavailable during Repose Gold projection.");

            object originalCatalog = R.Get(balance, "bodies_data");
            IList source = originalCatalog as IList;
            if (source == null)
                throw new InvalidOperationException("GameBalance.bodies_data is not an IList.");

            IList projected = Activator.CreateInstance(originalCatalog.GetType()) as IList;
            if (projected == null)
                throw new InvalidOperationException("Could not create a scoped bodies_data projection.");

            int retainedCandidates = 0;
            foreach (object body in source)
            {
                if (body == null)
                {
                    projected.Add(body);
                    continue;
                }

                int tier = R.Int(R.Get(body, "tier"));
                if (tier < bestTier || tier > tierMax)
                {
                    projected.Add(body);
                    continue;
                }

                string linkedItemId = R.Get(body, "linked_item_id") as string;
                if (tier != bestTier ||
                    !string.Equals(linkedItemId, "body", StringComparison.Ordinal))
                    continue;

                int score;
                if (!CorpseTierSemantics.TryGetBodySkullScore(body, out score))
                    throw new InvalidOperationException(
                        "Could not derive corpse skull score for " + (R.Id(body) ?? "<unknown>") + ".");

                if (score != maxScore) continue;

                projected.Add(body);
                retainedCandidates++;
            }

            if (retainedCandidates <= 0)
                throw new InvalidOperationException(
                    "Repose Gold projection produced no maximum-skull candidates for tier " + bestTier + ".");

            state = new ScopedBodyCatalogState
            {
                Balance = balance,
                OriginalCatalog = originalCatalog,
                ProjectedCatalog = projected
            };

            R.Set(balance, "bodies_data", projected);
        }

        private static void RestoreBodyCatalog(ScopedBodyCatalogState state)
        {
            if (state == null || state.Balance == null || state.OriginalCatalog == null)
                return;

            try
            {
                object current = R.Get(state.Balance, "bodies_data");
                if (ReferenceEquals(current, state.OriginalCatalog))
                    return;

                if (!ReferenceEquals(current, state.ProjectedCatalog))
                {
                    LogRuntimeError(
                        "Repose Gold bodies_data changed during its scoped projection; PrayerClarity will not overwrite the newer catalog",
                        new InvalidOperationException("Unexpected bodies_data owner change."));
                    return;
                }

                R.Set(state.Balance, "bodies_data", state.OriginalCatalog);
            }
            catch (Exception ex)
            {
                LogRuntimeError("Repose Gold bodies_data restoration failed", ex);
            }
        }

        private static void DropBodyCallbackPostfix()
        {
            _pendingMode = PendingMode.None;
        }

        private static void DropBodyCallbackFinalizer()
        {
            _pendingMode = PendingMode.None;
        }

        private static bool IsOrdinaryDonkeyDeliveryNode(object node)
        {
            if (node == null) return false;

            object graph = R.Get(node, "graph");
            string graphName = Convert.ToString(R.Get(graph, "name"));
            if (!string.Equals(graphName, "npc_donkey", StringComparison.Ordinal)) return false;

            object wgo = R.Get(node, "wgo");
            string objectId = Convert.ToString(GetAny(wgo, "obj_id", "_obj_id"));
            if (!string.Equals(objectId, "donkey", StringComparison.Ordinal)) return false;

            object tierMinSource;
            object tierMaxSource;
            object soulMinSource;
            object soulMaxSource;
            if (!TryGetUniqueSource(node, "Tier min", out tierMinSource)) return false;
            if (!TryGetUniqueSource(node, "Tier max", out tierMaxSource)) return false;
            if (!TryGetUniqueSource(node, "Tier min(soul)", out soulMinSource)) return false;
            if (!TryGetUniqueSource(node, "Tier max(soul)", out soulMaxSource)) return false;

            return IsIntegerAddFromPlayerParams(tierMinSource) &&
                   IsIntegerAddFromPlayerParams(tierMaxSource) &&
                   IsPlayerParamNode(soulMinSource) &&
                   IsPlayerParamNode(soulMaxSource);
        }

        private static bool TryGetUniqueSource(object node, string targetPortId, out object sourceNode)
        {
            sourceNode = null;
            IEnumerable connections = R.Get(node, "inConnections") as IEnumerable;
            if (connections == null) return false;

            int matches = 0;
            foreach (object connection in connections)
            {
                string target = Convert.ToString(GetAny(connection, "targetPortID", "_targetPortID"));
                if (!string.Equals(target, targetPortId, StringComparison.Ordinal)) continue;

                matches++;
                sourceNode = GetAny(connection, "sourceNode", "_sourceNode");
            }

            return matches == 1 && sourceNode != null;
        }

        private static bool IsIntegerAddFromPlayerParams(object sourceNode)
        {
            if (sourceNode == null) return false;
            string typeName = sourceNode.GetType().FullName ?? string.Empty;
            string nodeName = Convert.ToString(R.Get(sourceNode, "name"));
            if (typeName.IndexOf("IntegerAdd", StringComparison.Ordinal) < 0 &&
                !string.Equals(nodeName, "+", StringComparison.Ordinal))
                return false;

            IEnumerable connections = R.Get(sourceNode, "inConnections") as IEnumerable;
            if (connections == null) return false;

            int playerParamSources = 0;
            int total = 0;
            foreach (object connection in connections)
            {
                total++;
                object upstream = GetAny(connection, "sourceNode", "_sourceNode");
                if (IsPlayerParamNode(upstream)) playerParamSources++;
            }

            return total == 2 && playerParamSources == 2;
        }

        private static bool IsPlayerParamNode(object node)
        {
            if (node == null) return false;
            string typeName = node.GetType().FullName ?? string.Empty;
            return typeName.IndexOf("Flow_GetPlayerParamInt", StringComparison.Ordinal) >= 0;
        }

        private static object FindCapturedDropNode(object callback)
        {
            if (callback == null) return null;
            Type dropType = R.AnyType("FlowCanvas.Nodes.Flow_DropBody");
            if (dropType == null) return null;

            FieldInfo[] fields = callback.GetType().GetFields(R.Inst)
                .Where(field => field.FieldType == dropType)
                .ToArray();
            return fields.Length == 1 ? fields[0].GetValue(callback) : null;
        }

        private static object GetAny(object obj, params string[] names)
        {
            if (obj == null) return null;
            foreach (string name in names)
            {
                object value = R.Get(obj, name);
                if (value != null) return value;
            }
            return null;
        }

        private static void ResolveBuffApi()
        {
            if (_findBuffById != null) return;
            Type buffsLogics = R.GameType("BuffsLogics");
            _findBuffById = R.Method(buffsLogics, "FindBuffByID", true, new[] { typeof(string) });
            if (_findBuffById == null)
                throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");
        }

        private static bool HasLiveBuff(string buffId)
        {
            ResolveBuffApi();
            return _findBuffById.Invoke(null, new object[] { buffId }) != null;
        }

        private static void LogRuntimeError(string context, Exception ex)
        {
            if (_runtimeErrorLogged) return;
            _runtimeErrorLogged = true;
            _log?.LogError("PC_RUNTIME_FALLBACK edition=rebalanced feature=repose action=vanilla-generation context=\"" + context + "\" " + ex);
        }
    }
}
