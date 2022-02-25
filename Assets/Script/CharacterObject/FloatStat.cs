using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FloatStat {
    
    [SerializeField]
    float value;

    
    public float Value
    {
        get { return value; }
    }


    List<float> modifiers = new List<float>();
    List<float> multipliers = new List<float>();

    public void AddModifier(float modifier)
    {
        if (modifier != 0)
            modifiers.Add(modifier);   
    }

    public void RemoveModifier(float modifier)
    {
        if (modifier != 0)
            modifiers.Remove(modifier);
    }

    public void AddMultiplier(float multiplier)
    {
        if (value != 0)
            multipliers.Add(multiplier);
    }

    public void RemoveMultiplier(float multiplier)
    {
        if (value != 0)
            multipliers.Remove(multiplier);
    }


}
