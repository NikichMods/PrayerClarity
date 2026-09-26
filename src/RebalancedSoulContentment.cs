using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class RebalancedSoulContentment
    {
        private const float StockCoefficient = 0.1f;
        private const float RebalancedCoefficient = 0.5f;
        private const string ContentmentBuffId = "buff_gp_increase";
        private static ManualLogSource _log;
        private static bool _runtimeErrorLogged;
        private static bool _preservationErrorLogged;
        private static MemberInfo _itemDefinitionMember;
        private static MemberInfo _definitionTypeMember;
        private static MethodInfo _findBuffById;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            Type wgo = R.GameType("WorldGameObject");
            MethodInfo target = R.Method(wgo, "CheckNeededAttachedScript", false, Type.EmptyTypes);
            if (target == null) throw new MissingMethodException("WorldGameObject.CheckNeededAttachedScript()");
            R.Patch(harmonyId + ".soulcontentment", typeof(RebalancedSoulContentment), target, nameof(CheckNeededAttachedScriptPostfix));

            Type item = R.GameType("Item");
            Type itemDefinition = R.GameType("ItemDefinition");
            Type buffsLogics = R.GameType("BuffsLogics");
            if (item == null) throw new MissingMemberException("Item");
            if (itemDefinition == null) throw new MissingMemberException("ItemDefinition");
            if (buffsLogics == null) throw new MissingMemberException("BuffsLogics");

            MethodInfo updateDurability = R.Method(
                item,
                "UpdateDurability",
                false,
                new[] { typeof(float), typeof(float) });
            if (updateDurability == null)
                throw new MissingMethodException("Item.UpdateDurability(float,float)");

            _itemDefinitionMember = FindReadableMember(item, "definition");
            _definitionTypeMember = FindReadableMember(itemDefinition, "type");
            _findBuffById = R.Method(buffsLogics, "FindBuffByID", true, new[] { typeof(string) });
            if (_itemDefinitionMember == null) throw new MissingMemberException("Item.definition");
            if (_definitionTypeMember == null) throw new MissingMemberException("ItemDefinition.type");
            if (_findBuffById == null) throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");

            R.PatchPrefix(
                harmonyId + ".soulcontentment.preserve",
                typeof(RebalancedSoulContentment),
                updateDurability,
                nameof(UpdateDurabilityPrefix));
        }

        private static void CheckNeededAttachedScriptPostfix(object __instance)
        {
            if (!RebalancedRuntimeState.IsReady) return;
            try
            {
                object definition = R.Get(__instance, "obj_def");
                string attachedScript = Convert.ToString(R.Get(definition, "attached_script"));
                if (!string.Equals(attachedScript, "soul_portal", StringComparison.Ordinal)) return;

                object graph = FindSoulPortalGraph(__instance);
                if (graph == null) throw new InvalidOperationException("Live soul_portal graph was not found after attached-script initialization.");

                ApplyCoefficient(graph);
            }
            catch (Exception ex)
            {
                if (_runtimeErrorLogged) return;
                _runtimeErrorLogged = true;
                _log?.LogError("PC_RUNTIME_FALLBACK edition=rebalanced feature=soul-contentment-gain action=stock-coefficient " + ex);
            }
        }

        private static bool UpdateDurabilityPrefix(object __instance)
        {
            if (!RebalancedRuntimeState.IsReady) return true;
            try
            {
                object definition = ReadMember(__instance, _itemDefinitionMember);
                if (definition == null) return true;

                object type = ReadMember(definition, _definitionTypeMember);
                string typeName = Convert.ToString(type);
                if (!string.Equals(typeName, "Soul", StringComparison.Ordinal) &&
                    !string.Equals(typeName, "SoulBodyPart", StringComparison.Ordinal))
                    return true;

                // Extraction damage is applied directly by Flow_GenerateSoul and does
                // not pass through Item.UpdateDurability. Skipping this method therefore
                // blocks only passive time-based soul decay while the prayer buff lives.
                object activeBuff = _findBuffById.Invoke(null, new object[] { ContentmentBuffId });
                return activeBuff == null;
            }
            catch (Exception ex)
            {
                if (!_preservationErrorLogged)
                {
                    _preservationErrorLogged = true;
                    _log?.LogError("PC_RUNTIME_FALLBACK edition=rebalanced feature=soul-contentment-preservation action=stock-decay " + ex);
                }
                return true;
            }
        }

        private static MemberInfo FindReadableMember(Type type, string name)
        {
            if (type == null) return null;
            PropertyInfo property = type.GetProperty(name, R.Inst);
            if (property != null && property.CanRead) return property;
            return type.GetField(name, R.Inst);
        }

        private static object ReadMember(object instance, MemberInfo member)
        {
            PropertyInfo property = member as PropertyInfo;
            if (property != null) return property.GetValue(instance, null);
            FieldInfo field = member as FieldInfo;
            return field == null ? null : field.GetValue(instance);
        }

        private static object FindSoulPortalGraph(object wgo)
        {
            Component component = wgo as Component;
            if (component == null) return null;

            Type controllerType = R.AnyType("FlowCanvas.FlowScriptController");
            if (controllerType == null) throw new MissingMemberException("FlowCanvas.FlowScriptController");

            MethodInfo getComponents = typeof(Component).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                {
                    if (m.Name != "GetComponentsInChildren" || m.IsGenericMethodDefinition) return false;
                    ParameterInfo[] p = m.GetParameters();
                    return p.Length == 2 && p[0].ParameterType == typeof(Type) && p[1].ParameterType == typeof(bool);
                });
            if (getComponents == null) throw new MissingMethodException("Component.GetComponentsInChildren(Type,bool)");

            Array controllers = getComponents.Invoke(component, new object[] { controllerType, true }) as Array;
            if (controllers == null) return null;

            foreach (object controller in controllers)
            {
                object graph = R.Get(controller, "graph") ?? R.Get(controller, "_graph");
                if (graph == null) continue;
                string graphName = Convert.ToString(R.Get(graph, "name"));
                if (string.Equals(graphName, "soul_portal", StringComparison.Ordinal))
                    return graph;
            }
            return null;
        }

        private static void ApplyCoefficient(object graph)
        {
            List<object> nodes = GetNodes(graph);
            List<object> candidates = new List<object>();

            foreach (object multiply in nodes)
            {
                if (!IsNodeType(multiply, "FloatMultiply")) continue;

                object coefficientNode = FindIncomingSource(multiply, "b");
                if (!IsNodeType(coefficientNode, "GetVariable`1")) continue;

                object valueParameter = R.Get(coefficientNode, "value");
                object rawValue = valueParameter == null ? null : R.Get(valueParameter, "_value");
                float coefficient = R.Float(rawValue);
                if (Math.Abs(coefficient - StockCoefficient) > 0.0001f &&
                    Math.Abs(coefficient - RebalancedCoefficient) > 0.0001f)
                    continue;

                object playerParamSource = FindIncomingSource(multiply, "a");
                if (!IsNodeType(playerParamSource, "Flow_GetPlayerParam")) continue;
                if (!string.Equals(Convert.ToString(InputValue(playerParamSource, "param")), "increase_gp_gain", StringComparison.Ordinal)) continue;

                object add = FindOutgoingTarget(multiply, "b");
                if (!IsNodeType(add, "FloatAdd")) continue;
                if (Math.Abs(R.Float(InputValue(add, "a")) - 1f) > 0.0001f) continue;

                candidates.Add(coefficientNode);
            }

            candidates = candidates.Distinct(ReferenceEqualityComparer.Instance).ToList();
            if (candidates.Count != 1)
                throw new InvalidOperationException("Expected exactly one Soul Contentment coefficient node; found " + candidates.Count + ".");

            object parameter = R.Get(candidates[0], "value");
            if (parameter == null) throw new MissingMemberException("Soul Contentment GetVariable<float>.value");
            float current = R.Float(R.Get(parameter, "_value"));
            if (Math.Abs(current - RebalancedCoefficient) <= 0.0001f) return;
            if (Math.Abs(current - StockCoefficient) > 0.0001f)
                throw new InvalidOperationException("Soul Contentment coefficient changed unexpectedly: " + current);

            R.Set(parameter, "_value", RebalancedCoefficient);
        }

        private static List<object> GetNodes(object graph)
        {
            IEnumerable enumerable = R.Get(graph, "allNodes") as IEnumerable ?? R.Get(graph, "_nodes") as IEnumerable;
            if (enumerable == null) throw new MissingMemberException("NodeCanvas.Graph.allNodes");

            var nodes = new List<object>();
            foreach (object node in enumerable)
                if (node != null) nodes.Add(node);
            return nodes;
        }

        private static object FindIncomingSource(object targetNode, string targetPort)
        {
            List<object> matches = Connections(targetNode, "inConnections")
                .Where(connection => string.Equals(Port(connection, "targetPortID", "_targetPortID"), targetPort, StringComparison.Ordinal))
                .Select(connection => R.Get(connection, "sourceNode") ?? R.Get(connection, "_sourceNode"))
                .Where(node => node != null)
                .ToList();

            if (matches.Count > 1)
                throw new InvalidOperationException("Soul Contentment graph has multiple incoming connections for port '" + targetPort + "'.");
            return matches.Count == 1 ? matches[0] : null;
        }

        private static object FindOutgoingTarget(object sourceNode, string targetPort)
        {
            List<object> matches = Connections(sourceNode, "outConnections")
                .Where(connection => string.Equals(Port(connection, "targetPortID", "_targetPortID"), targetPort, StringComparison.Ordinal))
                .Select(connection => R.Get(connection, "targetNode") ?? R.Get(connection, "_targetNode"))
                .Where(node => node != null)
                .ToList();

            if (matches.Count > 1)
                throw new InvalidOperationException("Soul Contentment graph has multiple outgoing connections targeting port '" + targetPort + "'.");
            return matches.Count == 1 ? matches[0] : null;
        }

        private static IEnumerable<object> Connections(object node, string member)
        {
            IEnumerable enumerable = R.Get(node, member) as IEnumerable ?? R.Get(node, "_" + member) as IEnumerable;
            if (enumerable == null) yield break;
            foreach (object connection in enumerable)
                if (connection != null) yield return connection;
        }

        private static string Port(object connection, string propertyName, string fieldName)
        {
            return Convert.ToString(R.Get(connection, propertyName) ?? R.Get(connection, fieldName));
        }

        private static object InputValue(object node, string key)
        {
            IDictionary values = R.Get(node, "_inputPortValues") as IDictionary;
            if (values == null || !values.Contains(key)) return null;
            return values[key];
        }

        private static bool IsNodeType(object node, string marker)
        {
            string fullName = node?.GetType().FullName ?? string.Empty;
            return fullName.IndexOf(marker, StringComparison.Ordinal) >= 0;
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
