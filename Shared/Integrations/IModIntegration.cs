﻿namespace DaLion.Shared.Integrations;

/// <summary>Handles integration with a given mod.</summary>
/// <remarks>Original code by <see href="https://github.com/Pathoschild">Pathoschild</see>.</remarks>
public interface IModIntegration
{
    /// <summary>Gets a human-readable name for the mod.</summary>
    string ModName { get; }

    /// <summary>Gets the mod's unique ID.</summary>
    string ModId { get; }

    /// <summary>Gets a value indicating whether the mod is available.</summary>
    bool IsLoaded { get; }
}
