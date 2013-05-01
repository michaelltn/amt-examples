using UnityEngine;
using System.Collections;

public class FusionWeapon : ProjectileWeapon
{
}
	/*
	public FusionMissile ammo;
	private FusionMissile newMissile;
	
	override public void fire(Transform barrel) {
		if (ammo) {
			float timeBetweenShots = WeaponUpgrades.FusionMissileTimeBetweenShots(getLevel());
			if (Time.time - previousShotTime >= timeBetweenShots) {
				newMissile = Instantiate(ammo, barrel.position, barrel.rotation ) as FusionMissile;
				newMissile.assignSourceTransform(shipTransform);
				newMissile.assignSourceWeapon(this);
				newMissile.projectileLevel = this.getLevel();
				newMissile.init();
				previousShotTime = Time.time;
			}
		}
	}
	
	override public void stopFiring() {}
	*/
	
