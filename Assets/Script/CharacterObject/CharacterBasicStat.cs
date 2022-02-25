using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CharacterBasicStat : MonoBehaviour
{
    public FloatStat MaxHP;
    public IntStat SpellPower;
    public FloatStat CastSpeed;
    public FloatStat Speed;
    public FloatStat Range;
    public IntStat Def;
    public IntStat ImpactResistance;


    [SerializeField] bool initWithScriptableObj;
    [SerializeField] StatInitializer initializer;

    public float CurrentHP { get; protected set; }


    protected virtual void Awake()
    {
        CurrentHP = MaxHP.Value;

        if (initWithScriptableObj)
        {
            initStat();
        }
    }


    public virtual float DamageReductionRate
    {
        get
        {
            return 1.0f - (Def.Value / (Def.Value + 100));
        }
    }

    public virtual void TakeDamage(float dmg)
    {
        CurrentHP -= dmg * DamageReductionRate;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP.Value);

        if(CurrentHP <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        
    }

    public void initStat()
    {

    }


}
