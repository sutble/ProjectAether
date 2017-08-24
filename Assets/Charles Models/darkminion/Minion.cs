using UnityEngine;
using System.Collections;
using System;

public class Minion : MonoBehaviour
{


	public GameObject player;
	private Animator minionAnimator;
	private GameObject minionMonster;
    public ParticleSystem explosion;
    public int deadLayer;


	public Rigidbody[] bodyparts;
	public Collider[] cols;
	public int parts;

	private Rigidbody rb;
	private Collider coli;

    public float minDistance;


	public bool isDead = false;
	float lockPos = 0;

    private bool exploded = false;
    public bool isCrawling = false;

    private Renderer renderer;
    private Material mat;
    private float shineMultiplier = 0;
    private bool shineEnabled = false;
    private NavMeshAgent agent;


    // Set Up Model
    void Start()
	{
        // grab material and renderer
        renderer = GetComponentInChildren<Renderer>();
        mat = renderer.material;

        // Grab the parts of the object
        minionMonster = GetComponent<GameObject>();
		minionAnimator = GetComponent<Animator>();

		// Define rigidbody and collider privately
		rb = GetComponent<Rigidbody>();
		coli = GetComponent<Collider>();

		// Grab both in array form for loop
		bodyparts = GetComponentsInChildren<Rigidbody>();
		cols = GetComponentsInChildren<Collider>();

		// For each body part, set to kinematic
		int counter = 0;
		foreach(Rigidbody rig in bodyparts)
		{
			counter++;
			if (rig.gameObject.layer <= 12)
			{
				rig.isKinematic = true;
			}
		}
		foreach(Collider col in cols)
		{
			if (col.gameObject.layer <= 12)
			{
				col.isTrigger = true;
			}
		}
        cols[0].isTrigger = false;

        //Randomize if minion is crawling or not
        if (UnityEngine.Random.value > 0.5)
        {
            isCrawling = false;
            minionAnimator.SetBool("isCrawling", false);
        }
        else
        {
            isCrawling = true;
            BoxCollider collider = gameObject.GetComponent<BoxCollider>();
            Vector3 crawlingHeight = new Vector3(collider.center.x, collider.center.y * 1/3, collider.center.z);
            collider.center = crawlingHeight;
            Vector3 crawlingSize = new Vector3(collider.size.x * 2, collider.size.y * 1 / 2, collider.size.z * 8);
            collider.size = crawlingSize;
            minionAnimator.SetBool("isCrawling", true);


        }

        agent = GetComponent<NavMeshAgent>();
    }

    // Update model per frame
    void Update()
	{
		if (!isDead)
		{
            // Face player object
            //this.transform.LookAt(player.transform);

            // Reset position - not necessary here
            //transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            //transform.localEulerAngles = new Vector3(0, 0, 0);

            // shine white
            if (shineEnabled)
            {
                mat.SetColor("_EmissionColor", Color.white * shineMultiplier);
                shineMultiplier += 0.01f;
            }

            // Walk
            walking();
		}
        else
        {
            if (shineEnabled)
            {
                mat.SetColor("_EmissionColor", Color.white * shineMultiplier);
                shineMultiplier -= 0.01f;
            }
        }
	}

	void die()
	{
        isDead = true;
		minionAnimator.Stop();
        gameObject.layer = deadLayer; 
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<NavMeshAgent>().enabled = false;
        

		foreach (Rigidbody rig in bodyparts)
		{
            rig.gameObject.layer = deadLayer; 
			if (rig.gameObject.layer <= 12)
			{
				rig.isKinematic = false;
				rig.mass = 0;
				//Destroy(rig);

			}
		}
		foreach (Collider col in cols)
		{
			if (col.gameObject.layer <= 12)
			{
				col.isTrigger = false;
			}
			//col.enabled = false;
		}
		Destroy(rb);
        destroySelf();
        //Destroy(coli);
    }

	// I don't know what this does but it's a relic from broccolini
	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "projectile")
		{
			//Invoke("die", 0);
			die();
		}
	}

    void explode()
    {
		Destroy(Instantiate(explosion, transform.position, transform.rotation), 7);
        destroySelf();
    }

    void destroySelf()
    {
        Destroy(gameObject, 5);

    }

    // Walk speed is set here
    void walking()
	{
        // Die within 5f of player
        
        if (GetComponent<NavMeshAgent>().enabled == true && !(Vector3.Distance(transform.position, player.transform.position) <= minDistance))
        {
            agent.destination = player.transform.position;

            if (isCrawling)
            {
                // crawling animation
                //minionAnimator.SetFloat("xBlend", 0, 0.1f, Time.deltaTime);
            }
            else
            {
                // walking animation
                //minionAnimator.SetFloat("xBlend", 1, 0.1f, Time.deltaTime);
            }

        }
        /*// Die within 5f of player
        if (!(Vector3.Distance(transform.position, player.transform.position) <= minDistance))
    {
        Vector3 towardPlayer = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        this.transform.position = Vector3.MoveTowards(transform.position, towardPlayer, Time.deltaTime * (1.0f));

    }*/
        else
        {
            if (!exploded)
            {
                agent.velocity = Vector3.zero;
                agent.Stop();
                
                if (isCrawling)
                {
                    //minionAnimator.SetFloat("xBlend", 3, 0.2f, Time.deltaTime);
                    minionAnimator.SetTrigger("willExplode");
                    Invoke("willDie", 2.0f);

                }
                else
                {
                    //minionAnimator.SetFloat("xBlend", 3, 0.1f, Time.deltaTime);
                    minionAnimator.SetTrigger("willExplode");
                    Invoke("willDie", 2.0f);
                }
                mat.EnableKeyword("_EMISSION");
                shineEnabled = true;
            }
        }
    }

    void willDie()
    {
        if(!isDead)
        {
            die();
            exploded = true;
            explode();
        }
    }

}







