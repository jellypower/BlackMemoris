using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CharacterBasicStat : MonoBehaviour
{
    public FloatStat MaxHP;
    public IntStat Power;
    public FloatStat Speed;
    public IntStat ImpactResistance;

    PlayerController player;

    
    public float CurrentHP { get; protected set; }


    protected virtual void Awake()
    {
        CurrentHP = MaxHP.Value;
        player = GetComponent<PlayerController>();

    }




    public virtual void TakeDamage(float dmg)
    {
        CurrentHP -= dmg;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP.Value);

        if(CurrentHP <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        player.Die();
    }



}
