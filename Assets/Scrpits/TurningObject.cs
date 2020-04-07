using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurningObject : MonoBehaviour
{
    Rigidbody rb;
    public enum TurnDirection
    {
        NotSetted,
        Up,
        Forward
    }

    public TurnDirection turnDir;
    public int TurnSpeed;
    Vector3 turnVec;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if(TurnSpeed == 0)
        {
            Debug.LogError("Turn Speed on " + this.name + " not setted. Setting it to deafult to 1");
            TurnSpeed = 1;
        }

        if(turnDir == TurnDirection.NotSetted)
        {
            Debug.LogError("Turn Direction on " + this.name + " not setted. Setting it to deafult to up");
            turnDir = TurnDirection.Up;
        }

        if (turnDir == TurnDirection.Up)
            turnVec = transform.up;
        else if(turnDir == TurnDirection.Forward)
            turnVec = transform.forward;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddTorque(turnVec * 2.5f * TurnSpeed,  ForceMode.VelocityChange);
    }
}
