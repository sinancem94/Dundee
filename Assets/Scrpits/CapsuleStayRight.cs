using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleStayRight : MonoBehaviour
{

    bool collidingGround;
    RaycastHit raycastHit;

    public LayerMask layerMask;

    private void Start()
    {
        layerMask = layerMask | (1 << gameObject.layer); // Use to avoid raycasts to hit colliders on the character (ragdoll must be on an ignored layer)
        layerMask = ~layerMask;
    }

    private void Update()
    {
        isOnFeet();

        transform.rotation = Quaternion.FromToRotation(Vector3.up * -1f, raycastHit.normal);
    }
   

    void isOnFeet()
    {

        Debug.DrawRay(transform.position - (Vector3.up * 0.5f), -1f * Vector3.up, Color.red);
        bool didHit = Physics.Raycast(transform.position - (Vector3.up * 0.5f), -1f * Vector3.up, out raycastHit, 2f);

       // Debug.Log(raycastHit.normal);

        if (!didHit && collidingGround)
        {
            collidingGround = false;

        }
        else if (didHit && !collidingGround)
        {
            collidingGround = true;
        }
    }

}
