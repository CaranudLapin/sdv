﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.Automate;

#region using directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common;
using DaLion.Common.Attributes;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using HarmonyLib;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
[RequiresMod("Pathoschild.Automate")]
internal sealed class CrabPotMachineGetStatePatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="CrabPotMachineGetStatePatch"/> class.</summary>
    internal CrabPotMachineGetStatePatch()
    {
        this.Target = "Pathoschild.Stardew.Automate.Framework.Machines.Objects.CrabPotMachine"
            .ToType()
            .RequireMethod("GetState");
    }

    #region harmony patches

    /// <summary>Patch for conflicting Luremaster and Conservationist automation rules.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? CrabPotMachineGetStateTranspiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // Removed: || !this.PlayerNeedsBait()
        try
        {
            helper
                .FindFirst(new CodeInstruction(OpCodes.Brtrue_S))
                .RemoveInstructionsUntil(
                    new CodeInstruction(OpCodes.Call, "CrabPotMachine"
                        .ToType()
                        .RequireMethod("PlayerNeedsBait")))
                .SetOpCode(OpCodes.Brfalse_S);
        }
        catch (Exception ex)
        {
            Log.E("Immersive Professions failed while patching bait conditions for automated Crab Pots." +
                  "\n—-- Do NOT report this to Automate's author. ---" +
                  $"\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
