using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbDefault : Limb
{
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

    protected override void CollEnter(Collision collision)
    {
        base.CollEnter(collision);

        mySkeleton.mainLimbsCollisionSpd += collisionSpeed;
        mySkeleton.mainLimbsCollisionCount++;

       // Debug.LogError(this.name + " col spd : " + collision.relativeVelocity.magnitude);
    }

    protected override void CollStay(Collision collision)
    {
        base.CollStay(collision);
        mySkeleton.mainLimbsCollisionSpd += collisionSpeed;
    }

    protected override void CollExit()
    {
        base.CollExit();
        mySkeleton.mainLimbsCollisionCount--;
    }

}
