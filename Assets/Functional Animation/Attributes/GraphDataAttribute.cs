using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class GraphDataAttribute : PropertyAttribute
{
    public readonly string Name;
    public GraphDataAttribute(string name)
    {
        Name = name;
    }
}
