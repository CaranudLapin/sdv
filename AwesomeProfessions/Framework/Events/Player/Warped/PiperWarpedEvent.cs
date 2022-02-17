﻿namespace DaLion.Stardew.Professions.Framework.Events.Player;

#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;

using Extensions;
using GameLoop;
using Input;

#endregion using directives

internal class PiperWarpedEvent : WarpedEvent
{
    private readonly Func<int, double> _pipeChance = x => 19f / (x + 18f);

    /// <inheritdoc />
    protected override void OnWarpedImpl(object sender, WarpedEventArgs e)
    {
        if (e.NewLocation.Equals(e.OldLocation)) return;

        foreach (var piped in ModEntry.State.Value.PipedSlimes)
            piped.takeDamage(piped.Health, 0, 0, false, 0, e.Player);
        ModEntry.State.Value.PipedSlimes.Clear();

        if (!e.NewLocation.IsCombatZone())
        {
            EventManager.Disable(typeof(PiperButtonsChangedEvent), typeof(PiperUpdateTickedEvent));
            return;
        }

        if (!e.NewLocation.IsCombatZone(true)) return;

        // get valid tiles for spawning
        var validTiles = new HashSet<Vector2>();
        int playerx = Game1.player.getTileX(), playery = Game1.player.getTileY();
        for (var i = playery - 10; i < playery + 10; ++i)
            for (var j = playerx - 10; j < playerx + 10; ++j)
            {
                var tile = new Vector2(j, i);
                if (!e.NewLocation.isTileOnMap(tile) ||
                    !e.NewLocation.isTileLocationTotallyClearAndPlaceable(tile)) continue;
                
                if (e.NewLocation is MineShaft shaft)
                {
                    shaft.checkForMapAlterations(j, i);
                    if (!shaft.isTileClearForMineObjects(tile)) continue;
                }
                
                validTiles.Add(tile);
            }

        var r = new Random(Guid.NewGuid().GetHashCode());
        var raisedSlimes = e.Player.GetRaisedSlimes().ToHashSet();
        var chance = _pipeChance(raisedSlimes.Count);
        var enemyCount = Game1.player.currentLocation.characters.OfType<Monster>().Count(m => !m.IsSlime());
        var pipedCount = 0;
        foreach (var tamedSlime in raisedSlimes)
        {
            if (r.NextDouble() > chance) continue;

            // choose slime variation
            GreenSlime pipedSlime;
            switch (e.NewLocation)
            {
                case MineShaft shaft:
                {
                    pipedSlime = new(Vector2.Zero, shaft.mineLevel);
                    if (shaft.GetAdditionalDifficulty() > 0 &&
                        r.NextDouble() < Math.Min(shaft.GetAdditionalDifficulty() * 0.1f, 0.5f))
                        pipedSlime.stackedSlimes.Value = r.NextDouble() < 0.0099999997764825821 ? 4 : 2;

                    shaft.BuffMonsterIfNecessary(pipedSlime);
                    break;
                }
                case Woods:
                {
                    pipedSlime = Game1.currentSeason switch
                    {
                        "fall" => new(Vector2.Zero, r.NextDouble() < 0.5 ? 40 : 0),
                        "winter" => new(Vector2.Zero, 40),
                        _ => new(Vector2.Zero, 0)
                    };
                    break;
                }
                case IslandWest or VolcanoDungeon:
                {
                    pipedSlime = new(Vector2.Zero, 0);
                    pipedSlime.makeTigerSlime();
                    break;
                }
                default:
                {
                    pipedSlime = new(Vector2.Zero, 121);
                    break;
                }
            }

            // adjust color
            if (tamedSlime.Name == "Tiger Slime" && pipedSlime.Name != tamedSlime.Name)
            {
                pipedSlime.makeTigerSlime();
            }
            else
            {
                pipedSlime.color.R = (byte) (tamedSlime.color.R + r.Next(-20, 21));
                pipedSlime.color.G = (byte) (tamedSlime.color.G + r.Next(-20, 21));
                pipedSlime.color.B = (byte) (tamedSlime.color.B + r.Next(-20, 21));
            }

            // spawn
            pipedSlime.setTileLocation(validTiles.ElementAt(r.Next(validTiles.Count)));
            e.NewLocation.characters.Add(pipedSlime);
            ModEntry.State.Value.PipedSlimes.Add(pipedSlime);
            ++pipedCount;
            if (pipedCount >= enemyCount) break;
        }

        Log.D($"Spawned {pipedCount} Slimes after {raisedSlimes.Count} attempts.");

        if (pipedCount > 0 || e.NewLocation.characters.Any(npc => npc is GreenSlime))
            EventManager.Enable(typeof(PiperButtonsChangedEvent), typeof(PiperUpdateTickedEvent));
    }
}