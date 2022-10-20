﻿namespace DaLion.Stardew.Arsenal.Framework.Patches;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using DaLion.Stardew.Arsenal.Framework.Enchantments;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Tools;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class MeleeWeaponTriggerClubFunctionPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="MeleeWeaponTriggerClubFunctionPatch"/> class.</summary>
    internal MeleeWeaponTriggerClubFunctionPatch()
    {
        this.Target = this.RequireMethod<MeleeWeapon>(nameof(MeleeWeapon.triggerClubFunction));
    }

    #region harmony patches

    /// <summary>Doubles AoE of Infinity Club's special smash move.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? MeleeWeaponTriggerClubFunctionTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // Injected: if (this.hasEnchantmentOfType<InfinityEnchantment>() areaOfEffect.Inflate(96, 96);
        // After: new Rectangle((int)lastUser.Position.X - 192, lastUser.GetBoundingBox().Y - 192, 384, 384)
        try
        {
            var notInfinity = generator.DefineLabel();
            var aoe = generator.DeclareLocal(typeof(Rectangle));
            helper
                .FindFirst(
                    new CodeInstruction(OpCodes.Newobj))
                .Advance()
                .InsertInstructions(
                    new CodeInstruction(OpCodes.Stloc_S, aoe))
                .InsertWithLabels(
                    new[] { notInfinity },
                    new CodeInstruction(OpCodes.Ldloc_S, aoe))
                .Retreat()
                .InsertInstructions(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(MeleeWeapon)
                            .RequireMethod(nameof(MeleeWeapon.hasEnchantmentOfType))
                            .MakeGenericMethod(typeof(InfinityEnchantment))),
                    new CodeInstruction(OpCodes.Brfalse_S, notInfinity),
                    new CodeInstruction(OpCodes.Ldloca_S, aoe),
                    new CodeInstruction(OpCodes.Ldc_I4_S, 96),
                    new CodeInstruction(OpCodes.Ldc_I4_S, 96),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(Rectangle).RequireMethod(nameof(Rectangle.Inflate), new[] { typeof(int), typeof(int) })));
        }
        catch (Exception ex)
        {
            Log.E($"Failed adding infinity club effect.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
