using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class EnemySettings                  //Settings for this type of enemy for this specific wave
{
    public GameObject enemyPrefab;
    public int spawnCount;
    public int countVariablity;
}

[System.Serializable]
public class Wave                           //Settings for this wave
{
    public EnemySettings[] enemySettings;   // Array of enemyParamter structs
    public float coolDownAfter;             // cooldown after this wave
}


public class ExpertSpawnHandler : MonoBehaviour
{
    //These need to be set in editor
    public Wave[] definedWaves;                     //Array of waveParameter structs
    public GameObject[] SpawnPoints;
    public GameObject player;
    public int spawnRadius;                 // radius of circle within each spawnpoint

    //These are displayed in editor for reference
    public int waveNumber; 
    public int aiAliveNum;

    //These guide the Update() function to determine whether a wave is spawning
    private int newWaveSize;
    private bool spawning = false;
    private bool coolingDown = false;

    //These guide the Spawning functions (ie. InitWave(), AttemptSpawn(), and SelectEnemytoSpawn())
    private Wave currentWave;                       
    Dictionary<int, int> currentWaveDict;
    private int[] currentWaveSpawnCounts;
    private GameObject currentEnemytoSpawn;

    

    void Start()                                             //init wave1
    {
        aiAliveNum = 0;                            
        waveNumber = 0;
        currentWave = definedWaves[waveNumber];
        coolingDown = true;                             
        StartCoroutine(WaveCoolDown(currentWave.coolDownAfter));
    }


    /// -----------------------------------------------------------------
    /// Checks various bools every frame to determine whether to keep spawning or whether to initiate cooldown + new wave
    /// -----------------------------------------------------------------
   
    void Update()                                           // waves are initiated in Start and Case 2
    {
        
        aiAliveNum = GameObject.FindGameObjectsWithTag("enemy").Length;

        if (spawning && aiAliveNum < newWaveSize)           // still spawning for this wave
        {
            AttemptSpawn();
        }

        if ((aiAliveNum == 0) && (waveNumber < definedWaves.Length) && (!coolingDown))       // initiate wave if no enemies left, not at waveLimit, and not cooling down atm
        {                                                                           // wavenumber is current wavenumber (before initiating new wave), so should stop at limit -1
            coolingDown = true;
            StartCoroutine(WaveCoolDown(currentWave.coolDownAfter));
        }
    }


    ///Initiate cooldown + new wave
    IEnumerator WaveCoolDown(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        InitWave();
    }

    /// ------------------------------------------------------------------------
    /// InitWave() 
    /// ------------------------------------------------------------------------
    /// Goal is to setup booleans and variables to start a new wave. This means it also must reset everything from previous wave
    ///     -currenWaveDict is pairs of (enemyType, number to spawn [fixed])
    ///     -currentWaveSpawnCounts[enemyType] is (number of enemytype spawned)
    ///     -newWaveSize = Summation(number to spawn for each type)
    ///         (number to spawn for each type) = user specfied spawnCount for type + random.range(user specified variablity in count for this type)
    /// ------------------------------------------------------------------------


    void InitWave()
    {
        coolingDown = false;
        waveNumber++;

        spawning = true;
        currentWaveDict= new Dictionary<int, int>();
        currentWaveSpawnCounts = new int[currentWave.enemySettings.Length];
        newWaveSize = 0;

        for (int i = 0; i < currentWave.enemySettings.Length; i++)          //cycle through all user-specified enemy types in this wave
        {
            //sum each enemy type
            int enemyofTypeCount = currentWave.enemySettings[i].spawnCount;
            int enemyofTypeVariability = currentWave.enemySettings[i].countVariablity;

            if (enemyofTypeCount < enemyofTypeVariability)
            {
                enemyofTypeVariability = enemyofTypeCount;
            }

            int newWaveDelta = enemyofTypeCount + Random.Range(-1 * enemyofTypeVariability, enemyofTypeVariability + 1);

            newWaveSize += newWaveDelta;
            currentWaveDict.Add(i, newWaveDelta);
            currentWaveSpawnCounts[i] = 0;
        }

        Debug.Log("STARTING WAVE " + waveNumber + " with " + newWaveSize + " enemies");
    }




    /// ---------------------------------------------------------------------
    /// AttemptSpawn()
    /// ---------------------------------------------------------------------
    /// Basically picks a random spawnPoint from list of available user specified spawnPoints and tests if it is clear
    ///     if clear, 
    ///         find an enemy to spawn
    ///         spawn it at this location
    ///         check if wave spawning should end
    ///     else 
    ///         do nothing
    /// -----------------------------------------------------------------------    

    void AttemptSpawn()                                 
    {
        //Test SpawnPoint
        int spawnIndex = Random.Range(0, SpawnPoints.GetLength(0));
        Vector3 potentialSpawnBox = new Vector3(spawnRadius / 1.41f, 0.1f, spawnRadius / 1.41f);
        bool areaClear = !Physics.CheckBox(SpawnPoints[spawnIndex].transform.position, potentialSpawnBox);

        //If clear:
        if (areaClear)
        {
            //Assign spawnPoint
            Vector3 spawnLocation = SpawnPoints[spawnIndex].transform.position + new Vector3(Random.Range(0, spawnRadius), 0, Random.Range(0, spawnRadius));

            //Find an enemy type to spawn -> assigns to currentEnemytoSpawn
            SelectEnemyToSpawn();
            
            //Spawn it
            GameObject spawnedAI = (GameObject)Instantiate(currentEnemytoSpawn, spawnLocation, Quaternion.identity);

            //Give it a target
            Minion minionScript = spawnedAI.GetComponent<Minion>();
            minionScript.player = player;

            //Update this
            aiAliveNum++;

            //Stop spawning if reached quota
            if (aiAliveNum == newWaveSize)                      // this was last spawn needed for this wave
            {
                spawning = false;
            }
        }
    }



    ///-------------------------------------------------------------------
    /// SelectEnemyToSpawn()
    /// -----------------------------------------------------------------
    /// Attempts to find an enemy type to spawn
    ///     Picks a random enemy type from userdefined choices for wave
    ///         if the quota for this type has not been reached, 
    ///             assign the currentEnemytoSpawn gameObject
    ///             update the currentWaveSpawnCounts array
    ///         else
    ///             try again
    /// --------------------------------------------------------------------         

    void SelectEnemyToSpawn()
    {
        //pick enemy index
        int enemyType = Random.Range(0, currentWave.enemySettings.Length);

        //figure out how many of this type have been spawned
        int spawnedCount = currentWaveSpawnCounts[enemyType];

        //compare to count needed. if still need to meet quota, assign enemyPrefab
        if (spawnedCount < currentWaveDict[enemyType])
        {
            currentEnemytoSpawn = currentWave.enemySettings[enemyType].enemyPrefab;
            spawnedCount++;
            currentWaveSpawnCounts[enemyType] = spawnedCount;
            return;
        }

        // try again
        else
        {
            SelectEnemyToSpawn();
        }
        
    }




    //-----------------------------------
    //These are listeners

    void AIKilled()
    {
        aiAliveNum--;
    }

    void OnEnable()         //Listen for Minion.Die()
    {
        EventManager.StartListening("Death", AIKilled);
    }

    void OnDisable()
    {
        EventManager.StopListening("Death", AIKilled);
    }
}
