using UnityEngine;
using System.Collections;

public class WallController : MonoBehaviour {

	public GameObject hand;
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
	private bool shouldCollideWithGround = false;

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
        Falling,
		None
	}

	StateAlias state = StateAlias.None;

	// Use this for initialization
	void Start() {
		//Debug.Log("rock controller reading this object for hand: " + hand.name);
		rb = this.GetComponent<Rigidbody>();
		shouldCollideWithGround = false;
		if (rb.mass == initialMass)
		{
			startTime = Time.time;

			// Rock spawns just below ground
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - gameObject.GetComponent<Renderer>().bounds.size.y - 0.3F, gameObject.transform.position.z);

			//gameObject.GetComponent<MeshCollider>().isTrigger = true;
			CI = hand.GetComponent<ControllerInterface>();
			changeState(StateAlias.FollowingHand);

			rb.velocity = new Vector3 (0.0f, 10.0f, 0.0f);//new Vector3(0.5F * pullVelocity.x, 3F * pullVelocity.y, 0.5F * pullVelocity.z);
		} else
		{
			changeState(StateAlias.None);
		}

	}

	void changeState(StateAlias targetState)
	{

		switch (targetState)
		{
		    case StateAlias.None:
			    Debug.Log("STATE NONE!");
			    CI.controllerEvents.TriggerUnclicked -= unfollowHand;
			    Invoke("DeleteRock", 10);
			    rb.isKinematic = false;
			    rb.useGravity = true;
			    break;
		    case StateAlias.FollowingHand:
			    Debug.Log("STATE FOLLOWING!");
                rb.velocity = pullVelocity;
			    rb.isKinematic = false;
			    rb.useGravity = false;
                Invoke("Held", .32f);
			    CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(unfollowHand);
			    break;
		    case StateAlias.Floating:
			    Debug.Log("STATE FLOATING!");
			    rb.isKinematic = false;
			    rb.useGravity = false;
			    break;
            case StateAlias.Falling:
                rb.isKinematic = false;
                rb.useGravity = true;
                Invoke("DeleteRock", 10);
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
		gameObject.GetComponent<Rigidbody> ().angularVelocity = new Vector3 (0.0f, 0.0f, 0.0f);
		//gameObject.transform.rotation = Quaternion.identity;

		shouldCollideWithGround = false;
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
		var velocity = col.relativeVelocity;

		if (col.gameObject.tag == "ground")
		{
			velocity *= 1.5F;
			//StartSummonParticles();
		}
		else if (col.gameObject.tag == "enemy")
		{
			velocity *= 4;
		}

		this.gameObject.GetComponent<SimpleFracture>().FractureAtPoint(col.contacts[0].point, velocity);

		if (col.gameObject.tag != "ground")
		{
			changeState(StateAlias.None);
			rb.WakeUp();
		}



		if (col.gameObject.tag == "ground" && (shouldCollideWithGround==false))
		{
			Debug.Log("entered oncolenter");
			//rb.detectCollisions = false;
			Physics.IgnoreCollision(col.collider, rb.GetComponent<Collider>(), true);
		}

		else if (col.gameObject.tag == "ground" && (shouldCollideWithGround == true)){
			Debug.Log("entered oncolenter2");
			//rb.detectCollisions = true;
			Physics.IgnoreCollision(col.collider, rb.GetComponent<Collider>(), false);
		}
	}

    void Held()
    {

        rb.velocity = new Vector3(0, 0, 0);
        Invoke("Falling", 10);

    }
    
    void Falling()
    {

        changeState(StateAlias.Falling);

    }

	void OnTriggerEnter(Collider other)
	{

		Debug.Log(other.gameObject.tag);

		if (other.gameObject.tag == "hand")
		{
			changeState(StateAlias.None);

			//SteamVR_Controller.Input(hand.GetComponent<EarthController>().GetDeviceIndex()).TriggerHapticPulse(3999);

			GameObject fist = other.gameObject;
			int i = fist.GetComponent<SpellController>().GetDeviceIndex();
			var origin = fist.GetComponent<SteamVR_TrackedObject>().origin ? fist.GetComponent<SteamVR_TrackedObject>().origin : fist.GetComponent<SteamVR_TrackedObject>().transform.parent;
			var device = SteamVR_Controller.Input(i);

			rb.velocity = Vector3.zero;
			var velocity = device.velocity * punchMultiplier;
			//Debug.Log("Velocity of punch: " + velocity);
			float punchWaveMultiplier = 0.075f * velocity.magnitude;

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

			hand = null;

		} else
		{
			Debug.Log("TRIGGER " + other.gameObject.tag);
		}

	}

	void OnTriggerExit(Collider other)
	{
		Debug.Log("On trigger exit");

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
		bool isHandNeg = hand.transform.position.y < gameObject.transform.position.y + gameObject.GetComponent<Renderer>().bounds.size.y * 3/4;

		float forceMultiplier = 1.0F - Mathf.Max(0.0F, Mathf.Min(pullVelocity.y / 3.0F, 0.2F));

		float diff = Mathf.Abs(hand.transform.position.y - gameObject.transform.position.y); // Mathf.Min(10F, 1.0F/Mathf.Abs(hand.transform.position.y - gameObject.transform.position.y));

		float multiplier = .1F ;
		float cMultiplier = 0.1F;// * forceMultiplier;

		if (diff > 0.1) {
			rb.velocity = new Vector3 (rb.velocity.x * 0.5F, rb.velocity.y - (isHandNeg ? multiplier * diff : -multiplier * diff) - cMultiplier * rb.velocity.y, rb.velocity.z * 0.5F);
		} else {
			rb.velocity = Vector3.zero;
		}

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
			amplitude = 0f;

			rb.velocity = Vector3.zero;
			transform.position = origY + new Vector3(0, 0, 0);
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
