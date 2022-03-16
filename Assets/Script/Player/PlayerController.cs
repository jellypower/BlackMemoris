using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;


/***
할 일들
1. Stat에서 modifier 추가하기.
2. 공격 가하기, 당하기, 밀기 만들기
3. 스킬 만들기
4. UI만들기
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
    int castTimeExtenderId;


    LinkedList<Vector2> path;


    LayerMask obstaclesLayerMask;


    PlayerSkillContainer skillContainer;
    Skill[] equippedSkills;




    Skill skillToCast = null;


    bool skillCastingEnterTrigger = false;
    bool skillInterruptTrigger = false;
    bool impactEnterTrigger = false;
    bool impactExitTrigger = false;


    OrderState prevOrder;
    Character prevCastTarget;
    Vector2 prevCastTarPos;
    Skill prevSkillToCast;

    public Vector2 FaceDirection { get; private set; }


    Rigidbody2D rb2d;
    float impactedTime;
    Vector2 impactedForce = Vector2.zero;


    // Awake 는 script의 내부 변수 init
    // Awake 는 누굴 참조할지 init
    // Start 는 참조한 애들로부터 정보 가지고 오기

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

        startPathFinder();
        initSkill();

    }

    void Update()
    {
        UpdateState();

        Action();
        Animate();

        UpdateTrigger();


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
        
    }

    #endregion

    #region transition functions for fsm
    protected override void UpdateState() // FSM 에서 transition 부분
    {



        if (OrderExecutable) updateWithOrder();

        updateFromInteraction();

        //Update with other object

        updateFromItself();
        //Update with itself


        updateFaceDirection();

    }

    void updateWithOrder()
    {
        BlockDupOrder(); // 이미 실행하고 있는 행동과 동일한 명령이 들어오면 무시함

        switch (state)
        {
            case PlayerActionState.idle:
            case PlayerActionState.Moving:
            case PlayerActionState.MovingToTargetToCast:
            case PlayerActionState.MovingToTarPosToCast:
                TransitionByOrder();
                break;

            case PlayerActionState.SkillCasting:
                if (order != OrderState.None) interruptCasting();
                TransitionByOrder();
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
            case OrderState.CastToTarget: //쿨다운인 스킬을 사용하게 하면 사용 못하게 막기
                if (skillToCast.State == SkillState.CoolDown)
                {
                    clearAction(PlayerActionState.idle);
                    break;
                }
                state = PlayerActionState.MovingToTargetToCast;
                break;
            case OrderState.CastToGround:
                state = PlayerActionState.MovingToTarPosToCast;
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

            if (state == PlayerActionState.Impacted) return false;
            else if (state == PlayerActionState.SkillCasting && skillToCast.isFreezingPlayer())
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
                else if (skillToCast.CastType == SkillCastType.NonTargeting)
                    FaceDirection = CastTargetPos - (Vector2)transform.position;
                break;

        }
        FaceDirection = FaceDirection.normalized;
    }

    void updateFromItself()
    {
        switch (state)
        {
            case PlayerActionState.SkillCasting:
                if (skillToCast == null)
                {

                    clearAction(PlayerActionState.idle);
                }
                
                if (skillToCast.State != SkillState.Casting)
                {
                    // 여기 문장 없이도 player가 casting 연발해도 오류 안나게 고치자.

                    clearAction(PlayerActionState.idle);
                }
                break;

            case PlayerActionState.Moving:
                if (path.Count == 0) state = PlayerActionState.idle;
                break;
            case PlayerActionState.MovingToTargetToCast:
                if (skillToCast.State == SkillState.Castable)
                {
                    skillCastingEnterTrigger = true;
                    state = PlayerActionState.SkillCasting;
                }
                break;

            case PlayerActionState.Impacted:
                if (impactExitTrigger)
                {
                    state = PlayerActionState.idle;
                }
                break;
        }

    }



    void updateFromInteraction()
    {
        if (impactEnterTrigger)
        { 
            rb2d.AddForce(impactedForce);
            clearAction(PlayerActionState.Impacted);
        }
    }

    #endregion

    #region action for FSM
    protected override void Action() // FSM에서 Action 부분
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
                break;
            case PlayerActionState.Impacted:
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

        CastTarget = null;
        CastTargetPos = transform.position;
        path.Clear();
        skillToCast = null;
        state = nextState;


    }

    void skillCasting()
    {
        if (skillCastingEnterTrigger)
        {
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
                anim.ResetTrigger(skillInterruptTriggerId);
                anim.SetTrigger(skillCastTriggerId);
                setCastAnimTime(skillToCast.PlayerAnimCastTime);
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
            anim.SetFloat(impactTimeId, 1/impactedTime);
        }


    }

    void setCastAnimTime(float castTime)
    {
        const uint FRAME_NO = 60;

        float castTimeMagnif;

        float castTimeExtender;

        if (Mathf.Approximately(castTime, 1.0f))
        {
            castTimeMagnif = 1.0f;
            anim.SetFloat(skillCastingTimeId , castTimeMagnif);
            anim.SetFloat(castTimeExtenderId, FRAME_NO);
        }
        else if (castTime < 1.017f) // 0.017f 는 한 프레임에 걸리는 시간
        {
            castTimeMagnif = 1.0f / castTime;
            anim.SetFloat(skillCastingTimeId, castTimeMagnif); ;
            anim.SetFloat(castTimeExtenderId, FRAME_NO);
        }
        else
        {
            castTimeMagnif = 1.0f;
            castTimeExtender = (castTime - 1);
            anim.SetFloat(skillCastingTimeId, castTimeMagnif);
            anim.SetFloat(castTimeExtenderId, castTimeExtender);
        }
    }

    #endregion

    #region trigger function
    void UpdateTrigger()
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
        CastTarget = obj.GetComponent<Character>();
    }

    void setOrder(OrderState val)
    {

        if (order != OrderState.None)
            prevOrder = order;
        order = val;

    }

    void setSkillToCast(Skill skill)
    {

        if (skill.CastType != SkillCastType.Passive)
        {
            prevSkillToCast = skillToCast;
            skillToCast = skill;
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
        Debug.Log("HP: " + CharacterStat.CurrentHP+ ", damage: " + damage + ", dmgReduction:" + CharacterStat.DamageReductionRate);
    }

    public override void GetImpact(float impact, float impactedTime, Vector2 force ,Character Attacker)
    {
        if(impact > Stat.ImpactResistance.Value)
        {
            interruptCasting();
            this.impactedTime = impactedTime;
            this.impactedForce = force;
            impactEnterTrigger = true;

        }

    }

    public override void RecoverFromImpact()
    {
        impactExitTrigger = true;
    }


    #endregion







    #region debug
#if DEBUG


#endif
    #endregion




}