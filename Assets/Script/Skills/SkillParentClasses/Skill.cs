using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Skill : MonoBehaviour
{


    protected PlayerController player; // ��ų�� cast�ϴ� �÷��̾� ����
    public PlayerController Player { get { return player; } }

    protected PlayerStat playerStat; // �÷��̾��� ���� ����



    [Header("Skill Info")] // ��ų�� �⺻ ����

    public GameObject IndicatorPrefab;
    public Sprite SkillIcon;

  
    [SerializeField] protected SkillCastType castType; 
    public SkillCastType CastType{ get { return castType; }}
    // Ÿ����, ��Ÿ����, ���, �нú� �� � ������ ��ų���� �˷���.
    // castType�� ���� ���� ��ų�� ��� ����ؾ� �ϴ����� ���� hint�� �־���.

    [SerializeField] protected AnimatorOverrideController playerCastAnimator;
    [HideInInspector] public AnimatorOverrideController PlayerCastAnimator {get { return playerCastAnimator; }}



    protected bool isCasting = false; // ��ų�� ĳ���� �ǰ������� true


    [Header("Skill Stat")] // ���� ��ų cast �ÿ� ���Ǵ� ����
    [SerializeField] protected float coolDownTime;
    protected float coolDownTimer;

    [SerializeField] protected float totalCastTime; // �÷��̾ �ش� ��ų�� ĳ��Ʈ �ϴµ� �ɸ��� �ð� -> �÷��̾��� ĳ���� �ִϸ��̼��� �󸶳� �ɸ��� ������
    public virtual float TotalCastTime { get { return totalCastTime; } }

    [SerializeField] protected float castDelay; // �÷��̾ cast �ϴ� ���� �Է����� ���� ��ų�� interrupt �Ǵ°� ���� ���� �ð� == ��������
    public virtual float CastDelay { get { return castDelay; } }


    protected float castDelayTimer; // castDelay �ð��� �󸶳� ���Ҵ��� �����ϴ� Ÿ�̸�
    public float CastDelayTimer { get { return castDelayTimer; } }








    #region init functions
    protected virtual void Awake()
    {
        GetDependentComponents();

    }

    protected virtual void Start()
    {
        playerStat = player.Stat;

    }

    protected virtual void GetDependentComponents() // �ʿ��� GetComponent���� ���ִ� �Լ�.
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null) throw new Exception("PlayerMotor not found");


    }
    #endregion



    #region skill cast manage function
    public abstract void CastSkill(); // ���߿� bool������ �ٲ㼭 cast �������� ���¸� �Ѱ��ְ� ����.
    public abstract void InterruptCasting();
    protected abstract void FinishCasting();
    #endregion


    public bool OnCastDelay()
    {
        return castDelayTimer > 0.0f;
    }


    protected void UpdateCooldown()
    {
        coolDownTimer = Mathf.Clamp(coolDownTimer - Time.deltaTime, 0, float.MaxValue);
    }

    protected void UpdateCastDelayTime()
    {
        castDelayTimer = Mathf.Clamp(castDelayTimer - Time.deltaTime, 0, float.MaxValue);
    }

    public abstract SkillState GetState();
    //������ state ������ �ΰ� Update �Լ����� state�� �������� �ʰ� �Լ��� �̿��ϴ� ������
    //�ٸ� ��ũ��Ʈ���� �ش� ������ �̿��� ��,
    //�ٸ� ��ũ��Ʈ�� ��ų�� ���¸� ��ȭ��Ų ���Ŀ� ��ȭ�� ���¸� �̿��Ϸ� �� �� �ֱ� �����̴�.

}

