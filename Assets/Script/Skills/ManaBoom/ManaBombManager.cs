using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBombManager : Skill
{

    [Space]
    [Header("ManaBomb/Skill Prefabs")]
    public GameObject ProjectilePrefab; // 나중에 부모 클래스로 옮기자.
    public GameObject IndicatorPrefab; // 나중에 부모 클래스로 옮기자.
    public GameObject UIPrefab; // 나중에 부모 클래스로 옮기자. -> 요놈은 UI스프라이트로만 바꿔도 될듯.




    [Header("ManaBomb/Skill Stats")]
    [SerializeField] float ProjectileSpeed;
    [SerializeField] float dmgConst;
    [SerializeField] float dmgCoeff;
    [SerializeField] float impactConst;
    [SerializeField] float impactCoeff;
    [SerializeField] float bulletSpawnTime;
    [SerializeField] float bulletChargeTime;

    [Header("ManaBomb/Skill Settings")]
    [SerializeField] int objPoolMaxIndex;

    [Header("MamaBomb/Debugs")]
    [SerializeField] bool drawGizmo;


    IEnumerator castCoroutine;

    Queue<ManaBombBullet> bulletPool;
    ManaBombBullet bulletToCast;


    Character castTarget;


    float Damage
    {
        get
        {
            return dmgConst + dmgCoeff * player.Stat.SpellPower.Value;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        initBulletPool();
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
        UpdateFreezeTime();

        updateState();

    }

    IEnumerator CastCoroutine()
    {
        yield return new WaitForSeconds(bulletSpawnTime);

        startChargeProjectile();

        yield return new WaitForSeconds(bulletChargeTime);

        castProjectile();

    }


    public override void CastSkill()
    {
        castTarget = player.CastTarget;
        castCoroutine = CastCoroutine();
        StartCoroutine(castCoroutine);
        isCasting = true;
        coolDownTimer = defaultCoolDown;
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



    // interrupt 가능하게 고치기


    void startChargeProjectile()
    {
        bulletToCast.CastBullet(
            castTarget,
            ProjectileSpeed,
            player.Stat.SpellPower.Value * dmgCoeff + dmgConst);

    }

    void castProjectile()
    {
        
        bulletToCast.CastTrigger();
        findNextBulletInPool();


    }
    
    void findNextBulletInPool()
    {
        bulletPool.Enqueue(bulletToCast);
        bulletToCast = bulletPool.Dequeue();

    }



    protected override void updateState()
    {

        if (isCasting) State =  SkillState.Casting;

        else if (coolDownTimer > 0)
            State = SkillState.CoolDown;

        else if (player.CastTarget == null) State = SkillState.TargetNotFound; // if문 조건 바꿔주자

        else if (Vector2.Distance(player.transform.position, player.CastTarget.transform.position) > Range)
            State = SkillState.FarToCast;
        else
            State = SkillState.Castable;
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
