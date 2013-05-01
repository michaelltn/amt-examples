using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Renderer))]
public class PlayerShield : MonoBehaviour
{

	void Start ()
	{

	}
	
	void Update ()
	{
	
	}
	
	public void enable()
	{
		renderer.enabled = true;
	}
	
	public void disable()
	{
		renderer.enabled = false;
	}
	
	
} // end of class