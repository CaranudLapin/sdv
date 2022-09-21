﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.CJBCheatsMenu;

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
[RequiresMod("CJBok.CheatsMenu")]
internal sealed class ProfessionsCheatSetProfessionPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="ProfessionsCheatSetProfessionPatch"/> class.</summary>
    internal ProfessionsCheatSetProfessionPatch()
    {
        this.Target = "CJBCheatsMenu.Framework.Cheats.Skills.ProfessionsCheat"
            .ToType()
            .RequireMethod("SetProfession");
    }

    #region harmony patches

    /// <summary>Patch to move bonus health from Defender to Brute.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? ProfessionsCheatSetProfessionTranspiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // From: case <defender_id>
        // To: case <brute_id>
        try
        {
            helper
                .FindFirst(new CodeInstruction(OpCodes.Ldc_I4_S, Farmer.defender))
                .SetOperand(Profession.Brute.Value);
        }
        catch (Exception ex)
        {
            Log.E(
                "Immersive Professions failed while moving CJB Profession Cheat health bonus from Defender to Brute." +
                "\n—-- Do NOT report this to CJB Cheats Menu's author. ---" +
                $"\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
