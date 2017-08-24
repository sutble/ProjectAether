using UnityEngine;
using System.Collections;

public class EarthWavePillarController : MonoBehaviour {

    public GameObject summonParticle;

    void Start()
    {
        //StartSummonParticles();
    }

    void OnCollisionEnter(Collision col)
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void StartSummonParticles()
    {
        // Particle effect of pillar summon
        GameObject rockSummon = (GameObject)Instantiate(summonParticle);
        rockSummon.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        rockSummon.transform.localRotation = transform.localRotation;
    }
}
