﻿namespace DaLion.Stardew.Rings.Framework.Patches;

#region using directives

using DaLion.Stardew.Rings.Framework.VirtualProperties;
using HarmonyLib;
using StardewValley.Objects;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class CombinedRingOnLeaveLocationPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="CombinedRingOnLeaveLocationPatch"/> class.</summary>
    internal CombinedRingOnLeaveLocationPatch()
    {
        this.Target = this.RequireMethod<CombinedRing>(nameof(CombinedRing.onLeaveLocation));
    }

    #region harmony patches

    /// <summary>Remove Infinity Band resonance location effects.</summary>
    [HarmonyPostfix]
    private static void CombinedRingOnLeaveLocationPostfix(CombinedRing __instance, GameLocation environment)
    {
        __instance.Get_Chord()?.OnLeaveLocation(environment);
    }

    #endregion harmony patches
}
