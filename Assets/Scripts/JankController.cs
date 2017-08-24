using UnityEngine;
using System.Collections;

public class JankController : SpellController
{

    public ControllerInterface CI;

	// Use this for initialization
	void Start () {
        CI = GetComponent<ControllerInterface>();

        CI.controllerEvents.TriggerClicked += new ControllerClickedEventHandler(DoTriggerClicked);
        CI.controllerEvents.TriggerUnclicked += new ControllerClickedEventHandler(DoTriggerUnclicked);
        CI.controllerEvents.TouchpadClicked += new ControllerClickedEventHandler(DoTouchpadClicked);
        CI.controllerEvents.GripClicked += new ControllerClickedEventHandler(DoGripClicked);
        CI.controllerEvents.GripUnclicked += new ControllerClickedEventHandler(DoGripUnclicked);
       /* CI.gestureEvents.UpwardGesture += new GestureEventHandler(DoUpwardGesture);
        CI.gestureEvents.DownwardGesture += new GestureEventHandler(DoDownwardGesture);
        CI.gestureEvents.InwardGesture += new GestureEventHandler(DoInwardGesture);
        CI.gestureEvents.OutwardGesture += new GestureEventHandler(DoOutwardGesture);
        CI.gestureEvents.TwistGesture += new GestureEventHandler(DoTwistGesture); */
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void DoUpwardGesture(object sender, GestureEventArgs e)
    {
        Debug.Log("UPWARDDDD GESTURE!");
    }
    void DoDownwardGesture(object sender, GestureEventArgs e)
    {
        Debug.Log("DOOOOWNWARD GESTURE!");
    }
    void DoInwardGesture(object sender, GestureEventArgs e)
    {
        Debug.Log("IIIIIIIIINWARD GESTURE!");
    }
    void DoOutwardGesture(object sender, GestureEventArgs e)
    {
        Debug.Log("OUOUTWARD GESTURE!");
    }
    void DoTwistGesture(object sender, GestureEventArgs e)
    {
        Debug.Log("TWIIIIISTING GESTURE!");
    }

    void DoTriggerClicked(object sender, ControllerClickedEventArgs e)
    {
        Debug.Log("Trigger clicked");
    }

    void DoTriggerUnclicked(object sender, ControllerClickedEventArgs e)
    {
        Debug.Log("Trigger unclicked!!!");
    }


    void DoTouchpadClicked(object sender, ControllerClickedEventArgs e)
    {
        Debug.Log("Touchpad clicked!!!");
    }

    void DoGripUnclicked(object sender, ControllerClickedEventArgs e)
    {
        //ShowReticle();
        Debug.Log("Grip Unclicked!!!");
    }


    void DoGripClicked(object sender, ControllerClickedEventArgs e)
    {
        Debug.Log("Grip clicked!!!!");
    }
}
