﻿namespace DaLion.Stardew.Tweex.Framework.Patches;

#region using directives

using System;
using DaLion.Common.Extensions.Stardew;
using DaLion.Stardew.Tweex.Extensions;
using HarmonyLib;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class ObjectDayUpdatePatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="ObjectDayUpdatePatch"/> class.</summary>
    internal ObjectDayUpdatePatch()
    {
        this.Target = this.RequireMethod<SObject>(nameof(SObject.DayUpdate));
        this.Postfix!.priority = Priority.LowerThanNormal;
    }

    #region harmony patches

    /// <summary>Age bee houses and mushroom boxes.</summary>
    [HarmonyPostfix]
    [HarmonyPriority(Priority.LowerThanNormal)]
    private static void ObjectDayUpdatePostfix(SObject __instance)
    {
        if (__instance.IsBeeHouse() && ModEntry.Config.AgeImprovesBeeHouses)
        {
            __instance.Increment("Age");
        }
        else if (__instance.IsMushroomBox() && ModEntry.Config.AgeImprovesMushroomBoxes)
        {
            __instance.Increment("Age");
            if (__instance.heldObject.Value is null)
            {
                return;
            }

            __instance.heldObject.Value.Quality = ModEntry.ProfessionsApi is null
                ? Game1.player.professions.Contains(Farmer.botanist)
                    ? SObject.bestQuality
                    : __instance.GetQualityFromAge()
                : Math.Max(
                    ModEntry.ProfessionsApi.GetEcologistForageQuality(Game1.player),
                    __instance.GetQualityFromAge());
        }
    }

    #endregion harmony patches
}
