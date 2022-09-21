﻿namespace DaLion.Stardew.Rings.Extensions;

#region using directives

using StardewValley.Objects;

#endregion using directives

/// <summary>Extensions for the <see cref="int"/> primitive type.</summary>
public static class Int32Extensions
{
    /// <summary>Determines whether the number corresponds to a valid <see cref="Ring"/> index.</summary>
    /// <param name="index">A <see cref="Item"/> index.</param>
    /// <returns><see langword="true"/> if the <paramref name="index"/> corresponds any <see cref="Ring"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsRingIndex(this int index)
    {
        return index is (>= 516 and <= 534) or 810 or 811;
    }
}
