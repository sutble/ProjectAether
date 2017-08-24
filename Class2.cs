using UnityEngine;
using System.Collections;

public class RockController : MonoBehaviour
{

    public GameObject hand;

    public bool isSelected;
    private bool onetime = false;
    private bool hasNotReleased = false; // true if you have not let go of the trigger ever
    private bool isShard = false;

    private Rigidbody rb;

    private float startTime;


    private Vector3 punchMotion;
    private float punchMultiplier = 5;

    private Vector3 origY;
    private float pangle;
    private float pfactor;
    public float amplitude = 0.1f;
    private float omega;
    private float springConstant = 200f;
    private float finalOmega;

    public GameObject punchParticle;
    public GameObject summonParticle;

    // Use this for initialization
    void Start()
    {
        //Debug.Log("rock controller reading this object for hand: " + hand.name);
        isSelected = true;
        rb = this.GetComponent<Rigidbody>();
        startTime = Time.time;

        // Rock spawns just below ground
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - gameObject.GetComponent<Renderer>().bounds.size.y, gameObject.transform.position.z);

        // hack, delayed summon of particles
        Invoke("StartSummonParticles", 1);

        gameObject.GetComponent<MeshCollider>().isTrigger = true;

    }

    // Update is called once per frame
    void Update()
    {



        if (hand && hand.GetComponent<SteamVR_ControllerEvents>().triggerPressed && !onetime && isSelected)
        {
            hasNotReleased = true;
            Levitate();
        }

        if (hand && !hand.GetComponent<SteamVR_ControllerEvents>().triggerPressed && !onetime && isSelected)
        {

            origY = transform.position;
            pangle = Time.time;
            amplitude = 0.1f;
            hasNotReleased = false;
            onetime = true;
            Invoke("deSelect", 6);
        }

        if (!hasNotReleased && isSelected)
        {
            transform.position = origY + new Vector3(0, amplitude * Mathf.Sin((Time.time - pangle)), 0);
            // Debug.Log("Sin Function " + transform.position);
        }

        if (isSelected)
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            rb.angularVelocity = new Vector3(Random.Range(0.0f, 0.25f), Random.Range(0.0f, 0.25f), Random.Range(0.0f, 0.25f));
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        //Debug.Log("COLLISION OF ROCK : " + col.relativeVelocity.magnitude);
        //Debug.Log("hand2");

        if (col.gameObject.tag == "projectile")
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            col.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            col.gameObject.GetComponent<Rigidbody>().detectCollisions = true;
        }

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

        Debug.Log("hand2");

        if (other.gameObject.tag == "hand")
        {
            deSelect();

            SteamVR_Controller.Input(hand.GetComponent<EarthController>().GetDeviceIndex()).TriggerHapticPulse(3999);

            GameObject fist = other.gameObject;
            int i = fist.GetComponent<SpellController>().GetDeviceIndex();
            var origin = fist.GetComponent<SteamVR_TrackedObject>().origin ? fist.GetComponent<SteamVR_TrackedObject>().origin : fist.GetComponent<SteamVR_TrackedObject>().transform.parent;
            var device = SteamVR_Controller.Input(i);

            rb.velocity = Vector3.zero;
            var velocity = device.velocity * punchMultiplier;
            Debug.Log("Velocity of punch: " + velocity);
            float punchWaveMultiplier = 0.075f * velocity.magnitude;

            deSelect();
            rb.isKinematic = false;

            if (punchWaveMultiplier > 0.5)
            {
                // punch rock shockwave
                Quaternion rot = Quaternion.identity;
                Vector3 particlePos = other.transform.position;
                Quaternion particleRot = other.transform.rotation;
                GameObject punchWave = (GameObject)Instantiate(punchParticle, particlePos, particleRot);
                punchWave.transform.LookAt(this.transform);
                GameObject punchWaveRing = punchWave.transform.GetChild(0).gameObject;

                // shockwave is variable depending on the velocity of the punch
                punchWaveRing.GetComponent<ParticleSystem>().startSize = punchWaveMultiplier;
                Debug.Log("PUNCH START SIZE: " + punchWaveRing.GetComponent<ParticleSystem>().startSize);
            }

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
            hand = null;

        }

    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("On trigger exit");

        if (other.tag == "ground")
        {
            gameObject.GetComponent<MeshCollider>().isTrigger = false;

        }

    }

    void StartSummonParticles()
    {
        // Particle effect of rock summon
        GameObject rockSummon = (GameObject)Instantiate(summonParticle);
        Vector3 summonParticlePosition = new Vector3(this.transform.position.x, rockSummon.transform.position.y, this.transform.position.z);
        rockSummon.transform.position = summonParticlePosition;
    }



    //private void ThrowReleasedObject(Rigidbody rb, uint controllerIndex)
    //{
    //    var origin = hand.origin ? trackedController.origin : trackedController.transform.parent;
    //    var device = SteamVR_Controller.Input((int)controllerIndex);
    //    if (origin != null)
    //    {
    //        rb.velocity = origin.TransformVector(device.velocity);
    //        rb.angularVelocity = origin.TransformVector(device.angularVelocity);
    //    }
    //    else
    //    {
    //        rb.velocity = device.velocity;
    //        rb.angularVelocity = device.angularVelocity;
    //    }
    //    rb.maxAngularVelocity = rb.angularVelocity.magnitude;
    //}

    void Levitate()
    {
        Vector3 rockTransformY = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        Vector3 handTransformY = new Vector3(gameObject.transform.position.x, hand.transform.position.y, gameObject.transform.position.z);
        gameObject.transform.position = Vector3.MoveTowards(rockTransformY, handTransformY, 3.0f * GetFractionOfSpeed() * Time.deltaTime);

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
