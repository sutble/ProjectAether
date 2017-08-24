using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EarthWaveChildController : MonoBehaviour {
    public float _shootFactor = 5;
    public GameObject summonParticle;

    [SerializeField]
    private List<Rigidbody> pillarRBs;


    void Start () {
        transform.position = FindGround();
        foreach (Transform pillar in transform)
        {
            pillarRBs.Add(pillar.gameObject.GetComponent<Rigidbody>());
        }
        StartCoroutine(ShootPillars());
        StartSummonParticles();
    }

    private Vector3 FindGround()
    {
        RaycastHit[] UpHits;
        RaycastHit[] DownHits;
        UpHits = Physics.RaycastAll(transform.position, Vector3.up, 100.0F);
        DownHits = Physics.RaycastAll(transform.position, Vector3.down, 100.0F);
        var hits = new RaycastHit[UpHits.Length + DownHits.Length];
        UpHits.CopyTo(hits, 0);
        DownHits.CopyTo(hits, UpHits.Length);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.tag == "ground")
            {
                return hit.point;
            }
        }
        return transform.position;
    }

    IEnumerator ShootPillars()
    {
        foreach (Rigidbody pillarRB in pillarRBs)
        {
            pillarRB.velocity = transform.up * _shootFactor;
            StartCoroutine(HaltPillar(pillarRB));
            StartCoroutine(RetractPillar(pillarRB));
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator RetractPillar(Rigidbody pillarRB)
    {
        yield return new WaitForSeconds(5.0f);
        pillarRB.isKinematic = false;
        pillarRB.velocity = -transform.up * _shootFactor;
    }

    /*
    void HaltPillars()
    {
        foreach (Rigidbody pillarRB in pillarRBs)
        {
            if (pillarRB.velocity.y > 0)
            {
                pillarRB.velocity = new Vector3(pillarRB.velocity.x * 0.96f, pillarRB.velocity.y * 0.96f, pillarRB.velocity.z * 0.96f);
            }
            else
            {
                pillarRB.velocity = new Vector3(0, 0, 0);
            }
        }
    }*/

    IEnumerator HaltPillar(Rigidbody pillarRB)
    {
        yield return new WaitForSeconds(0.25f);
        pillarRB.velocity = new Vector3(0, 0, 0);
    }

    void StartSummonParticles()
    {
        // Particle effect of pillar summon
        GameObject rockSummon = (GameObject)Instantiate(summonParticle);
        rockSummon.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        rockSummon.transform.localRotation = transform.localRotation;
    }
}
