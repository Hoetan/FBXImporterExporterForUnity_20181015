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

namespace Neuron
{
	public class NeuronTransformsPhysicalReference
	{
		GameObject referenceObject = null;
		Transform[] referenceTransforms = null;
		
		public bool Init( Transform root, string prefix, Transform[] bound_transforms )
		{
			if( root == null )
			{
				Debug.LogError(	"[NeuronTransformsPhysicalReference] Invalid root Transform" );
				return false;
			}
			
			// check if there is enough Rigidbody Component on the bones,
			// if not return false to prevent init reference_object
			if( !CheckRigidbodies( bound_transforms ) )
			{
				Debug.LogError(	string.Format(
					"[NeuronTransformsPhysicalReference] Trying to use physics update but no Rigidbody Component in Actor \"{0}\". Did you forget to add Rigidbody Component?",
					root.gameObject.name ), root );
				return false;
			}
			
			// duplicate bound object as reference object,
			// we only use this reference object's transforms to get world transforms
			referenceObject = (GameObject)GameObject.Instantiate( root.gameObject, root.position, root.rotation );
			referenceObject.name = string.Format( "{0} (neuron reference)", root.gameObject.name );
			referenceTransforms = new Transform[(int)NeuronBones.NumOfBones];
			NeuronHelper.Bind( referenceObject.transform, referenceTransforms, prefix, false );
			
			NeuronTransformsInstance referenceInstance = referenceObject.GetComponent<NeuronTransformsInstance>();
			if( referenceInstance != null )
			{
				referenceInstance.physicalUpdate = false;
			}
			
			// remove all unnecessary components, this will prevent rendering and any unexpected behaviour from custom scripts
			Component[] components = referenceObject.GetComponentsInChildren<Component>();
			for( int i = 0; i < components.Length; ++i )
			{
				Component c = components[i];
				if( c.GetType() != typeof( Transform )
				&& components[i].GetType() != typeof( NeuronTransformsInstance ) )
				{
					GameObject.DestroyImmediate( c );
				}
			}
			
			return true;
		}
		
		public void Release()
		{
			if( referenceObject != null )
			{
				GameObject.DestroyImmediate( referenceObject );
				referenceObject = null;
				referenceTransforms = null;
			}
		}
		
		public bool Initiated()
		{
			return referenceObject != null && referenceTransforms != null;
		}
		
		public GameObject GetReferenceObject()
		{
			return referenceObject;
		}
		
		public Transform[] GetReferenceTransforms()
		{
			return referenceTransforms;
		}
		
		bool CheckRigidbodies( Transform[] transforms )
		{
			if( transforms == null )
			{
				return false;
			}
			
			bool ret = true;
			for( int i = 0; i < transforms.Length; ++i )
			{
				Transform t = transforms[i];
				if( t != null )
				{
					if( t.GetComponent<Rigidbody>() == null )
					{
						ret = false;
						break;
					}
				}
			}
			
			return ret;
		}
	}
}