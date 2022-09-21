﻿namespace DaLion.Stardew.Tweex.Framework.Patches;

#region using directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using HarmonyLib;
using StardewValley.TerrainFeatures;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class TreeShakePatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="TreeShakePatch"/> class.</summary>
    internal TreeShakePatch()
    {
        this.Target = this.RequireMethod<Tree>("shake");
    }

    #region harmony patches

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? TreeShakeTranspiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // From: Game1.createObjectDebris(seedIndex, tileLocation.X, tileLocation.Y - 3, (tileLocation.Y + 1) * 64, 0, 1f, location);
        // To: Game1.createObjectDebris(seedIndex, tileLocation.X, tileLocation.Y - 3, (tileLocation.Y + 1) * 64, GetCoconutQuality(), 1f, location);
        //     -- and again for golden coconut immediately below
        try
        {
            var callCreateObjectDebrisInst = new CodeInstruction(
                OpCodes.Call,
                typeof(Game1).RequireMethod(
                    nameof(Game1.createObjectDebris),
                    new[]
                    {
                        typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(float),
                        typeof(GameLocation),
                    }));

            helper
                // the normal coconut
                .FindFirst(callCreateObjectDebrisInst)
                .RetreatUntil(new CodeInstruction(OpCodes.Ldc_I4_0))
                .ReplaceInstructionWith(
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(TreeShakePatch).RequireMethod(nameof(GetCoconutQuality))))
                .InsertInstructions(new CodeInstruction(OpCodes.Ldloc_2))
                // the golden coconut
                .FindNext(new CodeInstruction(OpCodes.Ldc_I4, 791))
                .AdvanceUntil(callCreateObjectDebrisInst)
                .RetreatUntil(new CodeInstruction(OpCodes.Ldc_I4_0))
                .ReplaceInstructionWith(
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(TreeShakePatch).RequireMethod(nameof(GetCoconutQuality))))
                .InsertInstructions(
                    new CodeInstruction(OpCodes.Ldc_I4, 791));
        }
        catch (Exception ex)
        {
            Log.E($"Failed applying Ecologist/Botanist perk to shaken coconut.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches

    #region injected subroutines

    private static int GetCoconutQuality(int seedIndex)
    {
        if (seedIndex is not (Constants.CoconutIndex or Constants.GoldenCoconutIndex) ||
            !ModEntry.Config.ProfessionalForagingInGingerIsland ||
            !Game1.player.professions.Contains(Farmer.botanist))
        {
            return SObject.lowQuality;
        }

        return ModEntry.ProfessionsApi is null
            ? SObject.bestQuality
            : ModEntry.ProfessionsApi.GetEcologistForageQuality(Game1.player);
    }

    #endregion injected subroutines
}
