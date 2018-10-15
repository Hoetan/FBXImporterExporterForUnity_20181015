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
using UnityEngine;
using NeuronDataReaderWraper;

namespace Neuron
{
	public class NeuronTransformsInstance : NeuronInstance
	{	
		public Transform					root = null;
		public string						prefix = "Robot_";
		public bool							boundTransforms { get ; private set; }
		public bool							physicalUpdate = false;
		
		public Transform[]					transforms = new Transform[(int)NeuronBones.NumOfBones];
		NeuronTransformsPhysicalReference	physicalReference = new NeuronTransformsPhysicalReference();
		Vector3[]							bonePositionOffsets = new Vector3[(int)NeuronBones.NumOfBones];
		Vector3[]							boneRotationOffsets = new Vector3[(int)NeuronBones.NumOfBones];
		
		public NeuronTransformsInstance()
		{
		}
		
		public NeuronTransformsInstance( string address, int port, int commandServerPort, NeuronConnection.SocketType socketType, int actorID )
			:base( address, port, commandServerPort, socketType, actorID )
		{
		}
		
		public NeuronTransformsInstance( Transform root, string prefix, string address, int port, int commandServerPort, NeuronConnection.SocketType socketType, int actorID )
			:base( address, port, commandServerPort, socketType, actorID )
		{
			Bind( root, prefix );
		}
		
		public NeuronTransformsInstance( Transform root, string prefix, NeuronActor actor )
			:base( actor )
		{
			Bind( root, prefix );
		}
		
		public NeuronTransformsInstance( NeuronActor actor )
			:base( actor )
		{
		}
		
		new void OnEnable()
		{
			base.OnEnable();
			
			if( root == null )
			{
				root = transform;
			}
			
			Bind( root, prefix );
		}
		
		new void Update()
		{
			base.ToggleConnect();
			base.Update();
			
			if( boundActor != null && boundTransforms && !physicalUpdate )
			{				
				if( physicalReference.Initiated() )
				{
					ReleasePhysicalContext();
				}
				
				ApplyMotion( boundActor, transforms, bonePositionOffsets, boneRotationOffsets );
			}
		}
		
		void FixedUpdate()
		{
			base.ToggleConnect();
			
			if( boundActor != null && boundTransforms && physicalUpdate )
			{				
				if( !physicalReference.Initiated() )
				{
					physicalUpdate = InitPhysicalContext();
				}
				
				ApplyMotionPhysically( physicalReference.GetReferenceTransforms(), transforms );
			}
		}
		
		public Transform[] GetTransforms()
		{
			return transforms;
		}
		
		static bool ValidateVector3( Vector3 vec )
		{
			return !float.IsNaN( vec.x ) && !float.IsNaN( vec.y ) && !float.IsNaN( vec.z )
				&& !float.IsInfinity( vec.x ) && !float.IsInfinity( vec.y ) && !float.IsInfinity( vec.z );
		}
		
		// set position for bone
		static void SetPosition( Transform[] transforms, NeuronBones bone, Vector3 pos )
		{
			Transform t = transforms[(int)bone];
			if( t != null )
			{
				// calculate position when we have scale
				pos.Scale( new Vector3( 1.0f / t.parent.lossyScale.x, 1.0f / t.parent.lossyScale.y, 1.0f / t.parent.lossyScale.z ) );
			
				if( !float.IsNaN( pos.x ) && !float.IsNaN( pos.y ) && !float.IsNaN( pos.z ) )
				{
					t.localPosition = pos;
				}
			}
		}
		
		// set rotation for bone
		static void SetRotation( Transform[] transforms, NeuronBones bone, Vector3 rotation )
		{
			Transform t = transforms[(int)bone];
			if( t != null )
			{
				Quaternion rot = Quaternion.Euler( rotation );
				if( !float.IsNaN( rot.x ) && !float.IsNaN( rot.y ) && !float.IsNaN( rot.z ) && !float.IsNaN( rot.w ) )
				{
					t.localRotation = rot;
				}
			}
		}
		
		// apply transforms extracted from actor mocap data to bones
		public static void ApplyMotion( NeuronActor actor, Transform[] transforms, Vector3[] bonePositionOffsets, Vector3[] boneRotationOffsets )
		{			
			// apply Hips position
			SetPosition( transforms, NeuronBones.Hips, actor.GetReceivedPosition( NeuronBones.Hips ) );
			SetRotation( transforms, NeuronBones.Hips, actor.GetReceivedRotation( NeuronBones.Hips ) );
			
			// apply positions
			if( actor.withDisplacement )
			{
				for( int i = 1; i < (int)NeuronBones.NumOfBones && i < transforms.Length; ++i )
				{
					SetPosition( transforms, (NeuronBones)i, actor.GetReceivedPosition( (NeuronBones)i ) + bonePositionOffsets[i] );
					SetRotation( transforms, (NeuronBones)i, actor.GetReceivedRotation( (NeuronBones)i ) + boneRotationOffsets[i] );
				}
			}
			else
			{
				// apply rotations
				for( int i = 1; i < (int)NeuronBones.NumOfBones && i < transforms.Length; ++i )
				{
					SetRotation( transforms, (NeuronBones)i, actor.GetReceivedRotation( (NeuronBones)i ) );
				}
			}
		}
		
		// apply Transforms of src bones to dest Rigidbody Components of bone
		public static void ApplyMotionPhysically( Transform[] src, Transform[] dest )
		{
			if( src != null && dest != null )
			{
				for( int i = 0; i < (int)NeuronBones.NumOfBones; ++i )
				{
					Transform src_transform = src[i];
					Transform dest_transform = dest[i];
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
		}
		
		public bool Bind( Transform root, string prefix )
		{
			this.root = root;
			this.prefix = prefix;
			int bound_count = NeuronHelper.Bind( root, transforms, prefix, false );
			boundTransforms = bound_count >= (int)NeuronBones.NumOfBones;
			UpdateOffset();
			return boundTransforms;
		}
		
		bool InitPhysicalContext()
		{
			if( physicalReference.Init( root, prefix, transforms ) )
			{
				// break original object's hierachy of transforms, so we can use MovePosition() and MoveRotation() to set transform
				NeuronHelper.BreakHierarchy( transforms );
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
			// initiate values
			for( int i = 0; i < (int)HumanBodyBones.LastBone; ++i )
			{
				bonePositionOffsets[i] = Vector3.zero;
				boneRotationOffsets[i] = Vector3.zero;
			}
			
			if( boundTransforms )
			{
				bonePositionOffsets[(int)NeuronBones.LeftUpLeg] = new Vector3( 0.0f, transforms[(int)NeuronBones.LeftUpLeg].localPosition.y, 0.0f );
				bonePositionOffsets[(int)NeuronBones.RightUpLeg] = new Vector3( 0.0f, transforms[(int)NeuronBones.RightUpLeg].localPosition.y, 0.0f );
			}
		}
	}
}