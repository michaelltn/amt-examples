using UnityEngine;
using System.Collections;

public class Carrier2 : EnemyAI
{
	
	protected override void init ()
	{
		base.init ();
	}
	

	
	protected override void manageWeapons()
	{
		base.manageWeapons ();
	}
	
	
	
	protected override void collisionResponse (Collision collision)
	{
		base.collisionResponse (collision);
	}
	
	public override void playerCollisionResponse ()
	{
		base.playerCollisionResponse ();
	}
	
	protected override int getAvoidancePriority (Collider collider)
	{
		return base.getAvoidancePriority (collider);
	}
	
	protected override void hitResponse (float damageTaken)
	{
		base.hitResponse (damageTaken);
	}
}