using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected float speed = 0;
    protected float damage = 0;
    [SerializeField] protected float rotateSpeed;

    public abstract void SetSpawner(GameObject spawner);



}
