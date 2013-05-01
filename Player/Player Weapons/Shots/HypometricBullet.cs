using UnityEngine;
using System.Collections;

public class HypometricBullet: Projectile
{
	//public ParticleSystem[] particleSystemTrails;
	
	public void addCollisionPlane(Transform t)
	{
		if (t != null)
		{
			//foreach (ParticleSystem ps in particleSystemTrails)
			{
				// assign transform as plane to collision module
				// http://answers.unity3d.com/questions/265083/add-collision-planes-to-shuriken-particlesystem-pr.html
				// asked June 10th
				// unanswered as of June 16th
			}
		}
	}
	
}