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
using NSH = Neuron.NeuronHelper;

namespace Neuron
{
	public static class BoneRigidbodies
	{
		// add rigidbodies by animator
		static NSH.OnAddBoneComponent<Rigidbody> delegate_add_rigidbody = new NSH.OnAddBoneComponent<Rigidbody>( AddRigidBody );
		
		static void AddRigidBody( HumanBodyBones bone, Rigidbody rigidbody, object[] args )
		{
			rigidbody.useGravity = false;
			rigidbody.isKinematic = true;
		}
		
		public static void AddSkeletonRigidBodies( Animator animator )
		{
			int counter = NSH.AddBonesComponents<Rigidbody>( animator, delegate_add_rigidbody );
			Debug.Log( string.Format( "[NeuronUtilities] {0} Rigidbodies added to {1}.", counter, animator.gameObject.name ), animator.gameObject );
		}
		
		// add rigidbodies by transforms
		static NSH.OnAddBoneComponentTransform<Rigidbody> delegate_add_rigidbody_transform = new NSH.OnAddBoneComponentTransform<Rigidbody>( AddRigidBodyTransform );
		
		static void AddRigidBodyTransform( NeuronBones bone, Rigidbody rigidbody, object[] args )
		{
			rigidbody.useGravity = false;
			rigidbody.isKinematic = true;
		}
		
		public static void AddSkeletonRigidBodies( Transform root, string prefix )
		{
			int counter = NSH.AddBonesComponentsTransform<Rigidbody>( root, prefix, delegate_add_rigidbody_transform );
			Debug.Log( string.Format( "[NeuronUtilities] {0} Rigidbodies added to {1}.", counter, root.name ), root );
		}
		
		// remove rigidbodies by animator
		public static void RemoveSkeletonRigidBodies( Animator animator )
		{
			int counter = NSH.RemoveBonesComponents<Rigidbody>( animator, null );
			Debug.Log( string.Format( "[NeuronUtilities] {0} Rigidbodies removed from {1}.", counter, animator.gameObject.name ), animator.gameObject );
		}
		
		// remove rigidbodies by transforms
		public static void RemoveSkeletonRigidBodies( Transform root, string prefix )
		{
			int counter = NSH.RemoveBonesComponentsTransform<Rigidbody>( root, prefix, null );
			Debug.Log( string.Format( "[NeuronUtilities] {0} Rigidbodies removed from {1}.", counter, root.name ), root );
		}
		
	}
}