using UnityEngine;
using System.Collections;

public class HypometricBuster : HypometricBullet
{
	public HypometricBullet smallShotPrefab;
	private HypometricBullet newBullet;
	
	public int numberOfSmallShots = 6;
	private float offset;
	
	override protected void hitEnemy(Enemy e)
	{
		for (int i = 0; i < numberOfSmallShots; i++)
		{
			newBullet = Instantiate(smallShotPrefab, this.transform.position, this.transform.rotation ) as HypometricBullet;
			newBullet.assignSourceWeapon(this.getSourceWeapon());
			newBullet.setDamageMultiplier(this.damageMultiplier);
			
			offset = (i * 360.0f/numberOfSmallShots);
			newBullet.transform.Rotate(0, 0, offset);
			
			newBullet.init();
			
		}
		base.hitEnemy(e);
	}
}
