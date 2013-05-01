using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Plane))]
[RequireComponent (typeof(Renderer))]
public class TurretBuildPlane : MonoBehaviour
{
	public Color flashColour;
	private Color defaultColour;
	
	public float flashRate = 1f;
	private bool isFlashing = false;
	
	void Awake()
	{
		if (this.renderer)
			defaultColour = this.renderer.material.color;
		else
			Debug.Log("no renderer");
	}
	
	void Update()
	{
		if (isFlashing && !HelpManager.HelpShowing)
		{
			if (this.renderer)
			{
				float t = 0.5f + (Mathf.Sin(Time.time * Mathf.PI * 2f / flashRate) * 0.5f);
				this.renderer.material.color = Color.Lerp(defaultColour, flashColour, t);
			}
		}
	}
	
	public void startFlashing()
	{
		isFlashing = true;
	}
	
	public void stopFlashing()
	{
		isFlashing = false;
		if (this.renderer)
			this.renderer.material.color = defaultColour;
	}
	
	
	
}