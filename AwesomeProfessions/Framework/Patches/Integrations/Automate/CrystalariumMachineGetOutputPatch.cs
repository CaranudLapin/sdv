﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.Automate;

#region using directives

using HarmonyLib;
using JetBrains.Annotations;
using StardewValley;

using Stardew.Common.Harmony;
using Extensions;

using SObject = StardewValley.Object;

#endregion using directives

[UsedImplicitly]
internal class CrystalariumMachineGetOutputPatch : BasePatch
{
    /// <summary>Construct an instance.</summary>
    internal CrystalariumMachineGetOutputPatch()
    {
        try
        {
            Original = "Pathoschild.Stardew.Automate.Framework.Machines.Objects.CrystalariumMachine".ToType()
                .MethodNamed("GetOutput");
        }
        catch
        {
            // ignored
        }
    }

    #region harmony patches

    /// <summary>Patch to increment minerals collected from automated Crystalariums.</summary>
    [HarmonyPostfix]
    private static void CrystalariumMachineGetOutputPostfix(object __instance)
    {
        if (__instance is null || !ModEntry.Config.ShouldCountAutomatedHarvests) return;

        var machine = ModEntry.ModHelper.Reflection.GetProperty<SObject>(__instance, "Machine").GetValue();
        if (machine?.heldObject.Value is null) return;

        var owner = Game1.getFarmerMaybeOffline(machine.owner.Value) ?? Game1.MasterPlayer;
        if (!owner.HasProfession(Profession.Gemologist) ||
            !machine.heldObject.Value.IsForagedMineral() && !machine.heldObject.Value.IsGemOrMineral()) return;

        ModData.Increment<uint>(DataField.GemologistMineralsCollected, owner);
    }

    #endregion harmony patches
}