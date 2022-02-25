using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected Skill spawner;


    protected float speed;
    protected float damage;
    [SerializeField] protected float rotateSpeed;

    public abstract void SetSpawner(Skill spawner);




}
