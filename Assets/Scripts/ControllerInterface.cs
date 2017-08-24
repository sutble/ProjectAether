using UnityEngine;
//using UnityEditor;
using System.Collections;

public class ControllerInterface : MonoBehaviour
{
    public GameObject head;
    public GameObject hand;
    public GameObject otherHand;
    public GameObject room;

    public SteamVR_TrackedObject R;
    public SteamVR_TrackedObject L;
    public SteamVR_ControllerEvents controllerEvents;
    public GestureEvents gestureEvents;

    /*public event ControllerClickedEventHandler TriggerPressed;
    public event ControllerClickedEventHandler TriggerReleased;

    public event ControllerClickedEventHandler TriggerAxisChanged;

    public event ControllerClickedEventHandler ApplicationMenuPressed;
    public event ControllerClickedEventHandler ApplicationMenuReleased;

    public event ControllerClickedEventHandler GripPressed;
    public event ControllerClickedEventHandler GripReleased;

    public event ControllerClickedEventHandler TouchpadPressed;
    public event ControllerClickedEventHandler TouchpadReleased;

    public event ControllerClickedEventHandler TouchpadTouchStart;
    public event ControllerClickedEventHandler TouchpadTouchEnd;

    public event ControllerClickedEventHandler TouchpadAxisChanged;

    public event ControllerClickedEventHandler AliasPointerOn;
    public event ControllerClickedEventHandler AliasPointerOff;

    public event ControllerClickedEventHandler AliasGrabOn;
    public event ControllerClickedEventHandler AliasGrabOff;

    public event ControllerClickedEventHandler AliasUseOn;
    public event ControllerClickedEventHandler AliasUseOff;

    public event ControllerClickedEventHandler AliasMenuOn;
    public event ControllerClickedEventHandler AliasMenuOff;

    public event GestureEventHandler UpwardGesture;
    public event GestureEventHandler DownwardGesture;
    public event GestureEventHandler OutwardGesture;
    public event GestureEventHandler InwardGesture;
    public event GestureEventHandler TwistGesture;*/

    void Awake()
    {
        // init device
        hand = this.gameObject;

        controllerEvents = hand.GetComponent<SteamVR_ControllerEvents>();
        gestureEvents = hand.GetComponent<GestureEvents>();
        //R.SetDeviceIndex(1);
        //L.SetDeviceIndex(2);

		if (IsRight()) {
			Debug.Log ("RIGHT EARTH CONTROLLER ACTIVE");
		} 
		if (IsLeft()) {
			Debug.Log ("LEFT EARTH CONTROLLER ACTIVE");
		}
			
	}

	public bool IsRight()
	{
		return (hand.name == "PlayerObject_Controller (right)");
        
    }

	public bool IsLeft()
	{
		return (hand.name == "PlayerObject_Controller (left)");
	}

    public bool PalmUp()
    {
        if ((hand.name == "PlayerObject_Controller (right)"))
        {
            return hand.transform.eulerAngles.z < 180.0;
        }
        else
        {
            return hand.transform.eulerAngles.z > 180.0;
        }
    }

    public Vector3 PointerGroundPosition()
    {
        return GetComponent<ReticleController>().projectedBeamDown.position;
    }

    public Vector3 FixedPointerGroundPosition()
    {
        return GetComponent<ReticleController>().GetFixedPosition();
    }


    public float PointerDistanceToOther()
    {
        return (Vector3.Distance(PointerGroundPosition(), otherHand.GetComponent<ControllerInterface>().PointerGroundPosition()));
    }

    public Vector3 PointerCentroidPosition()
    {
        return (PointerGroundPosition() + otherHand.GetComponent<ControllerInterface>().PointerGroundPosition())/2.0F;
    }

    public Vector3 FixedPointerCentroidPosition()
    {
        return (FixedPointerGroundPosition() + otherHand.GetComponent<ControllerInterface>().FixedPointerGroundPosition()) / 2.0F;
    }

    public bool NearGround()
    {
        return (hand.transform.position.y < room.transform.position.y + 0.5);
    }

    public bool AboveHead()
    {
        return (hand.transform.position.y > head.transform.position.y);
    }

    public float DistanceToOther()
    {
        return (Vector3.Distance(hand.transform.position, otherHand.transform.position));
    }

	public float AngleBetween()
	{
		Vector3 a = hand.transform.position;
		Vector3 b = otherHand.transform.position;
		Vector3 c = head.transform.position;

		a.y = 0;
		b.y = 0;
		c.y = 0;

		Vector3 ca = a - c;
		Vector3 cb = b - c;

		return Vector3.Angle(ca, cb); 
	}

	public Vector3 AverageVector()
	{
		Vector3 a = hand.transform.position;
		Vector3 b = otherHand.transform.position;
		Vector3 c = head.transform.position;


		a.y = 0;
		b.y = 0;
		c.y = 0;

		Vector3 ca = a - c;
		ca = ca.normalized;
		Vector3 cb = b - c;
		cb = cb.normalized;

		return (ca + cb).normalized;
	}

    public bool IsCrossed()
    {
        Vector3 a, b, c;

		if(IsRight())
        {
            a = hand.transform.position;
            b = otherHand.transform.position;
        } else
        {
            a = otherHand.transform.position;
            b = hand.transform.position;
        }

        c = head.transform.position;

		a.y = 0;
		b.y = 0;
		c.y = 0;

        Vector3 ca = a - c, cb = b - c;
		Vector3 cross = Vector3.Cross(ca, cb);

		if (IsRight()) {
			//Debug.Log ("hname right value = " + cross.y);
			if (cross.y < 0.0f){
				return false;
			}
			return true;
		}

		else {
			//Debug.Log ("hname left value = " + cross.y);
			if (cross.y < 0.0f){
				return false;
			}
			return true;
		}


    }

    public Vector3 ForwardVector()
    {
        return hand.transform.forward;
    }
}