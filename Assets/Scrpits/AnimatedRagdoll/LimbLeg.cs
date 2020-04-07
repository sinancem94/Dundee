using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbLeg : Limb
{
    Transform foot;
    bool collidingGround;
    RaycastHit raycastHit;

    public LayerMask layerMask;

    protected override LimbProfile SetLimbProfile()
    {
        LimbProfile prof = new LimbProfile();

        prof.pFollowForce = 1f;
        prof.pFollowTorque = 1f;
        prof.pJointSpring = 1f;

        prof.pAppliedForce = 1f;

        prof.pFollowRate = 0.5f;

        return prof;
    }


    private void Start()
    {
        foot = this.transform.GetChild(0);

        layerMask = layerMask | (1 << gameObject.layer); // Use to avoid raycasts to hit colliders on the character (ragdoll must be on an ignored layer)
        layerMask = ~layerMask;

    }

    private void Update()
    {
        isOnFeet();
       
    }

    //Shoot rays from foots in order to understand whether on ground or not
    void isOnFeet()
    {
        
        Debug.DrawRay(foot.position, Vector3.up * -0.25f, Color.green);
        bool didHit = Physics.Raycast(foot.position, Vector3.up * -1f, out raycastHit, 0.25f, layerMask);

        if (!didHit && collidingGround)
        {
            collidingGround = false;
            mySkeleton.groundCollidingFoot--;
        }
        else if(didHit && !collidingGround)
        {
            collidingGround = true;
            mySkeleton.groundCollidingFoot++;
        }
        
    }

 /*   protected override void AddFollowTorque(Transform followedLimb)
    {
     
        if (collidingGround)
        {
            Quaternion to = Quaternion.FromToRotation(Vector3.up * -1f, raycastHit.normal);

            if(this.name.ToLower().Contains("left"))
                to = Quaternion.Euler(to.eulerAngles.x, to.eulerAngles.y , followedLimb.rotation.eulerAngles.z);
            else
                to = Quaternion.Euler(to.eulerAngles.x, followedLimb.rotation.eulerAngles.y, to.eulerAngles.z);

            limbRb.MoveRotation(Quaternion.Slerp(limbRb.rotation, to, FollowTorque));
        }
        else
        {
            base.AddFollowTorque(followedLimb);
        }

    }*/
}
