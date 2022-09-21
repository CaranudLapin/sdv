﻿namespace DaLion.Stardew.Professions.Framework.Events.GameLoop;

#region using directives

using DaLion.Common.Events;
using DaLion.Stardew.Professions.Framework.Ultimates;
using DaLion.Stardew.Professions.Framework.VirtualProperties;
using StardewModdingAPI.Events;

#endregion using directives

[UsedImplicitly]
internal sealed class AmbushUpdateTickedEvent : UpdateTickedEvent
{
    /// <summary>Initializes a new instance of the <see cref="AmbushUpdateTickedEvent"/> class.</summary>
    /// <param name="manager">The <see cref="ProfessionEventManager"/> instance that manages this event.</param>
    internal AmbushUpdateTickedEvent(ProfessionEventManager manager)
        : base(manager)
    {
    }

    /// <inheritdoc />
    protected override void OnUpdateTickedImpl(object? sender, UpdateTickedEventArgs e)
    {
        if ((!Game1.game1.IsActiveNoOverlay && Game1.options.pauseWhenOutOfFocus) || !Game1.shouldTimePass())
        {
            return;
        }

        var ambush = Game1.player.Get_Ultimate() as Ambush;
        if (ambush!.IsActive)
        {
            Game1.player.temporarilyInvincible = true;
        }
        else
        {
            ambush.SecondsOutOfAmbush += 1d / 60d;
            if (ambush.SecondsOutOfAmbush > 1.5d)
            {
                this.Disable();
            }
        }
    }
}
