﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.SpaceCore;

#region using directives

using System.Linq;
using DaLion.Common.Attributes;
using DaLion.Common.Extensions;
using DaLion.Common.Extensions.Reflection;
using DaLion.Stardew.Professions.Extensions;
using DaLion.Stardew.Professions.Framework.Textures;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
[RequiresMod("spacechase0.SpaceCore")]
internal sealed class NewSkillsPagePerformHoverActionPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="NewSkillsPagePerformHoverActionPatch"/> class.</summary>
    internal NewSkillsPagePerformHoverActionPatch()
    {
        this.Target = "SpaceCore.Interface.NewSkillsPage"
            .ToType()
            .RequireMethod("performHoverAction");
    }

    #region harmony patches

    /// <summary>Patch to add prestige ribbon hover text + truncate profession descriptions in hover menu.</summary>
    [HarmonyPostfix]
    private static void NewSkillsPagePerformHoverActionPostfix(
        IClickableMenu __instance, int x, int y, ref string ___hoverText)
    {
        ___hoverText = ___hoverText.Truncate(90);

        if (!ModEntry.Config.EnablePrestige)
        {
            return;
        }

        var bounds = ModEntry.Config.PrestigeProgressionStyle switch
        {
            ModConfig.ProgressionStyle.StackedStars => new Rectangle(
                __instance.xPositionOnScreen + __instance.width + Textures.ProgressionHorizontalOffset - 22,
                __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Textures.ProgressionVerticalOffset + 8,
                0,
                (int)(Textures.SingleStarWidth * Textures.StarsScale)),
            ModConfig.ProgressionStyle.Gen3Ribbons or ModConfig.ProgressionStyle.Gen4Ribbons => new Rectangle(
                __instance.xPositionOnScreen + __instance.width + Textures.ProgressionHorizontalOffset,
                __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Textures.ProgressionVerticalOffset,
                (int)(Textures.RibbonWidth * Textures.RibbonScale),
                (int)(Textures.RibbonWidth * Textures.RibbonScale)),
            _ => Rectangle.Empty,
        };

        for (var i = 0; i < 5; ++i)
        {
            bounds.Y += 56;

            // need to do this bullshit switch because mining and fishing are inverted in the skills page
            var skill = i switch
            {
                1 => Skill.Mining,
                3 => Skill.Fishing,
                _ => Skill.FromValue(i),
            };
            var professionsForThisSkill = Game1.player.GetProfessionsForSkill(skill, true);
            var count = professionsForThisSkill.Length;
            if (count == 0)
            {
                continue;
            }

            bounds.Width = ModEntry.Config.PrestigeProgressionStyle is ModConfig.ProgressionStyle.Gen3Ribbons
                or ModConfig.ProgressionStyle.Gen4Ribbons
                ? (int)(Textures.RibbonWidth * Textures.RibbonScale)
                : (int)(((Textures.SingleStarWidth / 2 * count) + 4) * Textures.StarsScale);
            if (!bounds.Contains(x, y))
            {
                continue;
            }

            ___hoverText = ModEntry.i18n.Get("prestige.skillpage.tooltip", new { count });
            ___hoverText = professionsForThisSkill
                .Select(p => p.DisplayName)
                .Aggregate(___hoverText, (current, name) => current + $"\n• {name}");
        }

        if (ModEntry.SpaceCoreApi is null)
        {
            return;
        }

        foreach (var skill in CustomSkill.Loaded.Values)
        {
            bounds.Y += 56;
            var professionsForThisSkill =
                Game1.player.GetProfessionsForSkill(skill, true);
            var count = professionsForThisSkill.Length;
            if (count == 0)
            {
                continue;
            }

            bounds.Width = ModEntry.Config.PrestigeProgressionStyle is ModConfig.ProgressionStyle.Gen3Ribbons
                or ModConfig.ProgressionStyle.Gen4Ribbons
                ? (int)(Textures.RibbonWidth * Textures.RibbonScale)
                : (int)(((Textures.SingleStarWidth / 2 * count) + 4) * Textures.StarsScale);
            if (!bounds.Contains(x, y))
            {
                continue;
            }

            ___hoverText = ModEntry.i18n.Get("prestige.skillpage.tooltip", new { count });
            ___hoverText = professionsForThisSkill
                .Select(p => p.DisplayName)
                .Aggregate(___hoverText, (current, name) => current + $"\n• {name}");
        }
    }

    #endregion harmony patches
}
