using UnityEngine;
using System.Collections;

public class EnemySpawnZone : MonoBehaviour {

	public Rect rect;
	
	public Vector3 randomLocation() {
		return new Vector3(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax), 0);
	}
	
	void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMin, 0), new Vector3(rect.xMax, rect.yMin, 0));
		Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMin, 0), new Vector3(rect.xMax, rect.yMax, 0));
		Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMax, 0), new Vector3(rect.xMin, rect.yMax, 0));
		Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMax, 0), new Vector3(rect.xMin, rect.yMin, 0));
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawLine(new Vector3(rect.xMin+1f, rect.yMin+1f, 0), new Vector3(rect.xMax-1f, rect.yMin+1f, 0));
		Gizmos.DrawLine(new Vector3(rect.xMax-1f, rect.yMin+1f, 0), new Vector3(rect.xMax-1f, rect.yMax-1f, 0));
		Gizmos.DrawLine(new Vector3(rect.xMax-1f, rect.yMax-1f, 0), new Vector3(rect.xMin+1f, rect.yMax-1f, 0));
		Gizmos.DrawLine(new Vector3(rect.xMin+1f, rect.yMax-1f, 0), new Vector3(rect.xMin+1f, rect.yMin+1f, 0));
	}
	
	
} // end of class