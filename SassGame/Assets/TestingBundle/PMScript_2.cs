using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMScript_2 : MonoBehaviour
{
    [SerializeField] private Vector2 sensitivity;
    [SerializeField] private Vector2 acceleration;
    [SerializeField] private float inputLagPeriod;
    [SerializeField] private float maxVerticalAngleFromHorizon;
    [SerializeField] private float speed = 10.0f;
    //[SerializeField] private float surfaceLevel = 3.5f;
    private Vector2 velocity;
    private Vector2 rotation; //the current rotation in degrees
    private Vector2 lastInputEvent;//The last recieved non-zerro input value
    private float inputLagTimer;//The time since the last recieved non-zero input value
    private float activeForwardSpeed, activeStrafeSpeed; //Horizontal/Vertical movement
    
    private void OnEnable() {
        //Reset the slate
        velocity = Vector2.zero;
        inputLagTimer = 0;
        lastInputEvent = Vector2.zero;

        //Calculate the current rotation by getting the gamObject's local euler angle
        Vector3 euler = transform.localEulerAngles;
        //Euler angles range from [0, 360), but we want [-100, 100)
        if (euler.x >= 180) {
            euler.x -=360;
        }
        euler.x = ClampVerticalAngle(euler.x);
        //Set the angle here to clamp the current rotation
        transform.localEulerAngles = euler;
        //Rotation stores as (horizontal, vertical), which corresponds to the euler angles
        //around the y (up) axis and the x (right) axis
        rotation = new Vector2(euler.y, euler.x);
    }
    private float ClampVerticalAngle(float angle){
        return Mathf.Clamp(angle, -maxVerticalAngleFromHorizon, maxVerticalAngleFromHorizon);
    }
    private Vector2 GetInput() {
        //Add to the lag timer
        inputLagTimer += Time.deltaTime;
        Vector2 input = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
        //Sometimes at fast framerates, Unity will not receive input events every frame, which results
        //in zero values being given above. This can cause shuttering and make it difficult to fine
        //tune the acceleration setting. To fix this, disregard zero values. If the lag time has passed the
        //lag period, we can assume that the user is not giving any input, so we actually want to set
        //the input value to zero at that time.
        //Thus, save the input value if it is non-zero or the lag timer is met.
        if((Mathf.Approximately(0, input.x) && Mathf.Approximately(0, input.y)) == false || inputLagTimer >= inputLagPeriod) {
            lastInputEvent = input;
            inputLagTimer = 0;
        } 
        return lastInputEvent;
    }
        void Start()
    {
        Cursor.lockState = CursorLockMode.Confined; //Locks Cursor in window until ESC is pressed to leave game view
        Cursor.visible = false; //Makes Cursor invisible
    }
    void FixedUpdate()
    {
        activeForwardSpeed = Input.GetAxisRaw("Vertical") * speed;
        activeStrafeSpeed = Input.GetAxisRaw("Horizontal") * speed;

        transform.position += (transform.forward * activeForwardSpeed * Time.deltaTime) + (transform.right * activeStrafeSpeed * Time.deltaTime);
        //transform.position = Vector3.ClampMagnitude(transform.position, speed * speed);
            //Debug.Log("Y Position: " + transform.position.y);
    }
    // Update is called once per frame
    void Update()
    {
        //The wanted velocity is the current input scaled by ensitivity
        //This is also the maximum velocity
        Vector2 wantedVelocity = GetInput() * sensitivity;


        //Calculate new rotation
        velocity = new Vector2(
            Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x * Time.deltaTime),
            Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y * Time.deltaTime)
        );
        rotation += velocity * Time.deltaTime;
        rotation.y = ClampVerticalAngle(rotation.y);
        //rotation += wantedVelocity *Time.deltaTime;

        //Convert the rotation to euler angles
        transform.localEulerAngles = new Vector3(rotation.y, rotation.x, 0);

        

        if (Input.GetKeyUp(KeyCode.Escape)){
            Cursor.visible = true;
        }
        
    }
}
