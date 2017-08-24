using UnityEngine;
using System.Collections;

public class RockController : MonoBehaviour {

    public GameObject hand;
    private Rigidbody rb;
    public SteamVR_TrackedObject fisty;

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

    public float initialMass;

    public Vector3 pullVelocity;

    ControllerInterface CI;

    bool isFollowingHand = false;
    bool isFloating = false;

    public enum StateAlias
    {
        FollowingHand,
        Floating,
        None
    }

    StateAlias state = StateAlias.None;

    //Audio stuff
    private AudioSource aSource;
    public AudioClip Crumble;
    public AudioClip Crash;
    public AudioClip Collide;

    // Use this for initialization
    void Start() {
        //Debug.Log("rock controller reading this object for hand: " + hand.name);
		//this.gameObject.tag = "rock";
        rb = this.GetComponent<Rigidbody>();
		//rb.tag = "rocknation";
        if (rb.mass == initialMass)
        {
            startTime = Time.time;

            // Rock spawns just below ground
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - gameObject.GetComponent<Renderer>().bounds.size.y - 0.3F, gameObject.transform.position.z);

            //gameObject.GetComponent<MeshCollider>().isTrigger = true;
            CI = hand.GetComponent<ControllerInterface>();
            changeState(StateAlias.FollowingHand);

            rb.velocity = new Vector3(0 * pullVelocity.x, pullVelocity.y, 0 * pullVelocity.z);
        } else
        {
            changeState(StateAlias.None);
        }

        aSource = gameObject.GetComponent<AudioSource>();

    }

   public void changeState(StateAlias targetState)
    {
		if (state == StateAlias.FollowingHand) {
			CI.controllerEvents.TriggerUnclicked -= unfollowHand;
		}
        switch (targetState)
        {
            case StateAlias.None:
                //Debug.Log("STATE NONE!");
               // CI.controllerEvents.TriggerUnclicked -= unfollowHand;
                Invoke("DeleteRock", 10);
                rb.isKinematic = false;
                rb.useGravity = true;
                break;
            case StateAlias.FollowingHand:
                rb.isKinematic = false;
                rb.useGravity = false;
                CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(unfollowHand);
                break;
            case StateAlias.Floating:
                rb.isKinematic = false;
                rb.useGravity = false;
                break;

        }
        state = targetState;
    }

    void unfollowHand(object sender, ControllerClickedEventArgs e)
    {
        CI.controllerEvents.TriggerUnclicked -= unfollowHand;
        changeState(StateAlias.Floating);
    }

    // Update is called once per frame
    void Update() {
        if (hand && state == StateAlias.FollowingHand)
        {
            Levitate();
        } else if (state == StateAlias.Floating)
        {
            Float();
        } else
        {

        }
    }

    bool isUncontrolled()
    {
        return state == StateAlias.None;
    }

    void OnCollisionEnter(Collision col)
    {
        
        //TODO - make this more extensible
        if (col.gameObject.tag == "projectile" && col.gameObject.GetComponent<RockController>().isUncontrolled())
        {
            changeState(StateAlias.None);
            if (!aSource.isPlaying) {
                aSource.PlayOneShot(Crash, 0.5f);
            }
            
        }

        var velocity = col.relativeVelocity;

        if (col.gameObject.tag == "ground")
        {
            velocity *= 1.5F;
            //StartSummonParticles();
            if (!aSource.isPlaying)
            {
                aSource.PlayOneShot(Crumble, 0.5f);
            }
        }
        else if (col.gameObject.tag == "enemy")
        {
            velocity *= 4;
            if (!aSource.isPlaying)
            {
                aSource.PlayOneShot(Collide, 0.5f);
            };
        }

        this.gameObject.GetComponent<SimpleFracture>().FractureAtPoint(col.contacts[0].point, velocity);

    }

	float punchStrength(float vel){
		bool isBoulder = this.gameObject.GetComponent<Collider> ().bounds.size.magnitude > 1.75f;
		if(vel > 20	){
			return 3999.0f;
		}
		else {
			return (isBoulder ? 1.15f * Mathf.Pow (vel, 2) + 1990.0f : 2.75f * Mathf.Pow (vel, 2) + 275.0f);
		}
	}

	float punchLength(float vel){
		bool isBoulder = this.gameObject.GetComponent<Collider> ().bounds.size.magnitude > 1.75f;
		if(vel > 20	){
			return (isBoulder ? 30.0f : 15.0f);
			}
		else {
			return 	(isBoulder ? 0.008f * Mathf.Pow (vel, 2) + 4.9f :  0.03f * Mathf.Pow (vel, 2) + 0.7f); 						
		}
	}

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.tag);

        if (other.gameObject.tag == "hand")
        {

            changeState(StateAlias.None);

            //SteamVR_Controller.Input(hand.GetComponent<EarthController>().GetDeviceIndex()).TriggerHapticPulse(3999);

            GameObject fist = other.gameObject;

            fisty = other.gameObject.GetComponent<SteamVR_TrackedObject>();
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)fisty.index);
            

           // int i = fist.GetComponent<NewEarthController>().GetDeviceIndex();
            var origin = fisty.GetComponent<SteamVR_TrackedObject>().origin ? fisty.GetComponent<SteamVR_TrackedObject>().origin : fisty.GetComponent<SteamVR_TrackedObject>().transform.parent;
            // var device = SteamVR_Controller.Input(i);


            //rb.velocity = Vector3.zero;
            var velocity = device.velocity * punchMultiplier;
			//Debug.Log("Velocity of punch: " + velocity.magnitude);
            float punchWaveMultiplier = 0.075f * velocity.magnitude;
            //Debug.Log("pwmulti is : " + punchWaveMultiplier);

			//fisty.GetComponent<SteamVR_ControllerActions> ().TriggerHapticPulse ((int) punchLength(velocity.magnitude), (ushort)punchStrength (velocity.magnitude));			//fist.GetComponent<SteamVR_ControllerActions> ().TriggerHapticPulse ((int) punchLength(velocity.magnitude), (ushort)punchStrength (velocity.magnitude));
			//fist.GetComponent<SteamVR_ControllerActions> ().TriggerHapticPulse ((int) 30, (ushort)punchStrength (velocity.magnitude));

		//	Debug.Log ((int) punchLength (velocity.magnitude));
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
                //Debug.Log("PUNCH START SIZE: " + punchWaveRing.GetComponent<ParticleSystem>().startSize);
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

            hand = null;

        } else
        {
            //Debug.Log("TRIGGER " + other.gameObject.tag);
        }

    }

    void OnTriggerExit(Collider other)
    {
        //Debug.Log("On trigger exit");

        if (other.tag == "ground")
        {
            gameObject.GetComponent<MeshCollider>().isTrigger = false;
            StartSummonParticles();
        }

    }

    void StartSummonParticles()
    {
        // Particle effect of rock summon
        GameObject rockSummon = (GameObject)Instantiate(summonParticle);
        Vector3 summonParticlePosition = new Vector3(this.transform.position.x, rockSummon.transform.position.y, this.transform.position.z);
        rockSummon.transform.position = summonParticlePosition;
    }

    void Levitate()
    {
        //TODO: make these variables public and easily changeable
        bool isHandNeg = hand.transform.position.y < gameObject.transform.position.y;

        float forceMultiplier = 1.0F - Mathf.Max(0.0F, Mathf.Min(pullVelocity.y / 3.0F, 0.2F));

        float diff = Mathf.Abs(hand.transform.position.y - gameObject.transform.position.y); // Mathf.Min(10F, 1.0F/Mathf.Abs(hand.transform.position.y - gameObject.transform.position.y));

        float multiplier = .5F ;
        float cMultiplier = 0.1F * forceMultiplier;

        rb.velocity = new Vector3(rb.velocity.x * 0.5F, rb.velocity.y - (isHandNeg ? multiplier * diff : -multiplier * diff) - cMultiplier*rb.velocity.y, rb.velocity.z * 0.5F);
    }

    void Float()
    {
        if (rb.velocity.y > 0.05)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - 0.1F * rb.velocity.y, rb.velocity.z);
        }
        else
        {
            origY = transform.position;
            pangle = Time.time;
            amplitude = 0.1f;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = new Vector3(Random.Range(0.0f, 0.25f), Random.Range(0.0f, 0.25f), Random.Range(0.0f, 0.25f));
            transform.position = origY + new Vector3(0, amplitude * Mathf.Sin((Time.time - pangle)), 0);
        }
    }

    //follows y = 20^x
    float GetFractionOfSpeed()
    {
        float diff = (Time.time - startTime) / (float)1.0;
        if(diff > 1.0)
        {
            return (float)1.0;
        } else
        {
            diff -= (float)1.0;

            return Mathf.Pow(20.0F, diff);
        }

    }

    void DeleteRock()
    {
        Destroy(gameObject);
    }

}
