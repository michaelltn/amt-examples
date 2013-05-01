using UnityEngine;
using System.Collections;

public class EnemyShot : MonoBehaviour {

	protected Transform sourceTransform;
	protected Transform targetTransform = null;
	
	public int damage = 10;
	//public float damageToMotherShip = 1f;

	
	void Start() {
		init();
	}
	
	virtual protected void init() {}
	
	public void assignTargetTransform(Transform t) {
		targetTransform = t;
	}
	
	public void assignSourceTransform(Transform t) {
		sourceTransform = t;
	}
	
	public Transform getSourceTransform() {
		return sourceTransform;
	}
	
} // end of class