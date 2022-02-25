using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStat : CharacterBasicStat
{
    public FloatStat MaxMP;
    public IntStat Agility;
    public IntStat Level;
    public IntStat Exp;
    public IntStat SkillPoint;
    public IntStat Money;

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
