﻿namespace DaLion.Stardew.Tweex.Framework.Patches;

#region using directives

using DaLion.Common.Extensions.Stardew;
using HarmonyLib;
using StardewValley.TerrainFeatures;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class FruitTreeDayUpdatePatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="FruitTreeDayUpdatePatch"/> class.</summary>
    internal FruitTreeDayUpdatePatch()
    {
        this.Target = this.RequireMethod<FruitTree>(nameof(FruitTree.dayUpdate));
        this.Prefix!.before = new[] { "DaLion.ImmersiveProfessions", "atravita.MoreFertilizers" };
        this.Postfix!.after = new[] { "DaLion.ImmersiveProfessions", "atravita.MoreFertilizers" };
    }

    #region harmony patches

    /// <summary>Record growth stage.</summary>
    [HarmonyPrefix]
    [HarmonyBefore("DaLion.ImmersiveProfessions", "atravita.MoreFertilizers")]
    private static void FruitTreeDayUpdatePrefix(FruitTree __instance, ref (int DaysUntilMature, int GrowthStage) __state)
    {
        __state.DaysUntilMature = __instance.daysUntilMature.Value;
        __state.GrowthStage = __instance.growthStage.Value;
    }

    /// <summary>Undo growth during winter.</summary>
    [HarmonyPostfix]
    [HarmonyAfter("DaLion.ImmersiveProfessions", "atravita.MoreFertilizers")]
    private static void FruitTreeDayUpdatePostfix(FruitTree __instance, (int DaysUntilMature, int GrowthStage) __state)
    {
        if (!ModEntry.Config.PreventFruitTreeGrowthInWinter || __instance.growthStage.Value >= FruitTree.treeStage ||
            !Game1.IsWinter || __instance.currentLocation.IsGreenhouse ||
            __instance.Read<int>("atravita.MoreFertilizer.FruitTree") > 0)
        {
            return;
        }

        __instance.daysUntilMature.Value = __state.DaysUntilMature;
        __instance.growthStage.Value = __state.GrowthStage;
    }

    #endregion harmony patches
}
