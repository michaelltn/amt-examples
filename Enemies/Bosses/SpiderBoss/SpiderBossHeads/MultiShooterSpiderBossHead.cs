using UnityEngine;
using System.Collections;

public class MultiShooterSpiderBossHead : SpiderBossHead
{
	public string activeAnim = "multishooter head active";
	
	
	public EnemyWeapon[] weapons;
	
	
	protected override void init ()
	{
		
	}
	
	
	protected override void activeUpdate ()
	{
		if (!animator.IsPlaying(activeAnim))
		{
			animator.CrossFade(activeAnim);
		}
		
		foreach (EnemyWeapon w in weapons)
		{
			w.fire(w.transform);
		}
	}
	
	protected override void inactiveUpdate ()
	{
		// do nothing
	}
	
}
