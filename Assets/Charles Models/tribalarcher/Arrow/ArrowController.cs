using UnityEngine;
using System.Collections;

public class ArrowController : MonoBehaviour {
    private Rigidbody rb;
    private ParticleSystem dust;
    private TrailRenderer trail;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dust = GetComponent<ParticleSystem>();
        trail = GetComponent<TrailRenderer>();
        trail.enabled = false;
        Invoke("DestroySelf", 5);
    }

    // Update is called once per frame
    void Update () {
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ground")
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        if (other.tag == "MainCamera")
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            trail.enabled = true;
            dust.Play();

        }

    }


    void FixedUpdate()
    {
        if (rb.velocity != Vector3.zero)
           rb.rotation = Quaternion.LookRotation(rb.velocity);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
