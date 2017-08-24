using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class FadeWalker : MonoBehaviour
{

    private ControllerInterface CI;

    public GameObject cam;
    public GameObject room;

    private BlurOptimized BO;

    private Vector2 movement;

    public float maxWalkSpeed = 10f;
    public float deceleration = 0.1f;

    private float movementSpeed = 0f;
    private float strafeSpeed = 0f;

    // Use this for initialization
    void Start()
    {
        CI = GetComponent<ControllerInterface>();

        BO = cam.GetComponent<BlurOptimized>();

        CI.controllerEvents.TouchpadAxisChanged += new ControllerClickedEventHandler(startMovement);
        CI.controllerEvents.TouchpadClicked += new ControllerClickedEventHandler(startBlur);
        CI.controllerEvents.TouchpadUnclicked += new ControllerClickedEventHandler(endMovement);

        turnBlurOff();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CalculateSpeed(ref movementSpeed, movement.y);
        CalculateSpeed(ref strafeSpeed, movement.x);
        // Debug.Log("Touchpad " + CI.controllerEvents.touchpadPressed);
        if(CI.controllerEvents.touchpadPressed)
        {
            Move();
        }
    }

    void startBlur(object sender, ControllerClickedEventArgs e)
    {
        Debug.Log("STARTING BLUR");
        turnBlurOn();
    }

    void startMovement(object sender, ControllerClickedEventArgs e)
    {
        movement = e.touchpadAxis;
    }

    void endMovement(object sender, ControllerClickedEventArgs e)
    {
        movement = Vector2.zero;
        turnBlurOff();
    }

    void turnBlurOn()
    {
        SteamVR_Fade.Start(new Color(0f, 0f, 0f, 0.92F), 0.5F);
        cam.GetComponent<BlurOptimized>().enabled = true;
    }

    void turnBlurOff()
    {
        cam.GetComponent<BlurOptimized>().enabled = false;
        SteamVR_Fade.Start(new Color(0f, 0f, 0f, 0f), 0.5f);
    }

    private void CalculateSpeed(ref float speed, float inputValue)
    {
        if (inputValue != 0f)
        {
            speed = (maxWalkSpeed * inputValue);
        }
        else
        {
            Decelerate(ref speed);
        }
    }

    private void Decelerate(ref float speed)
    {
        /*if (speed > 0)
        {
            speed -= Mathf.Lerp(deceleration, maxWalkSpeed, 0f);
        }
        else if (speed < 0)
        {
            speed += Mathf.Lerp(deceleration, -maxWalkSpeed, 0f);
        }
        else
        {
            turnBlurOff();
            speed = 0;
        }

        float deadzone = 0.1f;
        if (speed < deadzone && speed > -deadzone)
        {
            speed = 0;
            turnBlurOff();
        }*/
        speed = 0;
    }

    private void Move()
    {
		RaycastHit hit;
        var movement = cam.transform.forward * movementSpeed * Time.deltaTime;
        var strafe = cam.transform.right * strafeSpeed * Time.deltaTime;
        float fixY = room.transform.position.y;
        room.transform.position += (movement + strafe);
        room.transform.position = new Vector3(room.transform.position.x, fixY, room.transform.position.z);

		if (Physics.Raycast (cam.transform.position - new Vector3 (0, 0.1F, 0), -Vector3.up, out hit)) {
			//Debug.Log ("Hit: " + hit.point + " HEAD: " + cam.transform.position + " ROOM: " + room.transform.position);
			room.transform.position = new Vector3 (room.transform.position.x, hit.point.y + 0.5F, room.transform.position.z);
		}
    }
}
