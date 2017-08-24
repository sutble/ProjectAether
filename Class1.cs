using UnityEngine;
using System.Collections;

public class RockController : MonoBehaviour
{

    public GameObject hand;
    public bool isSelected;

    private Rigidbody rb;

    private bool onetime = false;

    private Vector3 punchMotion;

    private float punchMultiplier = 5;

    private float startTime;

    // Use this for initialization
    void Start()
    {
        Debug.Log("rock controller reading this object for hand: " + hand.name);
        isSelected = true;
        rb = this.GetComponent<Rigidbody>();
        startTime = Time.time;

    }

    // Update is called once per frame
    void Update()
    {

        if (!hand.GetComponent<SteamVR_ControllerEvents>().triggerPressed && !onetime)
        {
            onetime = true;
            Invoke("deSelect", 3);
        }
        else if (!onetime)
        {
            Levitate();

        }
        /*
        if (hand.GetComponent<SteamVR_ControllerEvents>().gripPressed)
        {
            Debug.Log("GRIP PRESSED IN ROCK");
            Vector3 towardHand = -hand.transform.forward;
            rb.AddForce(towardHand * 50);
        }*/

        if (isSelected)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;


        }
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("COLLISION OF ROCK : " + col.relativeVelocity.magnitude);

        if (col.gameObject.tag != "ground")
        {
            deSelect();
            onetime = true;
            rb.WakeUp();
        }

        var velocity = col.relativeVelocity;

        if (col.gameObject.tag == "ground")
        {
            velocity *= 1.5F;
        }
        else if (col.gameObject.tag == "enemy")
        {
            velocity *= 4;
        }

        this.gameObject.GetComponent<SimpleFracture>().FractureAtPoint(col.contacts[0].point, velocity);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "hand")
        {

            SteamVR_Controller.Input(hand.GetComponent<EarthController>().GetDeviceIndex()).TriggerHapticPulse(3999);



            GameObject fist = other.gameObject;

            int i = fist.GetComponent<SpellController>().GetDeviceIndex();
            var origin = fist.GetComponent<SteamVR_TrackedObject>().origin ? fist.GetComponent<SteamVR_TrackedObject>().origin : fist.GetComponent<SteamVR_TrackedObject>().transform.parent;
            var device = SteamVR_Controller.Input(i);

            if (origin != null)
            {
                rb.velocity = origin.TransformVector(device.velocity * punchMultiplier);
                rb.angularVelocity = origin.TransformVector(device.angularVelocity);
            }
            else
            {
                rb.velocity = device.velocity * punchMultiplier;
                rb.angularVelocity = device.angularVelocity;
            }
            rb.maxAngularVelocity = rb.angularVelocity.magnitude;
            rb.WakeUp();

            deSelect();


        }
    }


    /*
    private void ThrowReleasedObject(Rigidbody rb, uint controllerIndex)
    {
        var origin = hand.origin ? trackedController.origin : trackedController.transform.parent;
        var device = SteamVR_Controller.Input((int)controllerIndex);
        if (origin != null)
        {
            rb.velocity = origin.TransformVector(device.velocity);
            rb.angularVelocity = origin.TransformVector(device.angularVelocity);
        }
        else
        {
            rb.velocity = device.velocity;
            rb.angularVelocity = device.angularVelocity;
        }
        rb.maxAngularVelocity = rb.angularVelocity.magnitude;
    }*/

    void Levitate()
    {
        Vector3 rockTransformY = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        Vector3 handTransformY = new Vector3(gameObject.transform.position.x, hand.transform.position.y, gameObject.transform.position.z);
        gameObject.transform.position = Vector3.MoveTowards(rockTransformY, handTransformY, 1.5f * GetFractionOfSpeed() * Time.deltaTime);

        SteamVR_Controller.Input(hand.GetComponent<EarthController>().GetDeviceIndex()).TriggerHapticPulse((ushort)(GetFractionOfSpeed() * 1000));
    }

    //follows y = 20^x
    float GetFractionOfSpeed()
    {
        float diff = (Time.time - startTime) / (float)1.0;
        if (diff > 1.0)
        {
            return (float)1.0;
        }
        else
        {
            diff -= (float)1.0;

            return Mathf.Pow(20.0F, diff);
        }

    }

    void deSelect()
    {
        isSelected = false;
        Invoke("DeleteRock", 10);
    }

    void DeleteRock()
    {
        Destroy(gameObject);
    }





}