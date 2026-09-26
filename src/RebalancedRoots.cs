using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedRoots
    {
        private const string StockPlantTerm = "0.2*WGOpar(\"buff_plant\")";
        private const string StockGrowTimeTerm = "0.2*WGOpar(\"grow_time\")";
        private const string StockPlantParam = "buff_plant";
        private const string StockGrowTimeParam = "grow_time";
        private const float StockReductionPerUnit = 0.20f;

        internal const float MaxCombinedGrowthReduction = 0.95f;

        private static readonly HashSet<string> PlantCraftIds =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "garden_wheat_growing",
                "garden_cannabis_growing",
                "garden_cabbage_growing",
                "garden_carrot_growing",
                "garden_beet_growing",
                "garden_grapes_growing",
                "garden_lentils_growing",
                "garden_pumpkin_growing",
                "garden_onion_growing",
                "garden_hop_growing",
                "garden_wheat_grow_desk_planting",
                "garden_cabbage_grow_desk_planting",
                "garden_carrot_grow_desk_planting",
                "garden_beet_grow_desk_planting",
                "garden_lentils_grow_desk_planting_1",
                "garden_lentils_grow_desk_planting_2",
                "garden_lentils_grow_desk_planting_3",
                "garden_pumpkin_grow_desk_planting_1",
                "garden_pumpkin_grow_desk_planting_2",
                "garden_pumpkin_grow_desk_planting_3",
                "garden_onion_grow_desk_planting_1",
                "garden_onion_grow_desk_planting_2",
                "garden_onion_grow_desk_planting_3",
                "garden_grapes_grow_vineyard_planting_1",
                "garden_grapes_grow_vineyard_planting_2",
                "garden_grapes_grow_vineyard_planting_3",
                "garden_hops_grow_vineyard_planting_1",
                "garden_hops_grow_vineyard_planting_2",
                "garden_hops_grow_vineyard_planting_3",
                "refugee_garden_wheat_grow_desk_planting",
                "refugee_garden_cabbage_grow_desk_planting",
                "refugee_garden_carrot_grow_desk_planting",
                "refugee_garden_beet_grow_desk_planting",
                "refugee_garden_lentils_grow_desk_planting_1",
                "refugee_garden_lentils_grow_desk_planting_2",
                "refugee_garden_lentils_grow_desk_planting_3",
                "refugee_garden_pumpkin_grow_desk_planting_1",
                "refugee_garden_pumpkin_grow_desk_planting_2",
                "refugee_garden_pumpkin_grow_desk_planting_3",
                "refugee_garden_onion_grow_desk_planting_1",
                "refugee_garden_onion_grow_desk_planting_2",
                "refugee_garden_onion_grow_desk_planting_3",
                "bush_berry_garden",
                "bush_berry_garden_growing",
                "bush_1_berry_respawn",
                "bush_2_berry_respawn",
                "bush_3_berry_respawn",
                "tree_apple_garden_growing",
                "tree_apple_garden_crops_growing",
                "spawn_tree",
                "tree_growing",
                "flower_spawn",
                "hiccup_grass_spawn",
                "mushroom_spawn",
                "spawn_tree_apple",
                "tree_apple_growing_1",
                "tree_apple_growing_2",
                "tree_apple_growing_3"
            };

        private static readonly HashSet<string> GrowTimeCraftIds =
            new HashSet<string>(StringComparer.Ordinal);

        private sealed class ScopedPlantParamState
        {
            internal object RuntimeEffect;
            internal float OriginalValue;
        }

        private static ManualLogSource _log;
        private static MethodInfo _runtimeEffectGet;
        private static MethodInfo _runtimeEffectSet;
        private static MethodInfo _runtimeEffectRemoveZeroValues;
        private static MethodInfo _wgoGetParam;
        private static MethodInfo _getRawExpression;
        private static bool _runtimeFailureLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type craftComponent = R.GameType("CraftComponent");
            Type worldGameObject = R.GameType("WorldGameObject");
            Type smartExpression = R.GameType("SmartExpression");
            Type gameRes = R.GameType("GameRes");
            if (craftComponent == null) throw new MissingMemberException("CraftComponent");
            if (worldGameObject == null) throw new MissingMemberException("WorldGameObject");
            if (smartExpression == null) throw new MissingMemberException("SmartExpression");
            if (gameRes == null) throw new MissingMemberException("GameRes");

            MethodInfo doAction = R.Method(
                craftComponent,
                "DoAction",
                false,
                new[] { worldGameObject, typeof(float), typeof(bool) });
            if (doAction == null)
                throw new MissingMethodException("CraftComponent.DoAction(WorldGameObject,float,bool)");

            _runtimeEffectGet = gameRes.GetMethod(
                "Get",
                R.Inst,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            _runtimeEffectSet = gameRes.GetMethod(
                "Set",
                R.Inst,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            _runtimeEffectRemoveZeroValues = gameRes.GetMethod(
                "RemoveZeroValues",
                R.Inst,
                null,
                Type.EmptyTypes,
                null);
            _wgoGetParam = worldGameObject.GetMethod(
                "GetParam",
                R.Inst,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            _getRawExpression = smartExpression.GetMethod(
                "GetRawExpressionString",
                R.Inst,
                null,
                Type.EmptyTypes,
                null);

            if (_runtimeEffectGet == null) throw new MissingMethodException("GameRes.Get(string,float)");
            if (_runtimeEffectSet == null) throw new MissingMethodException("GameRes.Set(string,float)");
            if (_runtimeEffectRemoveZeroValues == null) throw new MissingMethodException("GameRes.RemoveZeroValues()");
            if (_wgoGetParam == null) throw new MissingMethodException("WorldGameObject.GetParam(string,float)");
            if (_getRawExpression == null) throw new MissingMethodException("SmartExpression.GetRawExpressionString()");

            R.PatchHooks(
                harmonyId + ".roots.scope",
                typeof(RebalancedRoots),
                doAction,
                nameof(DoActionPrefix),
                null,
                nameof(DoActionFinalizer));
        }

        internal static void ValidateDefinitions()
        {
            GrowTimeCraftIds.Clear();

            foreach (string craftId in PlantCraftIds)
            {
                object craft = R.BalanceData(craftId, "CraftDefinition", true);
                if (craft == null)
                {
                    if (IsOptionalPlantConsumer(craftId)) continue;
                    throw new MissingMemberException("Missing verified Roots consumer craft " + craftId);
                }

                object craftTime = R.Get(craft, "craft_time");
                string raw = craftTime == null ? null : _getRawExpression.Invoke(craftTime, null) as string;
                if (string.IsNullOrEmpty(raw))
                    throw new InvalidOperationException("Roots consumer " + craftId + " has no craft_time expression.");

                int first = raw.IndexOf(StockPlantTerm, StringComparison.Ordinal);
                if (first < 0 || raw.IndexOf(StockPlantTerm, first + StockPlantTerm.Length, StringComparison.Ordinal) >= 0)
                    throw new InvalidOperationException(
                        "Roots consumer " + craftId + " no longer contains exactly one verified stock prayer term: " + raw);

                if (raw.IndexOf(StockGrowTimeTerm, StringComparison.Ordinal) >= 0)
                    GrowTimeCraftIds.Add(craftId);
            }
        }

        private static void DoActionPrefix(object __instance, ref ScopedPlantParamState __state)
        {
            __state = null;
            if (!RebalancedRuntimeState.IsReady) return;
            try
            {
                object craft = R.Get(__instance, "current_craft");
                string craftId = R.Id(craft);
                if (string.IsNullOrEmpty(craftId) || !PlantCraftIds.Contains(craftId)) return;

                float active = RebalancedTierState.GetPlayerParam(StockPlantParam, 0f);
                if (active <= 0.0001f) return;

                float reduction = RebalancedTierState.GetPlayerParam(RebalancedTierState.PlantReductionParam, 0f);
                if (reduction <= 0.0001f) return;

                object wgo = R.Get(__instance, "wgo");
                if (wgo == null) return;

                // WGOpar(name) is data.GetParam(name, 0) + wgo.totem_effect.Get(name, 0).
                // totem_effect is a native NonSerialized runtime aggregate, so this scopes
                // the Rebalanced bridge to the current native craft evaluation without
                // touching the plant's serialized Item data.
                object runtimeEffect = R.Get(wgo, "totem_effect");
                if (runtimeEffect == null) return;

                float original = Convert.ToSingle(
                    _runtimeEffectGet.Invoke(runtimeEffect, new object[] { StockPlantParam, 0f }));

                // Preserve the game's native additive relationship while preventing
                // fertilizer + prayer from reducing craft time to zero. Count only
                // modifiers that this verified stock expression actually consumes.
                float effectivePlant = Convert.ToSingle(
                    _wgoGetParam.Invoke(wgo, new object[] { StockPlantParam, 0f }));
                float existingReduction = Math.Max(0f, effectivePlant) * StockReductionPerUnit;

                if (GrowTimeCraftIds.Contains(craftId))
                {
                    float growTime = Convert.ToSingle(
                        _wgoGetParam.Invoke(wgo, new object[] { StockGrowTimeParam, 0f }));
                    existingReduction += Math.Max(0f, growTime) * StockReductionPerUnit;
                }

                float availableReduction = Math.Max(0f, MaxCombinedGrowthReduction - existingReduction);
                float appliedReduction = Math.Min(reduction, availableReduction);
                if (appliedReduction <= 0.0001f) return;

                float projectedStockValue = appliedReduction / StockReductionPerUnit;

                __state = new ScopedPlantParamState
                {
                    RuntimeEffect = runtimeEffect,
                    OriginalValue = original
                };

                _runtimeEffectSet.Invoke(
                    runtimeEffect,
                    new object[] { StockPlantParam, original + projectedStockValue });
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PrayerClarity: Rebalanced Roots scope injection failed; this growth tick keeps stock behavior. ", ex);
                Restore(__state);
                __state = null;
            }
        }

        private static Exception DoActionFinalizer(Exception __exception, ScopedPlantParamState __state)
        {
            Restore(__state);
            return __exception;
        }

        private static void Restore(ScopedPlantParamState state)
        {
            if (state == null || state.RuntimeEffect == null) return;
            try
            {
                _runtimeEffectSet.Invoke(
                    state.RuntimeEffect,
                    new object[] { StockPlantParam, state.OriginalValue });

                // A temporary zero entry is semantically empty and totem_effect is
                // NonSerialized. Removing zero entries keeps the runtime aggregate clean.
                if (Math.Abs(state.OriginalValue) <= 0.0001f)
                    _runtimeEffectRemoveZeroValues.Invoke(state.RuntimeEffect, null);
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PrayerClarity: Rebalanced Roots scope restoration failed. ", ex);
            }
        }

        private static bool IsOptionalPlantConsumer(string craftId)
        {
            return craftId != null && craftId.StartsWith("refugee_", StringComparison.Ordinal);
        }

        private static void LogRuntimeFailure(string message, Exception ex)
        {
            if (_runtimeFailureLogged) return;
            _runtimeFailureLogged = true;
            _log?.LogError("PC_RUNTIME_FALLBACK edition=rebalanced feature=roots action=stock " + message + ex);
        }
    }
}
