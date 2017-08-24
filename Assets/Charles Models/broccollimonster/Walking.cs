using UnityEngine;
using System.Collections;
using System;

public class Walking : MonoBehaviour
{


    public GameObject player;
    private Animator brocolliAnimator;
    private GameObject brocolliMonster;

    public GameObject healthbar;
    public float healthpoints = 100;
    private float totalhealthpoints;
    private float healthunit;
    private float fireballdmg = 40;
    private float flamethrowerdamage = 15;


    public Rigidbody[] bodyparts;
    public Collider[] cols;
    public int parts;

    private Rigidbody rb;
    private Collider coli;



    private bool attack;
    public bool isDead = false;
    private bool isExploded = false;
    


    // Use this for initialization
    void Start()
    {
        totalhealthpoints = healthpoints;
        brocolliMonster = GetComponent<GameObject>();
        brocolliAnimator = GetComponent<Animator>();

        //store base health unit
        healthunit = healthbar.transform.localScale.x;

        rb = GetComponent<Rigidbody>();
        coli = GetComponent<Collider>();

        attack = true;

        bodyparts = GetComponentsInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
        int counter = 0;
        foreach(Rigidbody rig in bodyparts)
        {
            counter++;
            if (rig.gameObject.layer == 9)
            {
                rig.isKinematic = true;
            }
        }
        foreach(Collider col in cols)
        {
            if (col.gameObject.layer == 9)
            {
               col.isTrigger = true;
            }
        }
        

    }
    

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            this.transform.LookAt(player.transform);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

            if (Vector3.Distance(this.transform.position, player.transform.position) < (5.0f))
            {
                if (!isExploded)
                {
                    explode();
                } else
                {
                    // ??
                }
            }
            else
            {
                if (!brocolliAnimator.GetBool("closePunch") && !brocolliAnimator.GetBool("gethit"))
                    walking();
            }
        }
    }

    void die()
    {


        isDead = true;
        healthbar.GetComponent<MeshRenderer>().enabled = false;
        brocolliAnimator.Stop();

        foreach (Rigidbody rig in bodyparts)
        {
            if (rig.gameObject.layer == 9)
            {
                rig.isKinematic = false;
                rig.mass = 0;
                //Destroy(rig);

            }
        }
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer == 9)
            {
                col.isTrigger = false;
            }
            //col.enabled = false;
        }
        Destroy(rb);
        //Destroy(coli);

        //brocolliMonster.GetComponent<BoxCollider>().enabled = true;

        //Destroy(transform.root.gameObject, 10.0f);
        //transform.tag = "Pickupable";
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "projectile")
        {
            Invoke("die", 0);
            //die();
        }
    }

    void explode()
    {
        player.GetComponentInParent<Player>().doDamage(10.0f);
    }

    void attacking()
    {
        brocolliAnimator.SetBool("closePunch", true);
        Invoke("stopPunch", 1.4f);
    }

    void stopPunch()
    {
        brocolliAnimator.SetBool("closePunch", false);
    }

    void takehit()
    {
        brocolliAnimator.SetBool("gethit", true);
        Invoke("stopHit", 0.35f);
    }


    void stopHit()
    {
        brocolliAnimator.SetBool("gethit", false);
    }

    void walking()
    {
        //if (Vector3.Distance(this.transform.position, player.transform.position) < 60.0f)
        //{
            this.transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime * (1.0f));
            brocolliAnimator.SetFloat("xBlend", 2.0f, 0.1f, Time.deltaTime);
        //}

    }



}







