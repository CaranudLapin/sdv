﻿namespace DaLion.Stardew.Tools.Commands;

#region using directives

using System.Linq;
using DaLion.Common;
using DaLion.Common.Commands;
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
    public override string Documentation => "Add the specified enchantment to the selected tool." + this.GetUsage();

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1012:Opening braces should be spaced correctly", Justification = "Paradoxical.")]
    public override void Callback(string[] args)
    {
        if (Game1.player.CurrentTool is not ({ } tool and (Axe or Hoe or Pickaxe or WateringCan or FishingRod)))
        {
            Log.W("You must select a tool first.");
            return;
        }

        while (args.Length > 0)
        {
            BaseEnchantment? enchantment = args[0].ToLower() switch
            {
                "auto-hook" or "autohook" => new AutoHookEnchantment(),
                "archaeologist" => new ArchaeologistEnchantment(),
                "bottomless" => new BottomlessEnchantment(),
                "efficient" => new EfficientToolEnchantment(),
                "generous" => new GenerousEnchantment(),
                "master" => new MasterEnchantment(),
                "powerful" => new PowerfulEnchantment(),
                "preserving" => new PreservingEnchantment(),
                "reaching" => new ReachingToolEnchantment(),
                "shaving" => new ShavingEnchantment(),
                "swift" => new SwiftToolEnchantment(),
                _ => null,
            };

            if (enchantment is null)
            {
                Log.W($"Ignoring unknown enchantment {args[0]}.");
                args = args.Skip(1).ToArray();
                continue;
            }

            if (!enchantment.CanApplyTo(tool))
            {
                Log.W($"Cannot apply {enchantment.GetDisplayName()} enchantment to {tool.DisplayName}.");
                args = args.Skip(1).ToArray();
                continue;
            }

            tool.enchantments.Add(enchantment);
            Log.I($"Applied {enchantment.GetDisplayName()} enchantment to {tool.DisplayName}.");

            args = args.Skip(1).ToArray();
        }
    }

    /// <summary>Tell the dummies how to use the console command.</summary>
    private string GetUsage()
    {
        var result = $"\n\nUsage: {this.Handler.EntryCommand} {this.Triggers.First()} <enchantment>";
        result += "\n\nParameters:";
        result += "\n\t- <enchantment>: a tool enchantment";
        result += "\n\nExample:";
        result += $"\n\t- {this.Handler.EntryCommand} {this.Triggers.First()} powerful";
        return result;
    }
}
