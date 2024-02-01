using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Method)]
public class GraphMethodAttribute : Attribute
{
    public string name;
    public GraphMethodAttribute(string name)
    {
        this.name = name;
    }
}
