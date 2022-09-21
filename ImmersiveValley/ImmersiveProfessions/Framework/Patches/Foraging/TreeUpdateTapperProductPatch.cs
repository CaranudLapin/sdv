﻿namespace DaLion.Stardew.Professions.Framework.Patches.Foraging;

#region using directives

using DaLion.Common.Extensions.Stardew;
using DaLion.Stardew.Professions.Extensions;
using HarmonyLib;
using StardewValley.TerrainFeatures;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class TreeUpdateTapperProductPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="TreeUpdateTapperProductPatch"/> class.</summary>
    internal TreeUpdateTapperProductPatch()
    {
        this.Target = this.RequireMethod<Tree>(nameof(Tree.UpdateTapperProduct));
    }

    #region harmony patches

    /// <summary>Patch to decrease syrup production time for Tapper.</summary>
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void TreeUpdateTapperProductPostfix(Tree __instance, SObject? tapper_instance)
    {
        if (tapper_instance is null || (__instance.treeType.Value == Tree.mushroomTree && Game1.currentSeason == "winter"))
        {
            return;
        }

        var owner = ModEntry.Config.LaxOwnershipRequirements ? Game1.player : tapper_instance.GetOwner();
        if (!owner.HasProfession(Profession.Tapper))
        {
            return;
        }

        if (tapper_instance.MinutesUntilReady > 0)
        {
            tapper_instance.MinutesUntilReady = (int)(tapper_instance.MinutesUntilReady *
                                                      (owner.HasProfession(Profession.Tapper, true) ? 0.5 : 0.75));
        }
    }

    #endregion harmony patches
}
