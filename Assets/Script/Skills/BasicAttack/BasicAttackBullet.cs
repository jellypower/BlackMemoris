using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackBullet : Projectile
{

	[SerializeField] int defaultImpact;
	[SerializeField] float impactMagnitute;

	Character target;
	Transform targetTransform;

	Animator anim;
	int explodeAnimId;

	bool getToTarget;

	private void Start()
	{
		
		anim = GetComponent<Animator>();
		explodeAnimId = Animator.StringToHash("explode");
	}


	private void OnEnable()
	{
		transform.parent = null;

		if (spawner != null)
		{
			transform.position = spawner.transform.position;

			if (target != null) {
				targetTransform = target.transform;

				transform.rotation = TransformUtils.getAngleTo(transform.position, targetTransform.position);
			}
		}

		if (target == null)
		{
			gameObject.SetActive(false);
			return;
		}
		else if(speed == 0)
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
			AttackTarget(damage, target);
			ImpactTarget(defaultImpact , target);
		}

		if (!getToTarget)
		{
			transform.position = Vector2.MoveTowards(transform.position, targetTransform.position, speed * Time.deltaTime);

			transform.rotation = TransformUtils.RotateTo(transform.rotation,
			transform.position, targetTransform.position, Time.deltaTime * rotateSpeed);
		}

		
	}
	
	
	public void ObjectDisablebyAnim()
	{
		transform.parent = spawner.transform;
		transform.position = spawner.transform.position;
		gameObject.SetActive(false);
		
	}

	public void SetProjectileProperty(Character target, float speed, float damage)
	{
		this.target = target;
		this.speed = speed;
		this.damage = damage;

	}

	public override void SetSpawner(Skill Spawner)
	{

		spawner = Spawner;

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
			*GameConst.ForceAdjuster
			* impactMagnitute;

		
		target.GetImpact(impact , 1.0f,force , target);
    }


}
