﻿namespace DaLion.Stardew.Arsenal.Framework.Patches;

#region using directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using HarmonyLib;
using StardewValley.Tools;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class EventCommandAwardFestivalPrizePatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="EventCommandAwardFestivalPrizePatch"/> class.</summary>
    internal EventCommandAwardFestivalPrizePatch()
    {
        this.Target = this.RequireMethod<Event>(nameof(Event.command_awardFestivalPrize));
    }

    #region harmony patches

    /// <summary>Replaces rusty sword with wooden blade in Marlon's intro event.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? EventCommandAwardFestivalPrizeTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        var rusty = generator.DefineLabel();
        var resumeExecution = generator.DefineLabel();
        try
        {
            helper
                .FindFirst(
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Newobj, typeof(MeleeWeapon).RequireConstructor(typeof(int))))
                .AddLabels(rusty)
                .InsertInstructions(
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(ModEntry).RequirePropertyGetter(nameof(ModEntry.Config))),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(ModConfig).RequirePropertyGetter(nameof(ModConfig.WoodyReplacesRusty))),
                    new CodeInstruction(OpCodes.Brfalse_S, rusty),
                    new CodeInstruction(OpCodes.Ldc_I4_S, Constants.WoodenBladeIndex),
                    new CodeInstruction(OpCodes.Br_S, resumeExecution))
                .Advance()
                .AddLabels(resumeExecution);
        }
        catch (Exception ex)
        {
            Log.E($"Failed replacing rusty sword festival reward with wooden blade.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
