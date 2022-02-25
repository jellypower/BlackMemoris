using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class BasicAttackManager : Skill
{

    [Space]
    [Header("BasicAttack/Skill Prefabs")]
    public GameObject ProjectilePrefab;
    public GameObject IndicatorPrefab;
    public GameObject UIPrefab;




    [Header("BasicAttack/Skill Stats")]
    [SerializeField] float ProjectileSpeed;
    [SerializeField] float dmgConst;
    [SerializeField] float dmgCoeff;
    [SerializeField] float impactConst;
    [SerializeField] float impactCoeff;

    [Header("BasicAttack/Skill Settings")]
    [SerializeField] int objPoolMaxIndex;

    [Header("BasicAttack/Debugs")]
    [SerializeField] bool drawGizmo;




    Queue<BasicAttackBullet> bulletPool;
    BasicAttackBullet bulletToCast;



    protected Animator Anim;
    protected int animCastTriggerId;
    protected int interruptTriggerId;

    public float Damage
    {
        get
        {
            return dmgConst + dmgCoeff * player.Stat.SpellPower.Value;
        }
    }

    public float Impact
    {
        get
        {
            return impactConst;
        }
    }


    protected override void Awake()
    {
        GetDependentComponents();
        awakeAnimator();
        initBulletPool();

        CharacterMask = LayerMask.GetMask("Character");

    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
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

        Anim = GetComponent<Animator>();
        if (Anim == null) throw new Exception("Animator not found");

    }

    protected void awakeAnimator()
    {
        animCastTriggerId = Animator.StringToHash("cast");
        interruptTriggerId = Animator.StringToHash("interrupt");
    }

    public override void InterruptCasting()
    {
        Anim.ResetTrigger(animCastTriggerId);
        Anim.SetTrigger(interruptTriggerId);
        isCasting = false;

    }

    public void finishByAnim()
    {
        isCasting = false;
    }




    // Update is called once per frame
    void Update()
    {
        updateState();

        if (player.CastTarget != null)
            transform.rotation = TransformUtils.getAngleTo(transform.position, player.CastTarget.transform.position);

        UpdateCooldown();
        UpdateFreezeTime();


    }





    void castProjectilebyAnim()
    {
        //Debug.Log(player);

        bulletToCast.SetProjectileProperty(
            player.CastTarget,
            ProjectileSpeed,
            player.Stat.SpellPower.Value * dmgCoeff + dmgConst);

        bulletToCast.gameObject.SetActive(true);

        findNextBulletInPool();

        transform.rotation = TransformUtils.getAngleTo(transform.position, player.CastTarget.gameObject.transform.position);


    }

    void findNextBulletInPool()
    {
        bulletPool.Enqueue(bulletToCast);
        bulletToCast = bulletPool.Dequeue();

    }



    protected override void updateState()
    {

        if (isCasting)
            State = SkillState.Casting;
        else if (coolDownTimer > 0)
            State = SkillState.CoolDown;
        else if (player.CastTarget == null)
            State = SkillState.TargetNotFound;
        else if (Vector2.Distance(player.transform.position, player.CastTarget.transform.position) > Range)
            State = SkillState.FarToCast;
        else 
            State = SkillState.Castable;
    }

    public override void CastSkill()
    {

        Anim.ResetTrigger(interruptTriggerId);
        Anim.SetTrigger(animCastTriggerId);
        isCasting = true;
        coolDownTimer = defaultCoolDown;
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
