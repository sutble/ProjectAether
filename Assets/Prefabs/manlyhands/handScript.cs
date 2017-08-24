using UnityEngine;
using System.Collections;

public class handScript : MonoBehaviour {
    private Animator handAnimator;
    private float currentBlend = 0.0f;
    //public SteamVR_Controller.Device VR_Controller_Script;
    private bool canGrab = false;
    private int deviceIndex = -1;

    // Use this for initialization
    void Start () {
        handAnimator = GetComponent<Animator>();

        

        if (this.GetComponentInParent<SteamVR_ControllerEvents>() == null)
        {
            Debug.LogError("SteamVR_ControllerEvents_ListenerExample is required to be attached to a SteamVR Controller that has the SteamVR_ControllerEvents script attached to it");
            return;
        }

        this.GetComponentInParent<SteamVR_ControllerEvents>().TriggerAxisChanged += new ControllerClickedEventHandler(DoTriggerAxisChanged);
        this.GetComponentInParent<SteamVR_ControllerEvents>().TriggerClicked += new ControllerClickedEventHandler(DoTriggerClicked);
        this.GetComponentInParent<SteamVR_ControllerEvents>().TriggerUnclicked += new ControllerClickedEventHandler(DoTriggerUnclicked);
        this.GetComponentInParent<SteamVR_ControllerEvents>().TouchpadClicked += new ControllerClickedEventHandler(DoTouchpadClicked);
        this.GetComponentInParent<SteamVR_ControllerEvents>().GripClicked += new ControllerClickedEventHandler(DoGripClicked);
        this.GetComponentInParent<SteamVR_ControllerEvents>().GripUnclicked += new ControllerClickedEventHandler(DoGripUnclicked);

    }

    // Update is called once per frame
    void Update () {
        //Debug.Log("device index: " + deviceIndex);
        /*
        if (deviceIndex != -1 && SteamVR_Controller.Input(deviceIndex).GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {
            //Debug.Log("is pulling trigger on hand");
            handAnimator.SetFloat("handBlend", 1.0f, 0.1f, Time.deltaTime);
        }
        else if (transform.parent.GetComponent<InteractPromptScript>().canGrab)
        {
            handAnimator.SetFloat("handBlend", 0.5f, 0.1f, Time.deltaTime);
        }
        else
        {
            handAnimator.SetFloat("handBlend", 0.0f, 0.1f, Time.deltaTime);
        }*/

        if (this.GetComponentInParent<SteamVR_ControllerEvents>().triggerPressed)
        {
            //CloseHand();
        }
        else if (!this.GetComponentInParent<SteamVR_ControllerEvents>().triggerPressed)
        {
           // DefaultHand();
        }

    }

    void DefaultHand()
    {
        handAnimator.SetFloat("handBlend", 0.0f, 0.1f, Time.deltaTime);
    }

    void CloseHand(float rate)
    {
        handAnimator.SetFloat("handBlend", 1.0f, 0.1f, Time.deltaTime);
    }

    void OpenHand()
    {
        handAnimator.SetFloat("handBlend", 0.0f, 0.5f, Time.deltaTime);

    }


    void DoTriggerAxisChanged(object sender, ControllerClickedEventArgs e)
    {
        Debug.Log("TRIGGER axis changed: " + e.buttonPressure);
        handAnimator.SetFloat("handBlend", e.buttonPressure);
    }

    void DoTriggerClicked(object sender, ControllerClickedEventArgs e)
    {
        //CloseHand(e.buttonPressure);
    }

    void DoTriggerUnclicked(object sender, ControllerClickedEventArgs e)
    {
        //DefaultHand(e.buttonPressure);
    }


    void DoTouchpadClicked(object sender, ControllerClickedEventArgs e)
    {

    }

    void DoGripUnclicked(object sender, ControllerClickedEventArgs e)
    {

    }

    void DoGripClicked(object sender, ControllerClickedEventArgs e)
    {
        
    }
    
}
