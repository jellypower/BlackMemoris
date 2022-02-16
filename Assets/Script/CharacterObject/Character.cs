using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    // Start is called before the first frame update

    public CharacterBasicStat CharacterStat;

    #region start functions

    #endregion


    #region update functions
    protected abstract void Action();
    protected abstract void UpdateState();
    protected abstract void Animate();
    #endregion


    #region public functions
    public abstract void GetAttack(float damage, Character Attacker);

    public abstract void GetImpact(float impact, Character Attacker);

    #endregion

}