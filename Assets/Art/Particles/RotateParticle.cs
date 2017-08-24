using UnityEngine;
using System.Collections;

public class RotateParticle : MonoBehaviour {
	
    public float Speed = 0.2f;

	// Update is called once per frame
	void Update () {
        //rotating particle
        transform.Rotate(Vector3.forward * Speed * Time.deltaTime);

	}
}
