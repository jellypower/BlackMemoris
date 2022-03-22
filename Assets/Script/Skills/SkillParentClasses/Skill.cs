using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Skill : MonoBehaviour
{


    protected PlayerController player; // 스킬을 cast하는 플레이어 저장
    public PlayerController Player { get { return player; } }

    protected PlayerStat playerStat; // 플레이어의 스탯 저장



    [Header("Skill Info")] // 스킬의 기본 정보

    public GameObject IndicatorPrefab;
    public Sprite SkillIcon;

  
    [SerializeField] protected SkillCastType castType; 
    public SkillCastType CastType{ get { return castType; }}
    // 타게팅, 논타게팅, 토글, 패시브 등 어떤 유형의 스킬인지 알려줌.
    // castType을 통해 실제 스킬을 어떻게 사용해야 하는지에 대한 hint가 주어짐.

    [SerializeField] protected AnimatorOverrideController playerCastAnimator;
    [HideInInspector] public AnimatorOverrideController PlayerCastAnimator {get { return playerCastAnimator; }}



    protected bool isCasting = false; // 스킬이 캐스팅 되고있으면 true


    [Header("Skill Stat")] // 실제 스킬 cast 시에 사용되는 정보
    [SerializeField] protected float coolDownTime;
    protected float coolDownTimer;

    [SerializeField] protected float totalCastTime; // 플레이어가 해당 스킬을 캐스트 하는데 걸리는 시간 -> 플레이어의 캐스팅 애니메이션이 얼마나 걸릴지 결정함
    public virtual float TotalCastTime { get { return totalCastTime; } }

    [SerializeField] protected float castDelay; // 플레이어가 cast 하는 동안 입력으로 인해 스킬이 interrupt 되는걸 방진 방지 시간 == 선딜레이
    public virtual float CastDelay { get { return castDelay; } }


    protected float castDelayTimer; // castDelay 시간이 얼마나 남았는지 측정하는 타이머
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

    protected virtual void GetDependentComponents() // 필요한 GetComponent들이 모여있는 함수.
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null) throw new Exception("PlayerMotor not found");


    }
    #endregion



    #region skill cast manage function
    public abstract void CastSkill(); // 나중에 bool형으로 바꿔서 cast 가능한지 상태를 넘겨주게 하자.
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
    //별도의 state 변수를 두고 Update 함수에서 state를 추적하지 않고 함수를 이용하는 이유는
    //다른 스크립트에서 해당 변수를 이용할 때,
    //다른 스크립트가 스킬의 상태를 변화시킨 직후에 변화한 상태를 이용하려 할 수 있기 때문이다.

}

