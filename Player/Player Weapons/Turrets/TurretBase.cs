using UnityEngine;
using System.Collections;

public class TurretBase : MonoBehaviour
{
	public bool useFixedUpdate = false;
	
	public float rotationSpeed = 360f;
	private float currentRotation, newRotation;
	
	public Renderer[] buildPlaneRenderers;
	public float buildRadius = 0.5f;
	public float innerBuildRadius = 2f;
	public float outerBuildRadius = 5f;
	
	// look direction of turret
	private Vector2 lookInput;
	
	private Transform myTransform;
	
	void Start ()
	{
		myTransform = this.transform;
		lookInput = new Vector2(1, 0);
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
		foreach (Renderer r in buildPlaneRenderers)
		{
			if (r.enabled != GameState.BuildMenuReady)
				r.enabled = GameState.BuildMenuReady;
		}
		
		if (!GameState.IsPaused && !PlayerShip.IsDead)
		{
			lookInput.x = InputManager.HorizontalLookInput;
			lookInput.y = InputManager.VerticalLookInput;
			if (lookInput.magnitude > 0) {
				lookInput.Normalize();
				currentRotation = myTransform.eulerAngles.z;
				newRotation = Mathm.Rotation2D(currentRotation, lookInput, rotationSpeed * Time.deltaTime);
				myTransform.eulerAngles = new Vector3(0, 0, newRotation);
			}
		}
	}
	
} // end of class