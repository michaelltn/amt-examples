using UnityEngine;
using System.Collections;

public abstract class Shot : MonoBehaviour
{
	public bool useFixedUpdate = true;
	
	public LayerMask enemyLayer;
	public LayerMask obstacleLayer;
	public float hypometricDamage;
	public float superluminalDamage;
	public float fusionDamage;
	public float untypedDamage;
	public float pushBackForce = 0f;
	
	public float getHypometricDamage() { return hypometricDamage * damageMultiplier; }
	public float getSuperluminalDamage() { return superluminalDamage * damageMultiplier; }
	public float getFusionDamage() { return fusionDamage * damageMultiplier; }
	public float getUntypedDamage() { return untypedDamage * damageMultiplier; }
	
	protected float damageMultiplier = 1.0f;
	public void setDamageMultiplier(float newDamageMultiplier)
	{
		damageMultiplier = newDamageMultiplier;
	}
	
	private bool isVisible = true;
	
	protected bool _enabled = true;
	public bool isEnabled { get { return _enabled; } }
	virtual public void enable()
	{
		if (!_enabled)
		{
			_enabled = true;
		}
	}
	virtual public void disable()
	{
		if (_enabled)
		{
			_enabled = false;
		}
	}
	virtual public void disableImmediate()
	{
		if (_enabled)
		{
			isVisible = _enabled = false;
			SetVisibility(this.transform, isVisible);
		}
	}

	private static void SetVisibility(Transform t, bool visible)
	{
		foreach (Transform c in t)
		{
			SetVisibility(c, visible);
		}
		if (t.gameObject.GetComponent<Renderer>() != null)
			t.gameObject.GetComponent<Renderer>().enabled = visible;
	}
	
	void Start ()
	{
		start();
	}
	
	void Update ()
	{
		if (!useFixedUpdate)
			UpdateFunction();
	}
	
	void FixedUpdate ()
	{
		if (useFixedUpdate)
			UpdateFunction();
	}
	
	
	private void UpdateFunction()
	{
		if (isVisible != _enabled)
		{
			isVisible = _enabled;
			SetVisibility(this.transform, isVisible);
		}
		
		update();
	}
	
	public virtual void init() {}
	
	protected virtual void start() {}
	protected virtual void update() {}
}
