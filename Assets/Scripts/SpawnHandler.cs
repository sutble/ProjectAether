using UnityEngine;
using UnityEngine.Events;
using System.Collections;



public class SpawnHandler : MonoBehaviour {


    public GameObject[] SpawnPoints;
    public GameObject player;
    public GameObject minion;

    public int waveLimit;                   // number of waves to spawn
    public float coolDownTime;              // cooldown between wavees

    public int waveSize;                    // enemies/wave
    public int waveSizeVariablity;          // wavsize +/-
    public int spawnRadius;                 // radius of circle within each spawnpoint

    public int waveNumber = 0;
    public int aiAliveNum;
    private int newWaveSize;

    private bool spawning = false;
    private bool coolingDown = false;



    // Use this for initialization
    void Start () {
        aiAliveNum = 0;
        if (waveSize < waveSizeVariablity)
        {
            waveSizeVariablity = waveSize;
        }

        coolingDown = true;
        StartCoroutine(WaveCoolDown(coolDownTime)); //inits wave1
    }

    void Update()                                           // waves are initiated in Start and Case 2
    {
        aiAliveNum = GameObject.FindGameObjectsWithTag("enemy").Length;

        if (spawning && aiAliveNum < newWaveSize)           // still spawning for this wave
        {
            AttemptSpawn();
        }

        if ((aiAliveNum == 0) && (waveNumber < waveLimit) && (!coolingDown))       // initiate wave if no enemies left, not at waveLimit, and not cooling down atm
        {                                                                           // wavenumber is current wavenumber (before initiating new wave), so should stop at limit -1
            coolingDown = true;
            StartCoroutine(WaveCoolDown(coolDownTime));
        }
    }

    IEnumerator WaveCoolDown(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        InitWave();
    }

    void AIKilled()
    {
        aiAliveNum--;
    }

    void OnEnable()
    {
        //      EventManager.StartListening ("Spawn", spawnListener);
        EventManager.StartListening("Death", AIKilled);
    }

    void OnDisable()
    {
        //      EventManager.StopListening ("Spawn", spawnListener);
        EventManager.StopListening("Death", AIKilled);
    }

    void InitWave()
    {
        spawning = true;
        newWaveSize = waveSize + UnityEngine.Random.Range(-1 * waveSizeVariablity, waveSizeVariablity);
        waveNumber++;
        coolingDown = false;
        Debug.Log("STARTING WAVE " + waveNumber + " with " + newWaveSize + " enemies");
    }

    void AttemptSpawn()
    {
        int spawnIndex = Random.Range(0, SpawnPoints.GetLength(0) - 1);
        Vector3 potentialSpawnBox = new Vector3(spawnRadius / 1.41f, 0.1f, spawnRadius / 1.41f);
        bool areaClear = !Physics.CheckBox(SpawnPoints[spawnIndex].transform.position, potentialSpawnBox);
        if (areaClear)
        {
            Vector3 spawnLocation = SpawnPoints[spawnIndex].transform.position + new Vector3(Random.Range(0, spawnRadius), 0, Random.Range(0, spawnRadius));
            GameObject spawnedAI = (GameObject)Instantiate(minion, spawnLocation, Quaternion.identity);
            Minion minionScript = spawnedAI.GetComponent<Minion>();
            minionScript.player = player;
            aiAliveNum++;
        }

        if (aiAliveNum == newWaveSize)                      // this was last spawn needed for this wave
        {
            spawning = false;
        }
    }
}
