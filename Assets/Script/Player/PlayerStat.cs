using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStat : CharacterBasicStat
{
    public FloatStat MaxMP;

    public float CurrentMP { get; protected set; }


    protected override void Awake()
    {
        base.Awake();
        CurrentMP = MaxMP.Value;
    }

    public bool ConsumeMP(float mp)
    {
        if (CurrentMP < mp)
            return false;

        CurrentMP -= mp;
        return true;
    }

}
