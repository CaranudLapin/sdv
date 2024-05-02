﻿namespace DaLion.Shared.Integrations.GMCM.Attributes;

/// <summary>Assigns a priority to GMCM property.</summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class GMCMPriorityAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="GMCMPriorityAttribute"/> class.</summary>
    /// <param name="priority">The priority of the property in the page.</param>
    internal GMCMPriorityAttribute(uint priority)
    {
        this.Priority = priority;
    }

    /// <summary>Gets the priority of the property in the page.</summary>
    internal uint Priority { get; }
}
