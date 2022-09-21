﻿namespace DaLion.Stardew.Slingshots.Framework.Enchantments;

#region using directives

using System.Xml.Serialization;

#endregion using directives

/// <summary>Fire an energy projectile when ammunition is not equipped.</summary>
/// <remarks>
///     The quincy projectile has zero knockback and cannot crit, but scales in size and damage with Desperado's
///     overcharge.
/// </remarks>
[XmlType("Mods_DaLion_QuincyEnchantment")]
public sealed class QuincyEnchantment : BaseSlingshotEnchantment
{
    /// <inheritdoc />
    public override string GetName()
    {
        return ModEntry.i18n.Get("enchantments.quincy");
    }
}
