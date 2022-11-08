using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Unity.Burst;


/***
�� �ϵ�
1. Stat���� modifier �߰��ϱ�.
2. ���� ���ϱ�, ���ϱ�, �б� �����
3. ��ų �����
4. UI�����
***/



public class PlayerController : Character
{

    OrderState order;

    public Character CastTarget { get; private set; }


    [HideInInspector] public Vector2 CastTargetPos;

    PathFinder pathFinder;
    NodeGridGenerator gridInfo;

    [HideInInspector]
    public PlayerStat Stat;




    Animator anim;
    PlayerActionState state;
    int animPlayerStateId, animXDirId, animYDirId;
    int skillCastTriggerId, skillCastingTimeId;
    int skillInterruptTriggerId;
    int impactTriggerId, impactTimeId;
    int impactRecoverTriggerId;
    int castTimeExtenderId;
    RuntimeAnimatorController originAnimator;




    LinkedList<Vector2> path;


    LayerMask obstaclesLayerMask;


    PlayerSkillContainer skillContainer;
    Skill[] equippedSkills;




    Skill skillToCast = null;


    bool skillCastingEnterTrigger = false;
    bool skillInterruptTrigger = false;
    bool impactEnterTrigger = false;
    bool impactExitTrigger = false; //impact ���¿��� ���




    OrderState prevOrder;
    Character prevCastTarget;
    Vector2 prevCastTarPos;
    Skill prevSkillToCast;

    public Vector2 FaceDirection { get; private set; }


    Rigidbody2D rb2d;
    float impactedTime;
    Vector2 impactedForce = Vector2.zero;

    bool isDead = false;


    // Awake �� script�� ���� ���� init
    // Awake �� ���� �������� init
    // Start �� ������ �ֵ�κ��� ���� ������ ����

    private void Awake()
    {
        awakeDependentComponents();

        awakePlayerMotor();
        awakeAnimate();


    }
    void Start()
    {
        String[] obstascleArr = { "Obstacles" };
        obstaclesLayerMask = LayerMask.GetMask(obstascleArr);

        originAnimator = anim.runtimeAnimatorController;

        startPathFinder();
        initSkill();

    }

    void Update()
    {


        UpdateState();

        if (isDead)
        {
            gameObject.layer = LayerMask.NameToLayer("Ghost");
            ResetTrigger();
            return;
        }

        Action();



        Animate();



        ResetTrigger();


    }


    #region init functions

    protected virtual void awakePlayerMotor()
    {
        FaceDirection = Vector2.down;

        order = OrderState.None;
        prevOrder = OrderState.None;
        state = PlayerActionState.idle;
        CastTargetPos = transform.position;



    }

    void awakeDependentComponents()
    {
        pathFinder = GetComponent<PathFinder>();
        if (pathFinder == null) throw new Exception("PathFinder not Found");

        gridInfo = GetComponent<NodeGridGenerator>();
        if (gridInfo == null) throw new Exception("NodeGridGenerator not Found");

        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null) throw new Exception("Rigidbody2D not found");

        anim = GetComponent<Animator>();
        if (anim == null) throw new Exception("Animator not found");

        skillContainer = GetComponent<PlayerSkillContainer>();
        if (skillContainer == null) throw new Exception("PlayerSkillContainer not found");

        CharacterStat = GetComponent<PlayerStat>();
        Stat = (PlayerStat)CharacterStat;
        if (Stat == null) throw new Exception("PlayerStat not found");

        hitbox = gameObject.GetComponentInChildren<HitBoxHandler>();
        if (Stat == null) throw new Exception("Hitbox GameObject not found in children");

    }

    void startPathFinder()
    {
        path = pathFinder.getPath();

    }

    void initSkill()
    {
        equippedSkills = skillContainer.Skills;
    }


    void awakeAnimate()
    {


        animPlayerStateId = Animator.StringToHash("PlayerState");
        animXDirId = Animator.StringToHash("xDir");
        animYDirId = Animator.StringToHash("yDir");
        skillCastTriggerId = Animator.StringToHash("Cast");
        skillCastingTimeId = Animator.StringToHash("CastTimeMagnif");
        skillInterruptTriggerId = Animator.StringToHash("Interrupt");
        impactTriggerId = Animator.StringToHash("Impact");
        impactTimeId = Animator.StringToHash("ImpactTime");
        castTimeExtenderId = Animator.StringToHash("CastTimeExtender");
        impactRecoverTriggerId = Animator.StringToHash("ImpactRecover");

    }

    #endregion

    #region transition functions for fsm
    protected override void UpdateState() // FSM ���� transition �κ�
    {


        if (OrderExecutable)
        {
            updateWithOrder();


        }
        else
        {
            // ���� �̸� �Էµ� order�� �����ص� �ʿ䰡 �ִ�. ��� E Q ���Է� ������ ��ó��
            setOrder(OrderState.None);
        }

        updateFromInteraction();
        //Update with other object

        updateFromItself();
        //Update with itself


        updateFaceDirection();

    }

    void updateWithOrder()
    {
        BlockDupOrder(); // �̹� �����ϰ� �ִ� �ൿ�� ������ ����� ������ ������


        switch (state)
        {

            case PlayerActionState.idle:
            case PlayerActionState.Moving:
            case PlayerActionState.MovingToTargetToCast:
            case PlayerActionState.MovingToTarPosToCast:
                TransitionByOrder();
                break;
            case PlayerActionState.SkillCasting:
                // total cast time�� cast delay�� ������ �� ���̿� frame�� �ʹ� ��� frame ���� order�� ���� interrupt�� ���� �Ұ��ϴ�.

                if (order != OrderState.None)
                {
                    interruptCasting();
                    TransitionByOrder();

                }
                break;
        }

        setOrder(OrderState.None);
    }

    void TransitionByOrder()
    {


        switch (order) // state change from input
        {
            case OrderState.MoveToTargetPos:
                state = PlayerActionState.Moving;
                break;
            case OrderState.CastToTarget:
                if (skillToCast != null && skillToCast.GetState() == SkillState.CoolDown)
                {
                    //skillToCast = null;
                    break;
                }
                if (skillToCast.GetState() == SkillState.FarToCast)
                {
                    state = PlayerActionState.MovingToTargetToCast;
                }
                else if (skillToCast.GetState() == SkillState.Castable)
                {
                    skillCastingEnterTrigger = true;
                    state = PlayerActionState.SkillCasting;
                }
                break;
            case OrderState.CastToGround:
                if (skillToCast != null && skillToCast.GetState() == SkillState.CoolDown)
                {
                    //skillToCast = null;
                    break;
                }
                if (skillToCast.GetState() == SkillState.FarToCast)
                {
                    state = PlayerActionState.MovingToTarPosToCast;
                }
                else if (skillToCast.GetState() == SkillState.Castable)
                {
                    print("A");
                    skillCastingEnterTrigger = true;
                    state = PlayerActionState.SkillCasting;
                }
                break;
            case OrderState.Stop:
                clearAction(PlayerActionState.idle);
                break;
        }


    }



    bool OrderExecutable
    {
        get
        {


            if (state == PlayerActionState.CrowdControl) return false;
            else if (state == PlayerActionState.SkillCasting && skillToCast.OnCastDelay())
                return false;

            return true;
        }
    }


    bool isDupOrder()
    {
        switch (state)
        {
            case PlayerActionState.SkillCasting:
                if (prevSkillToCast == skillToCast &&
                    order == OrderState.CastToTarget &&
                    prevCastTarget == CastTarget)
                    return true;
                else if (prevSkillToCast == skillToCast &&
                    order == OrderState.CastToGround &&
                    prevCastTarPos == CastTargetPos)
                    return true;
                break;

            case PlayerActionState.MovingToTargetToCast:
                if (order == OrderState.CastToTarget &&
                    prevSkillToCast == skillToCast &&
                    prevCastTarget == CastTarget)
                    return true;
                break;
            case PlayerActionState.MovingToTarPosToCast:
                if (order == OrderState.CastToGround &&
                    prevSkillToCast == skillToCast &&
                    prevCastTarPos == CastTargetPos)
                    return true;
                break;
        }


        return false;
    }

    void BlockDupOrder()
    {
        if (isDupOrder())
        {
            setOrder(OrderState.None);
        }
    }

    void updateFaceDirection()
    {
        switch (state)
        {
            case PlayerActionState.Moving:
            case PlayerActionState.MovingToTargetToCast:
            case PlayerActionState.MovingToTarPosToCast:
                if (path.Count > 0) FaceDirection = path.First.Value - (Vector2)transform.position;
                break;
            case PlayerActionState.SkillCasting:
                if (skillToCast.CastType == SkillCastType.Targeting)
                    FaceDirection = (Vector2)(CastTarget.transform.position - transform.position);
                else if (skillToCast.CastType == SkillCastType.NonTargeting && skillCastingEnterTrigger)
                    FaceDirection = CastTargetPos - (Vector2)transform.position;
                break;

        }
        FaceDirection = FaceDirection.normalized;
    }

    void updateFromItself()
    {
        switch (state)
        {

            case PlayerActionState.SkillCasting: // ���� �ذ��ؾߵǳ�
                if (skillToCast.GetState() != SkillState.Casting && !skillCastingEnterTrigger)
                {
                    clearAction(PlayerActionState.idle);
                }
                break;

            case PlayerActionState.Moving:
                if (path.Count == 0) state = PlayerActionState.idle;
                break;

            case PlayerActionState.MovingToTargetToCast:
            case PlayerActionState.MovingToTarPosToCast:
                if (skillToCast.GetState() == SkillState.Castable)
                {
                    skillCastingEnterTrigger = true;
                    state = PlayerActionState.SkillCasting;
                }
                break;


            case PlayerActionState.CrowdControl:
                impactedTime -= Time.deltaTime;

                if (impactedTime <= 0)
                {

                    state = PlayerActionState.idle;
                    impactedTime = 0;
                    impactExitTrigger = true;
                }
                break;

        }

    }



    void updateFromInteraction()
    {
        if (impactEnterTrigger)
        {
            rb2d.AddForce(impactedForce);
            clearAction(PlayerActionState.CrowdControl);
        }
    }

    #endregion

    #region action for FSM
    protected override void Action() // FSM���� Action �κ�
    {

        switch (state)
        {
            case PlayerActionState.SkillCasting:
                skillCasting();
                break;
            case PlayerActionState.Moving:
                followPath();
                break;
            case PlayerActionState.MovingToTargetToCast:
                MovingToTargetToCast();
                break;
            case PlayerActionState.MovingToTarPosToCast:
                followPath();
                break;
            case PlayerActionState.CrowdControl:
                break;


        }

    }

    void followPath()
    {
        if (path.Count == 0) return;

        transform.position = Vector2.MoveTowards(transform.position, path.First.Value, Stat.Speed.Value * Time.deltaTime);
        if (Mathf.Approximately(
            Vector2.Distance(transform.position, path.First.Value), 0)
            ) path.RemoveFirst();
    }

    void MovingToTargetToCast()
    {
        UpdateShortestPathToTarget();
        followPath();
    }

    void UpdateShortestPathToTarget()
    {

        if (path.Count != 0 && !Physics2D.Raycast(
                path.Last.Value, CastTarget.transform.position,
                ((Vector2)CastTarget.transform.position - path.Last.Value).magnitude,
                 obstaclesLayerMask))
        {
            path.Last.Value = CastTarget.transform.position;
        }
        else
        {
            path = pathFinder.getShortestPath(transform.position, CastTarget.transform.position);
        }
    }

    void clearAction(PlayerActionState nextState)
    {

        //CastTarget = null;
        CastTargetPos = transform.position;
        path.Clear();
        //skillToCast = null;
        state = nextState;


    }

    void skillCasting()
    {
        if (skillCastingEnterTrigger)
        {
            print("AA");
            skillToCast.CastSkill();
        }
    }

    void interruptCasting()
    {
        if (state == PlayerActionState.SkillCasting && skillToCast != null)
        {
            skillToCast.InterruptCasting();
            skillInterruptTrigger = true;
        }
    }




    #endregion

    #region animation functions
    protected override void Animate()
    {

        anim.SetFloat(animXDirId, FaceDirection.x);
        anim.SetFloat(animYDirId, FaceDirection.y);


        if (state == PlayerActionState.Moving || state == PlayerActionState.MovingToTargetToCast
            || state == PlayerActionState.MovingToTarPosToCast)
        {
            anim.SetInteger(animPlayerStateId, (int)PlayerAnimState.Moving);
        }
        else if (state == PlayerActionState.idle)
        {
            anim.SetInteger(animPlayerStateId, (int)PlayerAnimState.idle);
        }
        else if (state == PlayerActionState.SkillCasting)
        {
            if (skillCastingEnterTrigger)
            {
                anim.SetInteger(animPlayerStateId, (int)PlayerAnimState.idle);
                anim.ResetTrigger(skillInterruptTriggerId);
                anim.SetTrigger(skillCastTriggerId);
                setAnimCastTime(skillToCast.TotalCastTime);
            }
        }
        if (skillInterruptTrigger)
        {

            anim.ResetTrigger(skillCastTriggerId);
            anim.SetTrigger(skillInterruptTriggerId);
        }
        if (impactEnterTrigger)
        {
            anim.ResetTrigger(skillCastTriggerId);
            anim.ResetTrigger(skillInterruptTriggerId);
            anim.SetTrigger(impactTriggerId);
        }
        if (impactExitTrigger)
        {
            anim.SetTrigger(impactRecoverTriggerId);
        }


    }

    void setAnimCastTime(float castTime)
    {
        const uint FRAME_NO = 60;

        float castTimeMagnif; //��ų ĳ���� �ð��� 1�� ���ϸ� ��ų ĳ���� ����� �ٿ������
        float castTimeExtender; //��ų ĳ���� �ð��� 1�� �̻��̸� ��ų ĳ���� ����� �÷������

        if (Mathf.Approximately(castTime, 1.0f))
        {
            castTimeMagnif = 1.0f;
            anim.SetFloat(skillCastingTimeId, castTimeMagnif);
            anim.SetFloat(castTimeExtenderId, FRAME_NO);
        }
        else if (castTime < 1.017f) // 0.017f �� �� �����ӿ� �ɸ��� �ð�
        {
            castTimeMagnif = 1.0f / castTime;
            anim.SetFloat(skillCastingTimeId, castTimeMagnif);
            anim.SetFloat(castTimeExtenderId, FRAME_NO);
        }
        else
        {
            castTimeMagnif = 1.0f;
            castTimeExtender = 1 / (castTime - 1);
            anim.SetFloat(skillCastingTimeId, castTimeMagnif);
            anim.SetFloat(castTimeExtenderId, castTimeExtender);
        }
    }

    #endregion

    #region trigger function
    void ResetTrigger()
    {
        skillCastingEnterTrigger = false;
        skillInterruptTrigger = false;
        impactEnterTrigger = false;
        impactExitTrigger = false;
    }
    #endregion



    #region private functions to set player
    void setTarPos(Vector2 pos)
    {
        Vector2 start = transform.position;
        prevCastTarPos = CastTargetPos;
        CastTargetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        path = pathFinder.getShortestPath(start, pos);
    }
    void setTarget(GameObject obj)
    {
        prevCastTarget = CastTarget;
        CastTarget = obj.GetComponent<HitBoxHandler>().character;
    }

    void setOrder(OrderState val)
    {

        if (order != OrderState.None)
            prevOrder = order;
        order = val;

    }



    void setSkillToCast(Skill skill)
    {
        if (OrderExecutable)
        {

            if (skill.CastType != SkillCastType.Passive)
            {
                prevSkillToCast = skillToCast;
                skillToCast = skill;

                if (skillToCast.PlayerCastAnimator != null)
                    anim.runtimeAnimatorController = skillToCast.PlayerCastAnimator;
                else
                    anim.runtimeAnimatorController = originAnimator;
            }
        }



    }
    #endregion

    #region public functions to set player


    public void OrderToMove(Vector2 pos)
    {


        setTarPos(pos);
        setOrder(OrderState.MoveToTargetPos);

    }

    public void OrderToCastToTarget(Skill skill, GameObject tar)
    {
        setSkillToCast(skill);
        setTarget(tar);
        setOrder(OrderState.CastToTarget);
    }

    public void OrderToCastToTarPos(Skill skill, Vector2 tarPos)
    {
        setSkillToCast(skill);
        setTarPos(tarPos);
        setOrder(OrderState.CastToGround);
    }

    public void OrderToStop()
    {
        setOrder(OrderState.Stop);
    }

    public void OrderToCastImmediately(Skill skill)
    {
        setSkillToCast(skill);
        setOrder(OrderState.StartCastSkill);
    }

    public override void GetAttack(float damage, Character Attacker)
    {
        CharacterStat.TakeDamage(damage);
        Debug.Log("HP: " + CharacterStat.CurrentHP + ", damage: " + damage);
    }

    public override void GetImpact(float impact, float impactedTime, Vector2 force, Character Attacker)
    {
        if (impact > Stat.ImpactResistance.Value)
        {

            interruptCasting();
            this.impactedTime = impactedTime;
            this.impactedForce = force;
            impactEnterTrigger = true;

        }

    }

    public void DashTowards(Vector2 Direction, float speed)
    // rigidbody2d �� ���ؼ��� �ƴ� ������� �ܺ� class�� character�� �̵����� �ִ� �Լ�.
    // rb2d�� �������� ��ü�ν� �̵��ϱ� ������ �����ϰ� ���ϴ� �Ÿ���ŭ, ���ϴ� �ð����ȸ� character�� �����̴°� �Ұ��ϴ�.
    {
        //if (Physics2D.OverlapPoint((Vector2)transform.position + (Direction * speed * Time.deltaTime)) != null) return;
        

        transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + Direction, speed * Time.deltaTime);
    }

    public void Die()
    {
        anim.SetTrigger("Die");
        anim.ResetTrigger(impactTriggerId);
        isDead = true;
        

    }

    #endregion

    protected override void RecoverFromImpact()
    {
        impactedTime = 0;
    }







    #region debug
#if DEBUG


#endif
    #endregion




}