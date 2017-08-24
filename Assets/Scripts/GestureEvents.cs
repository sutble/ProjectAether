using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public struct GestureEventArgs
{
    public uint controllerIndex;
    public Vector3 velocity;
    public Transform transform;
}

public struct QueueInfo
{
    public SteamVR_Utils.RigidTransform transform;
    public Vector3 velocity;
    public GameObject voxel;
}

public delegate void GestureEventHandler(object sender, GestureEventArgs e);

public class GestureEvents : MonoBehaviour
{
    public GameObject v;

    public Queue<QueueInfo> transformQueue;
    public int NUM_FRAMES = 15;

    private uint controllerIndex;
    private SteamVR_TrackedObject trackedController;
    private SteamVR_Controller.Device device;

    public event GestureEventHandler UpwardGesture;
    public event GestureEventHandler DownwardGesture;
    public event GestureEventHandler OutwardGesture;
    public event GestureEventHandler InwardGesture;
    public event GestureEventHandler TwistGesture;

    public float velocityFudgeFactor = 1.2f;
	public float distanceFudgeFactor = 0.35f;

    public Vector3 avgVelocity;

    public SteamVR_Utils.RigidTransform currentTransform;

    public virtual void OnUpwardGesture(GestureEventArgs e)
    {
        if (UpwardGesture != null)
            UpwardGesture(this, e);
    }

    public virtual void OnDownwardGesture(GestureEventArgs e)
    {
        if (DownwardGesture != null)
            DownwardGesture(this, e);
    }

    public virtual void OnOutwardGesture(GestureEventArgs e)
    {
        if (OutwardGesture != null)
            OutwardGesture(this, e);
    }

    public virtual void OnInwardGesture(GestureEventArgs e)
    {
        if (InwardGesture != null)
            InwardGesture(this, e);
    }


    public virtual void OnTwistGesture(GestureEventArgs e)
    {
        if (TwistGesture != null)
            TwistGesture(this, e);
    }


    void Awake()
    {
        // init device
        trackedController = GetComponent<SteamVR_TrackedObject>();

        transformQueue = new Queue<QueueInfo>();
    }



    void Update()
    {
        controllerIndex = (uint)trackedController.index;
        device = SteamVR_Controller.Input((int)controllerIndex);

        if (transformQueue.Count < NUM_FRAMES)
        {
            transformQueue.Enqueue(SetGestureInfo(device.transform, device.velocity));

        }
        else
        {
            Destroy(transformQueue.Peek().voxel);
            transformQueue.Dequeue();
            transformQueue.Enqueue(SetGestureInfo(device.transform, device.velocity));
        }



        currentTransform = device.transform;


        foreach (QueueInfo g in transformQueue)
        {
            avgVelocity += g.velocity;
        }

        avgVelocity = avgVelocity / transformQueue.Count;
        //Debug.Log("average velocity magnitude: " + avgVelocity.magnitude);

        if (DidUpwardGesture())
        {
            OnUpwardGesture(SetGestureEvent(device.velocity, this.gameObject.transform));
        }
        else if (DidDownwardGesture())
        {
            OnDownwardGesture(SetGestureEvent(device.velocity, this.gameObject.transform));
        }

        if (DidOutwardGesture())
        {
            OnOutwardGesture(SetGestureEvent(device.velocity, this.gameObject.transform));
        }
        else if (DidInwardGesture())
        {
            OnInwardGesture(SetGestureEvent(device.velocity, this.gameObject.transform));
        }

        if (DidTwistGesture())
        {
            OnTwistGesture(SetGestureEvent(device.velocity, this.gameObject.transform));
        }

    }

    QueueInfo SetGestureInfo(SteamVR_Utils.RigidTransform transform, Vector3 velocity)
    {
        QueueInfo g;
        g.transform = transform;
        g.velocity = velocity;
        g.voxel = (GameObject)Instantiate(v, this.gameObject.transform.position, Quaternion.identity);
        return g;
    }

    GestureEventArgs SetGestureEvent(Vector3 v, Transform t)
    {
        GestureEventArgs e;
        e.controllerIndex = controllerIndex;
        e.velocity = v;
        e.transform = t;
        return e;
    }

    bool DidUpwardGesture()
    {
        Vector3 upwardAxis = Vector3.up;
		Vector3 final = currentTransform.pos, start = transformQueue.Peek().transform.pos;
		Vector3 distanceTravelled = final - start;
		if (avgVelocity.magnitude > velocityFudgeFactor && distanceTravelled.magnitude > distanceFudgeFactor)
        {
            //Debug.Log(avgVelocity.magnitude + " is greater than " + fudgeFactor);
            return (Vector3.Dot(upwardAxis, avgVelocity) > 0);
        }
        else
        {
            //Debug.Log(avgVelocity.magnitude + " is less than " + fudgeFactor);
            return false;
        }
    }

    bool DidDownwardGesture()
    {
        Vector3 downwardAxis = Vector3.down;
		Vector3 final = currentTransform.pos, start = transformQueue.Peek().transform.pos;
		Vector3 distanceTravelled = final - start;
		if (avgVelocity.magnitude > velocityFudgeFactor && distanceTravelled.magnitude > distanceFudgeFactor)
        {
            return (Vector3.Dot(downwardAxis, avgVelocity) > 0);
        }
        else
        {
            return false;
        }
    }

    bool DidInwardGesture()
    {
        Vector3 outwardAxis = this.gameObject.transform.up;
		Vector3 final = currentTransform.pos, start = transformQueue.Peek().transform.pos;
		Vector3 distanceTravelled = final - start;
		if (avgVelocity.magnitude > velocityFudgeFactor && distanceTravelled.magnitude > distanceFudgeFactor)
        {
            return (Vector3.Dot(outwardAxis, avgVelocity) > 0);
        }
        else
        {
            return false;
        }
    }

    bool DidOutwardGesture()
    {
        Vector3 outwardAxis = -this.gameObject.transform.up;
		Vector3 final = currentTransform.pos, start = transformQueue.Peek().transform.pos;
		Vector3 distanceTravelled = final - start;
		if (avgVelocity.magnitude > velocityFudgeFactor && distanceTravelled.magnitude > distanceFudgeFactor)
        {
            return (Vector3.Dot(outwardAxis, avgVelocity) > 0);
        }
        else
        {
            return false;
        }
    }

    bool DidTwistGesture()
    {
        float diff = QuaternionDifference(currentTransform.rot, transformQueue.Peek().transform.rot);
        //Debug.Log(diff + " twist difference " + avgVelocity.magnitude + " velocity magnitude!!!");
        return diff > Mathf.PI * 3/4;// / 2;
    }

    float QuaternionDifference(Quaternion q1, Quaternion q2)
    {
        return 2 * Mathf.Acos((q1*Quaternion.Inverse(q2)).w);
    }
}