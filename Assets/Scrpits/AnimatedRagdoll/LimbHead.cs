using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbHead : Limb
{
    protected override LimbProfile SetLimbProfile()
    {
        LimbProfile prof = new LimbProfile();

        prof.pFollowForce = 1f;
        prof.pFollowTorque = 0.25f;
        prof.pJointSpring = 1f;

        //apply less force to arms
        prof.pAppliedForce = 0.5f;

        prof.pFollowRate = 0.5f;

        return prof;
    }

}
