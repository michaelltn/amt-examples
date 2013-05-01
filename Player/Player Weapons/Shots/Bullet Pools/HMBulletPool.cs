using UnityEngine;
using System.Collections;

public class HMBulletPool : BulletPool {
	
	public static BulletPool Pool;
	
	void Start() {
		if (Pool)
			Destroy(this.gameObject);
		else
			Pool = this;
	}
	
} // end of class