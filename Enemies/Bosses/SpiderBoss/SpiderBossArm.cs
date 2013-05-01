using UnityEngine;
using System.Collections;

public class SpiderBossArm : MonoBehaviour
{
	public Animation animator;
	// looping animations
	public string activeHealthyAnim = "arm healthy active";
	public string inactiveHealthyAnim = "arm healthy inactive";
	public string activeInjuredAnim = "arm injured active";
	public string inactiveInjuredAnim = "arm injured inactive";
	public string deadAnim = "arm dead";
	// once animations
	public string activatingAnim = "arm activating";
	public string deactivatingAnim = "arm deactivating";
	public string injuringAnim = "arm injuring";
	public string dieingAnim = "arm dieing";
	// healthy vs injured
	public string activeAnim { get { return isInjured ? activeInjuredAnim : activeHealthyAnim; } }
	public string inactiveAnim { get { return isInjured ? inactiveInjuredAnim : inactiveHealthyAnim; } }
	
	
	public SpiderBoss spiderBoss;
	public SpiderBossHead spiderBossHead;
	
	public delegate void armInjuryDelegate(SpiderBossArm arm);
	public event armInjuryDelegate onInjury;
	
	public delegate void armDestructionStartDelegate(SpiderBossArm arm);
	public event armDestructionStartDelegate onDestructionStart;
	
	public delegate void armDestructionCompleteDelegate(SpiderBossArm arm);
	public event armDestructionCompleteDelegate onDestructionComplete;
	
	public enum ArmState { Inactive, Active, Dead, Deactivating, Activating, Injuring, Dieing };
	private ArmState state;
	private bool injured = false;
	public bool isInjured { get { return injured; } }
	
	public bool isDead { get { return state == ArmState.Dead || state == ArmState.Dieing; } }
	public bool canActivate { get { return state == ArmState.Inactive || state == ArmState.Deactivating; } }
	public bool canDeactivate { get { return state == ArmState.Active || state == ArmState.Activating; } }
	
	
	void Start ()
	{
		if (spiderBoss)
		{
			spiderBoss.addArm(this);
		}
		else
		{
			Debug.Log("spider boss not assigned to arm: " + this.name);
		}
	}
	
	
	public void setHead(SpiderBossHead head)
	{
		if (spiderBossHead != head)
		{
			spiderBossHead = head;
			head.onInjury += headInjury;
			head.onDestructionStart += headDestructionStart;
			head.onDestructionComplete += headDestructionComplete;
		}
	}
	
	private void headInjury(SpiderBossHead head)
	{
		injure();
	}
	
	private void headDestructionStart(SpiderBossHead head)
	{
		// begin death animation
		deactivate();
	}
	
	private void headDestructionComplete(SpiderBossHead head)
	{
		kill();
	}
	
	
	public bool activate()
	{
		if (canActivate)
		{
			state = ArmState.Activating;
			if (animator != null)
				animator.CrossFade(activatingAnim);
			
			return true;
		}
		return false;
	}
	
	public bool deactivate()
	{
		if (canDeactivate)
		{
			state = ArmState.Deactivating;
			if (animator != null)
				animator.CrossFade(deactivatingAnim);
			
			if (spiderBossHead != null)
				spiderBossHead.activate();
			
			return true;
		}
		return false;
	}
	
	public bool injure()
	{
		if (!isInjured)
		{
			injured = true;
			state = ArmState.Injuring;
			if (animator != null)
				animator.CrossFade(injuringAnim);
			
			if (onInjury != null)
				onInjury(this);
		}
		return false;
	}
	
	public bool kill()
	{
		if (!isDead)
		{
			state = ArmState.Dieing;
			if (animator != null)
				animator.CrossFade(dieingAnim);
			
			if (onDestructionStart != null)
				onDestructionStart(this);
			
			return true;
		}
		return false;
	}
	
	
	
	void Update ()
	{
		switch (state)
		{
		case ArmState.Inactive:
			inactiveUpdate();
			break;
		case ArmState.Active:
			activeUpdate();
			break;
		case ArmState.Activating:
			activatingUpdate();
			break;
		case ArmState.Deactivating:
			deactivatingUpdate();
			break;
		case ArmState.Injuring:
			injuringUpdate();
			break;
		case ArmState.Dieing:
			dieingUpdate();
			break;
		case ArmState.Dead:
			deadUpdate();
			break;
		}
	}

	private void inactiveUpdate()
	{
		if (animator != null)
		{
			
		}
		else
		{
			
		}
	}
	
	private void activeUpdate()
	{
		if (animator != null)
		{
			
		}
		else
		{
			
		}
	}

	private void deadUpdate()
	{
		if (animator != null)
		{
			
		}
		else
		{
			
		}
	}	
	
	private void activatingUpdate()
	{
		if (animator != null)
		{
			if (!animator.IsPlaying(activatingAnim))
			{
				animator.CrossFade(activeAnim);
				state = ArmState.Active;
				
				if (spiderBossHead != null)
					spiderBossHead.activate();
			}
		}
		else
		{
			state = ArmState.Active;
		}
	}
	
	private void deactivatingUpdate()
	{
		if (animator != null)
		{
			if (!animator.IsPlaying(deactivatingAnim))
			{
				animator.CrossFade(inactiveAnim);
				state = ArmState.Inactive;
			}
		}
		else
		{
			state = ArmState.Inactive;
		}
	}
	
	private void injuringUpdate()
	{
		if (animator != null)
		{
			if (!animator.IsPlaying(injuringAnim))
			{
				animator.CrossFade(inactiveAnim);
				state = ArmState.Inactive;
			}
		}
		else
		{
			state = ArmState.Inactive;
		}
	}
	
	private void dieingUpdate()
	{
		if (animator != null)
		{
			if (!animator.IsPlaying(dieingAnim))
			{
				animator.CrossFade(deadAnim);
				state = ArmState.Dead;
				if (onDestructionComplete != null)
					onDestructionComplete(this);
			}
		}
		else
		{
			state = ArmState.Dead;
			if (onDestructionComplete != null)
				onDestructionComplete(this);
		}
	}
	

	
} // end of class