using UnityEngine;
using System.Collections;

public class VoxelScript : MonoBehaviour {
    float opacity;
    Color c;
    void Start()
    {
        opacity = 1.0f;
        c = this.gameObject.GetComponent<Renderer>().material.color;
    }

    void Update()
    {
        if (opacity == 0)
        {
            Destroy();
        }
        if (Time.time > 3)
        {
            c = new Color(c.r, c.g, c.b, opacity);
            this.gameObject.GetComponent<Renderer>().material.color = c;
            opacity -= 0.2f * Time.deltaTime;
        }
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}
