using UnityEngine;
using System.Collections;

public class WeaponUpgrades : MonoBehaviour {
	/*
	private static int maxLevel = 2;
	private static float sellingValue = 1.0f;
	
	// Generic
	private static int purchaseCost;
	private static int purchasePenalty;
	private static int[] upgradeCost;
	
	// Hypometric Upgrades
	private static float[] hmTimeBetweenShots;
	private static float[] hmDamage;
	private static int[] hmNumberOfShots;
	private static float hmAngleBetweenShots;
	
	// Superluminal Upgrades
	private static float[] slOnTime;
	private static float[] slOffTime;
	private static float[] slDPS;
	private static int[] slPenetration;
	
	// Fusion Upgrades
	private static float[] fmTimeBetweenShots;
	private static float[] fmImpactDamage;
	private static float[] fmBlastRadius;
	private static float[] fmBlastDPS;
	private static float[] fmBlastDuration;
	private static float[] fmHeatSeekingAngle;
	private static float[] fmMaxDistance;
	
	// Nanotech Upgrades
	private static float ntSlowTime;
	private static float ntSlowFactor;
	private static float[] ntParticleRange;
	
	
	static WeaponUpgrades() {
		upgradeCost = new int[maxLevel];
		
		hmTimeBetweenShots = new float[maxLevel+1];
		hmDamage = new float[maxLevel+1];
		hmNumberOfShots = new int[maxLevel+1];
		
		ntParticleRange = new float[maxLevel+1];
		
		slOnTime = new float[maxLevel+1];
		slOffTime = new float[maxLevel+1];
		slDPS = new float[maxLevel+1];
		slPenetration = new int[maxLevel+1];
		
		fmTimeBetweenShots = new float[maxLevel+1];
		fmImpactDamage = new float[maxLevel+1];
		fmBlastRadius = new float[maxLevel+1];
		fmBlastDPS = new float[maxLevel+1];
		fmBlastDuration = new float[maxLevel+1];
		fmHeatSeekingAngle = new float[maxLevel+1];
		fmMaxDistance = new float[maxLevel+1];
		
		initializeUpgrades();
	}
	
	/// <summary>
	/// Initialization
	/// </summary>
	private static void initializeUpgrades() {
		purchaseCost = 100;
		purchasePenalty = 25;
		upgradeCost[0] = 300;
		upgradeCost[1] = 600;
		
		// HYPOMETRIC
		hmTimeBetweenShots[0] = 0.2f;
		hmTimeBetweenShots[1] = 0.1f;
		hmTimeBetweenShots[2] = 0.1f;
		hmDamage[0] = 12; //10f;
		hmDamage[1] = 25; //20f;
		hmDamage[2] = 25; //20f;
		hmNumberOfShots[0] = 1;
		hmNumberOfShots[1] = 1;
		hmNumberOfShots[2] = 3;
		hmAngleBetweenShots = 20.0f;
		
		// SUPERLUMINAL
		slOnTime[0] =  0.125f;  slOffTime[0] = 0.375f;
		slOnTime[1] =  0.375f;  slOffTime[1] = 0.125f;
		slOnTime[2] =  1f;  slOffTime[2] = 0f;
		slDPS[0] = 200f;
		slDPS[1] = 200f;
		slDPS[2] = 200f;
		slPenetration[0] = 0;
		slPenetration[1] = 0;
		slPenetration[2] = 2;
		
		// FUSION MISSILES
		fmTimeBetweenShots[0] = 1.00f;
		fmTimeBetweenShots[1] = 1.00f;
		fmTimeBetweenShots[2] = 1.00f;
		fmImpactDamage[0] = 20f;
		fmImpactDamage[1] = 20f;
		fmImpactDamage[2] = 20f;
		fmBlastDPS[0] = 200f;
		fmBlastDPS[1] = 200f;
		fmBlastDPS[2] = 200f;
		fmBlastRadius[0] = 5f;
		fmBlastRadius[1] = 10f;
		fmBlastRadius[2] = 15f;
		fmBlastDuration[0] = 0.2f;
		fmBlastDuration[1] = 0.4f;
		fmBlastDuration[2] = 0.6f;
		fmHeatSeekingAngle[0] = 30f;
		fmHeatSeekingAngle[1] = 30f;
		fmHeatSeekingAngle[2] = 90f;
		fmMaxDistance[0] =  75f;
		fmMaxDistance[1] =  75f;
		fmMaxDistance[2] =  150f;
		
		// NANOTECH
		ntSlowTime = 5f;
		ntSlowFactor = 0.5f;
		ntParticleRange[0] = 15f;
		ntParticleRange[1] = 20f;
		ntParticleRange[2] = 25f;

	}

	
	// GENERIC WEAPONS
	public static int MaxLevel() { return maxLevel; }
	//public static int PurchaseCost() { return purchaseCost + (purchasePenalty * Turret.TurretList().Length); }
	public static int PurchaseCost(Weapon weaponType) {
		return purchaseCost + (purchasePenalty * Turret.WeaponCount(weaponType));
	}
	public static int UpgradeCost(int level) {
		return (level < 0 || level >= maxLevel) ? -1 : upgradeCost[level];
	}
	public static int UpgradeCost(Weapon weapon) {
		return UpgradeCost(weapon.getLevel());
	}
	public static int SellingPrice(int level) {
		if (level < 0 && level > maxLevel) {
			return -1;
		}
		else {
			int weaponValue = purchaseCost;
			for (int i = 0; i < level; i++) {
				weaponValue += upgradeCost[i];
			}
			return Mathf.FloorToInt(sellingValue * weaponValue);
		}
	}
	public static int SellingPrice(Weapon weapon) {
		return SellingPrice(weapon.getLevel());
	}
	
	// HYPOMETRIC
	public static float HypometricTimeBetweenShots(int level) { return hmTimeBetweenShots[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float HypometricDamage(int level)           { return hmDamage[Mathf.Clamp(level, 0, maxLevel)]; }
	public static int HypometricNumberOfShots(int level)      { return hmNumberOfShots[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float HypometricAngleBetweenShots()         {	return hmAngleBetweenShots; }

	// SUPERLUMINAL
	public static float SuperluminalOnTime(int level)          { return slOnTime[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float SuperluminalOffTime(int level)         { return slOffTime[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float SuperluminalDamage(int level, float t) { return t * slDPS[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float SuperluminalDPS(int level)             { return slDPS[Mathf.Clamp(level, 0, maxLevel)]; }
	public static int   SuperluminalPenetration(int level)     { return slPenetration[Mathf.Clamp(level, 0, maxLevel)]; }

	// FUSION MISSILES
	public static float FusionMissileTimeBetweenShots(int level) { return fmTimeBetweenShots[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float FusionMissileImpactDamage(int level)     { return fmImpactDamage[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float FusionMissileBlastRadius(int level)      { return fmBlastRadius[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float FusionMissileBlastDPS(int level)         { return fmBlastDPS[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float FusionMissileBlastDuration(int level)    { return fmBlastDuration[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float FusionMissileHeatSeekingAngle(int level) { return fmHeatSeekingAngle[Mathf.Clamp(level, 0, maxLevel)]; }
	public static float FusionMissileMaxDistance(int level)      { return fmMaxDistance[Mathf.Clamp(level, 0, maxLevel)]; }

	// NANOTECH
	public static float NanotechSlowFactor()          { return ntSlowFactor; }
	public static float NanotechSlowTime()               { return ntSlowTime; }
	public static float NanotechParticleRange(int level) { return ntParticleRange[Mathf.Clamp(level, 0, maxLevel)]; }
	*/
} // end of class