﻿namespace DaLion.Stardew.Tweex.Framework.Patches;

#region using directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common;
using DaLion.Common.Attributes;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using DaLion.Stardew.Tweex.Extensions;
using HarmonyLib;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
[RequiresMod("Pathoschild.Automate")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Integration patch.")]
internal sealed class BeeHouseMachineGetOutputPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="BeeHouseMachineGetOutputPatch"/> class.</summary>
    internal BeeHouseMachineGetOutputPatch()
    {
        this.Target = "Pathoschild.Stardew.Automate.Framework.Machines.Objects.BeeHouseMachine"
            .ToType()
            .RequireMethod("GetOutput");
    }

    #region harmony patches

    /// <summary>Adds aging quality to automated bee houses.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? BeeHouseMachineGetOutputTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // Injected: if (ModEntry.Config.AgeImprovesBeeHouses) object.Quality = obj.GetQualityFromAge();
        // Before: StardewValley.Object result = obj;
        var resumeExecution = generator.DefineLabel();
        try
        {
            helper
                .FindLast(new CodeInstruction(OpCodes.Stloc_S, helper.Locals[4]))
                .AddLabels(resumeExecution)
                .InsertInstructions(
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(ModEntry).RequirePropertyGetter(nameof(ModEntry.Config))),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(ModConfig).RequirePropertyGetter(nameof(ModConfig.AgeImprovesBeeHouses))),
                    new CodeInstruction(OpCodes.Brfalse_S, resumeExecution),
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(
                        OpCodes.Call,
                        "Pathoschild.Stardew.Automate.Framework.BaseMachine`1"
                            .ToType()
                            .MakeGenericType(typeof(SObject))
                            .RequirePropertyGetter("Machine")),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(SObjectExtensions).RequireMethod(nameof(SObjectExtensions.GetQualityFromAge))),
                    new CodeInstruction(
                        OpCodes.Callvirt,
                        typeof(SObject).RequirePropertySetter(nameof(SObject.Quality))));
        }
        catch (Exception ex)
        {
            Log.E("Immersive Tweaks failed improving automated honey quality with age." +
                  "\n—-- Do NOT report this to Automate's author. ---" +
                  $"\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
