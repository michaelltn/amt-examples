using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
	private static Boss main;
	public static Boss Main { get { return main; } }
	
	private BossAI bossAI;
	public BossAI ai { get { return bossAI; } }
	
	public string bossName = "Bossypants";
	
	private bool dead = false;
	public bool isDead { get { return dead; } }
	
	public void kill()
	{
		if (!dead)
		{
			dead = true;
		}
	}
	
	void Start ()
	{
		if (main == null)
		{
			main = this;
			bossAI = this.gameObject.GetComponent<BossAI>();
			bossAI.onDeathBegin += beginDeath;
			bossAI.onDeathEnd += endDeath;
			
		}
		else
		{
			Destroy (this.gameObject);
		}
	}
	
	void OnDestroy ()
	{
		if (main == this)
			main = null;
	}
	
	void Update ()
	{
		if (!GameState.IsPaused)
		{
			if (!isDead)
			{
				bossAI.defaultMovement();
			}
			else
			{
				bossAI.deadMovement();
			}
		}
	}
	
	virtual public void beginDeath()
	{
		kill();
	}
	
	virtual public void endDeath()
	{
		// issue story event
		// Destroy(this.gameObject);
	}	
	
	
} // end of class