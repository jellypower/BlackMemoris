using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class BasicAttackManager : Skill
{

    [Space]
    [Header("BasicAttack/Skill Prefabs")]
    public GameObject ProjectilePrefab;





    [Header("BasicAttack/Skill Stats")]
    [SerializeField] float ProjectileSpeed;
    [SerializeField] float dmgConst;
    [SerializeField] float dmgCoeff;
    [SerializeField] float impact;
    [SerializeField] protected float Range; // 기본 사거리


    [Header("BasicAttack/Skill Settings")]
    [SerializeField] int objPoolMaxIndex;

    [Header("BasicAttack/Debugs")]
    [SerializeField] bool drawGizmo;




    Queue<BasicAttackBullet> bulletPool;
    BasicAttackBullet bulletToCast;



    protected Animator anim;
    protected int animCastTriggerId;
    protected int interruptTriggerId;

    public float Damage
    {
        get
        {
            return dmgConst + dmgCoeff * player.Stat.Power.Value;
        }
    }

    public float Impact
    {
        get
        {
            return impact;
        }
    }


    protected override void Awake()
    {
        GetDependentComponents();
        awakeAnimator();
        initBulletPool();


    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (totalCastTime <= 0.1)
            throw new Exception("TotalCastTime have to be more than 0.1");
        else
            anim.SetFloat("CastTimeMultiplier", 1/totalCastTime);

    }

    void initBulletPool()
    {
        bulletPool = new Queue<BasicAttackBullet>();
        for (int i = 0; i < objPoolMaxIndex; i++)
        {
            BasicAttackBullet newBullet =
                GameObject.Instantiate(ProjectilePrefab)
                .GetComponent<BasicAttackBullet>();


            newBullet.gameObject.SetActive(false);
            newBullet.SetSpawner(this);
            newBullet.transform.parent = transform;

            bulletPool.Enqueue(newBullet);
        }
        bulletToCast = bulletPool.Dequeue();
    }

    protected override void GetDependentComponents()
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null) throw new Exception("PlayerMotor not found");

        anim = GetComponent<Animator>();
        if (anim == null) throw new Exception("Animator not found");

    }

    protected void awakeAnimator()
    {
        animCastTriggerId = Animator.StringToHash("cast");
        interruptTriggerId = Animator.StringToHash("interrupt");
    }

    public override void InterruptCasting()
    {
        anim.ResetTrigger(animCastTriggerId);
        anim.SetTrigger(interruptTriggerId);
        isCasting = false;

    }

    public void finishByAnim()
    {
        FinishCasting();
    }

    protected override void FinishCasting()
    {
        isCasting = false;
    }


    void Update()
    {

        if (player.CastTarget != null)
            transform.rotation = TransformUtils.getAngleTo(transform.position, player.CastTarget.transform.position);

        UpdateCooldown();
        UpdateCastDelayTime();


    }





    void castProjectilebyAnim()
    {
        //Debug.Log(player);

        bulletToCast.SetProjectileProperty(
            player.CastTarget,
            ProjectileSpeed,
            player.Stat.Power.Value * dmgCoeff + dmgConst);

        bulletToCast.gameObject.SetActive(true);

        findNextBulletInPool();

        transform.rotation = TransformUtils.getAngleTo(transform.position, player.CastTarget.gameObject.transform.position);


    }

    void findNextBulletInPool()
    {
        bulletPool.Enqueue(bulletToCast);
        bulletToCast = bulletPool.Dequeue();

    }



    public override SkillState GetState()
    {

        if (isCasting)
            return SkillState.Casting;
        else if (coolDownTimer > 0)
            return SkillState.CoolDown;
        else if (player.CastTarget == null)
            return SkillState.TargetNotFound;
        else if (Vector2.Distance(player.transform.position, player.CastTarget.transform.position) > Range)
            return SkillState.FarToCast;
        else 
            return SkillState.Castable;
    }



    public override void CastSkill()
    {

        anim.ResetTrigger(interruptTriggerId);
        anim.SetTrigger(animCastTriggerId);
        isCasting = true;
        coolDownTimer = coolDownTime;
        castDelayTimer = castDelay;
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
