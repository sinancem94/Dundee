using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Limb : MonoBehaviour
{
    [SerializeField] public int No { get; set; }
    protected Skeleton mySkeleton;

    protected Rigidbody limbRb;
    protected ConfigurableJoint limbJoint;

    protected JointDrive jointDrive = new JointDrive();

    public float JointDamper;
    public float JointSpring;

    public float FollowForce;
    public float FollowTorque;
    public float FollowRate;

    public float AppliedForce;

    public struct LimbProfile
    {
        // per limb profiles //////////////
        public float pJointDamper;
        public float pJointSpring;

        public float pFollowForce;
        public float pFollowTorque;
        public float pFollowRate;

        public float pAppliedForce;
        ////////////////////
    }

    public LimbProfile limbProfile;

    protected Vector3 rigidbodyPosToCOM; // assigned in awake. inverse of slave rigidbody rotation according to its world center of mass.
    protected Vector3 lastRigidbodyPosition;

    bool isColliding;
    protected float collisionSpeed;


    //-------  CACHED VALUES
    Quaternion startLocalRotation;
    Quaternion localToJointSpace;

    float torqueAngle;
    Vector3 torqueAxis;
    Vector3 torqueError;
    Vector3 torqueSignal;
    Vector3 torqueLastError;

    protected Vector3 forceSignal;
    protected Vector3 forceError;
    protected Vector3 forceLastError;

    void Awake()
    {
        mySkeleton = GetComponentInParent<Skeleton>();

        limbRb = GetComponent<Rigidbody>();
        lastRigidbodyPosition = limbRb.position;

        limbJoint = GetComponent<ConfigurableJoint>();

        if (limbJoint)
        {
            Vector3 forward = Vector3.Cross(limbJoint.axis, limbJoint.secondaryAxis);
            Vector3 up = limbJoint.secondaryAxis;
            localToJointSpace = Quaternion.LookRotation(forward, up);
            startLocalRotation = transform.localRotation * localToJointSpace;
            jointDrive = limbJoint.slerpDrive;
        }

        rigidbodyPosToCOM = Quaternion.Inverse(transform.rotation)  * (limbRb.worldCenterOfMass - transform.position);

        if (limbJoint)
            JointLimited(true);

        limbRb.collisionDetectionMode = mySkeleton.collisionDetectionMode;
        limbRb.useGravity = mySkeleton.useGravity;
        limbRb.angularDrag = mySkeleton.angularDrag;
        limbRb.drag = mySkeleton.drag;

        limbRb.interpolation = RigidbodyInterpolation.None;

        limbProfile = SetLimbProfile();
        SetLimb();
        //Debug.Log(this.name + " is setted");
    }

    private void FixedUpdate()
    {
        
        FollowAnimatedLimb(mySkeleton.animatedTransforms[No]);
        ControlLimb();

        //lastRigidbodyPosition += (limbRb.position - lastRigidbodyPosition);
        lastRigidbodyPosition = (transform.position);

        //   Debug.Log("lastRigibbodyPosition : " + lastRigibbodyPosition + " limbRb.position : " + limbRb.position);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!(collision.transform.name == "Terrain") && collision.transform.root != this.transform.root)
        {
            CollEnter(collision);
        }
        
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!(collision.transform.name == "Terrain") && collision.transform.root != this.transform.root)
        {
            CollStay(collision);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!(collision.transform.name == "Terrain") && collision.transform.root != this.transform.root)
        {
            CollExit();
        }
    }

    #region Protected

    protected abstract LimbProfile SetLimbProfile();

    protected virtual void CollEnter(Collision collision)
    {
        if (!isColliding)
        {
            isColliding = true;
        }

        collisionSpeed = collision.relativeVelocity.magnitude;
        mySkeleton.collisionCount++;

        //it will be resetted in next fixedUpdate. Total per frame
        mySkeleton.totalCollisionSpeed += collisionSpeed;
    }

    protected virtual void CollStay(Collision collision)
    {
        collisionSpeed = collision.relativeVelocity.magnitude;
        //it will be resetted in next fixedUpdate. Total per frame
        mySkeleton.totalCollisionSpeed += collisionSpeed;
    }


    protected virtual void CollExit()
    {
        if (isColliding)
        {
            isColliding = false;
        }

        mySkeleton.collisionCount--;
    }

    //update limbs parameters each fixedUpdate. skeleton animationRate and maxJointSpring is dynamic and changes in skeleton script
    protected virtual void SetLimb()
    {
        //if (FollowRate != limbProfile.pFollowRate)
        {
            FollowRate = limbProfile.pFollowRate * mySkeleton.animationRate;
            FollowForce = limbProfile.pFollowForce * mySkeleton.PForce * mySkeleton.animationRate;
            FollowTorque = limbProfile.pFollowTorque * mySkeleton.Torque * mySkeleton.animationRate;
            JointSpring = limbProfile.pJointSpring * mySkeleton.maxJointSpring * mySkeleton.animationRate;

            AppliedForce = limbProfile.pAppliedForce ;

            SetJointTorque();
        }
    }


    #endregion

    /// <summary>
    /// Control limbs for world effects. If on air go to full ragdoll 
    /// </summary>
    void ControlLimb()
    {
        limbRb.angularDrag = mySkeleton.angularDrag; // Set rigidbody drag and angular drag in real-time
        limbRb.drag = mySkeleton.drag;

        SetLimb();
    }

    /// <summary>
    /// Calculate torque and force to apply in order to stand like animated ragdoll
    /// Set joints target rotation in order to do same movements like animated ragdoll
    /// </summary>
    /// <param name="followedLimb"></param>
    void FollowAnimatedLimb(Transform followedLimb)
    {
        if (mySkeleton.torque) // Calculate and apply world torque
        {
            AddFollowTorque(followedLimb);
        }


        if (mySkeleton.force) // Calculate and apply world force
        {
            AddFollowForce(followedLimb);
        }

        //Sets joint target rotation to animated limbs
        if (limbJoint && mySkeleton.follow /*&& TimeEngine.fixedFrameCounter % FollowRate == 0*/)
        {
            Quaternion targetRot = Quaternion.Inverse(localToJointSpace) * Quaternion.Inverse(followedLimb.localRotation) * startLocalRotation;
            limbJoint.targetRotation = Quaternion.Slerp(limbJoint.targetRotation, targetRot, FollowRate);
        }
    }


    /// <summary>
    /// Calculate force to apply in order to stand like animated ragdoll
    /// </summary>
    /// <param name="followedLimb"></param>
    protected virtual void AddFollowForce(Transform followedLimb)
    {
        Vector3 lastRigidTransformsWCOM = lastRigidbodyPosition + followedLimb.rotation * rigidbodyPosToCOM;
        //forceError is how far is limb from desired position
        forceError = lastRigidTransformsWCOM - limbRb.worldCenterOfMass; // Doesn't work if collider is trigger

        PDControl(FollowForce, mySkeleton.DForce, out forceSignal, forceError, ref forceLastError, TimeEngine.reciprocalFixedDeltaTime);
        
        forceSignal.y = 0f;

        limbRb.AddForce(forceSignal, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Calculate torque to apply in order to rotate like animated ragdoll
    /// </summary>
    /// <param name="followedLimb"></param>
    protected virtual void AddFollowTorque(Transform followedLimb)
    {
       /* Quaternion targetRotation;
        targetRotation = followedLimb.rotation * Quaternion.Inverse(limbRb.rotation);
        targetRotation.ToAngleAxis(out torqueAngle, out torqueAxis);
        torqueError = FixEuler(torqueAngle) * torqueAxis;

        PDControl(FollowTorque, mySkeleton.DTorque, out torqueSignal, torqueError, ref torqueLastError, TimeEngine.reciprocalFixedDeltaTime);

        limbRb.AddTorque(torqueSignal, ForceMode.Impulse); // Add torque to the limbs
        */
        limbRb.MoveRotation(Quaternion.Slerp(limbRb.rotation, followedLimb.rotation, FollowTorque));
    }


    //Simple PD controller
    public void PDControl(float P, float D, out Vector3 signal, Vector3 error, ref Vector3 lastError, float reciDeltaTime) // A PD controller
    {
        // theSignal = P * (theError + D * theDerivative) This is the implemented algorithm.
        signal = P * (error + D * (error - lastError) * reciDeltaTime);
        lastError = error;
    }

    /// <summary>
    /// Apply the force which given by player, inputs etc.
    /// Called from skeleton
    /// </summary>
    public virtual void ApplyPlayerForce(Vector3 PlayerAppliedForce)
    {
       /*   float oldX = PlayerAppliedForce.x;
          float oldZ = PlayerAppliedForce.y;

          PlayerAppliedForce.x = oldZ;
          PlayerAppliedForce.z = oldX;*/

        //limbRb.AddRelativeForce(PlayerAppliedForce * AppliedForce, ForceMode.Force);
        //limbRb.AddRelativeForce(new Vector3((transform.right * PlayerAppliedForce.x).x, PlayerAppliedForce.y, (transform.forward * PlayerAppliedForce.z).z) * AppliedForce ,ForceMode.Force);
       // Quaternion.Inverse(localToJointSpace) * Quaternion.Inverse(followedLimb.localRotation) * startLocalRotation
       /* Vector3 forceVec = Quaternion.Inverse(localToJointSpace) * transform.rotation * PlayerAppliedForce;//new Vector3(PlayerAppliedForce.x * transform.forward.x, PlayerAppliedForce.y, PlayerAppliedForce.z * transform.forward.z);

        Debug.Log("Force Vector is : " + forceVec + "\n"
            + "Player Applied Force is :" + PlayerAppliedForce + "\n"
            + "Transform forward is : " + transform.forward);*/

        limbRb.AddForce(PlayerAppliedForce * AppliedForce, ForceMode.Impulse);
    }
    

    float FixEuler(float angle) // For the angle in angleAxis, to make the error a scalar
    {
        if (angle > 180f)
            return angle - 360f;
        else
            return angle;
    }

    void SetJointTorque()
    {
        if (!limbJoint)
            return;

        jointDrive.positionSpring = JointSpring;
        limbJoint.slerpDrive = jointDrive;
    }

    void JointLimited(bool limited)
    {
        if (limited)
        {
            limbJoint.angularXMotion = ConfigurableJointMotion.Limited;
            limbJoint.angularYMotion = ConfigurableJointMotion.Limited;
            limbJoint.angularZMotion = ConfigurableJointMotion.Limited;
        }
        else
        {
            limbJoint.angularXMotion = ConfigurableJointMotion.Free;
            limbJoint.angularYMotion = ConfigurableJointMotion.Free;
            limbJoint.angularZMotion = ConfigurableJointMotion.Free;
        }
    }
    
}
