using UnityEngine;
using System.Collections;

[RequireComponent (typeof(EnemyWeapon))]
public class ProjectileShooterSpiderBossHead : SpiderBossHead
{
	public string recoilAnim = "shooter head recoil";
	public string activeAnim = "shooter head active";
	
	
	private EnemyWeapon weapon;
	public Transform barrelTransform;
	
	
	protected override void init ()
	{
		weapon = this.gameObject.GetComponent<EnemyWeapon>();
	}
	
	
	protected override void activeUpdate ()
	{
		if (PlayerShip.MainPositionTransform)
		{
			this.turnToward(PlayerShip.MainPositionTransform.position);
			
			if (weapon.fire(barrelTransform))
			{
				if (animator)
					animator.CrossFade(recoilAnim);
			}
			else
			{
				if (!animator.isPlaying)
				{
					animator.CrossFade(activeAnim);
				}
			}
		}
	}
	
	protected override void inactiveUpdate ()
	{
		// do nothing
	}
	
}
