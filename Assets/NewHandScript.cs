using UnityEngine;
using System.Collections;

public class NewHandScript : MonoBehaviour {

    private Animator handAnimator;
    private float currentBlend = 0.0f;
    //public SteamVR_Controller.Device VR_Controller_Script;
    private bool canGrab = false;
    private int deviceIndex = -1;
    

    // Use this for initialization
    void Start()
    {
        handAnimator = GetComponent<Animator>();
        handAnimator.SetFloat("handBlend", 0.5f);

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
    void Update()
    {

    }

    void CloseHand()
    {
        handAnimator.SetFloat("handBlend", 0.5f, 0.1f, Time.deltaTime);
    }

    void OpenHand()
    {
        handAnimator.SetFloat("handBlend", 1.0f, 0.1f, Time.deltaTime);
    }


    void DoTriggerAxisChanged(object sender, ControllerClickedEventArgs e)
    {
        handAnimator.SetFloat("handBlend", e.buttonPressure/2 + 0.5f);
    }

    void DoTriggerClicked(object sender, ControllerClickedEventArgs e)
    {
    }

    void DoTriggerUnclicked(object sender, ControllerClickedEventArgs e)
    {
    }
    Vector3 u = Vector3.up;
    
    void DoTouchpadClicked(object sender, ControllerClickedEventArgs e)
    {

    }

    void DoGripUnclicked(object sender, ControllerClickedEventArgs e)
    {
        handAnimator.SetFloat("handBlend", 0.5f);

    }

    void DoGripClicked(object sender, ControllerClickedEventArgs e)
    {
        handAnimator.SetFloat("handBlend", 0.0f);

    }
}
