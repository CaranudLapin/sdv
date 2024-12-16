﻿namespace DaLion.Core;

#region using directives

using DaLion.Shared.Integrations.GMCM.Attributes;
using Newtonsoft.Json;
using StardewModdingAPI.Utilities;

#endregion using directives

/// <summary>Config schema for the Core mod.</summary>
public sealed class CoreConfig
{
    /// <summary>Gets the chance a crop may wither per day left un-watered.</summary>
    [JsonProperty]
    [GMCMRange(0f, 1f, 0.05f)]
    public float CropWitherChance { get; internal set; } = 0f;

    /// <summary>Gets a value indicating whether to consolidate farmer debuffs with the status conditions provided by this mod.</summary>
    [JsonProperty]
    public bool ConsistentFarmerDebuffs { get; internal set; } = true;

    /// <summary>Gets the key used to engage Debug Mode.</summary>
    [JsonProperty]
    [GMCMIgnore]
    public KeybindList DebugKey { get; internal set; } = KeybindList.Parse("OemQuotes, OemTilde");
}
