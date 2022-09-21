﻿namespace DaLion.Stardew.Professions.Framework.Events.GameLoop;

#region using directives

using DaLion.Common.Events;
using DaLion.Stardew.Professions.Extensions;
using DaLion.Stardew.Professions.Framework.Ultimates;
using StardewModdingAPI.Events;

#endregion using directives

[UsedImplicitly]
internal sealed class BruteUpdateTickedEvent : UpdateTickedEvent
{
    private const int BuffSheetIndex = 36;

    private readonly int _buffId = (ModEntry.Manifest.UniqueID + Profession.Brute).GetHashCode();

    /// <summary>Initializes a new instance of the <see cref="BruteUpdateTickedEvent"/> class.</summary>
    /// <param name="manager">The <see cref="ProfessionEventManager"/> instance that manages this event.</param>
    internal BruteUpdateTickedEvent(ProfessionEventManager manager)
        : base(manager)
    {
    }

    /// <inheritdoc />
    protected override void OnUpdateTickedImpl(object? sender, UpdateTickedEventArgs e)
    {
        if (ModEntry.State.BruteRageCounter <= 0)
        {
            return;
        }

        if ((Game1.game1.IsActiveNoOverlay || !Game1.options.pauseWhenOutOfFocus) && Game1.shouldTimePass() &&
            ModEntry.State.BruteRageCounter > 0 &&
            e.IsOneSecond)
        {
            ++ModEntry.State.SecondsOutOfCombat;
            // decay counter every 5 seconds after 30 seconds out of combat
            if (ModEntry.State.SecondsOutOfCombat > 30 && e.IsMultipleOf(300))
            {
                --ModEntry.State.BruteRageCounter;
            }
        }

        if (Game1.player.hasBuff(this._buffId))
        {
            return;
        }

        var magnitude = (ModEntry.State.BruteRageCounter * Frenzy.PercentIncrementPerRage).ToString("P");
        Game1.buffsDisplay.addOtherBuff(
            new Buff(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                1,
                "Brute",
                ModEntry.i18n.Get("brute.name" + (Game1.player.IsMale ? ".male" : ".female")) + " " + ModEntry.i18n.Get("brute.buff.name"))
            {
                which = this._buffId,
                sheetIndex = BuffSheetIndex,
                millisecondsDuration = 0,
                description =
                    ModEntry.i18n.Get(
                        "brute.buff.desc" + (Game1.player.HasProfession(Profession.Brute, true)
                            ? ".prestiged"
                            : string.Empty),
                        new { magnitude }),
            });
    }
}
