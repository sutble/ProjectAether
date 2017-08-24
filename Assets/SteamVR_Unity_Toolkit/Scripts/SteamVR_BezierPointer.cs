//====================================================================================
//
// Purpose: Provide curved laser pointer at the ground to VR Controller
//
// This script must be attached to a Controller within the [CameraRig] Prefab
//
// The SteamVR_ControllerEvents script must also be attached to the Controller
//
// Press the default 'Grip' button on the controller to activate the beam
// Released the default 'Grip' button on the controller to deactivate the beam
//
// This script is an implementation of the SteamVR_WorldPointer.
//
//====================================================================================

using UnityEngine;
using System.Collections;

public class SteamVR_BezierPointer : SteamVR_WorldPointer
{
	public float pointerLength = 10f;
	public int pointerDensity = 10;
	public bool showPointerCursor = true;
	public float pointerCursorRadius = 0.05f;
	public GameObject customPointerTracer;
	public GameObject customPointerCursor;

	private Transform projectedBeamContainer;
	private Transform projectedBeamForward;
	private Transform projectedBeamJoint;
	public Transform projectedBeamDown;

	private GameObject innerCursor;
	private GameObject pointerCursor;
	private CurveGenerator curvedBeam;

	public bool isMaster;
	public GameObject otherHand;

	private bool reticleHidden = false;
	private bool pauseReticle = false;

	private Vector3 startScale = new Vector3(0.0f, 0.0f, 0.0f);
	private Vector3 endScale = new Vector3(1.0f, 1.0f, 1.0f);
	private float startTriggerTime;
	private float animationDuration;
	private bool animatingReticle = false;
	private bool coroutineAllowed = true;
	private bool dualHandCast = false;

	private IEnumerator fadeReticleAnimation;
	public GameObject controllerAngle;
	public Renderer pointerCursorRenderer;

	// Use this for initialization
	protected override void Start()
	{
		if (GetComponent<SteamVR_ControllerEvents>() == null)
		{
			Debug.LogError("SteamVR_ControllerEvents_ListenerExample is required to be attached to a SteamVR Controller that has the SteamVR_ControllerEvents script attached to it");
			return;
		}

		GetComponent<SteamVR_ControllerEvents>().TriggerClicked += new ControllerClickedEventHandler(DoTriggerClicked);
		GetComponent<SteamVR_ControllerEvents>().TriggerUnclicked += new ControllerClickedEventHandler(DoTriggerUnclicked);
		GetComponent<SteamVR_ControllerEvents>().GripClicked += new ControllerClickedEventHandler(DoGripClicked);
		GetComponent<SteamVR_ControllerEvents>().GripUnclicked += new ControllerClickedEventHandler(DoGripUnclicked);

		base.Start();

		InitProjectedBeams();
		InitPointer();
		TogglePointer(false);

		//init inner cursor
		innerCursor = pointerCursor.transform.GetChild(0).gameObject;
		innerCursor.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
		resize (pointerCursorRadius);
		ResetReticle ();
		fadeReticleAnimation = FadeOut(1.0f);	
	}
	public bool BothHandsActive()
	{
		return (GetComponent<EarthController>().GetDeviceIndex() != -1) && (otherHand.GetComponent<EarthController>().GetDeviceIndex() != -1);
	}

	void DoTriggerClicked(object sender, ControllerClickedEventArgs e)
	{
		AnimateReticle(1.0f); 
	}

	void DoTriggerUnclicked(object sender, ControllerClickedEventArgs e)
	{
		ResetReticle ();
	}

	void DoGripClicked(object sender, ControllerClickedEventArgs e)
	{
		AnimateReticle(1.0f);
	}

	void DoGripUnclicked(object sender, ControllerClickedEventArgs e)
	{
		ResetReticle ();
	}


	private void AnimateReticle(float duration)
	{
		resetInnerCursor ();
		animatingReticle = true;
		PauseReticle();
		animationDuration = duration;
		startTriggerTime = Time.time;
		coroutineAllowed = true;
		fadeReticleAnimation = FadeOut(1.0f);
		dualHandCast = isClose();
	}

	private void ResetReticle(){
		resetInnerCursor(); 
		animatingReticle = false;
		UnpauseReticle();
		ShowReticle();
		StopCoroutine(fadeReticleAnimation);
		coroutineAllowed = true;
		dualHandCast = false;
	}

	private void ReticleSizeUpdate(){

		if (reticleHidden) {
			resize (0.0f);
		} 
		else {
			if (dualHandCast || (BothHandsActive() && isClose () && !animatingReticle && !otherHand.GetComponent<SteamVR_BezierPointer>().animatingReticle)) {
				if (isMaster) {
					projectedBeamDown.position = GetCentroid ();
					resize (pointerCursorRadius * 2);
				}
				else {
					resize (0.0f);
				}
			} 
			else {
				resize (pointerCursorRadius);
			}
		}
	}

	private void DrawReticleAnimation()
	{
		if (animatingReticle)
		{
			float secondsPassed = (Time.time - startTriggerTime);
			float fracJourney = Mathf.Min((secondsPassed / animationDuration), 1.0f);
			if (fracJourney == 1 && animatingReticle && coroutineAllowed)
			{
				StartCoroutine(fadeReticleAnimation);
				coroutineAllowed = false;
			}
			else {
				innerCursor.transform.localScale = Vector3.Lerp(startScale, endScale, fracJourney);
			}
		}
	}

	IEnumerator FadeOut(float time)
	{
		yield return new WaitForSeconds(time);
		HideReticle ();
		Invoke ("resetInnerCursor", 0.15f);
		animatingReticle = false;
	}

	public bool isClose()
	{
		float dist = Vector3.Distance(otherHand.GetComponent<SteamVR_BezierPointer>().projectedBeamDown.position, projectedBeamDown.position);
		if (dist < 0.5f)
		{
			return true;
		}
		return false;
	}

	private void resize(float scaleValue)
	{
		pointerCursor.transform.localScale = new Vector3(scaleValue, 0.02f, scaleValue);
	}

	private Vector3 GetCentroid()
	{
		return (otherHand.GetComponent<SteamVR_BezierPointer>().projectedBeamDown.position + projectedBeamDown.position) / 2;
	}

	void resetInnerCursor(){
		innerCursor.transform.localScale = startScale;
	}

	public void HideReticle()
	{
		reticleHidden = true;
	}

	public void ShowReticle()
	{
		reticleHidden = false;
	}

	public void PauseReticle()
	{
		pauseReticle = true;
	}

	public void UnpauseReticle()
	{
		pauseReticle = false;
	}

	protected override void Update()
	{
		base.Update();
		if (projectedBeamForward.gameObject.activeSelf)
		{
			ProjectForwardBeam();
			ProjectDownBeam();
			//DisplayCurvedBeam();
			ReticleSizeUpdate();
			DrawReticleAnimation();
			SetPointerCursor();
		}
	}

	protected override void InitPointer()
	{
		pointerCursor = (customPointerCursor ? Instantiate(customPointerCursor) : CreateCursor());
		//pointerCursor = customPointerCursor;

		pointerCursor.name = string.Format("[{0}]PlayerObject_WorldPointer_BezierPointer_PointerCursor", this.gameObject.name);
		pointerCursor.layer = 2;
		pointerCursor.SetActive(false);

		GameObject global = new GameObject(string.Format("[{0}]PlayerObject_WorldPointer_BezierPointer_CurvedBeamContainer", this.gameObject.name));
		global.SetActive(false);
		curvedBeam = global.gameObject.AddComponent<CurveGenerator>();
		curvedBeam.transform.parent = null;
		curvedBeam.Create(pointerDensity, pointerCursorRadius, customPointerTracer, this.transform);
		base.InitPointer();
	}

	private GameObject CreateCursor()
	{
		float cursorYOffset = 0.2f;
		GameObject cursor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		cursor.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		cursor.GetComponent<MeshRenderer>().receiveShadows = false;
		cursor.transform.localScale = new Vector3(pointerCursorRadius, cursorYOffset, pointerCursorRadius);
		Destroy(cursor.GetComponent<CapsuleCollider>());
		return cursor;
	}

	protected override void SetPointerMaterial()
	{
		if (pointerCursor.GetComponent<MeshRenderer>())
		{
			pointerCursor.GetComponent<MeshRenderer>().material = pointerMaterial;
		}

		foreach(MeshRenderer mr in pointerCursor.GetComponentsInChildren<MeshRenderer>())
		{
			mr.material = pointerMaterial;
		}

		if (pointerCursor.GetComponent<SkinnedMeshRenderer>())
		{
			pointerCursor.GetComponent<SkinnedMeshRenderer>().material = pointerMaterial;
		}

		foreach (SkinnedMeshRenderer mr in pointerCursor.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			mr.material = pointerMaterial;
		}

		base.SetPointerMaterial();
	}

	protected override void TogglePointer(bool state)
	{
		//charles touched this
		beamAlwaysOn = true;
		state = (beamAlwaysOn ? true : state);

		projectedBeamForward.gameObject.SetActive(state);
		projectedBeamJoint.gameObject.SetActive(state);
		projectedBeamDown.gameObject.SetActive(state);
	}

	protected override void DisablePointerBeam(object sender, ControllerClickedEventArgs e)
	{
		base.PointerSet();
		base.DisablePointerBeam(sender, e);
		TogglePointerCursor(false);
		curvedBeam.TogglePoints(false);
	}

	private void TogglePointerCursor(bool state)
	{
		//charles touched this
		beamAlwaysOn = true;

		state = (beamAlwaysOn ? true : state);

		bool pointerCursorState = (showPointerCursor && state ? showPointerCursor : false);
		bool playAreaCursorState = (showPlayAreaCursor && state ? showPlayAreaCursor : false);
		pointerCursor.gameObject.SetActive(pointerCursorState);
		base.TogglePointer(playAreaCursorState);
	}

	private void InitProjectedBeams()
	{
		projectedBeamContainer = new GameObject(string.Format("[{0}]PlayerObject_WorldPointer_BezierPointer_ProjectedBeamContainer", this.gameObject.name)).transform;
		projectedBeamContainer.transform.parent = this.transform;
		projectedBeamContainer.transform.localPosition = Vector3.zero;
		//projectedBeamContainer.TransformDirection(transform.TransformDirection(Vector3.down));

		projectedBeamForward = new GameObject(string.Format("[{0}]PlayerObject_WorldPointer_BezierPointer_ProjectedBeamForward", this.gameObject.name)).transform;
		projectedBeamForward.transform.parent = projectedBeamContainer.transform;

		projectedBeamJoint = new GameObject(string.Format("[{0}]PlayerObject_WorldPointer_BezierPointer_ProjectedBeamJoint", this.gameObject.name)).transform;
		projectedBeamJoint.transform.parent = projectedBeamContainer.transform;
		projectedBeamJoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

		projectedBeamDown = new GameObject(string.Format("[{0}]PlayerObject_WorldPointer_BezierPointer_ProjectedBeamDown", this.gameObject.name)).transform;
	}

	private float GetForwardBeamLength()
	{
		float actualLength = pointerLength;
		Ray pointerRaycast = new Ray(transform.position, -(this.transform.up));
		RaycastHit collidedWith;
		bool hasRayHit = Physics.Raycast(pointerRaycast, out collidedWith);

		//reset if beam not hitting or hitting new target
		if (!hasRayHit || (pointerContactTarget && pointerContactTarget != collidedWith.transform))
		{
			pointerContactDistance = 0f;
		}

		//check if beam has hit a new target
		if (hasRayHit)
		{
			pointerContactDistance = collidedWith.distance;
		}

		//adjust beam length if something is blocking it
		if (hasRayHit && pointerContactDistance < pointerLength)
		{
			actualLength = pointerContactDistance;
		}

		return actualLength;
	}

	private void ProjectForwardBeam()
	{
		float setThicknes = 0.01f;
		float setLength = GetForwardBeamLength();
		//if the additional decimal isn't added then the beam position glitches
		//zain was here
		float beamPosition = setLength / (2 + 0.00001f);

		projectedBeamForward.transform.localScale = new Vector3(setThicknes, setThicknes, setLength);
		projectedBeamForward.transform.localPosition = new Vector3(0f, 0f, beamPosition);
		// projectedBeamForward.transform.Rotate(90, 90, 90);

		projectedBeamJoint.transform.localPosition = new Vector3(0f, -(setLength - (projectedBeamJoint.transform.localScale.z / 2)), 0f);
		//projectedBeamJoint.transform.Rotate(90, 90, 90);

		projectedBeamContainer.transform.localRotation = Quaternion.identity;
		//  projectedBeamContainer.transform.Rotate(90, 90, 90);

	}

	private void ProjectDownBeam()
	{
		projectedBeamDown.transform.position = new Vector3(projectedBeamJoint.transform.position.x, projectedBeamJoint.transform.position.y, projectedBeamJoint.transform.position.z);

		Ray projectedBeamDownRaycast = new Ray(projectedBeamDown.transform.position, Vector3.down);
		RaycastHit collidedWith;
		bool downRayHit = Physics.Raycast(projectedBeamDownRaycast, out collidedWith);

		if (!downRayHit || (pointerContactTarget && pointerContactTarget != collidedWith.transform))
		{
			if (pointerContactTarget != null)
			{
				base.PointerOut();
			}
			pointerContactTarget = null;
			destinationPosition = Vector3.zero;
		}

		if (downRayHit)
		{
			projectedBeamDown.transform.position = new Vector3(projectedBeamJoint.transform.position.x, projectedBeamJoint.transform.position.y - collidedWith.distance, projectedBeamJoint.transform.position.z);
			projectedBeamDown.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			pointerContactTarget = collidedWith.transform;
			destinationPosition = projectedBeamDown.transform.position;

			base.PointerIn();
		}
	}

	private void SetPointerCursor()
	{
		if (!pauseReticle)
		{
			if (pointerContactTarget != null)
			{
				TogglePointerCursor(true);
				Vector3 offset = new Vector3(0f, 0.02f, 0f);
				pointerCursor.transform.position = projectedBeamDown.transform.position + offset;
				base.SetPlayAreaCursorTransform(pointerCursor.transform.position);
				//UpdatePointerMaterial(pointerHitColor);
			}
			else
			{
				TogglePointerCursor(false);
				//UpdatePointerMaterial(pointerMissColor);
			}
			//Debug.Log("Unpause drawing the reticle");
		}
	}

	private void DisplayCurvedBeam()
	{
		Vector3[] beamPoints = new Vector3[]
		{
			this.transform.position,
			projectedBeamJoint.transform.position + new Vector3(0f, 1f, 0f),
			projectedBeamDown.transform.position,
			projectedBeamDown.transform.position,
		};
		curvedBeam.SetPoints(beamPoints, pointerMaterial);
		curvedBeam.TogglePoints(true);
	}
}