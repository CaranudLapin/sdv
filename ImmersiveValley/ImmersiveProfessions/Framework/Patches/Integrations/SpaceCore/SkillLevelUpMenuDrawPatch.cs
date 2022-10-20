﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.SpaceCore;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Common.Attributes;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using DaLion.Stardew.Professions.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using HarmonyPatch = DaLion.Common.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
[RequiresMod("spacechase0.SpaceCore")]
internal sealed class SkillLevelUpMenuDrawPatch : HarmonyPatch
{
    private static readonly Lazy<Func<IClickableMenu, bool>> GetIsProfessionChooser = new(() =>
        "SpaceCore.Interface.SkillLevelUpMenu"
            .ToType()
            .RequireField("isProfessionChooser")
            .CompileUnboundFieldGetterDelegate<IClickableMenu, bool>());

    private static readonly Lazy<Func<IClickableMenu, List<int>>> GetProfessionsToChoose = new(() =>
        "SpaceCore.Interface.SkillLevelUpMenu"
            .ToType()
            .RequireField("professionsToChoose")
            .CompileUnboundFieldGetterDelegate<IClickableMenu, List<int>>());

    /// <summary>Initializes a new instance of the <see cref="SkillLevelUpMenuDrawPatch"/> class.</summary>
    internal SkillLevelUpMenuDrawPatch()
    {
        this.Target = "SpaceCore.Interface.SkillLevelUpMenu"
            .ToType()
            .RequireMethod("draw", new[] { typeof(SpriteBatch) });
    }

    #region harmony patches

    /// <summary>Patch to draw Prestige tooltip during profession selection.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? SkillLevelUpMenuDrawTranspiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // Injected: DrawSubroutine(this, b);
        // Before: else if (!isProfessionChooser)
        try
        {
            helper
                .FindFirst(
                    new CodeInstruction(
                        OpCodes.Ldfld,
                        "SkillLevelUpMenu"
                            .ToType()
                            .RequireField(nameof(LevelUpMenu.isProfessionChooser))))
                .Advance()
                .GetOperand(out var isNotProfessionChooser)
                .FindLabel((Label)isNotProfessionChooser)
                .Retreat()
                .InsertInstructions(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, typeof(LevelUpMenu).RequireField("currentLevel")),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(SkillLevelUpMenuDrawPatch).RequireMethod(nameof(DrawSubroutine))));
        }
        catch (Exception ex)
        {
            Log.E("Immersive Professions failed while patching level up menu prestige tooltip draw." +
                  "\n—-- Do NOT report this to SpaceCore's author. ---" +
                  $"\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony

    #region injected subroutines

    private static void DrawSubroutine(IClickableMenu menu, int currentLevel, SpriteBatch b)
    {
        if (!ModEntry.Config.EnablePrestige || !GetIsProfessionChooser.Value(menu) ||
            currentLevel > 10)
        {
            return;
        }

        var professionsToChoose = GetProfessionsToChoose.Value(menu);
        if (!CustomProfession.LoadedProfessions.TryGetValue(professionsToChoose[0], out var leftProfession) ||
            !CustomProfession.LoadedProfessions.TryGetValue(professionsToChoose[1], out var rightProfession))
        {
            return;
        }

        Rectangle selectionArea;
        if (Game1.player.HasProfession(leftProfession) &&
            Game1.player.HasAllProfessionsBranchingFrom(leftProfession))
        {
            selectionArea = new Rectangle(
                menu.xPositionOnScreen + 32,
                menu.yPositionOnScreen + 232,
                (menu.width / 2) - 40,
                menu.height - 264);
            b.Draw(Game1.staminaRect, selectionArea, new Color(Color.Black, 0.3f));

            if (selectionArea.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                var hoverText = ModEntry.i18n.Get(leftProfession.Id % 6 <= 1
                    ? "prestige.levelup.tooltip:5"
                    : "prestige.levelup.tooltip:10");
                IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
            }
        }

        if (Game1.player.HasProfession(rightProfession) &&
            Game1.player.HasAllProfessionsBranchingFrom(rightProfession))
        {
            selectionArea = new Rectangle(
                menu.xPositionOnScreen + (menu.width / 2) + 8,
                menu.yPositionOnScreen + 232,
                (menu.width / 2) - 40,
                menu.height - 264);
            b.Draw(Game1.staminaRect, selectionArea, new Color(Color.Black, 0.3f));

            if (selectionArea.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                var hoverText = ModEntry.i18n.Get(leftProfession.Id % 6 <= 1
                    ? "prestige.levelup.tooltip:5"
                    : "prestige.levelup.tooltip:10");
                IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
            }
        }
    }

    #endregion injected subroutines
}
