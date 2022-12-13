﻿namespace DaLion.Shared.Events;

#region using directives

using StardewModdingAPI.Events;

#endregion using directives

/// <summary>Wrapper for <see cref="IGameLoopEvents.ReturnedToTitle"/> allowing dynamic enabling / disabling.</summary>
internal abstract class ReturnedToTitleEvent : ManagedEvent
{
    /// <summary>Initializes a new instance of the <see cref="ReturnedToTitleEvent"/> class.</summary>
    /// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
    protected ReturnedToTitleEvent(EventManager manager)
        : base(manager)
    {
        manager.ModEvents.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        this.Manager.ModEvents.GameLoop.ReturnedToTitle -= this.OnReturnedToTitle;
    }

    /// <inheritdoc cref="IGameLoopEvents.ReturnedToTitle"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    internal void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        if (this.IsEnabled)
        {
            this.OnReturnedToTitleImpl(sender, e);
        }
    }

    /// <inheritdoc cref="OnReturnedToTitle"/>
    protected abstract void OnReturnedToTitleImpl(object? sender, ReturnedToTitleEventArgs e);
}
