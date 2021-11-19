using UnityEngine;
using System;
using System.Collections;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionHideAttribute : PropertyAttribute
{
    public string condition = "";
    public float rangeMin = 0f;
    public float rangeMax = 1f;

    public ConditionHideAttribute(string conditiona)
    {
        this.condition = conditiona;
    }

    public ConditionHideAttribute(string conditiona, float min, float max)
    {
        this.condition = conditiona;
        this.rangeMin = min;
        this.rangeMax = max;
    }
}



