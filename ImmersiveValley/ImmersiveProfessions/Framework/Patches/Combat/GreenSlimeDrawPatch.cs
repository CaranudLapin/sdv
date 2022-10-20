﻿namespace DaLion.Stardew.Professions.Framework.Patches.Combat;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common.Attributes;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Monsters;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
[Deprecated]
internal sealed class GreenSlimeDrawPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="GreenSlimeDrawPatch"/> class.</summary>
    internal GreenSlimeDrawPatch()
    {
        this.Target = this.RequireMethod<GreenSlime>(nameof(GreenSlime.draw), new[] { typeof(SpriteBatch) });
    }

    #region harmony patches

    /// <summary>Patch to fix Green Slime eye and antenna position when inflated.</summary>
    [HarmonyTranspiler]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1114:Parameter list should follow declaration", Justification = "Transpiler benefits from line-by-line commentary.")]
    private static IEnumerable<CodeInstruction>? GreenSlimeDrawTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // Injected: antenna position += GetAntennaOffset(this)
        //			 eyes position += GetEyesOffset(this)
        var drawInstructions = new CodeInstruction[]
        {
            new(OpCodes.Ldarg_1),
            new(OpCodes.Ldarg_0),
            new(OpCodes.Callvirt, typeof(Character).RequirePropertyGetter(nameof(Character.Sprite))),
            new(OpCodes.Callvirt, typeof(AnimatedSprite).RequirePropertyGetter(nameof(AnimatedSprite.Texture))),
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldsfld, typeof(Game1).RequireField(nameof(Game1.viewport))),
            new(OpCodes.Call, typeof(Character).RequireMethod(nameof(Character.getLocalPosition))),
        };

        try
        {
            helper
                .FindFirst(drawInstructions) // the main sprite draw call
                .FindNext(drawInstructions) // find antenna draw call
                .AdvanceUntil(new CodeInstruction(OpCodes.Ldloc_S, helper.Locals[5])) // advance until end of position argument
                .Retreat()
                .GetInstructions(out var got, advance: true) // copy vector addition instruction
                .InsertInstructions(
                    // insert custom offset
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(GreenSlimeDrawPatch).RequireMethod(nameof(GetAntennaeOffset))))
                .InsertInstructions(got) // insert addition
                .FindNext(drawInstructions) // find eyes draw call
                .AdvanceUntil(new CodeInstruction(OpCodes.Ldc_I4_S, 32)) // advance until end of position argument
                .InsertInstructions(
                    // insert custom offset
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(GreenSlimeDrawPatch).RequireMethod(nameof(GetEyesOffset))))
                .InsertInstructions(got); // insert addition
        }
        catch (Exception ex)
        {
            Log.E($"Failed while patching inflated Green Slime sprite.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches

    #region injected subroutines

    private static Vector2 GetAntennaeOffset(GreenSlime slime)
    {
        if (slime.Scale <= 1f)
        {
            return Vector2.Zero;
        }

        var x = MathHelper.Lerp(0, -32f, slime.Scale - 1f);
        var y = MathHelper.Lerp(0, -64f, slime.Scale - 1f);
        return new Vector2(x, y);
    }

    private static Vector2 GetEyesOffset(GreenSlime slime)
    {
        if (slime.Scale <= 1f)
        {
            return Vector2.Zero;
        }

        var x = MathHelper.Lerp(0, -32f, slime.Scale - 1f);
        var y = MathHelper.Lerp(0, -32f, slime.Scale - 1f);
        return new Vector2(x, y);
    }

    #endregion injected subroutines
}
