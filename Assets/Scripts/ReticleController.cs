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

public class ReticleController : MonoBehaviour
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

    //old world pointer traits
    private Material pointerMaterial;
    private GameObject playAreaCursor;
    public bool showPlayAreaCursor = false;
    protected Transform pointerContactTarget = null;
    protected float pointerContactDistance = 0f;
    protected Vector3 destinationPosition;

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

    private Vector3 fixedPosition;
    private bool isFixedPosition = false;

    public float closenessThreshold = 0.5F;

    private ControllerInterface CI;
    private ReticleController otherRC;

    public StateAlias state;

    public enum StateAlias
    {
        None,
        Singular,
        SingularFixed,
        MergedCircle,
        MergedRectangle
    }

    // Use this for initialization
    void Start()
    {
        this.name = "PlayerObject_" + this.name;
        InitProjectedBeams();
        InitPointer();
        resize(pointerCursorRadius);
        TogglePointerCursor(true);

        CI = GetComponent<ControllerInterface>();
        otherRC = CI.otherHand.GetComponent<ReticleController>();

        ChangeState(StateAlias.Singular);
    }

	public void resize(float scaleValue)
    {
        pointerCursor.transform.localScale = new Vector3(scaleValue, 0.02f, scaleValue);
    }

    private void SetPosition(Vector3 v)
    {
        isFixedPosition = true;
        fixedPosition = v;
    }

    private void ClearPosition()
    {
        isFixedPosition = false;
    }

    private bool IsFixedPosition()
    {
        return isFixedPosition;
    }

    void Update()
    {
        /*if (projectedBeamForward.gameObject.activeSelf)
        {
            ProjectForwardBeam();
            ProjectDownBeam();
            SetPointerCursor();
        }*/
        switch(state)
        {
            case StateAlias.None:
                break;
            case StateAlias.Singular:
                SingularUpdate();
                break;
            case StateAlias.SingularFixed:
                SingularFixedUpdate();
                break;
            case StateAlias.MergedCircle:
                MergedCircleUpdate();
                break;
            case StateAlias.MergedRectangle:
                MergedRectangleUpdate();
                break;
        }
    }

    void InitPointer()
    {
        pointerCursor = (customPointerCursor ? Instantiate(customPointerCursor) : CreateCursor());
        pointerCursor.name = string.Format("[{0}]PlayerObject_WorldPointer_BezierPointer_PointerCursor", this.gameObject.name);
        pointerCursor.layer = 2;
        pointerCursor.SetActive(false);
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

    void SetPointerMaterial()
    {
        if (pointerCursor.GetComponent<MeshRenderer>())
        {
            pointerCursor.GetComponent<MeshRenderer>().material = pointerMaterial;
        }

        foreach (MeshRenderer mr in pointerCursor.GetComponentsInChildren<MeshRenderer>())
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
    }

    private void TogglePointerCursor(bool state)
    {
        //charles touched this
        state = true;
        bool pointerCursorState = (showPointerCursor && state ? showPointerCursor : false);
        bool playAreaCursorState = (showPlayAreaCursor && state ? showPlayAreaCursor : false);
        pointerCursor.gameObject.SetActive(pointerCursorState);
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
		if (hasRayHit && collidedWith.transform.gameObject.name != "voxel(Clone)")
        {
            pointerContactDistance = collidedWith.distance;
        }

        //adjust beam length if something is blocking it
		if (hasRayHit && pointerContactDistance < pointerLength  && collidedWith.transform.gameObject.name != "voxel(Clone)")
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
                //base.PointerOut();
            }
            pointerContactTarget = null;
            destinationPosition = Vector3.zero;
        }

		if (downRayHit && collidedWith.transform.gameObject.name != "voxel(Clone)")
        {
            projectedBeamDown.transform.position = new Vector3(projectedBeamJoint.transform.position.x, projectedBeamJoint.transform.position.y - collidedWith.distance, projectedBeamJoint.transform.position.z);
            projectedBeamDown.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            pointerContactTarget = collidedWith.transform;
            destinationPosition = projectedBeamDown.transform.position;

            //base.PointerIn();
        }
    }

    private void SetPointerCursor()
    {
        if (!pauseReticle)
        {
            if (pointerContactTarget != null)
            {
                Vector3 offset = new Vector3(0f, 0.02f, 0f);
                if(IsFixedPosition())
                {
                    pointerCursor.transform.position = fixedPosition + offset;
                }
                else
                {
                    pointerCursor.transform.position = projectedBeamDown.transform.position + offset;
                }
            }
        }
    }

    public Vector3 GetPosition()
    {
        return projectedBeamDown.transform.position;
    }

    public Vector3 GetFixedPosition()
    {
        return fixedPosition; // IsFixedPosition() ? fixedPosition : GetPosition();
    }

    public void ChangeState(StateAlias s)
    {
        switch(s)
        {
            case StateAlias.None:
                ClearPosition();
                resize(0.0F);
                break;
            case StateAlias.Singular:
                ClearPosition();
                resize(pointerCursorRadius);
                break;
            case StateAlias.SingularFixed:
                SetPosition(GetPosition());
                resize(pointerCursorRadius);
                break;
            case StateAlias.MergedCircle:
                SetPosition(CI.PointerCentroidPosition());
                resize(pointerCursorRadius * 2);
                break;
            case StateAlias.MergedRectangle:
                SetPosition(CI.PointerCentroidPosition());
                resize(pointerCursorRadius * 2);
                break;
        }

        state = s;
    }

    private bool IsClose()
    {
        return CI.PointerDistanceToOther() < closenessThreshold;
    }

    private void MergedRectangleUpdate()
    {
        resize(pointerCursorRadius * 2);
        SetPointerCursor();
    }

    private void MergedCircleUpdate()
    {
        //resize(pointerCursorRadius * 2);
        SetPointerCursor();
    }

    private void SingularFixedUpdate()
    {
        resize(pointerCursorRadius);
        SetPointerCursor();
    }

    private void SingularUpdate()
    {
        ClearPosition();
        if (otherRC.state == StateAlias.Singular && IsClose())
        {
            DrawBigCircle();
        } else
        {
            DrawSmallCircle();
        }
    }

    private void DrawSmallCircle()
    {
        resize(pointerCursorRadius);
        ProjectForwardBeam();
        ProjectDownBeam();
        SetPointerCursor();
    }

    private void DrawBigCircle()
    {
        SetPosition(CI.PointerCentroidPosition());
        resize(pointerCursorRadius * 2);
        ProjectForwardBeam();
        ProjectDownBeam();
        SetPointerCursor();
    }
}