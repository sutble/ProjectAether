using UnityEngine;
using System.Collections;
using System;

public class TribalArcherController : MonoBehaviour
{
    public GameObject player;
    private Animator archerAnimator;
    private GameObject archerMinion;
    public int deadLayer;

    public Rigidbody[] bodyparts;
    public Collider[] cols;
    public int parts;

    private Rigidbody rb;
    private Collider coli;

    public float minDistance;
    private float archerRange;

    public bool isDead = false;
    float lockPos = 0;

    private Renderer renderer;
    private Material mat;
    private NavMeshAgent agent;

    public bool isInRange = false;
    public bool isShooting = false;

    public GameObject arrowModel;
    public Transform launchPos;
    private float loadArrow;

    //Audio Related
    private AudioSource aSource;
    public AudioClip[] HitSounds;
    public AudioClip WalkLoop;
    public AudioClip BowSound;


    // Set Up Model
    void Start()
    {
        // grab material and renderer
        renderer = GetComponentInChildren<Renderer>();
        mat = renderer.material;

        // Grab the parts of the object
        archerMinion = GetComponent<GameObject>();
        archerAnimator = GetComponent<Animator>();

        // Define rigidbody and collider privately
        rb = GetComponent<Rigidbody>();
        coli = GetComponent<Collider>();

        // Grab both in array form for loop
        bodyparts = GetComponentsInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();

        // For each body part, set to kinematic
        int counter = 0;
        foreach (Rigidbody rig in bodyparts)
        {
            counter++;
            if (rig.gameObject.layer <= 12)
            {
                rig.isKinematic = true;
            }
        }
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer <= 12)
            {
                col.isTrigger = true;
            }
        }
        cols[0].isTrigger = false;
        
        agent = GetComponent<NavMeshAgent>();

        archerRange = minDistance - (UnityEngine.Random.value * 5);

        aSource = gameObject.GetComponent<AudioSource>();
    }

    // Update model per frame
    void Update()
    {
        if (!isDead)
        {
            if (GetComponent<NavMeshAgent>().enabled == true && !(Vector3.Distance(transform.position, player.transform.position) <= archerRange))
            {
                Moving();
            }
            else
            {
                if (!isInRange)
                {
                    isInRange = true;
                    StopMoving();
                }
                else if (!isShooting)
                {
                    isShooting = true;
                    StartCoroutine(Shooting());
                }
            }
        }
    }

    void Die()
    {
        isDead = true;
        archerAnimator.Stop();
        gameObject.layer = deadLayer;
        aSource.spatialBlend = 0.25f; //Change blend to hear death better
        aSource.PlayOneShot(HitSounds[UnityEngine.Random.Range(0, HitSounds.Length)], 1.0f); //Play a random death sound
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<NavMeshAgent>().enabled = false;
        
        foreach (Rigidbody rig in bodyparts)
        {
            rig.gameObject.layer = deadLayer;
            if (rig.gameObject.layer <= 12)
            {
                rig.isKinematic = false;
                rig.mass = 0;
            }
        }
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer <= 12)
            {
                col.isTrigger = false;
            }
        }
        Destroy(rb);
        DestroySelf();
    }

    // I don't know what this does but it's a relic from broccolini
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "projectile")
        {
            Die();
        }
    }

    void DestroySelf()
    {
        Destroy(gameObject, 5);
    }

    // Walk speed is set here
    void Moving()
    {
        isInRange = false;
        agent.destination = player.transform.position;
        archerAnimator.SetBool("isMoving", true);
        if (!aSource.isPlaying) {
            aSource.loop = true;
            aSource.clip = WalkLoop;
            aSource.Play();
        }

    }

    void StopMoving()
    {
        agent.velocity = Vector3.zero;
        agent.Stop();
        archerAnimator.SetBool("isInRange", isInRange);
        archerAnimator.SetBool("isMoving", false);
        aSource.Stop();
        aSource.loop = false;
    }

    IEnumerator Shooting()
    {
        gameObject.transform.LookAt(player.transform);
        archerAnimator.SetTrigger("readyToDraw");
        loadArrow = UnityEngine.Random.value * 3 + 2;
        //Play audio when animation begins
        aSource.clip = BowSound;
        aSource.Play();
        yield return new WaitForSeconds(loadArrow);
        LaunchArrow();
    }

    void LaunchArrow()
    {
        archerAnimator.SetTrigger("readyToShoot");
        isShooting = false;

        GameObject arrow = (GameObject)Instantiate(arrowModel, launchPos.position, Quaternion.identity);


        arrow.transform.forward = gameObject.transform.forward;
        Vector3 arrowDirection = arrow.transform.forward * loadArrow * 350;

        //Ray ray = new Ray(arrow.transform.position, arrowDirection);

        //arrow.GetComponent<ArrowController>().destination = arrowDirection;
        //arrow.GetComponent<ArrowController>().speed = loadArrow;

        arrow.GetComponent<Rigidbody>().AddForce(arrowDirection);
        arrow.GetComponent<Rigidbody>().AddForce(Vector3.up * 250);

        //arrow.transform.up = Vector3.Slerp(arrow.transform.up, arrow.GetComponent<Rigidbody>().velocity.normalized, Time.deltaTime);

    }

}







