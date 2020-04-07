using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtmostInput;

public class PlayerInput : MonoBehaviour
{
    InputX inputX;
    
    AnimHash hash;
    Animator anim;

    public Vector3 InputForce;
    public bool pressedJump;

    float currSpeed;
    float defaultSpeed = 1f;
    float runSpeed = 3f;

    Quaternion mouseRotation = new Quaternion();
    Quaternion movementKeysRotation = new Quaternion();

    void Awake()
    {
        // Setting up the references.
        if (!(anim = GetComponent<Animator>()))
        {
            Debug.LogWarning("Missing Animator on " + this.name);
            
        }

        if (!(hash = GetComponent<AnimHash>()))
        {
            Debug.LogWarning("Missing Script: HashIDs on " + this.name);

        }

        if (anim.avatar)
            if (!anim.avatar.isValid)
                Debug.LogWarning("Animator avatar is not valid");
    }

    void Start()
    {
        inputX = new InputX();

        currSpeed = defaultSpeed;

        mouseRotation = transform.rotation;
        movementKeysRotation = transform.rotation;

        //set input events
        InputEventManager.inputEvent.onMouseMoved += OnMouseMoved;
        InputEventManager.inputEvent.onKeyboardMove += OnMove;
        InputEventManager.inputEvent.onPressedShift += OnPressedSprint;
        InputEventManager.inputEvent.onReleasedShift += OnReleasedSprint;
        InputEventManager.inputEvent.onPressedSpace += OnPressedJump;
        InputEventManager.inputEvent.onReleasedSpace += OnReleasedJump;
    }

    private void Update()
    {
        if (inputX.isSpacePressed() && !pressedJump)
        {
            pressedJump = true;
        }

        SetAnimation();
    }


    public void Jumped()
    {
        if (pressedJump)
        {
            pressedJump = false;
        }
    }

    Vector3 SetInputForce(Vector2 moveVector)
    {
        //set input force according to forward of animated character

        //total magnitude force vector can get 1.0f 
        //if vertical and horizontal is full addForce will double the force from intended so we are deviding force between vertical and horizontal 
        float forwardForce = Mathf.Abs(moveVector.y);
        
        //set force if neither is zero
        if (Mathf.Approximately(moveVector.y, 0f))
        {
            forwardForce = Mathf.Abs(moveVector.x);
        }

        Vector3 force = transform.rotation * new Vector3(0f, 0f, forwardForce);
       
        return force;
    }

    public void SetAnimation()
    {
        bool sneak = false;

        if ((Mathf.Abs(inputX.Vertical()) >= .1f || Mathf.Abs(inputX.Horizontal()) >= .1f))
        {
            if (inputX.isShift())            // ... set the speed parameter to 5.5f.
                anim.SetFloat(hash.speedFloat, 5f, 0f, Time.fixedDeltaTime);
            else
                anim.SetFloat(hash.speedFloat, 2.5f, 0f, Time.fixedDeltaTime);
        }
        else
            // Otherwise set the speed parameter to 0.
            anim.SetFloat(hash.speedFloat, 0, 0f, Time.fixedDeltaTime);

        anim.SetBool(hash.sneakingBool, sneak);
    }

    private void OnMouseMoved(Vector2 mouseAxis, float rotSpeed)
    {
        //Quaternion.AngleAxis(mouseAxis.x * rotSpeed * Time.deltaTime, Vector3.up);
        mouseRotation = (Quaternion.AngleAxis(mouseAxis.x * rotSpeed * Time.deltaTime, Vector3.up));

        //transform.rotation = mouseRotation;// (Quaternion.AngleAxis(mouseAxis.x * rotSpeed * Time.deltaTime, Vector3.up));

        //transform.Rotate(0f, mouseAxis.x * rotSpeed * Time.deltaTime, 0f);
    }

    private void OnMove(Vector2 moveVec)
    {
        //Before moving set characters rotation first
        //going right or left only changes characters rotation. 

        float reverseAngle = (moveVec.y < 0f) ? 180f * moveVec.y : 0f;

        movementKeysRotation = Quaternion.AngleAxis(reverseAngle + (moveVec.x * ((moveVec.y != 0f) ? 45f : 90f)), Vector3.up);
        //Debug.Log(moveVec.x);
        transform.rotation = mouseRotation * movementKeysRotation;

        //Input force will set force according to characters looking position
        InputForce = SetInputForce(moveVec) * currSpeed;

        //turningMouseRotation = 

        //transform.Rotate(0f, InputForce.z * Mathf.Rad2Deg * Time.deltaTime, 0f);

    }

    private void OnPressedJump()
    {

    }

    private void OnReleasedJump()
    {

    }

    private void OnPressedSprint()
    {
        currSpeed = runSpeed;
    }

    private void OnReleasedSprint()
    {
        currSpeed = defaultSpeed;
    }
}
