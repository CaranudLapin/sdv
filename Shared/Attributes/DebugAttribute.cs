﻿namespace DaLion.Shared.Attributes;

/// <summary>Indicates that an implicitly-used marked symbol should only be available in debug mode.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DebugAttribute : Attribute
{
}
