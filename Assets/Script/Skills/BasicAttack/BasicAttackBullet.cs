using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackBullet : Projectile
{

	GameObject SpawnerObj;
	Skill spawner;
	

	GameObject targetObj;
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
		

		if (SpawnerObj != null)
		{
			transform.position = SpawnerObj.transform.position;

			if (targetObj != null) {
				targetTransform = targetObj.transform;

				transform.rotation = SkillUtils.getAngleTo(transform.position, targetTransform.position);
			}
		}

		if (targetObj == null)
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

		if (Mathf.Approximately(Vector2.Distance(targetObj.transform.position, transform.position), 0) && !getToTarget)
		{
			anim.SetTrigger(explodeAnimId);
			getToTarget = true;
			AttackTarget(damage, target);
		}

		if (!getToTarget)
		{
			transform.position = Vector2.MoveTowards(transform.position, targetTransform.position, speed * Time.deltaTime);

			transform.rotation = SkillUtils.RotateTo(transform.rotation,
			transform.position, targetTransform.position, Time.deltaTime * rotateSpeed);
		}

		
	}
	
	
	public void ObjectDisablebyAnim()
	{
		transform.position = SpawnerObj.transform.position;
		gameObject.SetActive(false);
		
	}

	public void SetProjectileProperty(GameObject targetObj, Character target, float speed, float damage)
	{
		this.targetObj = targetObj;
		this.target = target;
		this.speed = speed;
		this.damage = damage;

	}

	public override void SetSpawner(GameObject SpawnerObj)
	{
		this.SpawnerObj = SpawnerObj;
		spawner = SpawnerObj.GetComponent<BasicAttackManager>();

	}

	void AttackTarget(float dmg, Character target)
    {

		target.GetAttack(dmg, spawner.Player);
		// target에서 hitbox를 치면 hitbox에는 character 클래스가 없어서 오류가 뜬다.
    }


}
