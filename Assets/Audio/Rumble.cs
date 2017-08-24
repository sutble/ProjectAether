using UnityEngine;
using System.Collections;

public class Rumble : MonoBehaviour {
	public AudioClip Loop;
	public float Volume;
	public float Rate;
	private float direction = -1f;
	private AudioSource source;
	// Use this for initialization
	void Start () {
		source = gameObject.AddComponent<AudioSource> ();
		source.clip = Loop;
		source.loop = true;
		source.volume = 0f;
		source.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		source.volume += Rate * direction;
		source.volume = Mathf.Clamp (source.volume, 0f, Volume);
		if (Input.GetKeyDown (KeyCode.Space)) {
			RumbleOn ();
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
		
			RumbleOff ();
		}
	}

	public void RumbleOn(){
		direction = 1f;
	}

	public void RumbleOff(){
		direction = -1f;
	}

}
