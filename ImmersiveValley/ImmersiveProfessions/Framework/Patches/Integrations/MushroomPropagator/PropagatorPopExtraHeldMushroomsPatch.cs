﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.MushroomPropagator;

#region using directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common;
using DaLion.Common.Attributes;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Extensions.Stardew;
using DaLion.Common.Harmony;
using DaLion.Stardew.Professions.Extensions;
using HarmonyLib;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
[RequiresMod("blueberry.MushroomPropagator")]
internal sealed class PropagatorPopExtraHeldMushroomsPatch : HarmonyPatch
{
    private static readonly Lazy<Func<SObject, int>> GetSourceMushroomQuality = new(() =>
        "BlueberryMushroomMachine.Propagator"
            .ToType()
            .RequireField("SourceMushroomQuality")
            .CompileUnboundFieldGetterDelegate<SObject, int>());

    /// <summary>Initializes a new instance of the <see cref="PropagatorPopExtraHeldMushroomsPatch"/> class.</summary>
    internal PropagatorPopExtraHeldMushroomsPatch()
    {
        this.Target = "BlueberryMushroomMachine.Propagator"
            .ToType()
            .RequireMethod("PopExtraHeldMushrooms");
    }

    #region harmony patches

    /// <summary>Patch for Propagator output quantity (Harvester) and quality (Ecologist).</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? PropagatorPopExtraHeldMushroomsTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // From: ceq 0
        // To: Game1.player.professions.Contains(<forager_id>) ? !cgt 0 : clt 0
        var isNotPrestiged = generator.DefineLabel();
        var resumeExecution = generator.DefineLabel();
        try
        {
            helper
                .FindProfessionCheck(Profession.Forager.Value) // find index of forager check
                .AdvanceUntil(new CodeInstruction(OpCodes.Ldc_I4_0))
                .SetOpCode(OpCodes.Ldc_I4_1)
                .Advance()
                .InsertProfessionCheck(Profession.Forager.Value + 100)
                .InsertInstructions(
                    new CodeInstruction(OpCodes.Brfalse_S, isNotPrestiged),
                    new CodeInstruction(OpCodes.Cgt_Un),
                    new CodeInstruction(OpCodes.Not),
                    new CodeInstruction(OpCodes.Br_S, resumeExecution))
                .InsertWithLabels(
                    new[] { isNotPrestiged },
                    new CodeInstruction(OpCodes.Clt_Un))
                .RemoveInstructions()
                .AddLabels(resumeExecution);
        }
        catch (Exception ex)
        {
            Log.E("Immersive Professions failed while patching Blueberry's Mushroom Propagator output quantity." +
                  "\n—-- Do NOT report this to Mushroom Propagator's author. ---" +
                  $"\nHelper returned {ex}");
            return null;
        }

        // From: int popQuality = Game1.player.professions.Contains(<ecologist_id>) ? 4 : SourceMushroomQuality);
        // To: int popQuality = PopExtraHeldMushroomsSubroutine(this);
        try
        {
            helper
                .FindProfessionCheck(Profession.Ecologist.Value) // find index of ecologist check
                .Retreat()
                .GetLabels(out var labels)
                .RemoveInstructionsUntil(new CodeInstruction(OpCodes.Ldc_I4_4))
                .InsertWithLabels(
                    labels,
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(PropagatorPopExtraHeldMushroomsPatch)
                            .RequireMethod(nameof(PopExtraHeldMushroomsSubroutine))))
                .RemoveLabels();
        }
        catch (Exception ex)
        {
            Log.E($"Failed while patching Blueberry's Mushroom Propagator output quality.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches

    #region injected subroutines

    private static int PopExtraHeldMushroomsSubroutine(SObject propagator)
    {
        var who = ModEntry.Config.LaxOwnershipRequirements ? Game1.player : propagator.GetOwner();
        return who.HasProfession(Profession.Ecologist)
            ? who.GetEcologistForageQuality()
            : GetSourceMushroomQuality.Value(propagator);
    }

    #endregion injected subroutines
}
