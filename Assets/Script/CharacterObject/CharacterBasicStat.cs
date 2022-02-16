using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterStat", menuName = "ScriptableObject/CharacterStat", order =0)]
public class CharacterBasicStat : ScriptableObject
{

    public virtual void OnEnable()
    {
        /*_*/currentHP = maxHP;
        
    }

    public float maxHP;

    public float currentHP;

    /*public float currentHP {
        get
        {
            return _currentHP;
        }
        set
        {
            _currentHP = maxHP < value ? maxHP : value; 
        }
    }*/

    public int spellPoint;
    public float CastSpeed;
    public float speed;
    public float range;
    public int defensivePoint;
    public int impactResistance;

    public virtual float damageReductionRate
    {
        get
        {
            return 1.0f- (defensivePoint / (defensivePoint + 100));
        }
    }

}
