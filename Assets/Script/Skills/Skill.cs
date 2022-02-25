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


    [SerializeField] protected float playerAnimCastTime; // �÷��̾ �ش� ��ų�� ĳ��Ʈ �ϴµ� �ɸ��� �ð�
    public virtual float PlayerAnimCastTime { get { return playerAnimCastTime; } }



    [SerializeField] protected float castDelay; // �÷��̾ cast �ϴ� ���� �Է� ���� �ð� == ��������
    public virtual float CastDelay { get { return castDelay; } }
    protected float castDelayTimer;
    public float CastDelayTimer { get { return castDelayTimer; } }



    
    protected bool isCasting = false; // ��ų�� ĳ���� �ǰ������� true�� �ǽð� updqte ����





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


    public abstract void CastSkill(); // ���߿� bool������ �ٲ㼭 cast �������� ���¸� �Ѱ��ְ� ����.
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

