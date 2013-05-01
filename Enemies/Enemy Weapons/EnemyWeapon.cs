using UnityEngine;
using System.Collections;

//[RequireComponent (typeof (Turret))]

public class EnemyWeapon : MonoBehaviour
{
	public static bool FreezeAll = false;
	
	public EnemyShot ammo;
	private EnemyShot newShot;
	
	public float timeBetweenShots = 1f;
	protected float previousShotTime;
	
	public Transform shipTransform;
	
	private Transform targetTransform;
	public Transform getTargetTransform() { return targetTransform; }
	public void assignTargetTransform(Transform t) { targetTransform = t; }
	
	// Use this for initialization
	void Start ()
	{
		previousShotTime = Time.time;
		if (!shipTransform)
			shipTransform = this.transform;
	}
	
	public bool fire(Transform barrel)
	{
		if (!FreezeAll && ammo != null)
		{
			if (Time.time - previousShotTime >= timeBetweenShots)
			{
				newShot = Instantiate(ammo, barrel.position, barrel.rotation ) as EnemyShot;
				newShot.assignSourceTransform(shipTransform);
				newShot.assignTargetTransform(targetTransform);
				previousShotTime = Time.time;
				return true;
			}
		}
		return false;
	}
	
} // end of class