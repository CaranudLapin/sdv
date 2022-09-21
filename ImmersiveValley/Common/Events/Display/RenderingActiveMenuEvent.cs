﻿namespace DaLion.Common.Events;

#region using directives

using StardewModdingAPI.Events;

#endregion using directives

/// <summary>Wrapper for <see cref="IDisplayEvents.RenderingActiveMenu"/> allowing dynamic enabling / disabling.</summary>
internal abstract class RenderingActiveMenuEvent : ManagedEvent
{
    /// <summary>Initializes a new instance of the <see cref="RenderingActiveMenuEvent"/> class.</summary>
    /// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
    protected RenderingActiveMenuEvent(EventManager manager)
        : base(manager)
    {
    }

    /// <inheritdoc cref="IDisplayEvents.RenderingActiveMenu"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    internal void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (this.IsEnabled)
        {
            this.OnRenderingActiveMenuImpl(sender, e);
        }
    }

    /// <inheritdoc cref="OnRenderingActiveMenu"/>
    protected abstract void OnRenderingActiveMenuImpl(object? sender, RenderingActiveMenuEventArgs e);
}
