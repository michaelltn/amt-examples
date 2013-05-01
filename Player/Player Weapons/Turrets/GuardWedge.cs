using UnityEngine;
using System.Collections;

public class GuardWedge : MonoBehaviour
{
	private Turret turretTarget;
	private Renderer[] renderers;
	private bool hidden = false;
	
	public void assignTurret(Turret turretTarget)
	{
		this.turretTarget = turretTarget;
		if (renderers == null)
			renderers = this.gameObject.GetComponentsInChildren<Renderer>();
		Color wedgeColor = turretTarget.color;
		//wedgeColor.a = 0.10f;
		foreach (Renderer r in renderers)
			r.material.SetColor("_TintColor", wedgeColor);
		
		hide();
	}
	
	void Awake ()
	{
		hide();
	}
	
	
	private Vector3 newRotation = Vector3.zero;
	private Vector3 newScale = new Vector3(4.5f, 4.5f, 1f);
	void LateUpdate ()
	{
		if (PlayerShip.TurretBaseTransform != null && turretTarget != null)
		{
			if (turretTarget.aiEnabled)
			{
				if (GameState.IsPaused || turretTarget.isParked)
				{
					if (!hidden) hide();
				}
				else
				{
					if (hidden) show();
				}
				
				newRotation.z = PlayerShip.TurretBaseTransform.localEulerAngles.z + turretTarget.guardAngle;
				this.transform.eulerAngles = newRotation;
				
				newScale.x = newScale.y = (Turret.AIRange + Vector3.Distance(turretTarget.transform.position, PlayerShip.TurretBaseTransform.position)) * 0.1f;
				this.transform.localScale = newScale;
			}
			else
			{
				Destroy(this.gameObject);
			}
		}
	}
	
	public void show()
	{
		if (renderers != null)
			foreach (Renderer r in renderers)
				r.enabled = true;
		hidden = false;
	}
	
	public void hide()
	{
		if (renderers != null)
			foreach (Renderer r in renderers)
				r.enabled = false;
		hidden = true;
	}
	
}