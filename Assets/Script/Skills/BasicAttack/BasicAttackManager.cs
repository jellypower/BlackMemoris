using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BasicAttackManager : Skill
{

    public GameObject ProjectilePrefab;
    public GameObject IndicatorPrefab;
    public GameObject UIPrefab;

    [SerializeField] int objPoolMaxIndex;

    GameObject[] BulletObjs;
    BasicAttackBullet[] Bullets;

    int nextPoolIndex;

    [SerializeField] float ProjectileSpeed;
    [SerializeField] float dmgConst;
    [SerializeField] float dmgCoeff;
    [SerializeField] float impactConst;
    [SerializeField] float impactCoeff;

    public float Damage
    {
        get
        {
            return dmgConst + dmgCoeff * player.Stat.spellPoint;
        }
    }

    public float Impact
    {
        get
        {
            return impactConst;
        }
    }



    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();


        BulletObjs = new GameObject[objPoolMaxIndex];
        Bullets = new BasicAttackBullet[objPoolMaxIndex];

        for (int i = 0; i < objPoolMaxIndex; i++)
        {
            BulletObjs[i] = GameObject.Instantiate(ProjectilePrefab);

            BulletObjs[i].SetActive(false);
            Bullets[i] = BulletObjs[i].GetComponent<BasicAttackBullet>();
            Bullets[i].SetSpawner(gameObject);
        }



    }




    // Update is called once per frame
    void Update()
    {
        if (player.CastTargetObj != null)
            transform.rotation = SkillUtils.getAngleTo(transform.position, player.CastTargetObj.transform.position);

        UpdateCooldown();
        UpdateFreezeTime();


    }





    void castProjectilebyAnim()
    {

        Bullets[nextPoolIndex].SetProjectileProperty(player.CastTargetObj, player.CastTarget, ProjectileSpeed,
            player.Stat.spellPoint * dmgCoeff + dmgConst);
        BulletObjs[nextPoolIndex].SetActive(true);
        nextPoolIndex = (nextPoolIndex + 1) % objPoolMaxIndex;
        transform.rotation = SkillUtils.getAngleTo(transform.position, player.CastTargetObj.transform.position);


    }



    public override SkillState getState()
    {


        if (isCasting) return SkillState.Casting;

        if (coolDownTimer > 0)
            return SkillState.CoolDown;

        if (player.CastTargetObj == null) return SkillState.TargetNotFound; // if문 조건 바꿔주자

        if (Vector2.Distance(player.transform.position, player.CastTargetObj.transform.position) > Range)
            return SkillState.FarToCast;


        return SkillState.Castable;
    }






    #region debug functions
#if DEBUG

    [SerializeField] bool drawGizmo;
    Color color0 = new Color(0, 1, 0, 0.5f);
    void OnDrawGizmos()
    {
        if (player != null && drawGizmo)
        {
            Gizmos.color = color0;
            Gizmos.DrawWireSphere(player.transform.position, Range);
        }
    }

#endif
    #endregion
}
