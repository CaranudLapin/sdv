﻿namespace DaLion.Stardew.Slingshots.Framework.Patches;

#region using directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common;
using DaLion.Common.Harmony;
using HarmonyLib;
using StardewValley.Tools;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class BaseEnchantmentGetEnchantmentFromItemPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="BaseEnchantmentGetEnchantmentFromItemPatch"/> class.</summary>
    internal BaseEnchantmentGetEnchantmentFromItemPatch()
    {
        this.Target = this.RequireMethod<BaseEnchantment>(nameof(BaseEnchantment.GetEnchantmentFromItem));
    }

    #region harmony patches

    /// <summary>Allow Slingshot forges.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? BaseEnchantmentGetEnchantmentFromItemTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // From: if (base_item == null || (base_item is MeleeWeapon && !(base_item as MeleeWeapon).isScythe()))
        // To: if (base_item == null || (base_item is MeleeWeapon && !(base_item as MeleeWeapon).isScythe()) || base_item is Slingshot)
        var isNotMeleeWeaponButMaybeSlingshot = generator.DefineLabel();
        var canForge = generator.DefineLabel();
        try
        {
            helper
                .AdvanceUntil(new CodeInstruction(OpCodes.Brfalse_S))
                .AdvanceUntil(new CodeInstruction(OpCodes.Brfalse))
                .GetOperand(out var cannotForge)
                .SetOperand(isNotMeleeWeaponButMaybeSlingshot)
                .AdvanceUntil(new CodeInstruction(OpCodes.Brtrue))
                .Advance()
                .AddLabels(canForge)
                .InsertInstructions(new CodeInstruction(OpCodes.Br_S, canForge))
                .InsertWithLabels(
                    new[] { isNotMeleeWeaponButMaybeSlingshot },
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Isinst, typeof(Slingshot)),
                    new CodeInstruction(OpCodes.Brfalse, cannotForge));
        }
        catch (Exception ex)
        {
            Log.E($"Failed allowing slingshot forges.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
