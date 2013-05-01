using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Collider))]
public class Distortion : MonoBehaviour
{
	public int damage;
	public float speed;
	private Vector3 direction = Vector3.zero;
	public float flyZoneBuffer = 40f;
	
	void Start ()
	{
		this.collider.isTrigger = true;
		if (direction.sqrMagnitude == 0)
			direction = this.transform.forward;
		direction.Normalize();
	}
	
	public void changeDirection(Vector3 newDirection)
	{
		if (newDirection.sqrMagnitude > 0)
			direction = newDirection.normalized;
	}
	
	void Update ()
	{
		this.transform.position += direction * speed * Time.deltaTime;
		if (!FlyZone.ContainsTransform(this.transform, flyZoneBuffer))
			Destroy(this.gameObject);
	}
	
	void OnTriggerStay(Collider other)
	{
		PlayerShip playerShip = other.gameObject.GetComponent<PlayerShip>();
		if (playerShip)
		{
			playerShip.applyDamage(damage);
		}
	}
}
