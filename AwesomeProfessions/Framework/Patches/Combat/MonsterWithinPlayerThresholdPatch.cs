﻿namespace DaLion.Stardew.Professions.Framework.Patches.Combat;

#region using directives

using System;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using StardewValley;
using StardewValley.Monsters;

using SuperMode;

#endregion using directives

[UsedImplicitly]
internal class MonsterWithinPlayerThresholdPatch : BasePatch
{
    /// <summary>Construct an instance.</summary>
    internal MonsterWithinPlayerThresholdPatch()
    {
        Original = RequireMethod<Monster>(nameof(Monster.withinPlayerThreshold), new Type[] { });
    }

    #region harmony patch

    /// <summary>Patch to make Poacher invisible in Super Mode.</summary>
    [HarmonyPrefix]
    private static bool MonsterWithinPlayerThresholdPrefix(Monster __instance, ref bool __result)
    {
        try
        {
            var foundPlayer = ModEntry.ModHelper.Reflection.GetMethod(__instance, "findPlayer").Invoke<Farmer>();
            if (!foundPlayer.IsLocalPlayer || ModEntry.State.Value.SuperMode is not PoacherColdBlood {IsActive: true})
                return true; // run original method

            __result = false;
            return false; // don't run original method
        }
        catch (Exception ex)
        {
            Log.E($"Failed in {MethodBase.GetCurrentMethod()?.Name}:\n{ex}");
            return true; // default to original logic
        }
    }

    #endregion harmony patch
}