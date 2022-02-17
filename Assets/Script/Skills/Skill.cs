using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Skill : MonoBehaviour
{
    protected PlayerMotor player;
    public PlayerMotor Player { get { return player; } }

    protected PlayerStat playerStat;


    [SerializeField] protected float defaultCoolDown;
    public float DefaultCoolDown { get { return defaultCoolDown; } }

    protected float coolDownTimer;
    public float CoolDownTimer { get { return coolDownTimer; } }


    [SerializeField] protected SkillFuncType skillType;
    [SerializeField] protected SkillCastType castType;
    public SkillCastType CastType
    {
        get { return castType; }
    }


    [SerializeField] protected float playerAnimCastTime; // 플레이어가 해당 스킬을 캐스트 하는데 걸리는 시간
    public virtual float PlayerAnimCastTime { get { return playerAnimCastTime; } }



    [SerializeField] protected float castDelay; // 플레이어가 cast 하는 동안 입력 방지 시간 == 선딜레이
    public virtual float CastDelay { get { return castDelay; } }
    protected float castDelayTimer;
    public float CastDelayTimer { get { return castDelayTimer; } }



    
    protected bool isCasting = false; // 스킬이 캐스팅 되고있으면 true인 실시간 updqte 변수

    protected Animator Anim;
    protected int animCastTriggerId;
    protected int interruptTriggerId;



    [SerializeField]protected float rangeCoeff;
    [SerializeField]protected float basicRangeConst;

    protected LayerMask CharacterMask;
    public virtual float Range { get
        {
            return basicRangeConst + rangeCoeff * player.Stat.range;
        }
    }

    protected virtual void Awake()
    {
        GetDependentComponents();

        awakeAnimator();

        CharacterMask = LayerMask.GetMask("Character");

    }

    protected virtual void Start()
    {
        playerStat = player.Stat;

    }

    protected virtual void GetDependentComponents()
    {
        player = GetComponentInParent<PlayerMotor>();
        if (player == null) throw new Exception("PlayerMotor not found");

        Anim = GetComponent<Animator>();
        if (Anim == null) throw new Exception("Animator not found");

    }

    void awakeAnimator()
    {
        animCastTriggerId = Animator.StringToHash("cast");
        interruptTriggerId = Animator.StringToHash("interrupt");
    }

    public virtual void InterruptCasting()
    {
        Anim.ResetTrigger(animCastTriggerId);
        Anim.SetTrigger(interruptTriggerId);
        isCasting = false;

        

    }

    public virtual void finishSkill()
    {

        isCasting = false;
    }

    protected void UpdateCooldown()
    {
        if (coolDownTimer > 0)
        {
            coolDownTimer -= Time.deltaTime;
        }
    }

    protected void UpdateFreezeTime()
    {
        if (castDelayTimer > 0)
        {
            castDelayTimer -= Time.deltaTime;
        }
    }



    public bool isFreezingPlayer()
    {
        return castDelayTimer > 0.0f;
    }

    public virtual void CastSkill()
    {
        Anim.ResetTrigger(interruptTriggerId);
        Anim.SetTrigger(animCastTriggerId);
        isCasting = true;
        castDelayTimer = castDelay;
    }

    public abstract SkillState getState();

}

