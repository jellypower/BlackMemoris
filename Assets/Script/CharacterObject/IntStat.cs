using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntStat {

    [SerializeField]
    int value;
    public int Value
    {
        get { return value; }
    }

    List<int> modifiers = new List<int>();
    List<int> multipliers = new List<int>();

    public void AddModifier(int modifier)
    {
        if (modifier != 0)
            modifiers.Add(modifier);
    }

    public void RemoveModifier(int modifier)
    {
        if (modifier != 0)
            modifiers.Remove(modifier);
    }


}
