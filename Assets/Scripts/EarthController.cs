using UnityEngine;
using System.Collections;

public delegate void SpellDelegate();

public class EarthController : SpellController {
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

    private Vector3 position;
    Vector3 properPos;
	public int WAVE_RANGE = 15;

	float reticleIncrease = 1.0f;

    GameObject placeholderRock;
    GameObject placeholderPillar;
    GameObject placeholderWall;
    GameObject placeholderBoulder;
	GameObject placeholderEarthwave;


    public SpellDelegate updateSpell;

	Vector3 spawnPos;
	bool bouldersummon = false;

    int ROCK_ID = 0;
	int PILLAR_ID = 1;
	int WALL_ID = 2;
	int FORCE_ID = 3;
	int BOULDER_ID = 4;
	int EARTHWAVE_ID = 5;

	float rockHaptic = 500.0f;
	float boulderHaptic = 750.0f;
	float pillarHaptic = 500.0f;
	float wallHaptic = 500.0f;
	float earthWaveHaptic = 500.0f;





    new void Start() {
        base.Start();
        if (GetComponent<SteamVR_ControllerEvents>() == null)
        {
            Debug.LogError("SteamVR_ControllerEvents_ListenerExample is required to be attached to a SteamVR Controller that has the SteamVR_ControllerEvents script attached to it");
            return;
        }

        AddSpellListeners();
        CI.controllerEvents.TriggerUnclicked += refresh;
		StartCoroutine (pulseHaptic (0.05f));
    }

    void AddSpellListeners()
    {
        CI.controllerEvents.TriggerClicked += new ControllerClickedEventHandler(summonRock);
        CI.controllerEvents.TriggerClicked += new ControllerClickedEventHandler(summonPillar);
        CI.controllerEvents.TriggerClicked += new ControllerClickedEventHandler(summonBoulder);
        CI.controllerEvents.TriggerClicked += new ControllerClickedEventHandler(summonWall);
        CI.controllerEvents.TriggerClicked += new ControllerClickedEventHandler (summonEarthwave);

		CI.controllerEvents.GripClicked += new ControllerClickedEventHandler (applyForce);
		/*
        CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(DoTriggerUnclicked);
        CI.controllerEvents.TouchpadClicked += new ControllerClickedEventHandler(DoTouchpadClicked);
        CI.controllerEvents.GripClicked += new ControllerClickedEventHandler(DoGripClicked);
        CI.controllerEvents.GripUnclicked += new ControllerClickedEventHandler(DoGripUnclicked);
        CI.gestureEvents.UpwardGesture += new GestureEventHandler(DoUpwardGesture);
        CI.gestureEvents.DownwardGesture += new GestureEventHandler(DoDownwardGesture);
        CI.gestureEvents.InwardGesture += new GestureEventHandler(DoInwardGesture);
        CI.gestureEvents.OutwardGesture += new GestureEventHandler(DoOutwardGesture);
        CI.gestureEvents.TwistGesture += new GestureEventHandler(DoTwistGesture);*/
    }

    void removeSpellListeners()
    {
		CI.controllerEvents.TriggerClicked -= summonRock;
		CI.controllerEvents.TriggerClicked -= summonPillar;
        CI.controllerEvents.TriggerClicked -= summonBoulder;
		CI.controllerEvents.TriggerClicked -= summonWall;
		CI.controllerEvents.TriggerClicked -= summonEarthwave;

		CI.controllerEvents.GripClicked -= applyForce;

		/*
        CI.controllerEvents.TriggerUnclicked -= new ControllerClickedEventHandler(DoTriggerUnclicked);
        CI.controllerEvents.TouchpadClicked -= new ControllerClickedEventHandler(DoTouchpadClicked);
        CI.controllerEvents.GripClicked -= new ControllerClickedEventHandler(DoGripClicked);
        CI.controllerEvents.GripUnclicked -= new ControllerClickedEventHandler(DoGripUnclicked);
        CI.gestureEvents.UpwardGesture -= new GestureEventHandler(DoUpwardGesture);
        CI.gestureEvents.DownwardGesture -= new GestureEventHandler(DoDownwardGesture);
        CI.gestureEvents.InwardGesture -= new GestureEventHandler(DoInwardGesture);
        CI.gestureEvents.OutwardGesture -= new GestureEventHandler(DoOutwardGesture);
        CI.gestureEvents.TwistGesture -= new GestureEventHandler(DoTwistGesture);*/
    }

	IEnumerator pulseHaptic(float waitTime)
	{ //in seconds
		while (true) {
			if (canSummonWall() && CI.otherHand.GetComponent<EarthController>().canSummonWall()) {
				this.gameObject.GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(1, (ushort) wallHaptic);
				CI.otherHand.GetComponent<SteamVR_ControllerActions> ().TriggerHapticPulse (1, (ushort)wallHaptic);
			}
			if (canSummonEarthwave() && CI.otherHand.GetComponent<EarthController>().canSummonEarthwave() && !bouldersummon) {
				//debug.Log (bouldersummon);
				this.gameObject.GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(1, (ushort) earthWaveHaptic);
				CI.otherHand.GetComponent<SteamVR_ControllerActions> ().TriggerHapticPulse (1, (ushort)earthWaveHaptic);
			}
//			
			yield return new WaitForSeconds (waitTime);
			//this.gameObject.GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(1, (ushort) wallHaptic);

		}
	}



    void clearAllListeners()
    {
        //clears all listeners, probably custom function
    }

    void refresh(object sender, ControllerClickedEventArgs e)
    {
        AddSpellListeners();
        CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.Singular);
        //show reticles
    }

    void Update()
    {
		//Debug.Log ("Can Summon Earthwave" + canSummonEarthwave ());
		if (updateSpell != null)
		{
			updateSpell ();
		}

		//this.gameObject.GetComponent<ReticleController>().resize(1.01f);
    }

    void HideReticle()
    {
        //GetComponent<ReticleController>().HideReticle();
    }

    void ShowReticle()
    {
        //GetComponent<ReticleController>().ShowReticle();
    }
    
    /*
    void DoUpwardGesture(object sender, GestureEventArgs e)
    {
        //debug.Log("UPWARDDDD GESTURE!");
    }
    void DoDownwardGesture(object sender, GestureEventArgs e)
    {
        //debug.Log("DOOOOWNWARD GESTURE!");
    }
    void DoInwardGesture(object sender, GestureEventArgs e)
    {
        //debug.Log("IIIIIIIIINWARD GESTURE!");
    }
    void DoOutwardGesture(object sender, GestureEventArgs e)
    {
        //debug.Log("OUOUTWARD GESTURE!");
    }
    void DoTwistGesture(object sender, GestureEventArgs e)
    {
        //debug.Log("TWIIIIISTING GESTURE!");
    }*/


    GameObject CreateProjectile(GameObject rockType, GameObject[] models, float scale = 1.0F)
    {
        GameObject placeholderObj = (GameObject)Instantiate(rockType, position, Quaternion.identity);
        GameObject randomModel = models[Random.Range(0, models.Length)];
        placeholderObj.GetComponent<MeshFilter>().sharedMesh = randomModel.GetComponent<MeshFilter>().sharedMesh;
        placeholderObj.GetComponent<MeshCollider>().sharedMesh = randomModel.GetComponent<MeshCollider>().sharedMesh;
        placeholderObj.GetComponent<Renderer>().sharedMaterial = randomModel.GetComponent<Renderer>().sharedMaterial;
        placeholderObj.transform.localScale = randomModel.transform.localScale*scale;
        return placeholderObj;
    }

	void CancelSummonEverything() {
		cancelSummonRock();
		cancelSummonPillar ();
        cancelSummonBoulder();
		cancelSummonWall ();
		cancelApplyForce ();
	}


    //// ROCK LOGIC STARTS HERE ////
    void summonRock(object sender, ControllerClickedEventArgs e)
    {
        ////debug.Log("is not cooldown summoning rock");
		//if (CanSummonRock()) {

		if(!BothPointersAreCloseTogether() && CanSummonRock())
        {
            removeSpellListeners();
            // freeze reticle
            CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.SingularFixed);

            properPos = this.GetComponent<ReticleController>().projectedBeamDown.position;
            //if (isSpellCooldown(ROCK_ID))
            //{
            //Debug.Log("summoning rock");


            updateSpell += prepareRock; // if rock reticle is somehow different

            CI.gestureEvents.UpwardGesture += new GestureEventHandler(instantiateRock);
            CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(cancelSummonRock);
            //}
            //}
        }
    }

	bool CanSummonRock() {
		////debug.Log ("not palm up for rock!");
		return !CI.NearGround();//!CI.PalmUp() && !(CI.NearGround());
	}

    void cancelSummonRock(object sender, ControllerClickedEventArgs e)
    {
        cancelSummonRock();
    }

    void cancelSummonRock() 
    {
        CI.controllerEvents.TriggerUnclicked -= cancelSummonRock;
        CI.gestureEvents.UpwardGesture -= instantiateRock;
        updateSpell = null;
    }

    void prepareRock()
    {
        //draws on the ground, either feeding mag to reticle or something else
    }

    void instantiateRock(object sender, GestureEventArgs e)
    {
        CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.None);
        position = GetComponent<ReticleController>().GetFixedPosition() + new Vector3(0, 1.0F, 0); //new Vector3(properPos.x, properPos.y + 1, properPos.z);
        placeholderRock = CreateProjectile(rockObject, rocks);
        placeholderRock.GetComponent<RockController>().hand = this.gameObject;
        placeholderRock.transform.rotation = Random.rotation;
        placeholderRock.GetComponent<RockController>().pullVelocity = e.velocity;
        //Debug.Log("instantiating rock");
		this.gameObject.GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(5, (ushort) rockHaptic);

		CancelSummonEverything();
		castSpell(ROCK_ID, 1000, 1000);
    }
    //// ROCK LOGIC ENDS HERE ////

        
    //// PILLAR LOGIC STARTS HERE ////
	void summonPillar(object sender, ControllerClickedEventArgs e)
    {
        //if (isCooldowned(PILLAR_ID) && twistGesture())
        //{
		if (CanSummonPillar() && !BothPointersAreCloseTogether()) {
			removeSpellListeners();
            CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.SingularFixed);

            //Debug.Log("summoning pillar");

            spawnPos = this.GetComponent<ReticleController>().projectedBeamDown.position;
			updateSpell += preparePillar;
			assignPillarListeners ();
		}
        //}
    }

	bool CanSummonPillar() {
		return CI.PalmUp () && !CI.IsCrossed();
	}

    void assignPillarListeners()
    {
		CI.gestureEvents.TwistGesture += new GestureEventHandler(instantiatePillar);
		CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(cancelSummonPillar);
    }

    void cancelSummonPillar(object sender, ControllerClickedEventArgs e)
    {
        cancelSummonPillar();
    }

	void cancelSummonPillar()
	{
		CI.controllerEvents.TriggerUnclicked -= cancelSummonPillar;
		CI.gestureEvents.TwistGesture -= instantiatePillar;
        CI.gestureEvents.UpwardGesture -= movePillar;
		updateSpell = null;
	}

	void preparePillar()
	{
	}

	void instantiatePillar(object sender, GestureEventArgs e)
    {
        CancelSummonEverything();
        CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.None);
        position = GetComponent<ReticleController>().GetFixedPosition() + new Vector3(0, 0.2F, 0); //new Vector3(properPos.x, properPos.y + 0.2F, properPos.z);
		placeholderPillar = CreateProjectile(pillarObject, pillars);
		placeholderPillar.GetComponent<PillarController>().hand = this.gameObject;
        //placeholderPillar.transform.rotation = gameObject.transform.rotation;
        placeholderPillar.transform.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
		this.gameObject.GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(10, (ushort) pillarHaptic);
        //placeholderPillar.transform.rotation = Quaternion.LookRotation(CI.ForwardVector());

        CI.gestureEvents.UpwardGesture += new GestureEventHandler(movePillar);

		////debug.Log("instantiating pillar");

		castSpell(PILLAR_ID, 1000, 1000);
    }

    void movePillar(object sender, GestureEventArgs e)
    {
        PillarController pc = placeholderPillar.GetComponent<PillarController>();
        pc.pullVector = 2F*e.velocity;
        pc.ChangeState(PillarController.StateAlias.Moving);
		this.gameObject.GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(10, (ushort) pillarHaptic);
        CancelSummonEverything();
    }
    //// PILLAR LOGIC ENDS HERE ////

    //// BOULDER LOGIC STARTS HERE ////

    bool BothAreCloseTogether()
    {
        return CI.DistanceToOther() < 0.5F;
    }

    bool BothPointersAreCloseTogether()
    {
        return CI.PointerDistanceToOther() < 0.5F;
    }

    void summonBoulder(object sender, ControllerClickedEventArgs e)
    {
        if (BothAreCloseTogether() && BothPointersAreCloseTogether() && !CI.IsCrossed() && canSummonBoulder() && CI.otherHand.GetComponent<EarthController>().canSummonBoulder())// && isCooldowned(BOULDER_ID))
        {
			bouldersummon = true;
            CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.MergedCircle);
            CI.otherHand.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.MergedCircle);
            properPos = this.GetComponent<ReticleController>().GetFixedPosition(); // CI.FixedPointerCentroidPosition(); //(CI.PointerGroundPosition() + CI.otherHand.GetComponent<ControllerInterface>().PointerGroundPosition())/2;
            updateSpell = prepareBoulder;
            assignBoulderListeners();
            CI.otherHand.GetComponent<EarthController>().assignBoulderListeners();
        }
    }

    void assignBoulderListeners()
    {
		bouldersummon = true;
        removeSpellListeners();
        CI.gestureEvents.UpwardGesture += new GestureEventHandler(instantiateBoulder);
        CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(cancelSummonBoulder);
        CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(CI.otherHand.GetComponent<EarthController>().cancelSummonBoulder);
    }

    bool canSummonBoulder()
    {
        return !CI.NearGround() && CI.controllerEvents.triggerPressed;
    }

    void prepareBoulder()
    {
		bouldersummon = true;
		float radius = CI.DistanceToOther ();
		float currentRadius = this.gameObject.GetComponent<ReticleController> ().pointerCursorRadius;
		this.gameObject.GetComponent<ReticleController>().resize(radius/(80.0f*currentRadius));
		CI.otherHand.gameObject.GetComponent<ReticleController>().resize(radius/(80.0f*currentRadius));
		//Debug.Log (reticleIncrease);
    }

    void instantiateBoulder(object sender, GestureEventArgs e)
    {
        if(e.velocity.y > 0.5)
        {
            //TODO: REMOVE ALL PROPER POS LOGIC
            position = CI.GetComponent<ReticleController>().GetFixedPosition() + new Vector3(0, -0.5F, 0);

            CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.None);
            CI.otherHand.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.None);
            cancelSummonBoulder();
            CI.otherHand.GetComponent<EarthController>().cancelSummonBoulder();

            float dist = CI.DistanceToOther();


           // Debug.Log("Summoning boulder with size " + dist);

            //position = new Vector3(properPos.x, properPos.y - 0.5f, properPos.z);
            placeholderBoulder = CreateProjectile(boulderObject, boulders, dist + 0.75F);
            placeholderBoulder.GetComponent<RockController>().hand = this.gameObject;
            placeholderBoulder.transform.rotation = Random.rotation;
            placeholderBoulder.GetComponent<RockController>().pullVelocity = e.velocity;
			this.gameObject.GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(7, (ushort) boulderHaptic);
			CI.otherHand.GetComponent<SteamVR_ControllerActions> ().TriggerHapticPulse (7, (ushort) boulderHaptic);
        }
        /*if (Time.time - otherDevice.lastUpwardGesture() > delay)
        {
            cancelSummonBoulder();
            CI.otherHand.GetComponent<EarthController>().cancelSummonBoulder();
            //instantiate the boulder
        }*/
    }

    void cancelSummonBoulder()
    {
		bouldersummon = false;
        CI.gestureEvents.UpwardGesture -= new GestureEventHandler(instantiateBoulder);
        CI.controllerEvents.TriggerUnclicked -= new ControllerClickedEventHandler(cancelSummonBoulder);
        CI.controllerEvents.TriggerUnclicked -= new ControllerClickedEventHandler(CI.otherHand.GetComponent<EarthController>().cancelSummonBoulder);
        updateSpell = null;
    }

    void cancelSummonBoulder(object sender, ControllerClickedEventArgs e)
    {
        cancelSummonBoulder();
    }

    //// BOULDER LOGIC ENDS HERE ////

    
	//// WALL LOGIC STARTS HERE ////
	void summonWall(object sender, ControllerClickedEventArgs e)
	{
        //THERE CURRENTLY IS A BUG WHERE YOU HAVE TO POINT DOWNWARDS
        //Debug.Log ("SUMMONING WALL: is crossed= " + CI.IsCrossed());
        if (canSummonWall() && CI.otherHand.GetComponent<EarthController>().canSummonWall())
        {
            CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.MergedRectangle);
            CI.otherHand.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.MergedRectangle);
            Debug.Log ("inside summon wall!!");
			properPos = this.GetComponent<ReticleController>().GetFixedPosition();
			updateSpell = prepareWall;
			assignWallListeners();
			CI.otherHand.GetComponent<EarthController>().assignWallListeners();
		}
	}

	void cancelSummonWall(object sender, ControllerClickedEventArgs e)
	{
		//Debug.Log ("cancel summon wall!!");

		CI.controllerEvents.TriggerUnclicked -= cancelSummonWall;
		CI.controllerEvents.TriggerUnclicked -= CI.otherHand.GetComponent<EarthController>().cancelSummonWall;
		CI.gestureEvents.UpwardGesture -= instantiateWall;

		//cleanup stuff with reticle
		updateSpell = null;
	}


	void cancelSummonWall()
	{
		//Debug.Log ("cancel summon wall!!");

		CI.controllerEvents.TriggerUnclicked -= cancelSummonWall;
		CI.controllerEvents.TriggerUnclicked -= CI.otherHand.GetComponent<EarthController>().cancelSummonWall;
		CI.gestureEvents.UpwardGesture -= instantiateWall;

		//cleanup stuff with reticle
		updateSpell = null;
	}


	void assignWallListeners()
	{
		//Debug.Log ("assign wall listeners!!");

		removeSpellListeners();
		CI.gestureEvents.UpwardGesture += instantiateWall;
		CI.controllerEvents.TriggerUnclicked += cancelSummonWall;
		CI.controllerEvents.TriggerUnclicked += CI.otherHand.GetComponent<EarthController>().cancelSummonWall;
	}


	bool canSummonWall()
	{
		return CI.controllerEvents.triggerPressed && CI.IsCrossed () && CI.NearGround ();// && (ADD IN WHEN DEBUG READY);
	}

	void prepareWall()
	{
		//draws on the ground, either feeding mag of dist to reticle or something else
	}

	void instantiateWall(object sender, GestureEventArgs e)
    {
        CI.GetComponent<ReticleController>().ChangeState(ReticleController.StateAlias.None);
        //Debug.Log("WALL CREATED AND SUMMONING");

		Vector3 direction = CI.AverageVector();
		float width = CI.DistanceToOther ();
		width = Mathf.Max (width, 0.6f);
		float angle = CI.AngleBetween ();

		direction.y = 0;
		direction = direction.normalized;

		//Debug.Log (direction);

		Vector3 wallPos = new Vector3 (CI.head.transform.position.x, properPos.y, CI.head.transform.position.z);


		position = wallPos + direction * 1.5f;

		placeholderWall = CreateProjectile(wallObject, walls);
		placeholderWall.GetComponent<WallController>().hand = this.gameObject;
		Vector3 wallScale = new Vector3 (placeholderWall.transform.localScale.x * width, placeholderWall.transform.localScale.y, placeholderWall.transform.localScale.z);
		placeholderWall.transform.localScale = wallScale;
		//Vector3 headTempPos = new Vector3(CI.head.transform.position.x, 0, CI.h;
		placeholderWall.transform.LookAt(wallPos);
		Vector3 rotationVector = new Vector3 (0.0f, 0.0f, 0.0f);
		placeholderWall.transform.Rotate (rotationVector);

		//placeholderWall.transform.Rotate(0.0f, 90 - angle, 0.0f);

		//scale by width

		CancelSummonEverything ();
		CI.otherHand.GetComponent<EarthController> ().CancelSummonEverything ();

		castSpell(WALL_ID, 1000, 1000);

		//}
	}




	//// EARTHWAVE LOGIC STARTS HERE////

	bool canSummonEarthwave()
	{
		return CI.AboveHead () && CI.PalmUp ();// && CI.controllerEvents.triggerPressed;
	}

	void prepareEarthwave()
	{
		//draws on the ground stuff for earthwave
	}

	void assignEarthwaveListeners()
	{
		removeSpellListeners();

		CI.gestureEvents.DownwardGesture += new GestureEventHandler(instantiateEarthWave);
		CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler (cancelSummonEarthwaveEventHandler);
		CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler (CI.otherHand.GetComponent<EarthController>().cancelSummonEarthwaveEventHandler);
	}

    void instantiateEarthWave(object sender, ControllerClickedEventArgs e)
    {
        //Debug.Log("InstantiateEarthWave");
        //on downward gesture
        if (CI.NearGround() && CI.otherHand.GetComponent<ControllerInterface>().NearGround())
        {
            //Direction

            //Method 1: avg of controller directions
            Vector3 directionVector = Vector3.Normalize(CI.hand.transform.forward + CI.otherHand.transform.forward);
            Vector3 earthwaveOrigin = (CI.hand.transform.position + CI.otherHand.transform.position) / 2;

			//StartCoroutine(EarthWaveCreation(directionVector, earthwaveOrigin));
            //Method 2: plane method #charles

            //remove all listeners
            cancelSummonEarthwave();
            CI.otherHand.GetComponent<EarthController>().cancelSummonEarthwave();
        }
    }

    void instantiateEarthWave(object sender, GestureEventArgs e)
	{
		//Debug.Log ("InstantiateEarthWave");
		//on downward gesture
		if (CI.NearGround () && CI.otherHand.GetComponent<ControllerInterface> ().NearGround ()) {
			//Direction

			//Method 1: avg of controller directions
			Vector3 directionVector = Vector3.Normalize(CI.hand.transform.forward + CI.otherHand.transform.forward);
			Vector3 earthwaveOrigin = (CI.hand.transform.position + CI.otherHand.transform.position) / 2;

			StartCoroutine(EarthWaveCreation(directionVector, earthwaveOrigin));

			//Method 2: plane method #charles

			//remove all listeners
			cancelSummonEarthwave();
			CI.otherHand.GetComponent<EarthController>().cancelSummonEarthwave();
		}
	}	

	IEnumerator EarthWaveCreation(Vector3 directionVector, Vector3 earthwaveOrigin) {
		float increment = 0;
		for (int i = 0; i < WAVE_RANGE; i++) {
			position = new Vector3(earthwaveOrigin.x, CI.room.transform.position.y - 1.5f, earthwaveOrigin.z);
            position += new Vector3(Random.value * 0.1f, 0.0f, Random.value * 0.1f);
            directionVector.y = 0;
			position += ((directionVector * 1.5f) + directionVector * i * 3.0f);
			placeholderEarthwave = CreateProjectile(earthwaveObject, earthwaves);
			//placeholderEarthwave.GetComponent<EarthwaveController>().hand = this.gameObject;

            Vector3 scale = placeholderEarthwave.transform.localScale;
            float rand = (Random.value * 1.0f);
            scale.y = scale.y * 3;
            scale.x = (scale.x * 0.5f) + rand; 
			scale.z = ((scale.z * 0.5f) + rand)*2.0F;
			placeholderEarthwave.transform.localScale = scale;
			//Debug.Log("instantiating rock");
			increment += 1;

            //placeholderEarthwave.transform.up = new Vector3(0, -1.0F, 0);
            placeholderEarthwave.transform.forward = directionVector;

            //placeholderEarthwave.transform.forwar

            //placeholderEarthwave.GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 10.0f, 0.0f);


            //Vector3 temp = (directionVector * 0.5f + placeholderEarthwave.GetComponent<Rigidbody>().transform.up).normalized;
            //placeholderEarthwave.GetComponent<Rigidbody>().transform.up = temp; // new Vector3(Random.value * 0.1f, 1.0f, Random.value * 0.1f);
            yield return new WaitForSeconds (0.05f);


		}
	}
	void cancelSummonEarthwaveEventHandler(object sender, ControllerClickedEventArgs e)
	{
		CI.controllerEvents.TriggerUnclicked -= cancelSummonEarthwaveEventHandler;
		CI.controllerEvents.TriggerUnclicked -= CI.otherHand.GetComponent<EarthController> ().cancelSummonEarthwaveEventHandler;
		CI.gestureEvents.DownwardGesture -= instantiateEarthWave;

		//cleanup stuff with reticlee
		updateSpell = null;
	}
	void cancelSummonEarthwave()
	{
		CI.controllerEvents.TriggerUnclicked -= cancelSummonEarthwaveEventHandler;
		CI.controllerEvents.TriggerUnclicked -= CI.otherHand.GetComponent<EarthController> ().cancelSummonEarthwaveEventHandler;
		CI.gestureEvents.DownwardGesture -= instantiateEarthWave;
		//cleanup stuff with reticlee
		updateSpell = null;
	}

	void summonEarthwave(object sender, ControllerClickedEventArgs e)
	{
		bool bhand = canSummonEarthwave (); //&& CI.otherHand.GetComponent<EarthController> ().canSummonEarthwave ();
		//Debug.Log ("Summoning earth wave" + bhand);
		if (canSummonEarthwave() && CI.otherHand.GetComponent<EarthController>().canSummonEarthwave()){// && isSpellCooldown(EARTHWAVE_ID)){
			//Debug.Log("Inside summon");		
			updateSpell += prepareEarthwave;
			assignEarthwaveListeners();
			CI.otherHand.GetComponent<EarthController>().assignEarthwaveListeners();
		}
	}





	float absFunction(float controllerHeight, float headHeight, float originFactor, float normalRangeVibration) //values already sanitized
	{
		return (float) normalRangeVibration * Mathf.Abs(controllerHeight - (headHeight / originFactor))		;
	}
	float squaredFunction(float controllerHeight, float headHeight, float originFactor, float normalRangeVibration) //values already sanitized
	{
		return (float)normalRangeVibration * Mathf.Pow (controllerHeight - (headHeight / originFactor), 2);
	}

	float sanitize(float height)
	{
		Vector3 ground = this.GetComponent<SteamVR_BezierPointer>().projectedBeamDown.position;
		return Mathf.Max(0, height - ground.y); 
	}


	//// EARTHWAVE LOGIC ENDS HERE////



	//// apply force logic happens here ////

	void applyForce(object sender, ControllerClickedEventArgs e)
	{
		if (CI.controllerEvents.gripPressed) {
			//Debug.Log ("Inside apply force");
			removeSpellListeners ();

			updateSpell += prepareApplyForce;

			CI.gestureEvents.InwardGesture += new GestureEventHandler (instantiatePullForce);
			//CI.gestureEvents.UpwardGesture += new GestureEventHandler (instantiatePullForce);

			CI.gestureEvents.OutwardGesture += new GestureEventHandler (instantiatePushForce);
			CI.controllerEvents.GripUnclicked += new ControllerClickedEventHandler (cancelApplyForce);

		}
	}





	void cancelApplyForce(object sender, ControllerClickedEventArgs e)
	{
		cancelApplyForce ();
	}


	void cancelApplyForce()
	{
		CI.controllerEvents.GripUnclicked -= cancelApplyForce;
		CI.gestureEvents.InwardGesture -= instantiatePullForce;
		//CI.gestureEvents.UpwardGesture -= instantiatePullForce;
		CI.gestureEvents.OutwardGesture -= instantiatePushForce;
		updateSpell = null;
	}

	void prepareApplyForce()
	{
		//draw reticle stuff if we want
	}

	void instantiatePullForce(object sender, GestureEventArgs e)
	{
		//debug.Log ("inside instantiatepullforce");
		RaycastHit[] hits;
		hits = Physics.SphereCastAll (this.gameObject.transform.position, 10f, -this.gameObject.transform.up, 10f);
		for(int i = 0; i< hits.Length; i++){
			////debug.Log ("length"+hits.Length);
			////debug.Log ("ayy"+hits[i].transform.gameObject.tag);
			if (hits [i].transform.gameObject.tag == "projectile") {
				//debug.Log ("pull rock");
				hits [i].rigidbody.GetComponent<RockController> ().changeState (RockController.StateAlias.None);	
				//hits[i].rigidbody.AddForce (this.gameObject.transform.up * 30);
				//hits[i].rigidbody.AddForce (this.gameObject.transform.up * 300);
				hits[i].rigidbody.AddForce (e.velocity * 300);
			}	
			//hit.rigidbody.AddForce (this.gameObject.transform.up * 3, ForceMode.Impulse);
		}
		CancelSummonEverything();
		castSpell(FORCE_ID, 1000, 1000);
	}
	void instantiatePushForce(object sender, GestureEventArgs e)
	{
		//debug.Log ("inside instantiatepushforce");
		RaycastHit[] hits;
		hits = Physics.SphereCastAll (this.gameObject.transform.position, 1f, -this.gameObject.transform.up, 10f);
		for(int i = 0; i< hits.Length; i++){
			////debug.Log ("length"+hits.Length);
			//	//debug.Log ("ayy"+hits[i].transform.gameObject.tag);
			if (hits [i].transform.gameObject.tag == "projectile") {
				//debug.Log ("pull rock");
				hits [i].rigidbody.GetComponent<RockController> ().changeState (RockController.StateAlias.None);	
				//hits[i].rigidbody.AddForce (-this.gameObject.transform.up * 3000);
				hits[i].rigidbody.AddForce (e.velocity * 1000);

			}	
			//hit.rigidbody.AddForce (this.gameObject.transform.up * 3, ForceMode.Impulse);
		}
		CancelSummonEverything();
		castSpell(FORCE_ID, 1000, 1000);
	}

	//// WALL LOGIC ENDS HERE ////
	/*
	/*
>>>>>>> summon in progress
    void SpawnPillar()
    {
        position = new Vector3(this.GetComponent<ReticleController>().projectedBeamDown.position.x, this.GetComponent<ReticleController>().projectedBeamDown.position.y - 0.5f, this.GetComponent<ReticleController>().projectedBeamDown.position.z);
        placeholderPillar = CreateProjectile(pillarObject, pillars);
        placeholderPillar.GetComponent<PillarController>().hand = this.gameObject;
        placeholderPillar.transform.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
    }

    void SpawnRock()
    {
        position = new Vector3(this.GetComponent<ReticleController>().projectedBeamDown.position.x, this.GetComponent<ReticleController>().projectedBeamDown.position.y - 0.5f, this.GetComponent<ReticleController>().projectedBeamDown.position.z);
        placeholderRock = CreateProjectile(rockObject, rocks);
        placeholderRock.GetComponent<RockController>().hand = this.gameObject;
        placeholderRock.transform.rotation = Random.rotation;
    }

    void SpawnBoulder()
    {
        ReticleController bezierScript = this.GetComponent<ReticleController>();
        if (bezierScript.isMaster)
        {
            position = new Vector3(this.GetComponent<ReticleController>().projectedBeamDown.position.x, this.GetComponent<ReticleController>().projectedBeamDown.position.y - 0.5f, this.GetComponent<ReticleController>().projectedBeamDown.position.z);
            placeholderBoulder = CreateProjectile(boulderObject, boulders);
            placeholderBoulder.GetComponent<RockController>().hand = this.gameObject;
            placeholderBoulder.transform.rotation = Random.rotation;
        }
    }

    void SpawnWall()
    {
        ReticleController bezierScript = this.GetComponent<ReticleController>();
        if (bezierScript.isMaster)
        {
            position = new Vector3(this.GetComponent<ReticleController>().projectedBeamDown.position.x, this.GetComponent<ReticleController>().projectedBeamDown.position.y - 0.5f, this.GetComponent<ReticleController>().projectedBeamDown.position.z);
            placeholderWall = CreateProjectile(wallObject, walls);
            placeholderWall.GetComponent<PillarController>().hand = this.gameObject;
            float xdiff = this.transform.position.x - placeholderWall.transform.position.x;
            float zdiff = this.transform.position.z - placeholderWall.transform.position.z;
            float diff = zdiff / xdiff;
            float yrotationrad = (Mathf.Atan(diff));
            float yrotationdeg = yrotationrad * Mathf.Rad2Deg;
            //debug.Log("yrotation: " + (90 - yrotationdeg));
            placeholderWall.transform.Rotate(0.0f, 90 - yrotationdeg, 0.0f);
        }
    }*/
}


/*
//Note that all onTriggerPressed is really GetComponent<GestureEvents>().onTriggerPressed
//=====//EarthController


Start()
{
    addSpellListeners()
}

void removeSpellListeners()
{
    onTriggerPressed -= summonRock

}

void addSpellListeners()
{
    onTriggerPressed += summonRock
    onTriggerPressed += summonPillar
    onTriggerPressed += summonWall
    onTriggerPressed += summonEarthwave
    onTriggerPressed += summonBoulder


    onTriggerReleased += refresh
}

void clearAllListeners()
{
    //clears all listeners, probably custom function
}

void refresh()
{
    addSpellListeners();
    //show reticles
}

//// ROCK LOGIC STARTS HERE ////
void summonRock()
{
    if (isCooldowned(ROCK_ID))
    {
        updateSpell = prepareRock; // if rock reticle is somehow different

        removeSpellListeners();
        onUpwardGesture += instantiateRock;
        onTriggerReleased += cancelSummonRock;
    }
}

void cancelSummonRock()
{
    onTriggerReleased -= cancelSummonRock;
    onUpwardGesture -= instantiateRock;

    //cleanup stuff with reticle
    spellUpdate = null
}

void prepareRock()
{
    //draws on the ground, either feeding mag to reticle or something else
}

void instantiateRock()
{
    cancelSummonRock();
    castSpell(ROCK_ID, 1000, 1000);

    //do actual rock summon logic here
}
//// ROCK LOGIC ENDS HERE ////

//// PILLAR LOGIC STARTS HERE ////
void summonPillar()
{
    if (isCooldowned(PILLAR_ID) && twistGesture())
    {
        updateSpell = preparePillar;
        assignPillarListeners();
        onTriggerReleased += cancelSummonPillar
    }
}

void assignPillarListeners()
{
    removeSpellListeners();
    onTwist += instantiatePillar
    OnTriggerReleased += cancelSummonPillar

}


void cancelSummonPillar()
{
    this.onTriggerReleased -= cancelSummonPillar;
    OnTwist -= instantiatePillar;
    spellUpdate = null
}


void instantiatePillar()
{
    cancelSummonPillar()
    castSpell(PILLAR_ID, 1000, 1000)

    //do actual pillar summon logic here
}

void preparePillar()
{

    //`

}
//// PILLAR LOGIC ENDS HERE ////

//// WALL LOGIC STARTS HERE ////
void cancelSummonWall()
{
    this.onTriggerReleased -= cancelSummonWall;
    this.onTriggerReleased -= otherDevice.cancelSummonWall;
    onUpwardGesture -= instantiateWall;

    //cleanup stuff with reticle
    spellUpdate = null
}

void summonWall(sender, event)
{
    if (canSummonWall() && other.canSummonWall() && isCooldowned(WALL_ID) && bothAreCrossed())
    {
        updateSpell = prepareWall;
        assignWallListeners();
        otherDevice.assignWallListeners();
    }
}

void assignWallListeners
{
    removeSpellListeners();
    onUpwardGesture += instantiateWall;
    onTriggerReleased += cancelSummonWall;
    onTriggerReleased += otherDevice.cancelSummonWall;
}

bool canSummonWall()
{
    Return nearGround() && triggerClicked;
}

void prepareWall()
{
    //draws on the ground, either feeding mag of dist to reticle or something else
}

void instantiateWall()
{
    if (Time.time() - otherDevice.lastUpwardGesture() > delay)
    {
        cancelSummonWall();
        otherDevice.cancelSummonWall();
        //instantiate the wall
    }
}
//// WALL LOGIC ENDS HERE ////

//// BOULDER LOGIC STARTS HERE ////
void summonBoulder(sender, event)
{
    if (bothAreCloseTogether() && !bothAreCrossed() && canSummonBoulder() && otherDevice.canSummonBoulder() && isCooldowned(BOULDER_ID))
    {
        updateSpell = prepareBoulder;
        assignBoulderListeners();
        otherDevice.assignBoulderListeners();
    }
}

void assignBoulderListeners
{
    removeSpellListeners();
    onUpwardGesture += instantiateBoulder;
    onTriggerReleased += cancelSummonBoulder;
    onTriggerReleased += otherDevice.cancelSummonBoulder;
}

bool canSummonBoulder()
{
    Return(!nearGround() && triggerClicked);
}

void prepareBoulder()
{
    //draws on the ground, either feeding mag of dist to reticle or something else
}

void instantiateBoulder()
{
    if (Time.time() - otherDevice.lastUpwardGesture() > delay)
    {
        cancelSummonBoulder();
        otherDevice.cancelSummonBoulder();
        //instantiate the boulder
    }
}
//// BOULDER LOGIC ENDS HERE ////


//// EARTHWAVE LOGIC STARTS HERE////

bool canSummonEarthwave()
{
    return (aboveHead && triggerClicked && palmFacingUpward());
}

void prepareEarthwave()
{
    //draws on the ground stuff for earthwave
}

void assignEarthwaveListeners()
{
    removeSpellListeners();
    onDownwardGesture += instantiateEarthWave;
    onTriggerReleased += cancelSummonEarthwave;
    onTriggerReleased += otherDevice.cancelSummonEarthwave;
}

void instantiateEarthWave(startTime, sender, event)
{
    //dictate how fast according to diff between now and startTime
    if (Time.time() - otherDevice.lastDownwardGesture() > delay)
    {
        cancelSummonearthWave();
        otherDevice.cancelSummonEarthwave();
        //instantiate the wall
    }
}

void cancelSummonEarthwave()
{
    this.onTriggerReleased -= cancelSummonEarthwave;
    this.onTriggerReleased -= otherDevice.cancelSummonEarthwave;
    onUpwardGesture -= instantiateEarthWave;

    //cleanup stuff with reticle
    spellUpdate = null
}

void summonEarthwave(sender, event)
{
    if (canSummonEarthwave() && other.canSummonEarthwave() && isCooldowned(EARTHWAVE_ID))
    {
        updateSpell = prepareEarthwave
        assignEarthwaveListeners();
        otherDevice.assignEarthwaveListeners();
    }
}

//// EARTHWAVE LOGIC ENDS HERE////


void update()
{
    if (updateSpell != null)
    {
        updateSpell()
    }
}
*/

