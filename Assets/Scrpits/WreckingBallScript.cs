using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBallScript : MonoBehaviour
{
    Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        rb.AddTorque(Vector3.forward * 10f, ForceMode.VelocityChange);

    }

}















































/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBallScript : MonoBehaviour
{

    private Quaternion wreckingBallRotation;

    private bool isTurningRight;

    void Start()
    {
       
        StartCoroutine(SetWreckingBallPos());
        isTurningRight = false;
    }

    private void Update()
    {
        if (transform.localRotation.eulerAngles.x >= 315f)
        {
            isTurningRight = true;
        }
            
        else if (transform.localRotation.eulerAngles.x <= 225f)
            isTurningRight = false;
        Debug.Log("euler x " + transform.localRotation.eulerAngles.x + "isright" + isTurningRight);

    }

    IEnumerator SetWreckingBallPos()
    {
        while (true)
        {
            if (isTurningRight)
            {
                transform.Rotate(-2f, 0, 0);
            }
            else
            {
               transform.Rotate(2f, 0, 0);
            }
            
            yield return new WaitForEndOfFrame();
        }
    }
}*/
