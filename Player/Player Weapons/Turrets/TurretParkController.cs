using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class TurretAIGroup
{
	public Color color;
	public GUITexture activeTexture;
	public GUITexture inactiveTexture;
	public bool available;
}

public class TurretParkController : MonoBehaviour
{
	private static TurretParkController main;
	public static TurretParkController Main { get { return main; } }
	
	public static Color GetColor(int index)
	{
		if (Main) return Main.getColor(index);
		return Color.white;
	}
	
	public bool inputEnabled = true;
	public bool syncAIToParkMode = true;
	
	public TurretAIGroup[] aiGroups;
	public Color getColor(int index)
	{
		if (index >= 0 && index < aiGroups.Length)
		{
			return aiGroups[index].color;
		}
		return Color.white;
	}
	
	private int _currentIndex = 0;
	public int currentIndex { get { return _currentIndex; } }
	
	public int getNumAvailableGroups()
	{
		int output = 0;
		foreach(TurretAIGroup t in aiGroups)
		{
			if(t.available)
				output++;
		}
		return output;
	}
	
	public void nextIndex()
	{
		int newIndex = _currentIndex;
		do
		{
			newIndex++;
			if (newIndex >= aiGroups.Length) newIndex -= aiGroups.Length;
		}
		while (newIndex != _currentIndex && !aiGroups[newIndex].available);
		
		if (newIndex != _currentIndex)
		{
			aiGroups[currentIndex].activeTexture.enabled = false;
			aiGroups[currentIndex].inactiveTexture.enabled = true;
			_currentIndex = newIndex;
			aiGroups[currentIndex].activeTexture.enabled = true;
			aiGroups[currentIndex].inactiveTexture.enabled = false;
			PlayerShip.Main.currentAIGroupHighlighter.material.SetColor("_TintColor",aiGroups[_currentIndex].color);
			if(!PlayerShip.Main.currentAIGroupHighlighter.enabled)
				PlayerShip.Main.currentAIGroupHighlighter.enabled = true;
		}
	}
	public void prevIndex()
	{
		int newIndex = _currentIndex;
		do
		{
			newIndex--;
			if (newIndex < 0) newIndex += aiGroups.Length;
		}
		while (newIndex != _currentIndex && !aiGroups[newIndex].available);
		
		if (newIndex != _currentIndex)
		{
			aiGroups[_currentIndex].activeTexture.enabled = false;
			aiGroups[_currentIndex].inactiveTexture.enabled = true;
			_currentIndex = newIndex;
			aiGroups[_currentIndex].activeTexture.enabled = true;
			aiGroups[_currentIndex].inactiveTexture.enabled = false;
			PlayerShip.Main.currentAIGroupHighlighter.material.SetColor("_TintColor",aiGroups[_currentIndex].color);
			if(!PlayerShip.Main.currentAIGroupHighlighter.enabled)
				PlayerShip.Main.currentAIGroupHighlighter.enabled = true;
		}
	}
	
	public void updateGroupAvailability()
	{
		for (int i = 0; i < aiGroups.Length; i++)
		{
			aiGroups[i].available = false;
			aiGroups[i].activeTexture.enabled = false;
			aiGroups[i].inactiveTexture.enabled = false;
		}
		
		foreach (Turret t in Turret.TurretList())
		{
			if (t.aiIndex < 0 || t.aiIndex >= aiGroups.Length)
				continue;
			if (!aiGroups[t.aiIndex].available)
			{
				aiGroups[t.aiIndex].available = true;
				aiGroups[t.aiIndex].inactiveTexture.enabled = true;
			}
		}
		
		if (_currentIndex >= 0 && _currentIndex < aiGroups.Length)
		{
			if (!aiGroups[_currentIndex].available)
				nextIndex();
			if (aiGroups[_currentIndex].available)
				aiGroups[_currentIndex].activeTexture.enabled = true;
		}
	}
	
	private void togglePark(int index)
	{
		if (Turret.NumberParkedInGroup(index) > 0)
		{
			Turret.RecallGroup(index);
		}
		else
		{
			Turret.ParkGroup(index);
		}
	}
	
	void Awake ()
	{
		if (main == null)
			main = this;
	}
	
	void OnDestroy ()
	{
		if (main == this)
			main = null;
	}
	
	
	void Start ()
	{
		if (aiGroups.Length == 0)
			Debug.LogError("At least one AIGroup is required.");
		_currentIndex = 0;
		updateGroupAvailability();
//		for (int i = 0; i < aiGroups.Length; i++)
//		{
//			aiGroups[i].activeTexture.enabled = (i == _currentIndex);
//			aiGroups[i].inactiveTexture.enabled = (i != _currentIndex);
//		}
	}
	
	void Update ()
	{
		if (!GameState.IsPaused && inputEnabled)
		{
			if (InputManager.NextGroupButtonDown)
				nextIndex();
			
			if (InputManager.PrevGroupButtonDown)
				 prevIndex();
			
			
			if (InputManager.ParkCurrentGroupButtonDown)
			{
				togglePark(currentIndex);
			}
			
			for (int i = 0; i < 4; i++)
			{
				if (InputManager.ParkGroupButtonDown(i))
				{
					togglePark(i);
				}
			}
			
			if (InputManager.RecallAllButtonDown)
			{
				Turret.RecallAll();
			}
		}
	}
	/*
	public void parkAll()
	{
		for (int i = 0; i < parkTurrets.Length; i++)
		{
			if (parkTurrets[i].turret)
				parkTurrets[i].turret.park();
			
		}
	}
	*/
} // end of class