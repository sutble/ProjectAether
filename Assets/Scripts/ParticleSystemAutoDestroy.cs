using UnityEngine;
using System.Collections;

public class ParticleSystemAutoDestroy : MonoBehaviour {

    private ParticleSystem ps;
    public float delay = 3.0f;


    public void Start()
    {
        ps = GetComponent<ParticleSystem>();
        Invoke("DeleteSelf", delay);
    }

    void DeleteSelf()
    {
        DestroyImmediate(gameObject);
    }

    

    public void Update()
    {
        if (ps)
        {
            if (!ps.IsAlive())
            {
                DestroyImmediate(gameObject);
            }
        }
    }
    
}
