using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "PlayerStat", menuName = "ScriptableObject/PlayerStat", order = 0)]
public class PlayerStat : CharacterBasicStat
{

    public override void  OnEnable()
    {
        base.OnEnable();
        /*_*/currentMP = maxMP;
    }


    public float maxMP;
    public float currentMP;
    /*public float currentMP
    {
        get
        {
            return _currentMP;
        }
        set
        {
            _currentMP = maxMP < value ? maxMP : value;
        }
    }*/

    public int agility;


    public int level;
    public int exp;
    public int skillPoint;
    public int credit;

}
