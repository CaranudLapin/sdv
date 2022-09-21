﻿namespace DaLion.Common.Events;

#region using directives

using StardewModdingAPI.Events;

#endregion using directives

/// <summary>Wrapper for <see cref="IGameLoopEvents.GameLaunched"/> allowing dynamic enabling / disabling.</summary>
internal abstract class GameLaunchedEvent : ManagedEvent
{
    private static bool _launched;

    /// <summary>Initializes a new instance of the <see cref="GameLaunchedEvent"/> class.</summary>
    /// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
    protected GameLaunchedEvent(EventManager manager)
        : base(manager)
    {
        if (!_launched)
        {
            base.Enable();
        }
    }

    /// <inheritdoc />
    public override bool Enable()
    {
        return false;
    }

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    internal void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.OnGameLaunchedImpl(sender, e);
        _launched = true;
        this.Disable();
    }

    /// <inheritdoc cref="OnGameLaunched"/>
    protected abstract void OnGameLaunchedImpl(object? sender, GameLaunchedEventArgs e);
}
