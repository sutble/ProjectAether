using UnityEngine;
using System.Collections;

public class PillarController : MonoBehaviour {


    public GameObject hand;
    public SteamVR_TrackedObject fisty;
    public bool isSelected;
    private bool shouldCollideWithGround = false;

    public float initialMass = 10.0f; //Change this
    public Vector3 pullVector;

    private Rigidbody rb;

    private bool onetime = false;
    private bool isPunching = false;

    private Vector3 punchMotion;

    private float punchMultiplier = 1;

    private float startTime;

    public GameObject summonParticle;

    public StateAlias state = StateAlias.None;

    public enum StateAlias
    {
        Held,
        Moving,
        Falling,
        None
    }

    public void ChangeState(StateAlias target)
    {
        //Debug.Log("changing state to " + target);
        hand.GetComponent<ControllerInterface>().controllerEvents.TriggerUnclicked -= new ControllerClickedEventHandler(FallThroughEarth);
        //Debug.Log(hand + " trigger is unclicked");
        rb.isKinematic = false;
        shouldCollideWithGround = true;
        state = target;

        switch (target)
        {
            case StateAlias.Held:
                shouldCollideWithGround = false;
                hand.GetComponent<ControllerInterface>().controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(FallThroughEarth);
                //Debug.Log(hand + " trigger is clicked, falling state active");
                rb.useGravity = false;
                break;
            case StateAlias.Moving:
                //RotatePillar(Vector3.Normalize(pullVector));
                //rb.transform.rotation = Quaternion.LookRotation(pullVector);
                rb.velocity = pullVector;
                //Debug.Log("pull is " + rb.velocity);
                rb.useGravity = false;
                Invoke("Held", .4f);
                Invoke("FallThroughEarth", 10);
                break;
            case StateAlias.Falling:
                shouldCollideWithGround = false;
                rb.useGravity = true;
                Invoke("DeleteRock", 10);
                break;
            case StateAlias.None:
                rb.useGravity = true;
                Invoke("DeleteRock", 10);
                break;
        }
    }

    void FallThroughEarth()
    {
        ChangeState(StateAlias.Falling);
    }

    void FallThroughEarth(object sender, ControllerClickedEventArgs e)
    {
        Invoke("FallThroughEarth", 10);
    }

    /*void RotatePillar(Vector3 direction)
    {
        float halfSize = GetComponent<Renderer>().bounds.size.y / 2;
        rb.transform.position += new Vector3(0, halfSize, 0);
        rb.transform.up = direction;
        rb.transform.position -= halfSize*direction;
    }*/


    void Start()
    {        
        isSelected = true;
        rb = this.GetComponent<Rigidbody>();
        startTime = Time.time;

        // Pillar spawns just below ground
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - gameObject.GetComponent<Renderer>().bounds.size.y, gameObject.transform.position.z);

        // hack, delayed summon of particles
        Invoke("StartSummonParticles", 1);

        if(rb.mass >= initialMass)
        {
            ChangeState(StateAlias.Held);
        } else
        {
            ChangeState(StateAlias.None);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(state == StateAlias.Held)
        {
            Hold();
            ChangeState(StateAlias.Moving);
        } else if(state == StateAlias.Moving)
        {
            Move();
        }
    }

    void Hold()
    {
        rb.velocity -= 0.2F * rb.velocity;
        rb.angularVelocity -= 0.2F * rb.angularVelocity;
    }

    void Held()
    {
        rb.angularVelocity = new Vector3(0, 0, 0);
    }

    void Move()
    {
        rb.velocity -= 0.1F * rb.velocity;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag != "ground")
        {
            ChangeState(StateAlias.None);
            rb.WakeUp();
        }



        if (col.gameObject.tag == "ground" && (shouldCollideWithGround==false))
        {
            //Debug.Log("entered oncolenter");
            //rb.detectCollisions = false;
            Physics.IgnoreCollision(col.collider, rb.GetComponent<Collider>(), true);
        }

        else if (col.gameObject.tag == "ground" && (shouldCollideWithGround == true)){
            //Debug.Log("entered oncolenter2");
            //rb.detectCollisions = true;
            Physics.IgnoreCollision(col.collider, rb.GetComponent<Collider>(), false);
        }
    }

    /*
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "hand")
        {
            //Debug.Log("Pillar hit!");
            ChangeState(StateAlias.None);
            //SteamVR_Controller.Input(hand.GetComponent<EarthController>().GetDeviceIndex()).TriggerHapticPulse(3999);

            isPunching = true;

            GameObject fist = other.gameObject;

            fisty = other.gameObject.GetComponent<SteamVR_TrackedObject>();
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)fisty.index);

            //int i = fist.GetComponent<SpellController>().GetDeviceIndex();
            var origin = fisty.GetComponent<SteamVR_TrackedObject>().origin ? fisty.GetComponent<SteamVR_TrackedObject>().origin : fisty.GetComponent<SteamVR_TrackedObject>().transform.parent;
            //var device = SteamVR_Controller.Input(i);

            var velocity = device.velocity * punchMultiplier;
            var angularVelocity = device.angularVelocity;

            if (origin != null)
            {
                angularVelocity = origin.TransformVector(device.angularVelocity);
                velocity = origin.TransformVector(device.velocity * punchMultiplier);
            }
            else
            {
                angularVelocity = device.angularVelocity;
                velocity = device.velocity * punchMultiplier;
            }

            //device.TriggerHapticPulse(500);

            if(velocity.magnitude > punchMultiplier*2.0F)
            {

                rb.velocity = velocity*5;
                rb.angularVelocity = angularVelocity;
                rb.maxAngularVelocity = rb.angularVelocity.magnitude;


                //rb.isKinematic = false;
                //rb.useGravity = true;

                rb.WakeUp();

                //Debug.Log(velocity.magnitude);

                this.gameObject.GetComponent<SimpleFracture>().FractureAtPoint(origin.position, velocity * 1);

                // punchMotion = -hand.transform.up;
                //Vector3 handVelocity = hand.GetComponent<EarthController>().getVelocity();
                //Debug.Log("hand velocity: " + handVelocity);

                deSelect();
                
                    Debug.Log("Entered in triggerenter");
                    //Physics.IgnoreCollision(other.GetComponent<Collider>(), rb.GetComponent<Collider>());
                    //shouldCollideWithGround = true;

            } else
            {
                //Debug.Log("Force too low: " + velocity.magnitude);
            }
        }
    }*/

    void StartSummonParticles()
    {
        // Particle effect of rock summon
        GameObject rockSummon = (GameObject)Instantiate(summonParticle);
        Vector3 summonParticlePosition = new Vector3(this.transform.position.x, rockSummon.transform.position.y, this.transform.position.z);
        rockSummon.transform.position = summonParticlePosition;
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
