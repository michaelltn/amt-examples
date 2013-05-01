using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class PlayerSpawnFX : MonoBehaviour {
	
	public static PlayerSpawnFX main;
	
	public ParticleEmitter spawnProgressEmitter;
	public ParticleEmitter spawnCompleteEmitter;
	
	public AudioClip progressAudio;
	public AudioClip completeAudio;
	public AudioSource audioSource;
	
	void Start () {
		if (!main) {
			main = this;
			if (spawnProgressEmitter)
				spawnProgressEmitter.emit = false;
			if (spawnCompleteEmitter)
				spawnCompleteEmitter.emit = false;
			if (!audioSource)
				audioSource = this.gameObject.GetComponent<AudioSource>();
		}
		else
			Destroy(this.gameObject);
	}
	
	void OnDestroy() {
		if (main == this)
			main = null;
	}
	
	public void beginSpawn() {
		if (spawnProgressEmitter)
			spawnProgressEmitter.emit = true;
		
		if (audioSource && progressAudio)
			audioSource.PlayOneShot(progressAudio);
	}
	
	public void completeSpawn() {
		if (spawnProgressEmitter)
			spawnProgressEmitter.emit = false;
		if (spawnCompleteEmitter)
			spawnCompleteEmitter.Emit();
		
		if (audioSource && completeAudio)
			audioSource.PlayOneShot(completeAudio);
	}
	
} // end of class