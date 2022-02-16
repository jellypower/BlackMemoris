using UnityEngine;
using System.Collections.Generic;
using System;


/***
할 일들
1. scriptable object 문제 해결하기
2. 공격 가하기, 당하기, 밀기 만들기
3. 스킬 만들기
4. UI만들기
***/



public class Player : Character
{
    OrderState order;


    GameObject castTargetObj;
    public GameObject CastTargetObj
    {
        get
        {
            return castTargetObj;
        }
    }
    Character castTarget;
    public Character CastTarget
    {
        get
        {
            return castTarget;
        }
    }

    [HideInInspector] public Vector2 CastTargetPos;

    PathFinder pathFinder;
    NodeGridGenerator gridInfo;


    [HideInInspector] public PlayerStat Stat;



    Animator anim;
    PlayerActionState state;
    int animPlayerStateId, animXDirId, animYDirId;
    int skillCastTriggerId, skillCastingTimeId;
    int skillInterruptTriggerId;


    LinkedList<Vector2> path;


    LayerMask obstaclesLayerMask;


    [SerializeField] int CastableSkillNum;
    [HideInInspector] public GameObject[] SkillObjs; // 구현 0127
    [HideInInspector] public Skill[] skills;



    [HideInInspector] public GameObject SkillToCastObj = null;
    [HideInInspector] public Skill skillToCast = null;


    bool skillCastingEnterTrigger = false;
    bool skillInterruptTrigger = false;
    bool impactTrigger = false;


    OrderState prevOrder;
    GameObject prevCastTargetObj;
    Vector2 prevCastTarPos;
    GameObject prevSkillToCastObj;

    Vector2 FaceDirection;


    Rigidbody2D rb2d;


    void Start()
    {

        InitCharacter();
        InitAnimate();
        initPathFinder();

        initTmp();





    }
    void Update()
    {

        UpdateState();

        
        Action();
        Animate();
        
        UpdateTrigger();


    }


    #region init functions

    protected virtual void InitCharacter()
    {
        FaceDirection = Vector2.down;

        order = OrderState.None;
        prevOrder = OrderState.None;
        state = PlayerActionState.idle;
        CastTargetPos = transform.position;
        anim = GetComponent<Animator>();



        Stat = (PlayerStat)CharacterStat;

        rb2d = GetComponent<Rigidbody2D>();

    }

    void initPathFinder()
    {
        pathFinder = GetComponent<PathFinder>();
        gridInfo = GetComponent<NodeGridGenerator>();

        if (pathFinder == null) throw new Exception("PathFinder not Found");
        if (gridInfo == null) throw new Exception("NodeGridGenerator not Found");

        path = pathFinder.getPath();

    }

    void initTmp()
    {
        obstaclesLayerMask = LayerMask.GetMask("Obstacles");


        SkillObjs = new GameObject[CastableSkillNum];
        skills = new Skill[CastableSkillNum];

        SkillObjs[0] = transform.Find("BasicAttack").gameObject;
        if (SkillObjs[0] == null) throw new Exception("BasicAttack not found");
        skills[0] = SkillObjs[0].GetComponent<BasicAttackManager>();
        if (SkillObjs[0] == null) throw new Exception("BasicAttack not found");


    }
    #endregion

    #region transition functions for fsm
    protected override void UpdateState() // FSM 에서 transition 부분
    {
        if (OrderExecutable) UpdateWithOrder();

        //Update with other object

        updateFromItself();
        //Update with itself


        updateFaceDirection();

    }

    void UpdateWithOrder()
    {
        BlockDupOrder(); // 이미 실행하고 있는 행동과 동일한 명령이 들어오면 무시함

        switch (state)
        {
            case PlayerActionState.idle:
                TransitionByOrder();
                break;
            case PlayerActionState.SkillCasting:
                if (order != OrderState.None) interruptCasting();
                TransitionByOrder();
                break;
            case PlayerActionState.Moving:
                TransitionByOrder();
                break;
            case PlayerActionState.MovingToTargetToCast:
                TransitionByOrder();
                break;
            case PlayerActionState.MovingToTarPosToCast:
                TransitionByOrder(); //아직 미구현
                break;
            case PlayerActionState.Impacted:

                break;

        }

        Order = OrderState.None;
    }

    void TransitionByOrder()
    {
        switch (order) // state change from input
        {
            case OrderState.MoveToTargetPos:
                state = PlayerActionState.Moving;
                break;
            case OrderState.CastToTarget:
                if (skillToCast.getState() == SkillState.CoolDown)
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
                if (prevSkillToCastObj == SkillToCastObj &&
                    order == OrderState.CastToTarget &&
                    prevCastTargetObj == CastTargetObj)
                    return true;
                else if (prevSkillToCastObj == SkillToCastObj &&
                    order == OrderState.CastToGround &&
                    prevCastTarPos == CastTargetPos)
                    return true;
                break;

            case PlayerActionState.MovingToTargetToCast:
                if (order == OrderState.CastToTarget &&
                    prevSkillToCastObj == SkillToCastObj &&
                    prevCastTargetObj == CastTargetObj)
                    return true;
                break;
            case PlayerActionState.MovingToTarPosToCast:
                if (order == OrderState.CastToGround &&
                    prevSkillToCastObj == SkillToCastObj &&
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
            Order = OrderState.None;
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
                    FaceDirection = (Vector2)(CastTargetObj.transform.position - transform.position);
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
                if(skillToCast == null)
                {
                    clearAction(PlayerActionState.idle);
                }

                if(skillToCast.getState() != SkillState.Casting)
                {
                    clearAction(PlayerActionState.idle);
                }
                break;

            case PlayerActionState.Moving:
                if (path.Count == 0) state = PlayerActionState.idle;
                break;
            case PlayerActionState.MovingToTargetToCast:
                if (skillToCast.getState() == SkillState.Castable)
                {
                    skillCastingEnterTrigger = true;
                    state = PlayerActionState.SkillCasting;
                }
                break;
        }
    }

    #endregion

    #region public functions to set player
    public void setTarPos(Vector2 pos)
    {
        Vector2 start = transform.position;
        prevCastTarPos = CastTargetPos;
        CastTargetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        path = pathFinder.getShortestPath(start, pos);
    }
    public void setTarget(GameObject obj)
    {
        prevCastTargetObj = castTargetObj;
        castTargetObj = obj;
        castTarget = obj.GetComponent<Character>();
    }

    public OrderState Order
    {
        set
        {
            if (order != OrderState.None)
                prevOrder = order;
            order = value;
        }
    }


    public void setSkillToCast(Skill skill)
    {
        
        if (skill.CastType != SkillCastType.Passive)
        {
            prevSkillToCastObj = SkillToCastObj;
            SkillToCastObj = skill.gameObject;
            skillToCast = skill;
        }

    }

    public GameObject getSkillToCast()
    {
        return SkillToCastObj;
    }


    public override void GetAttack(float damage, Character Attacker)
    {
        CharacterStat.currentHP -= damage * CharacterStat.damageReductionRate;
        Debug.Log("HP: "+CharacterStat.currentHP +", damage: " + damage + ", dmgReduction:" + CharacterStat.damageReductionRate);
    }

    public override void GetImpact(float impact, Character Attacker)
    {
        if (skillToCast != null)
        {
            skillToCast.InterruptCasting();
            setSkillToCast(null);
        }
    }


    #endregion

    #region action for FSM
    protected override void Action() // FSM에서 Action 부분
    {

        switch (state)
        {
            case PlayerActionState.idle:
                break;
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

        transform.position = Vector2.MoveTowards(transform.position, path.First.Value, Stat.speed * Time.deltaTime);
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
                path.Last.Value, CastTargetObj.transform.position,
                ((Vector2)CastTargetObj.transform.position - path.Last.Value).magnitude,
                 obstaclesLayerMask))
        {
            path.Last.Value = CastTargetObj.transform.position;
        }
        else
        {
            path = pathFinder.getShortestPath(transform.position, CastTargetObj.transform.position);
        }
    }

    void clearAction(PlayerActionState nextState)
    {
        castTargetObj = null;
        castTarget = null;
        CastTargetPos = transform.position;
        path.Clear();
        skillToCast = null;
        SkillToCastObj = null;
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
        if(skillToCast != null)
            skillToCast.InterruptCasting();
        skillInterruptTrigger = true;
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
                anim.SetFloat(skillCastingTimeId, skillToCast.PlayerAnimCastTime);
            }
        }
        if (skillInterruptTrigger)
        {
            anim.ResetTrigger(skillCastTriggerId);
            anim.SetTrigger(skillInterruptTriggerId);
        }


    }

    void InitAnimate()
    {

        anim = GetComponent<Animator>();

        animPlayerStateId = Animator.StringToHash("PlayerState");
        animXDirId = Animator.StringToHash("xDir");
        animYDirId = Animator.StringToHash("yDir");
        skillCastTriggerId = Animator.StringToHash("Cast");
        skillCastingTimeId = Animator.StringToHash("SkillCastingTime");
        skillInterruptTriggerId = Animator.StringToHash("Interrupt");
    }
    #endregion

    void UpdateTrigger()
    {
        skillCastingEnterTrigger = false;
        skillInterruptTrigger = false;
        impactTrigger = false;
    }

    #region debug
#if DEBUG


#endif
    #endregion




}