using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBombManager : Skill
{


    [Space]
    [Header("ManaBomb/Skill Prefabs")]
    public GameObject ProjectilePrefab; // 나중에 부모 클래스로 옮기자.





    [Header("ManaBomb/Skill Stats")]
    [SerializeField] float ProjectileSpeed;
    [SerializeField] float dmgConst;
    [SerializeField] float dmgCoeff;
    [SerializeField] float impactConst;
    [SerializeField] float impactCoeff;
    [SerializeField] float bulletSpawnTime;
    [SerializeField] float bulletChargeTime;
    [SerializeField] protected float Range; // 기본 사거리



    [Header("ManaBomb/Skill Settings")]
    [SerializeField] int objPoolMaxIndex;

    [Header("MamaBomb/Debugs")]
    [SerializeField] bool drawGizmo;


    IEnumerator castCoroutine;

    Queue<ManaBombBullet> bulletPool;
    ManaBombBullet bulletToCast;


    Character castTarget;

    public Transform SpawnPoint { get; private set; }

    float Damage
    {
        get
        {
            return dmgConst + dmgCoeff * player.Stat.Power.Value;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        initBulletPool();

        SpawnPoint = transform.Find("BulletSpawnPoint").transform;

    }

    void initBulletPool()
    {
        bulletPool = new Queue<ManaBombBullet>();

        for (int i = 0; i < objPoolMaxIndex; i++)
        {
            ManaBombBullet newBullet =
                GameObject.Instantiate(ProjectilePrefab)
                .GetComponent<ManaBombBullet>();

            newBullet.gameObject.SetActive(false);
            newBullet.SetSpawner(this);
            newBullet.transform.parent = transform;

            bulletPool.Enqueue(newBullet);

        }

        bulletToCast = bulletPool.Dequeue();
    }


    protected override void Start()
    {
        base.Start();
    }



    // Update is called once per frame
    void Update()
    {

        UpdateCooldown();
        UpdateCastDelayTime();



    }

    IEnumerator CastCoroutine()
    {
        yield return new WaitForSeconds(bulletSpawnTime);

        startChargeProjectile();

        yield return new WaitForSeconds(bulletChargeTime);

        castProjectile();

        yield return new WaitForSeconds(Mathf.Max(totalCastTime-bulletSpawnTime-bulletChargeTime,0));

        FinishCasting();

    }


    public override void CastSkill()
    {
        castTarget = player.CastTarget;
        castCoroutine = CastCoroutine();
        StartCoroutine(castCoroutine);
        isCasting = true;
        coolDownTimer = coolDownTime;
        castDelayTimer = castDelay;

    }

    public override void InterruptCasting()
    {

        if (!bulletToCast.IsCasted)
        {
            StopCoroutine(castCoroutine);
            bulletToCast.InterruptCasting();
            
        }

        isCasting = false;


    }

    protected override void FinishCasting()
    {
        isCasting = false;
    }



    // interrupt 가능하게 고치기


    void startChargeProjectile()
    {
        bulletToCast.Spawn(
            castTarget,
            ProjectileSpeed,
            player.Stat.Power.Value * dmgCoeff + dmgConst);

    }

    void castProjectile()
    {
        
        bulletToCast.Cast();
        findNextBulletInPool();


    }
    
    void findNextBulletInPool()
    {
        bulletPool.Enqueue(bulletToCast);
        bulletToCast = bulletPool.Dequeue();

    }



    public override SkillState GetState()
    {

        if (isCasting) return SkillState.Casting;

        else if (coolDownTimer > 0)
            return SkillState.CoolDown;

        else if (player.CastTarget == null) return SkillState.TargetNotFound; // if문 조건 바꿔주자

        else if (Vector2.Distance(player.transform.position, player.CastTarget.transform.position) > Range)
            return SkillState.FarToCast;
        else
            return SkillState.Castable;
    }






    #region debug functions
#if DEBUG

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
