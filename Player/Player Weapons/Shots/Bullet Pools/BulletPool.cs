using UnityEngine;
using System.Collections;

public class BulletPool : MonoBehaviour {
	
	public GameObject bulletPrefab;
	public int startSize = 500;
	public int growSize = 50;
	
	private GameObject[] bulletList;
	public int listSize;
	public int availableBullets;
	private int nextBullet;
	private GameObject tempBullet;
	
	void Start () {
		if (startSize <= 0)
			startSize = 1;
		if (growSize <= 0)
			growSize = 1;
		
		listSize = 0;
		availableBullets = 0;
	}
	
	void Awake() {
		GrowList(startSize);
	}
	
	private bool GrowList(int growth) {
		GameObject[] newBulletList = new GameObject[listSize + growth];
		int b;
		for (b = 0; b < availableBullets; b++) {
			newBulletList[b] = bulletList[b];
		}
		for (b = availableBullets; b < availableBullets+growth; b++) {
			newBulletList[b] = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			newBulletList[b].transform.parent = this.transform;
			newBulletList[b].active = false;
			setChildVisibility(newBulletList[b].transform, false);
		}
		availableBullets += growth;
		listSize += growth;
		bulletList = newBulletList;
		return true;
	}
	
	public GameObject getBullet(Vector3 startPosition, Quaternion startRotation) {
		if (availableBullets > 0) {
			nextBullet = --availableBullets;
			if (bulletList[nextBullet]) {
				bulletList[nextBullet].active = true;
				setChildVisibility(bulletList[nextBullet].transform, true);
				bulletList[nextBullet].transform.position = startPosition;
				bulletList[nextBullet].transform.rotation = startRotation;
				return bulletList[nextBullet];
			}
		}
		else {
			if (GrowList(growSize)) {
				return getBullet(startPosition, startRotation);
			}
		}
		return null;
	}
	
	public bool returnBullet(GameObject bullet) {
		if (availableBullets < listSize) {
			bullet.active = false;
			setChildVisibility(bullet.transform, false);
			bulletList[availableBullets++] = bullet;
			return true;
		}
		/*
		else {
			if (GrowList(growSize)) {
				return returnBullet(bullet);
			}
		}
		*/
		return false;
	}
	
	public void setChildVisibility(Transform root, bool visiblilty) {
		foreach (Transform child in root)
			setChildVisibility(child, visiblilty);
		if (root.renderer)
			root.renderer.enabled = visiblilty;
	}
	
	
} // end of class