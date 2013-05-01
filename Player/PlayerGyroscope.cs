using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Collider))]
public class PlayerGyroscope : MonoBehaviour {

	public static PlayerGyroscope main;
	
	void Start () {
		if (main == null) {
			main = this;
			this.collider.isTrigger = true;
		}
		else
			Destroy(this);
	}
	
	void OnDestroy() {
		if (main == this)
			main = null;
	}
	
} // end of class