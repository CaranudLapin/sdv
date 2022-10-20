﻿namespace DaLion.Stardew.Taxes.Framework.Patches;

#region using directives

using DaLion.Common.Extensions.Stardew;
using HarmonyLib;
using StardewValley.Menus;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class PurchaseAnimalsSetUpForReturnAfterPurchasingAnimalPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="PurchaseAnimalsSetUpForReturnAfterPurchasingAnimalPatch"/> class.</summary>
    internal PurchaseAnimalsSetUpForReturnAfterPurchasingAnimalPatch()
    {
        this.Target = this.RequireMethod<PurchaseAnimalsMenu>(nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal));
    }

    #region harmony patches

    /// <summary>Patch to deduct animal expenses.</summary>
    [HarmonyPostfix]
    private static void PurchaseAnimalsMenuReceiveLeftClickPostfix(PurchaseAnimalsMenu __instance, int ___priceOfAnimal)
    {
        if (!ModEntry.Config.DeductibleAnimalExpenses)
        {
            return;
        }

        Game1.player.Increment(DataFields.BusinessExpenses, ___priceOfAnimal);
    }

    #endregion harmony patches
}
