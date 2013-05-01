using UnityEngine;
using System.Collections;

public class HypometricWeapon : ProjectileWeapon
{

}

	/*
	public BulletPool hypometricBulletPool;
	//public HypometricBullet ammo;
	private GameObject newGameObject;
	private HypometricBullet newBullet;
	
	private int bulletCount;
	private float zRotation;
	private Vector3 newPosition;
	
	public AudioClip shotSound;
	
	override public void init() {
		hypometricBulletPool = HMBulletPool.Pool;
	}
	
	override public void fire(Transform barrel) {
		if (hypometricBulletPool) {
			float timeBetweenShots = WeaponUpgrades.HypometricTimeBetweenShots(getLevel());
			if (Time.time - previousShotTime >= timeBetweenShots) {
				bulletCount = WeaponUpgrades.HypometricNumberOfShots(level);
				for (int b = 0; b < bulletCount; b++) {
					newGameObject = hypometricBulletPool.getBullet(barrel.position, barrel.rotation);
					if (newGameObject)
						newBullet = newGameObject.GetComponent<HypometricBullet>();
					else
						newBullet = null;
					
					if (newBullet) {
						newBullet.pool = hypometricBulletPool;
						newBullet.assignSourceTransform(shipTransform);
						newBullet.assignSourceWeapon(this);
						newBullet.projectileLevel = this.getLevel();
						newBullet.updateMaterial();
						
						// this part is totally hacked.  fix it you lazy slob!!
						zRotation = ((b + 1) * WeaponUpgrades.HypometricAngleBetweenShots()) / 2;
						if (bulletCount > 1)
							zRotation -= (bulletCount* WeaponUpgrades.HypometricAngleBetweenShots()) / 3;
						else
							zRotation -= (bulletCount* WeaponUpgrades.HypometricAngleBetweenShots()) / 2;
						newBullet.transform.Rotate(0, 0, zRotation);
						// move the stupid thing forward a little so it doesn't look dumb
						newPosition = newBullet.transform.position + (newBullet.transform.right * 2.0f);
						newPosition.z = 0;
						newBullet.transform.position = newPosition;
						
						if (audioSource && shotSound) {
							audioSource.Stop();
							audioSource.PlayOneShot(shotSound);
						}
						
						previousShotTime = Time.time;
					}
					else
						Debug.Log("HM Bullet could not be acquired.");
				}
			}
		}
		else {
			Debug.Log("No bullet pool.");
		}
	}
	
	override public void stopFiring() {}
	*/
	
