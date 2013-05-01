using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Boss))]
public abstract class BossAI : MonoBehaviour
{
	
	public delegate void beginDeathDelegate();
	public event beginDeathDelegate onDeathBegin;
	
	public delegate void endDeathDelegate();
	public event endDeathDelegate onDeathEnd;
	
	void Start ()
	{
		init();
	}
	
	abstract public void init();
	abstract public void defaultMovement();
	abstract public void deadMovement();
	
	virtual public void beginDeath()
	{
		if (onDeathBegin != null)
			onDeathBegin();
	}
	
	virtual public void endDeath()
	{
		if (onDeathEnd != null)
			onDeathEnd();
	}
	
} // end 