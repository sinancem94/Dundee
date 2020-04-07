using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbRoot : LimbDefault
{
    protected override LimbProfile SetLimbProfile()
    {
        LimbProfile prof = new LimbProfile();

        prof.pFollowForce = 1f;
        prof.pFollowTorque = 1f;
        prof.pJointSpring = 1f;

        prof.pAppliedForce = 2f;

        prof.pFollowRate = 0.5f;

        return prof;
    }
}
