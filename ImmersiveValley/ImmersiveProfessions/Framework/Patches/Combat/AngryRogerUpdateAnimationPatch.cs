﻿namespace DaLion.Stardew.Professions.Framework.Patches.Combat;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using DaLion.Stardew.Professions.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class AngryRogerUpdateAnimationPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="AngryRogerUpdateAnimationPatch"/> class.</summary>
    internal AngryRogerUpdateAnimationPatch()
    {
        this.Target = this.RequireMethod<AngryRoger>("updateAnimation", new[] { typeof(GameTime) });
    }

    #region harmony patches

    /// <summary>Patch to hide Poacher in ambush from Angry Roger gaze.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? AngryRogerUpdateAnimationTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // From: faceGeneralDirection(base.Player.getStandingPosition());
        // To: if (!base.Player.IsInAmbush()) faceGeneralDirection(base.Player.getStandingPosition());
        try
        {
            var skip = generator.DefineLabel();
            helper
                .FindLast(
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(Character).RequireMethod(
                            nameof(Character.faceGeneralDirection),
                            new[] { typeof(Vector2), typeof(int), typeof(bool) })))
                .RetreatUntil(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0))
                .StripLabels(out var labels)
                .InsertWithLabels(
                    labels,
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, typeof(Monster).RequirePropertyGetter(nameof(Monster.Player))),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(FarmerExtensions).RequireMethod(nameof(FarmerExtensions.IsInAmbush))),
                    new CodeInstruction(OpCodes.Brtrue_S, skip))
                .AdvanceUntil(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, typeof(Monster).RequireMethod("resetAnimationSpeed")))
                .AddLabels(skip);
        }
        catch (Exception ex)
        {
            Log.E($"Failed while patching Angry Roger eye-stalking hidden Poachers.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
