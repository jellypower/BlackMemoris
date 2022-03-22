using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveDash : Skill
// 지정한 방향으로 TotalCastTime동안 DashSpeed의 속도로이동합니다.
{
    [Header("WaveDash/SkillStat")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dmgConst;
    [SerializeField] float impactConst;
    [SerializeField] float impactMagnitute;
    [SerializeField] float impactTime;

    float totalCastTimer = 0;

    IEnumerator castCoroutine;

    int animCastTriggerId;
    int animInterruptTriggerId;
    int animCastSpeedFloatId;

    Animator anim;

    Vector2 dashVector;

    LinkedList<HitBoxHandler> objsInrange;
    LinkedList<HitBoxHandler> objsAlreadyAttacked;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<Animator>();

        objsInrange = new LinkedList<HitBoxHandler>();
        objsAlreadyAttacked = new LinkedList<HitBoxHandler>();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        animCastTriggerId = Animator.StringToHash("Cast");
        animInterruptTriggerId = Animator.StringToHash("Interrupt");


        totalCastTime = Mathf.Clamp(totalCastTime, 1/60.0f,60);
        anim.SetFloat("CastSpeedMultiplier", 1/totalCastTime);
        


    }

    // Update is called once per frame
    void Update()
    {

        UpdateCooldown();
        UpdateCastDelayTime();
        UpdateCastTime();

    }

    IEnumerator Dash()
    {
        while (totalCastTimer > 0)
        {
            player.DashTowards(dashVector, dashSpeed);
            yield return null;
        }

        FinishCasting();
        
        yield return null;
    }

    public override void CastSkill()
    {

        transform.rotation = TransformUtils.getAngleTo(Vector2.zero, dashVector);

        totalCastTimer = totalCastTime;
        coolDownTimer = coolDownTime;

        dashVector = player.CastTargetPos - (Vector2)player.transform.position;
        dashVector = dashVector.normalized;

        anim.ResetTrigger(animInterruptTriggerId);
        anim.SetTrigger(animCastTriggerId);

        attackObjsInRange();

        castCoroutine = Dash();
        StartCoroutine(castCoroutine);

        isCasting = true;

    }



    public override void InterruptCasting()
    {
        objsAlreadyAttacked.Clear();

        anim.SetTrigger(animInterruptTriggerId);
        anim.ResetTrigger(animCastTriggerId);

        StopCoroutine(castCoroutine);
        isCasting = false;
    }

    protected override void FinishCasting()
    {
        anim.ResetTrigger(animCastTriggerId);

        objsAlreadyAttacked.Clear();


        isCasting = false;

    }


    public override SkillState GetState()
    {

        if (isCasting)
            return SkillState.Casting;
        else if (coolDownTimer > 0)
            return SkillState.CoolDown;
        else
            return SkillState.Castable;
    }

    void UpdateCastTime()
    {
        totalCastTimer = Mathf.Clamp(totalCastTimer - Time.deltaTime, 0, float.MaxValue);
    }



    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HitBoxHandler victim = collision.GetComponent<HitBoxHandler>();

        if (victim == null) return;
        
        if (isCasting && !objsAlreadyAttacked.Contains(victim))
        {
            Attack(victim);
            objsAlreadyAttacked.AddLast(victim);
        }
        else if (!(objsInrange.Contains(victim) || objsAlreadyAttacked.Contains(victim)))
        {
            objsInrange.AddLast(victim);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        HitBoxHandler victim = collision.GetComponent<HitBoxHandler>();

        objsInrange.Remove(victim);
    }


    void Attack(HitBoxHandler target)
    {
        target.GetAttack(dmgConst, player);

        Vector2 pushVector = (target.transform.position - player.transform.position).normalized;

        target.GetImpact(impactConst, impactTime, pushVector * GameConst.ForceAdjuster * impactMagnitute, player);
    }

    void attackObjsInRange()
    {
        foreach (var obj in objsInrange)
        {
            Attack(obj);
            objsAlreadyAttacked.AddLast(obj);
        }

        objsInrange.Clear();
    }
}
