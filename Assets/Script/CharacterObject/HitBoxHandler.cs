using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxHandler : MonoBehaviour
{


    public Character character { get; private set; }
    void Awake()
    {
        character = transform.parent.GetComponent<Character>();
    }

    public void GetAttack(float damage, Character Attacker)
    {
        print("À¸¾Ç");
        character.GetAttack(damage, Attacker);
    }

    public void GetImpact(float impact, float time, Vector2 dir, Character Attacker)
    {
        character.GetImpact(impact, time, dir, Attacker);
    }

    
}
