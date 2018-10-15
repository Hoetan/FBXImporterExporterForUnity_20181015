/************************************************************************************
 Copyright: Copyright 2014 Beijing Noitom Technology Ltd. All Rights reserved.
 Pending Patents: PCT/CN2014/085659 PCT/CN2014/071006

 Licensed under the Perception Neuron SDK License Beta Version (the “License");
 You may only use the Perception Neuron SDK when in compliance with the License,
 which is provided at the time of installation or download, or which
 otherwise accompanies this software in the form of either an electronic or a hard copy.

 A copy of the License is included with this package or can be obtained at:
 http://www.neuronmocap.com

 Unless required by applicable law or agreed to in writing, the Perception Neuron SDK
 distributed under the License is provided on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing conditions and
 limitations under the License.
************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Neuron;

public class NeuronAnimatorInstance : NeuronInstance
{
	public Animator						boundAnimator = null;		
	public bool							physicalUpdate = false;

    public bool bLateUpdateMode = true;
    public bool bFixedUpdateMode = false;
    private float fLerpDeltaTime = 1.0f;
    public int iFPS = 60;
    public float fLerpPosition = 0.2f;
    public float fLerpRotation = 0.2f;

    NeuronAnimatorPhysicalReference physicalReference = new NeuronAnimatorPhysicalReference();
	Vector3[]							bonePositionOffsets = new Vector3[(int)HumanBodyBones.LastBone];
	Vector3[]							boneRotationOffsets = new Vector3[(int)HumanBodyBones.LastBone];
	
	public NeuronAnimatorInstance()
	{
	}
	
	public NeuronAnimatorInstance( string address, int port, int commandServerPort, NeuronConnection.SocketType socketType, int actorID )
		:base( address, port, commandServerPort, socketType, actorID )
	{
	}
	
	public NeuronAnimatorInstance( Animator animator, string address, int port, int commandServerPort, NeuronConnection.SocketType socketType, int actorID )
		:base( address, port, commandServerPort, socketType, actorID )
	{
		boundAnimator = animator;
		UpdateOffset();
	}
	
	public NeuronAnimatorInstance( Animator animator, NeuronActor actor )
		:base( actor )
	{
		boundAnimator = animator;
		UpdateOffset();
	}
	
	public NeuronAnimatorInstance( NeuronActor actor )
		:base( actor )
	{
	}
	
	new void OnEnable()
	{	
		base.OnEnable();
		if( boundAnimator == null )
		{
			boundAnimator = GetComponent<Animator>();
			UpdateOffset();
		}
	}
	
	new void Update()
	{
        if (!bFixedUpdateMode)
        {
            base.ToggleConnect();
            base.Update();

            if (boundActor != null && boundAnimator != null && !physicalUpdate)
            {
                if (physicalReference.Initiated())
                {
                    ReleasePhysicalContext();
                }

                fLerpDeltaTime = Time.deltaTime;
                ApplyMotion(boundActor, boundAnimator, bonePositionOffsets, boneRotationOffsets, bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            }
        }
	}
	
	void FixedUpdate()
	{
        if (bFixedUpdateMode)
        {
            base.ToggleConnect();
            base.Update();

            if (boundActor != null && boundAnimator != null)
            {
                if (physicalReference.Initiated())
                {
                    ReleasePhysicalContext();
                }

                ApplyMotion(boundActor, boundAnimator, bonePositionOffsets, boneRotationOffsets, bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            }
        }
        else
        {
            base.ToggleConnect();

            if (boundActor != null && boundAnimator != null && physicalUpdate)
            {
                if (!physicalReference.Initiated())
                {
                    physicalUpdate = InitPhysicalContext();
                }

                ApplyMotionPhysically(physicalReference.GetReferenceAnimator(), boundAnimator);
            }
        }
	}
	
	static bool ValidateVector3( Vector3 vec )
	{
		return !float.IsNaN( vec.x ) && !float.IsNaN( vec.y ) && !float.IsNaN( vec.z )
			&& !float.IsInfinity( vec.x ) && !float.IsInfinity( vec.y ) && !float.IsInfinity( vec.z );
	}
	
	static void SetScale( Animator animator, HumanBodyBones bone, float size, float referenceSize )
	{	
		Transform t = animator.GetBoneTransform( bone );
		if( t != null && bone <= HumanBodyBones.Jaw )
		{
			float ratio = size / referenceSize;
			
			Vector3 newScale = new Vector3( ratio, ratio, ratio );
			newScale.Scale( new Vector3( 1.0f / t.parent.lossyScale.x, 1.0f / t.parent.lossyScale.y, 1.0f / t.parent.lossyScale.z ) );
			
			if( ValidateVector3( newScale ) )
			{
				t.localScale = newScale;
			}
		}
	}
	
	// set position for bone in animator
	static void SetPosition( Animator animator, HumanBodyBones bone, Vector3 pos, bool bFixedUpdateMode, float fLerpPosition, int iFPS, float fLerpDeltaTime )
	{
		Transform t = animator.GetBoneTransform( bone );
		if( t != null )
		{
			if( !float.IsNaN( pos.x ) && !float.IsNaN( pos.y ) && !float.IsNaN( pos.z ) )
			{
                if (bFixedUpdateMode)
                {
                    t.localPosition = Vector3.Slerp(t.localPosition, pos, fLerpPosition * (120 / iFPS));
                }
                else
                {
                    t.localPosition = Vector3.Slerp(t.localPosition, pos, fLerpPosition * fLerpDeltaTime * iFPS);
                }
            }
		}
	}
	
	// set rotation for bone in animator
	static void SetRotation( Animator animator, HumanBodyBones bone, Vector3 rotation, bool bFixedUpdateMode, float fLerpPosition, int iFPS, float fLerpDeltaTime )
	{
		Transform t = animator.GetBoneTransform( bone );
		if( t != null )
		{
			Quaternion rot = Quaternion.Euler( rotation );
			if( !float.IsNaN( rot.x ) && !float.IsNaN( rot.y ) && !float.IsNaN( rot.z ) && !float.IsNaN( rot.w ) )
			{
                if (bFixedUpdateMode)
                {
                    t.localRotation = Quaternion.Slerp(t.localRotation, rot, fLerpPosition * (120 / iFPS));
                }
                else
                {
                    t.localRotation = Quaternion.Slerp(t.localRotation, rot, fLerpPosition * fLerpDeltaTime * iFPS);
                }
			}
		}
	}
	
	// apply transforms extracted from actor mocap data to transforms of animator bones
	public static void ApplyMotion( NeuronActor actor, Animator animator, Vector3[] positionOffsets, Vector3[] rotationOffsets, bool bFixedUpdateMode, float fLerpPosition, int iFPS, float fLerpDeltaTime )
	{
        // apply Hips position
        SetPosition( animator, HumanBodyBones.Hips, actor.GetReceivedPosition( NeuronBones.Hips ) + positionOffsets[(int)HumanBodyBones.Hips], bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
		SetRotation( animator, HumanBodyBones.Hips, actor.GetReceivedRotation( NeuronBones.Hips ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime );
		
		// apply positions
		if( actor.withDisplacement )
		{
			// legs
			SetPosition( animator, HumanBodyBones.RightUpperLeg,			actor.GetReceivedPosition( NeuronBones.RightUpLeg ) + positionOffsets[(int)HumanBodyBones.RightUpperLeg], bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime );
			SetPosition( animator, HumanBodyBones.RightLowerLeg, 			actor.GetReceivedPosition( NeuronBones.RightLeg ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightFoot, 				actor.GetReceivedPosition( NeuronBones.RightFoot ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftUpperLeg,				actor.GetReceivedPosition( NeuronBones.LeftUpLeg ) + positionOffsets[(int)HumanBodyBones.LeftUpperLeg], bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftLowerLeg,				actor.GetReceivedPosition( NeuronBones.LeftLeg ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftFoot,					actor.GetReceivedPosition( NeuronBones.LeftFoot ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            // spine
            SetPosition( animator, HumanBodyBones.Spine,					actor.GetReceivedPosition( NeuronBones.Spine ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.Chest,					actor.GetReceivedPosition( NeuronBones.Spine3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.Neck,						actor.GetReceivedPosition( NeuronBones.Neck ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.Head,						actor.GetReceivedPosition( NeuronBones.Head ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            // right arm
            SetPosition( animator, HumanBodyBones.RightShoulder,			actor.GetReceivedPosition( NeuronBones.RightShoulder ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightUpperArm,			actor.GetReceivedPosition( NeuronBones.RightArm ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightLowerArm,			actor.GetReceivedPosition( NeuronBones.RightForeArm ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            // right hand
            SetPosition( animator, HumanBodyBones.RightHand,				actor.GetReceivedPosition( NeuronBones.RightHand ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            //SetPosition( animator, HumanBodyBones.RightThumbProximal,		actor.GetReceivedPosition( NeuronBones.RightHandThumb1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightThumbIntermediate,   actor.GetReceivedPosition( NeuronBones.RightHandThumb2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightThumbDistal,			actor.GetReceivedPosition( NeuronBones.RightHandThumb3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            SetPosition( animator, HumanBodyBones.RightIndexProximal,		actor.GetReceivedPosition( NeuronBones.RightHandIndex1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightIndexIntermediate,	actor.GetReceivedPosition( NeuronBones.RightHandIndex2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightIndexDistal,			actor.GetReceivedPosition( NeuronBones.RightHandIndex3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            SetPosition( animator, HumanBodyBones.RightMiddleProximal,		actor.GetReceivedPosition( NeuronBones.RightHandMiddle1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightMiddleIntermediate,	actor.GetReceivedPosition( NeuronBones.RightHandMiddle2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightMiddleDistal,		actor.GetReceivedPosition( NeuronBones.RightHandMiddle3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            SetPosition( animator, HumanBodyBones.RightRingProximal,		actor.GetReceivedPosition( NeuronBones.RightHandRing1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightRingIntermediate,	actor.GetReceivedPosition( NeuronBones.RightHandRing2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightRingDistal,			actor.GetReceivedPosition( NeuronBones.RightHandRing3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            SetPosition( animator, HumanBodyBones.RightLittleProximal,		actor.GetReceivedPosition( NeuronBones.RightHandPinky1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightLittleIntermediate,	actor.GetReceivedPosition( NeuronBones.RightHandPinky2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.RightLittleDistal,		actor.GetReceivedPosition( NeuronBones.RightHandPinky3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            // left arm
            SetPosition( animator, HumanBodyBones.LeftShoulder,				actor.GetReceivedPosition( NeuronBones.LeftShoulder ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftUpperArm,				actor.GetReceivedPosition( NeuronBones.LeftArm ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftLowerArm,				actor.GetReceivedPosition( NeuronBones.LeftForeArm ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            // left hand
            SetPosition( animator, HumanBodyBones.LeftHand,					actor.GetReceivedPosition( NeuronBones.LeftHand ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            //SetPosition( animator, HumanBodyBones.LeftThumbProximal,        actor.GetReceivedPosition( NeuronBones.LeftHandThumb1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftThumbIntermediate,	actor.GetReceivedPosition( NeuronBones.LeftHandThumb2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftThumbDistal,			actor.GetReceivedPosition( NeuronBones.LeftHandThumb3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            SetPosition( animator, HumanBodyBones.LeftIndexProximal,		actor.GetReceivedPosition( NeuronBones.LeftHandIndex1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftIndexIntermediate,	actor.GetReceivedPosition( NeuronBones.LeftHandIndex2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftIndexDistal,			actor.GetReceivedPosition( NeuronBones.LeftHandIndex3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            SetPosition( animator, HumanBodyBones.LeftMiddleProximal,		actor.GetReceivedPosition( NeuronBones.LeftHandMiddle1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftMiddleIntermediate,	actor.GetReceivedPosition( NeuronBones.LeftHandMiddle2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftMiddleDistal,			actor.GetReceivedPosition( NeuronBones.LeftHandMiddle3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            SetPosition( animator, HumanBodyBones.LeftRingProximal,			actor.GetReceivedPosition( NeuronBones.LeftHandRing1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftRingIntermediate,		actor.GetReceivedPosition( NeuronBones.LeftHandRing2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftRingDistal,			actor.GetReceivedPosition( NeuronBones.LeftHandRing3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

            SetPosition( animator, HumanBodyBones.LeftLittleProximal,		actor.GetReceivedPosition( NeuronBones.LeftHandPinky1 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftLittleIntermediate,	actor.GetReceivedPosition( NeuronBones.LeftHandPinky2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
            SetPosition( animator, HumanBodyBones.LeftLittleDistal,			actor.GetReceivedPosition( NeuronBones.LeftHandPinky3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        }
		
		// apply rotations
		
		// legs
		SetRotation( animator, HumanBodyBones.RightUpperLeg,			actor.GetReceivedRotation( NeuronBones.RightUpLeg ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightLowerLeg, 			actor.GetReceivedRotation( NeuronBones.RightLeg ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightFoot, 				actor.GetReceivedRotation( NeuronBones.RightFoot ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftUpperLeg,				actor.GetReceivedRotation( NeuronBones.LeftUpLeg ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftLowerLeg,				actor.GetReceivedRotation( NeuronBones.LeftLeg ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftFoot,					actor.GetReceivedRotation( NeuronBones.LeftFoot ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        // spine
        SetRotation( animator, HumanBodyBones.Spine,					actor.GetReceivedRotation( NeuronBones.Spine ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.Chest,					actor.GetReceivedRotation( NeuronBones.Spine1 ) + actor.GetReceivedRotation( NeuronBones.Spine2 ) + actor.GetReceivedRotation( NeuronBones.Spine3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.Neck,						actor.GetReceivedRotation( NeuronBones.Neck ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.Head,						actor.GetReceivedRotation( NeuronBones.Head ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        // right arm
        SetRotation( animator, HumanBodyBones.RightShoulder,			actor.GetReceivedRotation( NeuronBones.RightShoulder ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightUpperArm,			actor.GetReceivedRotation( NeuronBones.RightArm ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightLowerArm,			actor.GetReceivedRotation( NeuronBones.RightForeArm ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        // right hand
        SetRotation( animator, HumanBodyBones.RightHand,				actor.GetReceivedRotation( NeuronBones.RightHand ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightThumbProximal,		actor.GetReceivedRotation( NeuronBones.RightHandThumb1)/* + new Vector3(0.0f, 50.0f, 0.0f)*/, bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightThumbIntermediate,	actor.GetReceivedRotation( NeuronBones.RightHandThumb2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightThumbDistal,			actor.GetReceivedRotation( NeuronBones.RightHandThumb3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        SetRotation( animator, HumanBodyBones.RightIndexProximal,		actor.GetReceivedRotation( NeuronBones.RightHandIndex1 ) + actor.GetReceivedRotation( NeuronBones.RightInHandIndex ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightIndexIntermediate,	actor.GetReceivedRotation( NeuronBones.RightHandIndex2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightIndexDistal,			actor.GetReceivedRotation( NeuronBones.RightHandIndex3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        SetRotation( animator, HumanBodyBones.RightMiddleProximal,		actor.GetReceivedRotation( NeuronBones.RightHandMiddle1 ) + actor.GetReceivedRotation( NeuronBones.RightInHandMiddle ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightMiddleIntermediate,	actor.GetReceivedRotation( NeuronBones.RightHandMiddle2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightMiddleDistal,		actor.GetReceivedRotation( NeuronBones.RightHandMiddle3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        SetRotation( animator, HumanBodyBones.RightRingProximal,		actor.GetReceivedRotation( NeuronBones.RightHandRing1 ) + actor.GetReceivedRotation( NeuronBones.RightInHandRing ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightRingIntermediate,	actor.GetReceivedRotation( NeuronBones.RightHandRing2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightRingDistal,			actor.GetReceivedRotation( NeuronBones.RightHandRing3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        SetRotation( animator, HumanBodyBones.RightLittleProximal,		actor.GetReceivedRotation( NeuronBones.RightHandPinky1 ) + actor.GetReceivedRotation( NeuronBones.RightInHandPinky ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightLittleIntermediate,	actor.GetReceivedRotation( NeuronBones.RightHandPinky2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.RightLittleDistal,		actor.GetReceivedRotation( NeuronBones.RightHandPinky3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        // left arm
        SetRotation( animator, HumanBodyBones.LeftShoulder,				actor.GetReceivedRotation( NeuronBones.LeftShoulder ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftUpperArm,				actor.GetReceivedRotation( NeuronBones.LeftArm ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftLowerArm,				actor.GetReceivedRotation( NeuronBones.LeftForeArm ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        // left hand
        SetRotation( animator, HumanBodyBones.LeftHand,					actor.GetReceivedRotation( NeuronBones.LeftHand ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftThumbProximal,		actor.GetReceivedRotation( NeuronBones.LeftHandThumb1 )/* + new Vector3(0.0f, -50.0f, 0.0f)*/, bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftThumbIntermediate,	actor.GetReceivedRotation( NeuronBones.LeftHandThumb2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftThumbDistal,			actor.GetReceivedRotation( NeuronBones.LeftHandThumb3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        SetRotation( animator, HumanBodyBones.LeftIndexProximal,		actor.GetReceivedRotation( NeuronBones.LeftHandIndex1 ) + actor.GetReceivedRotation( NeuronBones.LeftInHandIndex ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftIndexIntermediate,	actor.GetReceivedRotation( NeuronBones.LeftHandIndex2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftIndexDistal,			actor.GetReceivedRotation( NeuronBones.LeftHandIndex3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        SetRotation( animator, HumanBodyBones.LeftMiddleProximal,		actor.GetReceivedRotation( NeuronBones.LeftHandMiddle1 ) + actor.GetReceivedRotation( NeuronBones.LeftInHandMiddle ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftMiddleIntermediate,	actor.GetReceivedRotation( NeuronBones.LeftHandMiddle2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftMiddleDistal,			actor.GetReceivedRotation( NeuronBones.LeftHandMiddle3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        SetRotation( animator, HumanBodyBones.LeftRingProximal,			actor.GetReceivedRotation( NeuronBones.LeftHandRing1 ) + actor.GetReceivedRotation( NeuronBones.LeftInHandRing ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftRingIntermediate,		actor.GetReceivedRotation( NeuronBones.LeftHandRing2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftRingDistal,			actor.GetReceivedRotation( NeuronBones.LeftHandRing3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);

        SetRotation( animator, HumanBodyBones.LeftLittleProximal,		actor.GetReceivedRotation( NeuronBones.LeftHandPinky1 ) + actor.GetReceivedRotation( NeuronBones.LeftInHandPinky ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftLittleIntermediate,	actor.GetReceivedRotation( NeuronBones.LeftHandPinky2 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
        SetRotation( animator, HumanBodyBones.LeftLittleDistal,			actor.GetReceivedRotation( NeuronBones.LeftHandPinky3 ), bFixedUpdateMode, fLerpPosition, iFPS, fLerpDeltaTime);
    }
	
	// apply Transforms of src bones to Rigidbody Components of dest bones
	public static void ApplyMotionPhysically( Animator src, Animator dest )
	{
		for( HumanBodyBones i = 0; i < HumanBodyBones.LastBone; ++i )
		{
			Transform src_transform = src.GetBoneTransform( i );
			Transform dest_transform = dest.GetBoneTransform( i );
			if( src_transform != null && dest_transform != null )
			{
				Rigidbody rigidbody = dest_transform.GetComponent<Rigidbody>();
				if( rigidbody != null )
				{
					rigidbody.MovePosition( src_transform.position );
					rigidbody.MoveRotation( src_transform.rotation );
				}
			}
		}
	}
	
	bool InitPhysicalContext()
	{
		if( physicalReference.Init( boundAnimator ) )
		{
			// break original object's hierachy of transforms, so we can use MovePosition() and MoveRotation() to set transform
			NeuronHelper.BreakHierarchy( boundAnimator );
			return true;
		}
		
		return false;
	}
	
	void ReleasePhysicalContext()
	{
		physicalReference.Release();
	}
	
	void UpdateOffset()
	{
		// we do some adjustment for the bones here which would replaced by our model retargeting later

		// initiate values
		for( int i = 0; i < (int)HumanBodyBones.LastBone; ++i )
		{
			bonePositionOffsets[i] = Vector3.zero;
			boneRotationOffsets[i] = Vector3.zero;
		}
	
		if( boundAnimator != null )
		{			
			Transform leftLegTransform = boundAnimator.GetBoneTransform( HumanBodyBones.LeftUpperLeg );
			Transform rightLegTransform = boundAnimator.GetBoneTransform( HumanBodyBones.RightUpperLeg );
			if( leftLegTransform != null )
			{
				bonePositionOffsets[(int)HumanBodyBones.LeftUpperLeg] = new Vector3( 0.0f, leftLegTransform.localPosition.y, 0.0f );
				bonePositionOffsets[(int)HumanBodyBones.RightUpperLeg] = new Vector3( 0.0f, rightLegTransform.localPosition.y, 0.0f );
				bonePositionOffsets[(int)HumanBodyBones.Hips] = new Vector3( 0.0f, -( leftLegTransform.localPosition.y + rightLegTransform.localPosition.y ) * 0.5f, 0.0f );
			}
		}
	}
}