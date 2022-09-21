﻿namespace DaLion.Stardew.Professions.Extensions;

#region using directives

using System;
using System.Collections.Generic;
using DaLion.Common.Extensions;
using DaLion.Common.Extensions.Reflection;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Locations;

#endregion using directives

/// <summary>Extensions for the <see cref="MineShaft"/> class.</summary>
public static class MineShaftExtensions
{
    private static readonly Lazy<Func<MineShaft, NetBool>> GetNetIsTreasureRoom = new(() =>
        typeof(MineShaft)
            .RequireField("netIsTreasureRoom")
            .CompileUnboundFieldGetterDelegate<MineShaft, NetBool>());

    /// <summary>Determines whether the current mine level is a safe level; i.e. shouldn't spawn any monsters.</summary>
    /// <param name="shaft">The <see cref="MineShaft"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref name="shaft"/>'s level is a regular mine level multiple of 10 or a skull cavern level with either a Qi event or the treasure net flag, otherwise <see langword="false"/>.</returns>
    public static bool IsTreasureOrSafeRoom(this MineShaft shaft)
    {
        return (shaft.mineLevel <= 120 && shaft.mineLevel % 10 == 0) ||
               (shaft.mineLevel == 220 && Game1.player.secretNotesSeen.Contains(10) &&
                !Game1.player.mailReceived.Contains("qiCave")) || GetNetIsTreasureRoom.Value(shaft).Value;
    }

    /// <summary>Finds all tiles in a mine map containing either a ladder or sink-hole.</summary>
    /// <param name="shaft">The <see cref="MineShaft"/> instance.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of all the <see cref="Vector2"/> tiles that contain a ladder or sink-hole.</returns>
    /// <remarks>Credit to <c>pomepome</c>.</remarks>
    public static IEnumerable<Vector2> GetLadderTiles(this MineShaft shaft)
    {
        for (var i = 0; i < shaft.Map.GetLayer("Buildings").LayerWidth; ++i)
        {
            for (var j = 0; j < shaft.Map.GetLayer("Buildings").LayerHeight; ++j)
            {
                var index = shaft.getTileIndexAt(new Point(i, j), "Buildings");
                if (index.IsAnyOf(173, 174))
                {
                    yield return new Vector2(i, j);
                }
            }
        }
    }
}
