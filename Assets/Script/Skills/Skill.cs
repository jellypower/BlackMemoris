using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Skill : MonoBehaviour
{


    protected PlayerController player;
    public PlayerController Player { get { return player; } }

    protected PlayerStat playerStat;

    [Header("Skill Stats")]
    [SerializeField] protected float defaultCoolDown;
    protected float coolDownTimer;


    public SkillState State { get; protected set;}
  



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





    [SerializeField]protected float rangeCoeff;
    [SerializeField]protected float basicRangeConst;

    

    protected LayerMask CharacterMask;
    public virtual float Range { get
        {
            return basicRangeConst + rangeCoeff * player.Stat.Range.Value;
        }
    }
    
    protected virtual void Awake()
    {
        GetDependentComponents();
        CharacterMask = LayerMask.GetMask("Character");

    }

    protected virtual void Start()
    {
        playerStat = player.Stat;

    }

    protected virtual void GetDependentComponents()
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null) throw new Exception("PlayerMotor not found");


    }


    public abstract void CastSkill(); // 나중에 bool형으로 바꿔서 cast 가능한지 상태를 넘겨주게 하자.
    public abstract void InterruptCasting();




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

    
    protected abstract void updateState();

}

