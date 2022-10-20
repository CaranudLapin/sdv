﻿namespace DaLion.Stardew.Arsenal;

#region using directives

using System.Diagnostics.CodeAnalysis;
using DaLion.Common.Commands;
using DaLion.Common.Events;
using DaLion.Common.Harmony;
using DaLion.Common.Integrations.DynamicGameAssets;
using DaLion.Common.Integrations.WalkOfLife;
using DaLion.Stardew.Arsenal.Framework.Events;
using StardewModdingAPI.Utilities;

#endregion using directives

/// <summary>The mod entry point.</summary>
public sealed class ModEntry : Mod
{
    /// <summary>Gets the static <see cref="ModEntry"/> instance.</summary>
    internal static ModEntry Instance { get; private set; } = null!;

    /// <summary>Gets or sets the <see cref="ModConfig"/> instance.</summary>
    internal static ModConfig Config { get; set; } = null!;

    /// <summary>Gets the <see cref="EventManager"/> instance.</summary>
    internal static EventManager Events { get; private set; } = null!;

    /// <summary>Gets the <see cref="PerScreen{T}"/> <see cref="ModState"/>.</summary>
    internal static PerScreen<ModState> PerScreenState { get; private set; } = null!;

    /// <summary>Gets or sets the <see cref="ModState"/> of the local player.</summary>
    internal static ModState State
    {
        get => PerScreenState.Value;
        set => PerScreenState.Value = value;
    }

    /// <summary>Gets the <see cref="IModHelper"/> API.</summary>
    internal static IModHelper ModHelper => Instance.Helper;

    /// <summary>Gets the <see cref="IManifest"/> for this mod.</summary>
    internal static IManifest Manifest => Instance.ModManifest;

    /// <summary>Gets the <see cref="ITranslationHelper"/> API.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Preference.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Preference.")]
    internal static ITranslationHelper i18n => ModHelper.Translation;

    /// <summary>Gets or sets the <see cref="IDynamicGameAssetsApi"/>.</summary>
    internal static IDynamicGameAssetsApi? DynamicGameAssetsApi { get; set; }

    /// <summary>Gets or sets the <see cref="IImmersiveProfessionsApi"/>.</summary>
    internal static IImmersiveProfessionsApi? ProfessionsApi { get; set; }

    /// <summary>Gets a value indicating whether Immersive Rings mod is loaded in the current game session.</summary>
    internal static bool IsImmersiveRingsLoaded { get; private set; }

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        Instance = this;

        // initialize logger
        Log.Init(this.Monitor);

        // check for loaded mod integrations
        IsImmersiveRingsLoaded = helper.ModRegistry.IsLoaded("DaLion.ImmersiveRings");

        // get configs
        Config = helper.ReadConfig<ModConfig>();

        // enable events
        Events = new EventManager(helper.Events);
        Events.Enable(
            typeof(ArsenalAssetRequestedEvent),
            typeof(ArsenalGameLaunchedEvent),
            typeof(ArsenalSavedEvent),
            typeof(ArsenalSaveLoadedEvent),
            typeof(ArsenalSavingEvent),
            typeof(ComboButtonPressedEvent));
        if (Config.FaceMouseCursor)
        {
            Events.Enable<DriftButtonPressedEvent>();
        }

        // initialize mod state
        PerScreenState = new PerScreen<ModState>(() => new ModState());

        // apply patches
        new Harmonizer(helper.ModRegistry, this.ModManifest.UniqueID).ApplyAll();

        // register commands
        new CommandHandler(helper.ConsoleCommands).Register("ars", "Arsenal");
    }
}
