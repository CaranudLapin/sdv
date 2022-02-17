﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.Automate;

#region using directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;

using Stardew.Common.Extensions;
using Stardew.Common.Harmony;
using Extensions;

using SObject = StardewValley.Object;

#endregion using directives

internal class CheesePressMachineSetInput : BasePatch
{
    /// <summary>Construct an instance.</summary>
    internal CheesePressMachineSetInput()
    {
        try
        {
            Original = "Pathoschild.Stardew.Automate.Framework.Machines.Objects.CheesePressMachine".ToType()
                .MethodNamed("SetInput");
        }
        catch
        {
            // ignored
        }
    }

    #region harmony patches

    /// <summary>Patch to apply Artisan effects to automated Cheese Press.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> GenericObjectMachineGenericPullRecipeTranspiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var helper = new ILHelper(original, instructions);

        /// Injected: GenericPullRecipeSubroutine(this, consumable)
        /// Before: return true;

        try
        {
            helper
                .FindFirst(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call)
                )
                .ToBuffer(2)
                .FindNext(
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Ret)
                )
                .InsertBuffer()
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call,
                        typeof(CheesePressMachineSetInput).MethodNamed(
                            nameof(SetInputSubroutine)))
                );
        }
        catch (Exception ex)
        {
            Log.E($"Failed while patching modded Artisan behavior for automated Cheese Press.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches

    #region private methods

    private static void SetInputSubroutine(SObject machine, object consumable)
    {
        if (!machine.heldObject.Value.IsArtisanGood()) return;

        if (consumable.GetType().GetProperty("Sample")?.GetValue(consumable) is not SObject input) return;

        // large milk gives double output at normal quality
        var output = machine.heldObject.Value;
        if (input.Name.ContainsAnyOf("Large", "L."))
        {
            output.Stack = 2;
            output.Quality = SObject.lowQuality;
        }

        var owner = Game1.getFarmerMaybeOffline(machine.owner.Value) ?? Game1.MasterPlayer;
        if (!owner.HasProfession(Profession.Artisan)) return;

        output.Quality = input.Quality;
        if (output.Quality < SObject.bestQuality &&
            new Random(Guid.NewGuid().GetHashCode()).NextDouble() < 0.05)
            output.Quality += output.Quality == SObject.highQuality ? 2 : 1;

        if (owner.HasProfession(Profession.Artisan, true))
            machine.MinutesUntilReady -= machine.MinutesUntilReady / 4;
        else
            machine.MinutesUntilReady -= machine.MinutesUntilReady / 10;
    }

    #endregion private methods
}