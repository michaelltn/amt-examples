using UnityEngine;
using System.Collections;

/*
 * I shouldn't need to override the base functions as the state for 
 * the pack should always be "follow", but for now I've left them in
 * until I have time to test this theory.
 * */

public class Pack : Shooter1
{
	public static Pack leader;
	
	[System.NonSerialized]
	public Vector2 offsetFromLeader;
	public float catchupSpeed = 1.25f;
	//private float speedBoost = 1f;
	private Vector3 packTargetLocation;
	private Vector3 packTargetDirection;
	private float currentPackTargetDistance;
	private float previousPackTargetDistance;
	private static float offsetDist = 10f;

	
	private static void arrangePack()
	{
		// triangle
		int curRow = 1;
		int rowNum = 1;
		foreach (Enemy e in Enemy.EnemyList())
		{
			Pack p = e.gameObject.GetComponent<Pack>();
			if (p && p != Pack.leader)
			{
				p.offsetFromLeader.y = curRow * -offsetDist;
				p.offsetFromLeader.x = (rowNum-1) * offsetDist;
				p.offsetFromLeader.x -= (curRow-1) * offsetDist / 2;
				
				rowNum++;
				if (rowNum > curRow+1)
				{
					rowNum = 1;
					curRow++;
				}
			}
		} // end foreach
	}
	
	
	override protected void init()
	{
		base.init();
		
		this.addState("follow", followMovement);
		this.setState("follow");
		
		if (!Pack.leader)
		{
			Pack.leader = this;
		}
		else
		{
			arrangePack();
			currentPackTargetDistance = previousPackTargetDistance = 1000f;
		}
	}
	
	override protected void manageWeapons()
	{
		if (Vector3.Dot(this.transform.right, playerDirection) > 0)
		{
			//weapon.targetTransform = PlayerShip.playerTransform;
			weapon.fire(weapon.transform);
		}
	}
	
	override protected void chargeMovement()
	{
		if (Pack.leader == this)
			base.chargeMovement();
		else
			followMovement();
	}
	override protected void dodgeMovement()
	{
		if (Pack.leader == this)
			base.dodgeMovement();
		else
			followMovement();
	}
	override protected void retreatMovement()
	{
		if (Pack.leader == this)
			base.retreatMovement();
		else
			followMovement();
	}
	
	//override protected void defaultMovement()
	protected void followMovement()
	{
		if (!Pack.leader)
		{
			Pack.leader = this;
			//currentState = EnemyAI.AIState.Charging;
			arrangePack();
		}
		
		if (Pack.leader == this)
		{
			base.chargeMovement();
			Debug.DrawLine(this.transform.position + new Vector3(3, 3, 0), this.transform.position + new Vector3(-3, -3, 0), Color.cyan);
			Debug.DrawLine(this.transform.position + new Vector3(-3, 3, 0), this.transform.position + new Vector3(3, -3, 0), Color.cyan);
		}
		else
		{
			packTargetLocation = Pack.leader.transform.position;
			packTargetLocation += Pack.leader.transform.right * offsetFromLeader.y;
			packTargetLocation -= Pack.leader.transform.up * offsetFromLeader.x;
			Debug.DrawLine(packTargetLocation + new Vector3(3, 3, 0), packTargetLocation + new Vector3(-3, -3, 0), Color.yellow);
			Debug.DrawLine(packTargetLocation + new Vector3(-3, 3, 0), packTargetLocation + new Vector3(3, -3, 0), Color.yellow);
			
			previousPackTargetDistance = currentPackTargetDistance;
			currentPackTargetDistance = Vector3.Distance(this.transform.position, packTargetLocation);
			packTargetDirection = packTargetLocation - this.transform.position;
			
			if (Vector3.Dot(packTargetDirection, this.transform.right) < 0 &&
				currentPackTargetDistance < previousPackTargetDistance)
			{
				enemyMotor.turnAway(packTargetLocation);
				enemyMotor.moveForward(0.5f);
			}
			else
			{
				enemyMotor.turnToward(packTargetLocation);
				
				if (currentPackTargetDistance > (enemyMotor.movementSpeed * catchupSpeed * Time.deltaTime))
					enemyMotor.moveForward(1.25f);
				else
					enemyMotor.moveForward(1f);
			}
			enemyMotor.stopStrafeMovement();
		}
	}
	
} // end of class