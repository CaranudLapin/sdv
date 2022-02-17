﻿namespace DaLion.Stardew.Professions.Framework.Patches.Fishing;

#region using directives

using System;
using HarmonyLib;
using JetBrains.Annotations;
using StardewValley;
using StardewValley.Buildings;

using Stardew.Common.Extensions;

using SObject = StardewValley.Object;

#endregion using directives

[UsedImplicitly]
internal class FishPondAddFishToPondPatch : BasePatch
{
    /// <summary>Construct an instance.</summary>
    internal FishPondAddFishToPondPatch()
    {
        Original = RequireMethod<FishPond>("addFishToPond");
    }

    #region harmony patches

    /// <summary>Patch to increment total Fish Pond quality rating.</summary>
    [HarmonyPostfix]
    private static void FishPondOnFisTypeChangedPostfix(FishPond __instance, SObject fish)
    {
        if (!ModEntry.Config.EnableFishPondRebalance) return;

        var owner = Game1.getFarmerMaybeOffline(__instance.owner.Value) ?? Game1.MasterPlayer;
        var qualityRatingByFishPond =
            ModData.Read(DataField.QualityRatingByFishPond, owner).ToDictionary<int, int>(",", ";");
        var thisFishPond = __instance.GetCenterTile().ToString().GetDeterministicHashCode();
        qualityRatingByFishPond.TryGetValue(thisFishPond, out var currentRating);
        qualityRatingByFishPond[thisFishPond] = currentRating +
                                                (int) Math.Pow(16,
                                                    fish.Quality == SObject.bestQuality
                                                        ? fish.Quality - 1
                                                        : fish.Quality);
        ModData.Write(DataField.QualityRatingByFishPond, qualityRatingByFishPond.ToString(",", ";"), owner);
    }

    #endregion harmony patches
}