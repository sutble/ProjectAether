using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EarthwaveController : MonoBehaviour {

    public GameObject[] earthWaveChildModels;
    private List<Transform> earthWaves;
    private List<Vector3> childStartPos;
    public Vector3 direction;
    public Vector3 origin;
    public int WAVE_RANGE = 20;

    void Start() {
        StartCoroutine(EarthWaveCreation());
        Debug.Log("origin: " + origin);
        Invoke("DeleteSelf", 8);
    }

    IEnumerator EarthWaveCreation()
    {
        Vector3 newPosition = new Vector3(origin.x, origin.y, origin.z);
        for (int i = 0; i < WAVE_RANGE; i++)
        {
            newPosition += direction;
            GameObject earthWaveChild = earthWaveChildModels[Random.Range(0, earthWaveChildModels.Length)];
            GameObject placeholderEarthwaveChild = (GameObject)Instantiate(earthWaveChild, newPosition, Quaternion.identity);
            placeholderEarthwaveChild.transform.forward = direction;
            //placeholderEarthwaveChild.transform.rotation = transform.rotation;
            //placeholderEarthwaveChild.transform.localEulerAngles = new Vector3(placeholderEarthwaveChild.transform.localEulerAngles.x, placeholderEarthwaveChild.transform.localEulerAngles.y, placeholderEarthwaveChild.transform.localEulerAngles.z - 35);

            placeholderEarthwaveChild.transform.parent = gameObject.transform;
            yield return new WaitForSeconds(0.15f);
        }
    }

    void DeleteSelf()
    {
        DestroyImmediate(gameObject);
    }

    /*
    IEnumerator EarthWaveCreation2(Vector3 directionVector, Vector3 earthwaveOrigin)
    {
        float increment = 0;
        for (int i = 0; i < WAVE_RANGE; i++)
        {
            position = new Vector3(origin.x, RightCI.room.transform.position.y - 1.5f, earthwaveOrigin.z);
            position += new Vector3(Random.value * 0.1f, 0.0f, Random.value * 0.1f);
            directionVector.y = 0;
            position += ((directionVector * 1.5f) + directionVector * i * 3.0f);
            placeholderEarthwave = CreateProjectile(earthwaveObject, earthwaves);
            //placeholderEarthwave.GetComponent<EarthwaveController>().hand = RightController;

            Vector3 scale = placeholderEarthwave.transform.localScale;
            float rand = (Random.value * 1.0f);
            scale.y = scale.y * 3;
            scale.x = (scale.x * 0.5f) + rand;
            scale.z = ((scale.z * 0.5f) + rand) * 2.0F;
            placeholderEarthwave.transform.localScale = scale;
            //Debug.Log("instantiating rock");
            increment += 1;

            placeholderEarthwave.transform.forward = directionVector;

            yield return new WaitForSeconds(0.05f);


        }
    }
    */

}
