using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBombBullet : Projectile
{

	[SerializeField] int defaultImpact;
	[SerializeField] float impactMagnitute;
	[SerializeField] float explosionRange;

	
	Character target;
	Transform targetTransform;

	Animator anim;
	int explodeAnimId, castAnimId;

	bool getToTarget;

	public bool IsCasted { get; private set; } = false;


    private void Awake()
    {
		anim = GetComponent<Animator>();
		explodeAnimId = Animator.StringToHash("Explode");
		castAnimId = Animator.StringToHash("Cast");
	}


	private void OnEnable()
	{


		if (spawner != null)
		{
			transform.position = spawner.transform.position;

			if (target != null)
			{
				targetTransform = target.transform;

				transform.rotation = TransformUtils.getAngleTo(transform.position, targetTransform.position);
			}
		}

		if (target == null)
		{
			gameObject.SetActive(false);
			return;
		}
		else if (speed == 0)
		{
			gameObject.SetActive(false);
			return;
		}

		getToTarget = false;
	}

	void Update()
	{


		if (Vector2.Distance(target.transform.position, transform.position) < .1f && !getToTarget)
		{
			anim.SetTrigger(explodeAnimId);
			getToTarget = true;
			ExplosionAttack();
			IsCasted = false;
		}

		if (!getToTarget && IsCasted)
		{
			transform.position = Vector2.MoveTowards(transform.position, targetTransform.position, speed * Time.deltaTime);

			transform.rotation = TransformUtils.RotateTo(transform.rotation,
			transform.position, targetTransform.position, Time.deltaTime * rotateSpeed);
		}


	}

	void ExplosionAttack()
    {
		Collider2D[] colls = 
		Physics2D.OverlapCircleAll(
			transform.position,
			explosionRange,
			GameConst.instance.CharacterLayerMask);

		foreach(var coll in colls)
        {
			Character c = coll.gameObject.GetComponent<Character>();
			AttackTarget(damage, c);
			ImpactTarget(defaultImpact, c);
        }
		
    }

	public void ObjectDisablebyAnim()
	{
		transform.parent = spawner.transform;
		transform.position = spawner.transform.position;
		gameObject.SetActive(false);

	}

	public void CastBullet(Character target, float speed, float damage)
	{
		this.target = target;
		this.speed = speed;
		this.damage = damage;
		transform.parent = null;
		gameObject.SetActive(true);
	}

	public void InterruptCasting()
    {
		this.target = null;
		this.speed = 0;
		this.damage = 0;
		transform.parent = null;
		gameObject.SetActive(false);
    }

	public override void SetSpawner(Skill spawner)
	{
		this.spawner = spawner;

	}

	void AttackTarget(float dmg, Character target)
	{

		target.GetAttack(dmg, spawner.Player);
		// target에서 hitbox를 치면 hitbox에는 character 클래스가 없어서 오류가 뜬다.
	}

	void ImpactTarget(int impact, Character target)
	{

		Vector2 force =
			(target.transform.position - transform.position)
			.normalized
			* GameConst.ForceAdjuster
			* impactMagnitute;


		target.GetImpact(impact, 1.0f, force, target);
	}


	public void CastTrigger()
    {
		IsCasted = true;
		anim.SetTrigger(castAnimId);
    }


	#region debug functions
#if DEBUG

	Color color0 = new Color(0, 1, 0, 0.5f);
	void OnDrawGizmos()
	{
		Gizmos.color = color0;
		Gizmos.DrawWireSphere(transform.position, explosionRange);
	}


#endif
	#endregion
}

