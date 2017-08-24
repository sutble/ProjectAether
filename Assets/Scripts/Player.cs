using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    private double health;
    private double mana;

    public GameObject leftHand;
    public GameObject rightHand;

    public GameObject cameraObject;

    // Use this for initialization
    void Start () {
        health = 100.0;
        mana = 100.0;

        cameraObject.GetComponent<Camera>().farClipPlane = 1000;
    }

    // Update is called once per frame
    void Update () {
	    if(health <= 0.0f)
        {
            //you have died
        }
	}

    public void doDamage(float dmg)
    {
        health -= dmg;
    }


}
