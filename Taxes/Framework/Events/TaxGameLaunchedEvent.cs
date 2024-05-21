﻿namespace DaLion.Taxes.Framework.Events;

#region using directives

using DaLion.Shared.Events;
using StardewModdingAPI.Events;

#endregion using directives

/// <summary>Initializes a new instance of the <see cref="TaxGameLaunchedEvent"/> class.</summary>
/// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
[UsedImplicitly]
[AlwaysEnabledEvent]
internal sealed class TaxGameLaunchedEvent(EventManager? manager = null)
    : GameLaunchedEvent(manager ?? TaxesMod.EventManager)
{
    /// <inheritdoc />
    protected override void OnGameLaunchedImpl(object? sender, GameLaunchedEventArgs e)
    {
        if (TaxesConfigMenu.Instance?.IsLoaded == true)
        {
            TaxesConfigMenu.Instance.Register();
        }
    }
}
