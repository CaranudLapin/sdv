﻿namespace DaLion.Stardew.Professions.Framework.Events.GameLoop;

#region using directives

using DaLion.Common.Events;
using StardewModdingAPI.Events;

#endregion using directives

[UsedImplicitly]
internal sealed class ProspectorHuntDayStartedEvent : DayStartedEvent
{
    /// <summary>Initializes a new instance of the <see cref="ProspectorHuntDayStartedEvent"/> class.</summary>
    /// <param name="manager">The <see cref="ProfessionEventManager"/> instance that manages this event.</param>
    internal ProspectorHuntDayStartedEvent(ProfessionEventManager manager)
        : base(manager)
    {
    }

    /// <inheritdoc />
    protected override void OnDayStartedImpl(object? sender, DayStartedEventArgs e)
    {
        ModEntry.State.ProspectorHunt.Value.ResetChanceAccumulator();
    }
}
