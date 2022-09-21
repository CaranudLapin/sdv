﻿namespace DaLion.Stardew.Rings.Framework.Resonance;

#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using DaLion.Common;
using DaLion.Common.Extensions;
using DaLion.Common.Extensions.Collections;
using DaLion.Stardew.Rings.Extensions;
using DaLion.Stardew.Rings.Framework.Events;
using Microsoft.Xna.Framework;

#endregion using directives

/// <inheritdoc cref="IChord"/>
public sealed class Chord : IChord
{
    private static readonly double[] Range = Enumerable.Range(0, 120).Select(i => i / 120d).ToArray();
    private static int _position;

    private readonly Func<double, double> _map = x => Math.Max(-x + (MathConstants.SQRT2 / 3d) + 2d, 1d);
    private int _magnetism;

    /// <summary>Initializes a new instance of the <see cref="Chord"/> class.Construct a Dyad instance.</summary>
    /// <param name="first">The first <see cref="Gemstone"/> in the Dyad.</param>
    /// <param name="second">The second <see cref="Gemstone"/> in the  Dyad.</param>
    internal Chord(Gemstone first, Gemstone second)
    {
        this.Notes = first.Collect(second).ToArray();
        this.Harmonize();
        this.InitializeLightSource();
    }

    /// <summary>Initializes a new instance of the <see cref="Chord"/> class.Construct a Triad instance.</summary>
    /// <param name="first">The first <see cref="Gemstone"/> in the Triad.</param>
    /// <param name="second">The second <see cref="Gemstone"/> in the Triad.</param>
    /// <param name="third">The third <see cref="Gemstone"/> in the Triad.</param>
    internal Chord(Gemstone first, Gemstone second, Gemstone third)
    {
        this.Notes = first.Collect(second, third).ToArray();
        this.Harmonize();
        this.InitializeLightSource();
    }

    /// <summary>Initializes a new instance of the <see cref="Chord"/> class.Construct a Tetrad instance.</summary>
    /// <param name="first">The first <see cref="Gemstone"/> in the Tetrad.</param>
    /// <param name="second">The second <see cref="Gemstone"/> in the Tetrad.</param>
    /// <param name="third">The third <see cref="Gemstone"/> in the Tetrad.</param>
    /// <param name="fourth">The fourth <see cref="Gemstone"/> in the Tetrad.</param>
    internal Chord(Gemstone first, Gemstone second, Gemstone third, Gemstone fourth)
    {
        this.Notes = first.Collect(second, third, fourth).ToArray();
        this.Harmonize();
        this.InitializeLightSource();
    }

    /// <summary>Gets the <see cref="Gemstone"/>s that make up the <see cref="Chord"/>.</summary>
    /// <remarks>
    ///     The notes are sorted by resulting harmony, with the <see cref="Root"/> at index zero and remaining notes
    ///     ordered by increasing intervals with the former.
    /// </remarks>
    public Gemstone[] Notes { get; }

    /// <summary>
    ///     Gets the root <see cref="Gemstone"/> of the <see cref="Chord"/>, which determines the
    ///     perceived wavelength.
    /// </summary>
    public Gemstone Root => this.Notes[0];

    /// <summary>Gets the <see cref="HarmonicInterval"/>s formed between each <see cref="Gemstone"/>.</summary>
    public IGrouping<Gemstone, HarmonicInterval>[] GroupedIntervals { get; private set; } = null!;

    /// <summary>Gets the total resonance of each <see cref="Gemstone"/> due to interference with its neighbors.</summary>
    public Dictionary<Gemstone, double> ResonanceByGemstone { get; } = new();

    /// <summary>Gets the light source generated by the resounding <see cref="Root"/>, if any.</summary>
    public LightSource? LightSource { get; private set; }

    /// <summary>Gets the location or timing of a point within a vibration cycle.</summary>
    public double Phase { get; private set; }

    /// <summary>Gets a value that measures the diversity of overtones in the <see cref="Chord"/>.</summary>
    public double Richness { get; private set; }

    /// <summary>Gets the amplitude of the <see cref="Chord"/>.</summary>
    public double Amplitude { get; private set; }

    /// <inheritdoc />
    public void OnEquip(GameLocation location, Farmer who)
    {
        this.ResonanceByGemstone.ForEach(pair => pair.Key.Resonate((float)(pair.Value * this.Richness), who));
        who.MagneticRadius += this._magnetism;
        if (this.LightSource is null)
        {
            return;
        }

        while (location.sharedLights.ContainsKey(this.LightSource.Identifier))
        {
            ++this.LightSource.Identifier;
        }

        location.sharedLights[this.LightSource.Identifier] = this.LightSource;
        ModEntry.Events.Enable<ResonanceUpdateTickedEvent>();
    }

    /// <inheritdoc />
    public void OnUnequip(GameLocation location, Farmer who)
    {
        this.ResonanceByGemstone.ForEach(pair => pair.Key.Dissonate((float)(pair.Value * this.Richness), who));
        who.MagneticRadius += this._magnetism;
        if (this.LightSource is null)
        {
            return;
        }

        location.removeLightSource(this.LightSource.Identifier);
        if (!who.leftRing.Value.IsCombinedInfinityBand(out _) && !who.rightRing.Value.IsCombinedInfinityBand(out _))
        {
            ModEntry.Events.Disable<ResonanceUpdateTickedEvent>();
        }
    }

    /// <inheritdoc />
    public void OnNewLocation(GameLocation location)
    {
        if (this.LightSource is null)
        {
            return;
        }

        while (location.sharedLights.ContainsKey(this.LightSource.Identifier))
        {
            ++this.LightSource.Identifier;
        }

        location.sharedLights[this.LightSource.Identifier] = this.LightSource;
    }

    /// <inheritdoc />
    public void OnLeaveLocation(GameLocation location)
    {
        if (this.LightSource is null)
        {
            return;
        }

        location.removeLightSource(this.LightSource.Identifier);
    }

    /// <inheritdoc />
    public void Update(Farmer who)
    {
        this.Phase = Range[_position] * Math.PI / 180d;
        if (this.LightSource is null)
        {
            return;
        }

        this.LightSource.radius.Value = this.GetLightSourceAmplitude();
        this.LightSource.color.Value = this.GetLightSourceColor();

        var offset = Vector2.Zero;
        if (who.shouldShadowBeOffset)
        {
            offset += who.drawOffset.Value;
        }

        this.LightSource.position.Value = new Vector2(who.Position.X + 32f, who.Position.Y + 32) + offset;
    }

    /// <inheritdoc />
    public void Buffer(StatBuffer buffer)
    {
        this.ResonanceByGemstone.ForEach(pair => pair.Key.Buffer(buffer, (float)(pair.Value * this.Richness)));
        buffer.AddedMagneticRadius += this._magnetism;
    }

    /// <summary>Advance the vibration phase by one stage.</summary>
    internal static void Vibrate()
    {
        _position = (_position + 1) % 120;
    }

    /// <summary>Evaluate the <see cref="HarmonicInterval"/>s between <see cref="Notes"/> and the resulting harmonies.</summary>
    private void Harmonize()
    {
        Array.Sort(this.Notes);
        var distinctNotes = this.Notes.Distinct().ToArray();
        foreach (var note in distinctNotes)
        {
            this.ResonanceByGemstone[note] = 0;
        }

        // octaves and unisons can be ignored
        if (distinctNotes.Length == 1)
        {
            return;
        }

        // add sequence intervals first
        var intervals = distinctNotes
            .Select((t, i) => new HarmonicInterval(
                t,
                distinctNotes[(i + 1) % distinctNotes.Length]))
            .ToList();
        this._magnetism = (intervals.Count(i => i.Number == IntervalNumber.Third) - 1) * 32;

        // add composite intervals
        if (distinctNotes.Length >= 3)
        {
            intervals.AddRange(distinctNotes.Select((t, i) =>
                new HarmonicInterval(t, distinctNotes[(i + 2) % distinctNotes.Length])));
        }

        if (distinctNotes.Length >= 4)
        {
            intervals.AddRange(distinctNotes.Select((t, i) =>
                new HarmonicInterval(t, distinctNotes[(i + 3) % distinctNotes.Length])));
        }

        // evaluate total resonance of each note
        this.GroupedIntervals = intervals
            .GroupBy(i => i.First)
            .ToArray();

        this.GroupedIntervals.ForEach(group =>
            {
                this.ResonanceByGemstone[group.Key] +=
                    group.Sum(i => this.Notes.Count(n => n == i.Second) * i.Resonance);
            });

        // reposition root note
        this.Notes.ShiftUntilStartsWith(this.ResonanceByGemstone.MaxKey());

        // evaluate richness of the chord
        var numbers = intervals.Select(i => i.Number).ToList();
        while (numbers.Count != this.Notes.Length)
        {
            numbers.Add(IntervalNumber.Unison);
        }

        this.Amplitude = this.Notes.Sum(n => 1d + this.ResonanceByGemstone[n]);
        this.Richness = this._map(numbers.Cast<int>().StandardDeviation());
    }

    /// <summary>Initializes the <see cref="LightSource"/> if a resonant harmony exists in the <see cref="Chord"/>.</summary>
    private void InitializeLightSource()
    {
        if (this.Amplitude <= 1d)
        {
            return;
        }

        this.LightSource = new LightSource(
            ModEntry.Manifest.UniqueID.GetHashCode(),
            Vector2.Zero,
            (float)this.Amplitude,
            this.Root.InverseColor,
            playerID: Game1.player.UniqueMultiplayerID);
    }

    /// <summary>Evaluates the current amplitude of the <see cref="LightSource"/>.</summary>
    /// <returns>The amplitude of the <see cref="LightSource"/>.</returns>
    private float GetLightSourceAmplitude()
    {
        return (float)(this.Notes.Sum(n =>
            (1d + this.ResonanceByGemstone[n]) * Math.Sin(n.Frequency * this.Phase)) / this.Amplitude);
    }

    /// <summary>Evaluates the current <see cref="Color"/> of the <see cref="LightSource"/>.</summary>
    /// <returns>The <see cref="Color"/> of the <see cref="LightSource"/>.</returns>
    private Color GetLightSourceColor()
    {
        return this.Root.InverseColor;
    }
}
