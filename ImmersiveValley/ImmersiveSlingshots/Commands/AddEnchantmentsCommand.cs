﻿namespace DaLion.Stardew.Slingshots.Commands;

#region using directives

using System.Linq;
using DaLion.Common.Commands;
using DaLion.Stardew.Slingshots.Framework.Enchantments;
using StardewValley.Tools;

#endregion using directives

[UsedImplicitly]
internal sealed class AddEnchantmentsCommand : ConsoleCommand
{
    /// <summary>Initializes a new instance of the <see cref="AddEnchantmentsCommand"/> class.</summary>
    /// <param name="handler">The <see cref="CommandHandler"/> instance that handles this command.</param>
    internal AddEnchantmentsCommand(CommandHandler handler)
        : base(handler)
    {
    }

    /// <inheritdoc />
    public override string[] Triggers { get; } = { "add_enchants", "add", "enchant" };

    /// <inheritdoc />
    public override string Documentation =>
        "Add the specified enchantments to the selected slingshot." + this.GetUsage();

    /// <inheritdoc />
    public override void Callback(string[] args)
    {
        if (Game1.player.CurrentTool is not Slingshot slingshot)
        {
            Log.W("You must select a slingshot first.");
            return;
        }

        while (args.Length > 0)
        {
            BaseEnchantment? enchantment = args[0].ToLower() switch
            {
                // forges
                "ruby" => new RubyEnchantment(),
                "aquamarine" => new AquamarineEnchantment(),
                "jade" => new JadeEnchantment(),
                "emerald" => new EmeraldEnchantment(),
                "amethyst" => new AmethystEnchantment(),
                "topaz" => new TopazEnchantment(),
                "diamond" => new DiamondEnchantment(),

                // slingshot enchants
                "gatling" => new GatlingEnchantment(),
                "preserving" => new PreservingEnchantment(),
                "quincy" => new QuincyEnchantment(),
                "spreading" => new SpreadingEnchantment(),

                _ => null,
            };

            if (enchantment is null)
            {
                Log.W($"Ignoring unknown enchantment {args[0]}.");
                args = args.Skip(1).ToArray();
                continue;
            }

            if (!enchantment.CanApplyTo(slingshot))
            {
                Log.W($"Cannot apply {enchantment.GetDisplayName()} enchantment to {slingshot.DisplayName}.");
                args = args.Skip(1).ToArray();
                continue;
            }

            slingshot.enchantments.Add(enchantment);
            Log.I($"Applied {enchantment.GetDisplayName()} enchantment to {slingshot.DisplayName}.");

            args = args.Skip(1).ToArray();
        }
    }

    /// <summary>Tell the dummies how to use the console command.</summary>
    private string GetUsage()
    {
        var result = $"\n\nUsage: {this.Handler.EntryCommand} {this.Triggers.First()} <enchantment>";
        result += "\n\nParameters:";
        result += "\n\t- <enchantment>: a slingshot enchantment";
        result += "\n\nExample:";
        result += $"\n\t- {this.Handler.EntryCommand} {this.Triggers.First()} gatling";
        return result;
    }
}
