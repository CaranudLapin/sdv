﻿namespace DaLion.Stardew.Professions.Commands;

#region using directives

using System;
using System.Linq;
using DaLion.Common;
using DaLion.Common.Commands;
using DaLion.Common.Integrations.SpaceCore;
using DaLion.Stardew.Professions.Framework;

#endregion using directives

[UsedImplicitly]
internal sealed class ClearNewLevelsCommand : ConsoleCommand
{
    /// <summary>Initializes a new instance of the <see cref="ClearNewLevelsCommand"/> class.</summary>
    /// <param name="handler">The <see cref="CommandHandler"/> instance that handles this command.</param>
    internal ClearNewLevelsCommand(CommandHandler handler)
        : base(handler)
    {
    }

    /// <inheritdoc />
    public override string[] Triggers { get; } = { "clear_new_levels" };

    /// <inheritdoc />
    public override string Documentation =>
        "Clear the player's cache of new levels for the specified skills, or all vanilla skills if none are specified.";

    /// <inheritdoc />
    public override void Callback(string[] args)
    {
        if (args.Length <= 0)
        {
            Game1.player.newLevels.Clear();
        }
        else
        {
            foreach (var arg in args)
            {
                if (Skill.TryFromName(arg, true, out var skill))
                {
                    Game1.player.newLevels.Set(Game1.player.newLevels.Where(p => p.X != skill).ToList());
                }
                else
                {
                    var customSkill = CustomSkill.Loaded.Values.FirstOrDefault(s =>
                        string.Equals(s.DisplayName, arg, StringComparison.CurrentCultureIgnoreCase));
                    if (customSkill is null)
                    {
                        Log.W($"Ignoring unknown skill {arg}.");
                        continue;
                    }

                    var newLevels = ExtendedSpaceCoreApi.GetCustomSkillNewLevels.Value();
                    ExtendedSpaceCoreApi.SetCustomSkillNewLevels.Value(newLevels
                        .Where(pair => pair.Key != customSkill.StringId).ToList());
                }
            }
        }
    }
}
