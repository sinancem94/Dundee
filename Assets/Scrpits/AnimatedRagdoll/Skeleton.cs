using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stickman rigidbody configuration
//
// 0 : pelvis
// 1 : spine 2
// 2 : spine 3 
// 3 : UpperArm Left
// 4 : LowerArm Left
// 5 : UpperArm Right
// 6 : LowerArm Right
// 7 : Head
// 8 : Thigh Left
// 9 : Calf Left
// 10 : Thigh Right
// 11 : Calf Right

public class Skeleton : MonoBehaviour
{
	public GameObject animatedRagdoll;

	public Rigidbody rootBone;
	public List<Limb> AllLimbs = new List<Limb>();
	[HideInInspector] public List<Transform> animatedTransforms;
	
	// The ranges are not set in stone. Feel free to extend the ranges
	//[Range(0f, 100f)] public float maxForce = 100f;

	//[Range(0f, 100f)] public float maxTorque = 10f;

	[Range(0f, 1f)] public float animationRate; //character limbs target rotation will set at this rate
	RangeAttribute animationRange = new RangeAttribute(0f, 1f);

	[Range(5f, 2000f)] public float maxJointSpring;
	RangeAttribute jointSpringRange = new RangeAttribute(5f, 500f);

	[Range(0f, 1f)] public float Torque = 0.5f; // For all limbs Torque strength
	[Range(0f, 100f)] public float PForce = 25f; // For all limbs Force strength
	[Range(0f, .064f)] public float DForce = 0.02f;

	[SerializeField] bool hideAnimated = true;
	[SerializeField] public bool useGravity = false; // Ragdoll is affected by Unitys gravity

	[Range(0f, 340f)] public float angularDrag = 50f; // Rigidbodies angular drag. Unitys parameter
	[Range(0f, 2f)] public float drag = 0.2f; // Rigidbodies drag. Unitys parameter

	[SerializeField] public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;

	//[SerializeField] float ragdollMovementSpeed = 0.1f;
	[SerializeField] float movementSpeed;
	RangeAttribute movementSpdRange = new RangeAttribute(0.1f, 1f);

	[SerializeField] float jumpForce = 10f;

	PlayerInput inputs;
	Vector3 PlayerAppliedForce;

	[HideInInspector] public bool torque = false; // Use World torque to controll the ragdoll (if true)
	[HideInInspector] public bool force = true; // Use force to controll the ragdoll
	[HideInInspector] public bool follow = true; // Use force to controll the ragdoll

	[HideInInspector] public RagdollState rState = RagdollState.Animated;
	[HideInInspector] public AnimatedState aState = AnimatedState.Idle;

	[HideInInspector] public int collisionCount; // other than terrain

	[HideInInspector] public int mainLimbsCollisionCount;
	[HideInInspector] public float mainLimbsCollisionSpd;

	[HideInInspector] public float totalCollisionSpeed;

	[HideInInspector] public int groundCollidingFoot;

	float impactForce;
	

	//State whether character is animated or ragdoll
	public enum RagdollState
	{
		Animated,
		GoingRagdoll,
		Ragdoll,
		GoingAnimated
	}

	//States when character is animated
	public enum AnimatedState
	{
		Idle,
		Walking,
		OnAir
	}

	private void Awake()
	{
		if (!animatedRagdoll)
		{
			UnityEngine.Debug.LogWarning("animatedRagdoll not assigned in AnimFollow script on " + this.name + "\n");

			//Try to assign manually
			bool found = false;

			foreach (Transform tmpMaster in this.transform.parent.GetComponentInChildren<Transform>())
			{
				if (tmpMaster.name.ToLower().Contains("animated"))
				{
					animatedRagdoll = tmpMaster.gameObject;
					found = true;
					break;
				}
			}

			if (!found)
				UnityEngine.Debug.LogError("Could not found animatedRagdoll in " + this.name + "\n");
			else
				UnityEngine.Debug.LogWarning("animatedRagdoll " + animatedRagdoll.name + "assigned manually in " + this.name + " could be wrong\n");
		}

		if (hideAnimated) //master needed some time for debugging
		{
			HideAnimated();
		}

		List<Transform> ragdollTransforms;

		ragdollTransforms = new List<Transform>(GetComponentsInChildren<Transform>()); // Get all transforms in ragdoll. THE NUMBER OF TRANSFORMS MUST BE EQUAL IN RAGDOLL AS IN MASTER!
		animatedTransforms = new List<Transform>(animatedRagdoll.GetComponentsInChildren<Transform>()); // Get all transforms in animatedRagdoll. 		

		if (!(ragdollTransforms.Count == ragdollTransforms.Count))
			UnityEngine.Debug.LogError(this.name + " does not have a valid animatedRagdoll.\animatedRagdoll transform count does not equal slave transform count." + "\n");

		List<Rigidbody> allRigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());

		int currentTransform = 0;
		int currentRb = 0;

		List<Transform> tmpAnimatedRigids = new List<Transform>();

		foreach (Transform ragdollTransform in ragdollTransforms) // Sort the transform arrays
		{
			if (ragdollTransform.GetComponent<Rigidbody>())
			{
				if (!rootBone)
					rootBone = ragdollTransform.GetComponent<Rigidbody>();

				DistributeLimbs(ragdollTransform, currentRb);

				tmpAnimatedRigids.Add(animatedTransforms[currentTransform]);

				currentRb++;
			}
			currentTransform++;
		}

		//update animated transforms only to correspond ragdolls rigidbodies
		animatedTransforms.Clear();
		animatedTransforms.AddRange(tmpAnimatedRigids);
		tmpAnimatedRigids.Clear();

		if (allRigidbodies.Count == 0 || !rootBone)
			UnityEngine.Debug.LogError("There are no rigid body components on the ragdoll " + this.name + "\n");
	}

	private void Start()
	{
		inputs = animatedRagdoll.GetComponent<PlayerInput>();
		if (!inputs)
			Debug.LogError("Assign PlayerInput scrpit to master");
	}

	private void LateUpdate()
	{
		                                                                                           
		{
			Vector3 pos = new Vector3(rootBone.transform.position.x, /*rootBone.transform.position.y - followOffset*/  animatedRagdoll.transform.position.y, rootBone.transform.position.z);
			animatedRagdoll.transform.position = pos;
		}
	}

	private void Update()
	{
		SetForces();
		//SetStates();
	}

	private void FixedUpdate()
	{
		ApplyForcesToLimbs();
		//ControllRagdoll();
	}

	//Get all real-time forces that is not applied Unity Physics Engine
	void SetForces()
	{
		PlayerAppliedForce = inputs.InputForce * movementSpeed;
	}

	public float onAirTimer = 0f;
	void SetStates()
	{
		//Check if character isOnAir
		if (groundCollidingFoot == 0)
		{
			if(aState != AnimatedState.OnAir)
			{
				aState = AnimatedState.OnAir;
				OnAir();				
			}

			onAirTimer += Time.deltaTime;
		}
		else if(aState == AnimatedState.OnAir)
		{
			aState = AnimatedState.Idle;
			onAirTimer = 0f;
			Grounded();
		}

		if ((rootBone.velocity.magnitude > 2f) && (PlayerAppliedForce.magnitude > 0.1f) && (aState != AnimatedState.OnAir && aState != AnimatedState.Walking))
		{
			aState = AnimatedState.Walking;
		}
		else if((rootBone.velocity.magnitude < 2f) && (PlayerAppliedForce.magnitude < 0.1f) && (aState != AnimatedState.OnAir && aState != AnimatedState.Idle))
		{
			aState = AnimatedState.Idle;
		}
	}

	void OnAir()
	{
		animationRate = animationRange.min;
		maxJointSpring = jointSpringRange.min;

		follow = false;
		force = false;
		//torque = false;
	}

	void Grounded()
	{
		animationRate = animationRange.max;
		maxJointSpring = jointSpringRange.max;

		follow = true;
		force = true;
		//torque = true;
	}

	void ApplyForcesToLimbs()
	{
		//if player pressed jump only jump at that frame
		if(inputs.pressedJump)
		{
			PlayerAppliedForce.y = jumpForce;
			inputs.pressedJump = false;
		}

		//TO DO:
		//Distrubute input force to limbs. Total force is not changed but apply each limb according to their profile
		foreach(Limb currlimb in AllLimbs)
		{
			currlimb.ApplyPlayerForce(PlayerAppliedForce);
		}
	}

	float maximumImpact; //When something hits collider this value will take initial impact force. After each frame it will check impact force and update itself if force is bigger
	readonly float fallThreshold = 120f; //When this threshold is reached by impact force character will lose all control

	void ControllRagdoll()
	{
		
		//Check whether ragdoll or animated
		switch (rState)
		{
			case RagdollState.Animated:

				//1.Check if character is colliding with something. If not colliding break case
				//2.Check forces applied to character. - total collision force - main limbs total collision force - arms legs collision per limb etc.
				//3.If total collision force is bigger than thershold set state as ragdoll (character false) || if there is too much force on one limb (something collided with high speed) set state as ragdoll (character false)
				//4.If character did not fall and main limbs total collision bigger than threshold go to set state as goingRagdoll

				//if checks condition character will fall.After fall character will not retain full force until conditions satisfied (check case RagdollState.Ragdoll:)
				if (collisionCount > 0)
				{
					/*if((totalCollisionSpeed > 100f || (totalCollisionSpeed / collisionCount > 15f)))
					{
					//	Debug.LogWarning(totalCollisionSpeed / collisionCount);
					//	Debug.LogWarning(collisionCount + "\n");

						rState = RagdollState.Ragdoll;
						FullRagdoll();
					}
					else */
					bool totalForceExceeds = ((totalCollisionSpeed > 100f || (totalCollisionSpeed / collisionCount > 15f)));
					bool mainLimbForceExceeds = (mainLimbsCollisionCount > 0 && (mainLimbsCollisionSpd / mainLimbsCollisionCount > 5f));

					if (totalForceExceeds || mainLimbForceExceeds)
					{

						rState = RagdollState.GoingRagdoll;

						maximumImpact = totalCollisionSpeed;
						impactForce = totalCollisionSpeed;
					}
				}

				break;
			case RagdollState.GoingRagdoll:
				
				//Check if the character is still colliding. If not colliding set state as goingAnimated and break
				//if colliding ease up the ragdoll smoothly according to speed of collision
				//if characters animationRate is reached 0 set state as ragdoll

				//When main limbs collided with some force ease up the character. Smoothly decrease applied force, torque and decrease animationRate
				if(mainLimbsCollisionCount > 0)
				{
					//if GoingRagdoll returns true character is ragdoll
					if (GoingRagdoll(maximumImpact) && maximumImpact > fallThreshold) //if not ragdoll yet continue setting impactforce and easing up
					{
						FullRagdoll();
						rState = RagdollState.Ragdoll;
					} 

					impactForce = totalCollisionSpeed;

					//update maximum applied impact force each frame
					maximumImpact = (impactForce > maximumImpact) ? impactForce : maximumImpact;

				}
				else //character stopped colliding when going to ragdoll. Set state as going animated and break case
				{
					rState = RagdollState.GoingAnimated;
				}
				
				break;
			case RagdollState.Ragdoll:
				//Character will retain full force if this conditions satisfies

				//if maximumImpact is bigger than threshold character will use control untill collided to ground
				if ( maximumImpact > fallThreshold && (collisionCount > 2 && rootBone.velocity.magnitude < 1f))
				{
					maximumImpact = 0f;
					rState = RagdollState.Animated;
					FullAnimated();
				}

				break;
			case RagdollState.GoingAnimated:
				
				if(GoingAnimated(maximumImpact))
				{
					rState = RagdollState.Animated;
					FullAnimated();
				}

				break;
			default:
				break;
		}

		//Reset total collision in each fixedUpdate
		totalCollisionSpeed = 0f;
		mainLimbsCollisionSpd = 0f;
	}

	bool GoingAnimated(float impactForce)
	{
		//when impact force goes up t goes down
		float t = (0.1f / impactForce);

		animationRate = Mathf.Lerp(animationRate, animationRange.max, t);

		maxJointSpring = Mathf.Lerp(maxJointSpring, jointSpringRange.max, t);

		if (animationRate > 0.99f)
			return true;

		return false;
	}

	bool GoingRagdoll(float impactForce)
	{
		float t = (impactForce / mainLimbsCollisionCount) / (/*animationRange.max **/ 100f);

		animationRate = Mathf.Lerp(animationRate, animationRange.min, t);

		maxJointSpring = Mathf.Lerp(maxJointSpring, jointSpringRange.min, t);

		//Debug.LogWarning(animationRate);

		if (animationRate < 0.001f)
			return true;

		return false;
	}

	void FullAnimated()
	{
		animationRate = animationRange.max;
		maxJointSpring = jointSpringRange.max;

		follow = true;
		force = true;
		torque = true;
	}

	void FullRagdoll()
	{
		animationRate = animationRange.min;//animationRange.min;
		maxJointSpring = jointSpringRange.min;

		follow = false;
		force = false;
		torque = false;
	}
	
	

	void DistributeLimbs(Transform ragdollTransform,int no)
	{
		Limb limb;

		if(Object.Equals(ragdollTransform,rootBone.transform))
			limb = ragdollTransform.gameObject.AddComponent<LimbRoot>();
		else if(ragdollTransform.name.ToLower().Contains("calf") /*|| ragdollTransform.name.ToLower().Contains("thigh")*/)
			limb = ragdollTransform.gameObject.AddComponent<LimbLeg>();
		else if(ragdollTransform.name.ToLower().Contains("arm"))
			limb = ragdollTransform.gameObject.AddComponent<LimbArm>();
		else if(ragdollTransform.name.ToLower().Contains("head"))
			limb = ragdollTransform.gameObject.AddComponent<LimbHead>();
		else
			limb = ragdollTransform.gameObject.AddComponent<LimbDefault>();

		limb.No = no;

		AllLimbs.Add(limb);
	}

	//IF DEBUGGING THE ANIMATEDRAGDOLL OPEN ANIMATED
	void HideAnimated()
	{
		Debug.Log("Hiding Animated");

		SkinnedMeshRenderer visible;
		MeshRenderer visible2;
		if (visible = animatedRagdoll.GetComponentInChildren<SkinnedMeshRenderer>())
		{
			visible.enabled = false;
			SkinnedMeshRenderer[] visibles;
			visibles = animatedRagdoll.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer visiblen in visibles)
				visiblen.enabled = false;
		}
		if (visible2 = animatedRagdoll.GetComponentInChildren<MeshRenderer>())
		{
			visible2.enabled = false;
			MeshRenderer[] visibles2;
			visibles2 = animatedRagdoll.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer visiblen2 in visibles2)
				visiblen2.enabled = false;
		}
	}

}
