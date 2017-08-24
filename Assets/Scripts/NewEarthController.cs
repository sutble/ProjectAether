using UnityEngine;
using System.Collections;
using Edwon.VR.Gesture;
using Edwon.VR.Input;

//public delegate void SpellDelegate();

public class NewEarthController : SpellController {

    // Controller objects and CI
    public GameObject LeftController;
    public GameObject RightController;
    private GameObject OrientController;
    private ControllerInterface GetCI;
    private ControllerInterface LeftCI;
    private ControllerInterface RightCI;
    public SteamVR_TrackedObject Right;

    public bool isLeft;
    public bool Continuous;

    private bool haptic;

    // Summonable objects and lists (for randomization)
    public GameObject rockObject;
    public GameObject pillarObject;
    public GameObject wallObject;
    public GameObject boulderObject;
    public GameObject earthwaveObject;

    public GameObject[] rocks;
    public GameObject[] pillars;
    public GameObject[] walls;
    public GameObject[] boulders;
    public GameObject[] earthwaves;

    GameObject placeholderRock;
    GameObject placeholderPillar;
    GameObject placeholderWall;
    GameObject placeholderBoulder;
    GameObject placeholderEarthwave;

    //Magic
    public int RP = 7;
    public int W = 7;
    public int S = 14;
    public int Magic;

    public bool cooldown;

    // Position variables 
    private Vector3 summonPos;
    private Vector3 wallPosR;
    private Vector3 wallPosL;
    private Vector3 position;
    private Vector3 positionL;
    private Vector3 trackedR;
    private Vector3 trackedL;
    private Vector3 velocityR;
    private Vector3 pillarp = new Vector3(0, 10F, 0);

    // Trigger Settings
    private bool TriggerR = false;
    private bool TriggerL = false;
    private bool MergedRet = false;

    // Trails and Colors
    private TrailRenderer RightTrail;
    private TrailRenderer LeftTrail;
    public Color Correct;

    //Audio Stuff
    private GameObject Rumble;


    #region GestureEventHandler

    // Subscribe to the events, initialize
    void OnEnable ()
    {
        // Set CI for each controller
        LeftCI = LeftController.GetComponent<ControllerInterface>();
        RightCI = RightController.GetComponent<ControllerInterface>();

        Debug.Log("Gestures intialized");
        VRGestureManager.GestureDetectedEvent += OnGestureDetected;
        VRGestureManager.GestureRejectedEvent += OnGestureRejected;

        // Adds reticle lock and unlock
        RightCI.controllerEvents.TriggerClicked += new ControllerClickedEventHandler(LockReticleR);
        RightCI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(UnlockReticleR);
        LeftCI.controllerEvents.TriggerClicked += new ControllerClickedEventHandler(LockReticleL);
        LeftCI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(UnlockReticleL);

        isLeft = false;

        // Get trails
        RightTrail = RightController.GetComponent<TrailRenderer>();
        LeftTrail = LeftController.GetComponent<TrailRenderer>();

        Continuous = false;

        //Get Rumble object
        Rumble = GameObject.Find("MUX");


    }

    // Unsubscribe from the events
    void OnDisable()
    {
        VRGestureManager.GestureDetectedEvent -= OnGestureDetected;
        VRGestureManager.GestureRejectedEvent -= OnGestureRejected;
    }

    // Called when a gesture is detected  	
    void OnGestureDetected(string gestureName, double confidence)
    {
        if (cooldown == true)
        {
            cooldown = false;
        }
        else
        {

            switch (gestureName)
            {
                case "Raise":
                    //RightTrail.material.SetColor("_EmissionColor", Correct);
                    if (MergedRet == true && TriggerL == true)
                    {
                        Debug.Log("summon boulder");
                        summonBoulder();
                        TriggerL = false;
                        MergedRet = false;
                        isLeft = false;
                        Continuous = false;
                    }
                    else
                    {
                        //Debug.Log(BothPointersAreCloseTogether());
                        Debug.Log("did raise");
                        Debug.Log("hand is left: " + isLeft);
                        summonRock();
                        isLeft = false;
                        Continuous = false;
                    }
                    break;
                case "Twist":
                    if (TriggerL == true && TriggerR == true)
                    {
                        Debug.Log("Did Twist but both triggers pressed");
                        //summonWall();
                        isLeft = false;
                        TriggerL = false;
                        Continuous = false;
                    }
                    else if (MergedRet == true)
                    {
                        isLeft = false;
                    }
                    else
                    { 
                        Debug.Log("did twist");
                        isLeft = false;
                        TriggerL = false;
                        Continuous = false;
                        //Debug.Log("Continuous is: " + Continuous);
                        summonPillar();
                    }
                    break;
                case "TwistL":
                    if (TriggerL == true && TriggerR == true)
                    {
                        Debug.Log("Did Left Twist but both triggers pressed");
                        summonWall();
                        isLeft = false;
                        TriggerL = false;
                        Continuous = false;
                    }
                    else if (MergedRet == true)
                    {
                        isLeft = false;
                    }
                    else
                    {
                        Debug.Log("did left twist");
                        isLeft = false;
                        TriggerL = false;
                        Continuous = false;
                        //Debug.Log("Continuous is: " + Continuous);
                        summonPillar();
                    }
                    break;
                case "Trump":
                    if (TriggerL == true)
                    {
                        Debug.Log("did Trump");
                        summonWall();
                        TriggerL = false;
                        isLeft = false;
                        Continuous = false;
                    }
                    else
                    {
                        Debug.Log("did wall, but didn't press both triggers");
                        Debug.Log(TriggerR + " " + TriggerL);
                        isLeft = false;
                        Continuous = false;
                    }
                    break;
                case "TrumpL":
                    if (TriggerL == true)
                    {
                        Debug.Log("did Left Trump");
                        summonWall();
                        TriggerL = false;
                        isLeft = false;
                        Continuous = false;
                    }
                    else
                    {
                        Debug.Log("did wall, but didn't press both triggers");
                        Debug.Log(TriggerR + " " + TriggerL);
                        isLeft = false;
                        Continuous = false;
                    }
                    break;
                case "Smash":

                    if (BothPointersAreCloseTogether() && TriggerL == true)
                    //if (RightCI.NearGround() && LeftCI.NearGround() && TriggerL == true)
                    {
                        Debug.Log("did smash");
                        summonEarthwave();
                        TriggerL = false;
                        MergedRet = false;
                        isLeft = false;
                        Continuous = false;
                    }
                    else
                    {
                        Debug.Log("did smash, but trigger or reticles weren't close");
                        MergedRet = false;
                        isLeft = false;
                        Continuous = false;
                    }
                    break;
            }
        }
    }

    // Called when a gesture was captured but confidence was below the threshold  
    void OnGestureRejected(string error, string gestureName = null, double confidence = 0)
    {
        Debug.Log("Gesture rejected because confidence is " + confidence);
        if (TriggerL == true || TriggerR == true)
        {
            Continuous = true;
        }
        else
        {
            isLeft = false;
            Continuous = false;
        }
    }

    public void Cancel()
    {
        Continuous = false;
        Debug.Log("Gesture cancelled, took too long");
    }

    #region UPDATE
    void Update()
    {
        // Beam Positions
        trackedR = RightCI.GetComponent<ReticleController>().projectedBeamDown.position;
        trackedL = LeftCI.GetComponent<ReticleController>().projectedBeamDown.position;

        // Which controller is being used for the summon
        if (BothTriggersPressedApart() == true)
        {
            //Debug.Log("Apart");
            Magic = W;
            GetCI = RightCI;
            OrientController = RightController;
        }
        else if (TriggerR == true)
        {
            GetCI = RightCI;
            OrientController = RightController;
        }
        else if (isLeft == true)
        {
            GetCI = LeftCI;
            OrientController = LeftController;
        }
        else
        {
            GetCI = RightCI;
            OrientController = RightController;
        }

        // Are both controllers in use
        // Test

        // Haptic WIP
        if (haptic == true)
        {
            SteamVR_Controller.Input((int)Right.index).TriggerHapticPulse(500);
        }
    }
    #endregion

    #endregion

    #region ReticleControl

    bool BothPointersAreCloseTogether()
    {
        return Vector3.Distance(trackedR, trackedL) < 0.5F;
    }

    bool BothTriggersPressedApart()
    {
        return (TriggerL && TriggerR && !MergedRet); 
    }

    void LockReticleR(object sender, ControllerClickedEventArgs e)
    {
        isLeft = false;
        Continuous = true;
        haptic = true;

        if (TriggerL == true)
        {
            GameObject.Find("VR Gesture Manager").GetComponent<VRGestureManager>().Wipe();
        }

        if (BothPointersAreCloseTogether() == true)
        {
            RightCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.MergedCircle);
            position = RightCI.GetComponent<ReticleController>().projectedBeamDown.position;
            summonPos = RightCI.GetComponent<ReticleController>().projectedBeamDown.position;
            MergedRet = true;
            Magic = S;
        }
        else
        {
            RightCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.SingularFixed);
            TriggerR = true;
            wallPosR = RightController.transform.position;
            position = RightCI.GetComponent<ReticleController>().projectedBeamDown.position;
            summonPos = RightCI.GetComponent<ReticleController>().projectedBeamDown.position;
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)Right.index);
            velocityR = device.velocity;
            Rumble.SendMessage("RumbleOn");
            Magic = RP;
        }
    }

    void UnlockReticleR(object sender, ControllerClickedEventArgs e)
    {
        haptic = false;
        MergedRet = false;
        Continuous = false;
        RightCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);
        TriggerR = false;
        Rumble.SendMessage("RumbleOff");
    }

    void LockReticleL(object sender, ControllerClickedEventArgs e)
    {
        Continuous = true;

        if (TriggerR == true)
        {
            GameObject.Find("VR Gesture Manager").GetComponent<VRGestureManager>().Wipe();
        }

        if (MergedRet == true)
        {
            isLeft = false;
            LeftCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.MergedCircle);
            //summonPos = LeftCI.GetComponent<ReticleController>().projectedBeamDown.position;
            TriggerL = true;
            Magic = S;
        }
        else
        {
            isLeft = true;
            LeftCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.SingularFixed);
            wallPosL = LeftController.transform.position;
            positionL = LeftCI.GetComponent<ReticleController>().projectedBeamDown.position;
            summonPos = LeftCI.GetComponent<ReticleController>().projectedBeamDown.position;
            TriggerL = true;
            Rumble.SendMessage("RumbleOn");
            Magic = RP;
        }
    }

    void UnlockReticleL(object sender, ControllerClickedEventArgs e)
    {
        Continuous = false;
        LeftCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);
        Rumble.SendMessage("RumbleOff");
        //TriggerL = false;
    }

    #endregion

    #region Summons

    // How we create any object
    GameObject CreateProjectile(GameObject rockType, GameObject[] models, float scale = 1.0F)
    {
        GameObject placeholderObj = (GameObject)Instantiate(rockType, summonPos, Quaternion.identity);
        GameObject randomModel = models[Random.Range(0, models.Length)];
        if (placeholderObj.GetComponent<MeshFilter>()) //Adds a mesh filter only if needed
        {
            placeholderObj.GetComponent<MeshFilter>().sharedMesh = randomModel.GetComponent<MeshFilter>().sharedMesh;
            placeholderObj.GetComponent<MeshCollider>().sharedMesh = randomModel.GetComponent<MeshCollider>().sharedMesh;
            placeholderObj.GetComponent<Renderer>().sharedMaterial = randomModel.GetComponent<Renderer>().sharedMaterial;
            placeholderObj.transform.localScale = randomModel.transform.localScale * scale;
        }
        return placeholderObj;
    }

    void summonRock()
    {
        haptic = false;
        // Debug.Log("rock spawn start");
        placeholderRock = CreateProjectile(rockObject, rocks);
        // Debug.Log("creating projectile");
        placeholderRock.GetComponent<RockController>().hand = OrientController;
        placeholderRock.transform.rotation = Random.rotation;
        placeholderRock.GetComponent<RockController>().pullVelocity = 0.5f * velocityR;
        GetCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);
        cooldown = false;
    }

    void summonBoulder()
    {
        placeholderBoulder = CreateProjectile(boulderObject, boulders);
        placeholderBoulder.GetComponent<RockController>().hand = OrientController;
        placeholderBoulder.transform.rotation = Random.rotation;
        placeholderBoulder.GetComponent<RockController>().pullVelocity = 0.5f * velocityR;
        RightCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);
        GetCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);
        cooldown = false;
    }

    void summonPillar()
    {
        placeholderPillar = CreateProjectile(pillarObject, pillars);
        placeholderPillar.GetComponent<PillarController>().hand = OrientController;
        placeholderPillar.transform.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        GetCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);
        Continuous = false;
        movePillar();
        cooldown = false;
    }

    void movePillar()
    {      
        PillarController pc = placeholderPillar.GetComponent<PillarController>();
        pc.pullVector = pillarp;
        //Debug.Log("pullvector is " + pc.pullVector);
        pc.ChangeState(PillarController.StateAlias.Moving);
        //Debug.Log("Pillar state is moving");
    }

    void summonWall()
    {
        Vector3 direction = RightCI.AverageVector();
        float width = Vector3.Distance(wallPosR, wallPosL);
        // Debug.Log("pointer distance is " + width);
        width = Mathf.Max(width, 0.6f);
        float angle = RightCI.AngleBetween();
        // Debug.Log("angle between is " + angle);

        direction.y = 0;
        direction = direction.normalized;

        Vector3 wallPos = new Vector3(RightCI.head.transform.position.x, position.y, RightCI.head.transform.position.z);
        summonPos = wallPos + direction * 1.5f;
        // Debug.Log("position is " + position);

        placeholderWall = CreateProjectile(wallObject, walls);
        placeholderWall.GetComponent<WallController>().hand = this.gameObject;
        Vector3 wallScale = new Vector3(placeholderWall.transform.localScale.x * width, placeholderWall.transform.localScale.y, placeholderWall.transform.localScale.z);
        // Debug.Log("wall scale is " + wallScale);
        placeholderWall.transform.localScale = wallScale;
        placeholderWall.transform.LookAt(wallPos);
        Vector3 rotationVector = new Vector3(0.0f, 0.0f, 0.0f);
        placeholderWall.transform.Rotate(rotationVector);

        RightCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);
        LeftCI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);

        moveWall();
        cooldown = false;
    }

    void moveWall()
    {
        WallController wc = placeholderWall.GetComponent<WallController>();
        wc.pullVelocity = pillarp;
    }

    void summonEarthwave()
    {
        Vector3 directionVector = Vector3.Normalize(RightCI.hand.transform.forward + LeftCI.hand.transform.forward);
        Vector3 earthwaveOrigin = (RightCI.hand.transform.position + LeftCI.hand.transform.position) / 2;
        placeholderEarthwave = CreateProjectile(earthwaveObject, earthwaves);
        Debug.Log("direction vector " + directionVector);
        placeholderEarthwave.transform.forward = directionVector;
        placeholderEarthwave.GetComponent<EarthwaveController>().direction = directionVector;
        placeholderEarthwave.GetComponent<EarthwaveController>().origin = earthwaveOrigin;
    }

    #endregion

}
