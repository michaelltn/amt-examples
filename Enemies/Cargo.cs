using UnityEngine;
using System.Collections;

public class Cargo : MonoBehaviour
{
	public int score;
	
	public float shieldDropChance = 0.02f;
	public float moneyDropChance = 0.1f;
	public int moneyDropMin = 1;
	public int moneyDropMax = 8;
	
	public float lockDropFor = 0;
	public bool overrideDropLifeSpan = false;
	public float newDropLifeSpan = 0;
	
	public void drop(Vector3 dropLocation)
	{
		PlayerShip.IncScore(score);
		
		if (moneyDropChance < 1f)
		{
			if (Random.Range(0f, 1f) < moneyDropChance)
			{
				dropMoney(dropLocation);
			}
			else if (PlayerShip.Main.getShieldPercent() < 1f && Random.Range(0f, 1f) < shieldDropChance)
			{
				dropShield(dropLocation);
			}
		}
		else
		{
			if (PlayerShip.Main.getShieldPercent() < 1f && Random.Range(0f, 1f) < shieldDropChance)
			{
				dropShield(dropLocation);
			}
			else
			{
				dropMoney(dropLocation);
			}
		}
	}
	
	private void dropMoney(Vector3 dropLocation)
	{
		if (DropManager.Main)
		{
			int amount = Random.Range(moneyDropMin, moneyDropMax+1);
			MoneyDrop moneyDropPrefab = DropManager.Main.getMoneyPrefab(amount);
			
			if (moneyDropPrefab)
			{
				MoneyDrop newMoneyDrop = Instantiate(moneyDropPrefab, dropLocation, Quaternion.identity) as MoneyDrop;
				newMoneyDrop.amount = amount;
				newMoneyDrop.lockFor(lockDropFor);
				if (overrideDropLifeSpan)
					newMoneyDrop.lifeSpan = newDropLifeSpan;
			}
		}
	}
	
	private void dropShield(Vector3 dropLocation)
	{
		if (DropManager.Main)
		{
			if (DropManager.Main.shieldDropPrefab)
			{
				ShieldDrop newShieldDrop = Instantiate(DropManager.Main.shieldDropPrefab, dropLocation, Quaternion.identity) as ShieldDrop;
				newShieldDrop.lockFor(lockDropFor);
				if (overrideDropLifeSpan)
					newShieldDrop.lifeSpan = newDropLifeSpan;
			}
		}
	}
	
} // end of class