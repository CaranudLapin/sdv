﻿namespace DaLion.Common.Events;

#region using directives

using StardewModdingAPI.Events;

#endregion using directives

/// <summary>Wrapper for <see cref="IDisplayEvents.RenderingWorld"/> allowing dynamic enabling / disabling.</summary>
internal abstract class RenderingWorldEvent : ManagedEvent
{
    /// <summary>Initializes a new instance of the <see cref="RenderingWorldEvent"/> class.</summary>
    /// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
    protected RenderingWorldEvent(EventManager manager)
        : base(manager)
    {
    }

    /// <inheritdoc cref="IDisplayEvents.RenderingWorld"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    internal void OnRenderingWorld(object? sender, RenderingWorldEventArgs e)
    {
        if (this.IsEnabled)
        {
            this.OnRenderingWorldImpl(sender, e);
        }
    }

    /// <inheritdoc cref="OnRenderingWorld"/>
    protected abstract void OnRenderingWorldImpl(object? sender, RenderingWorldEventArgs e);
}
