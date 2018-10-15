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
using Neuron;

namespace Neuron
{
	[ExecuteInEditMode]
	class BoneLine : MonoBehaviour
	{
		public Material			RendererMaterial;
		public Transform		ParentTransform;
		public Transform		ChildTransform;
		public float			ParentWidth;
		public float			ChildWidth;
		public bool				Enabled { get { return line_renderer.enabled; } set { line_renderer.enabled = value; } }
		
		LineRenderer 			line_renderer = null;
		
		public void AddRenderer( Material material, float parent_width, float child_width, Transform parent_transform, Transform child_transform )
		{
			if( material != null )
			{
				line_renderer = GetComponent<LineRenderer>();
				if( line_renderer == null )
				{
					line_renderer = gameObject.AddComponent<LineRenderer>();	
				}
				
				line_renderer.material = material;
				line_renderer.SetWidth( parent_width, child_width );
				line_renderer.useWorldSpace = true;
				
				#if UNITY_4_6_1
				line_renderer.castShadows = false;
				#elif UNITY_5
				line_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				#endif
				
				line_renderer.receiveShadows = false;
				
				line_renderer.SetPosition( 0, parent_transform.position );
				line_renderer.SetPosition( 1, child_transform.position );
				
				RendererMaterial = material;
				ParentWidth = parent_width;
				ChildWidth = child_width;
				ParentTransform = parent_transform;
				ChildTransform = child_transform;
			}
			else
			{
				Debug.LogError( string.Format( "[NeuronBoneLine] Invalid material {0} for bone line.", material.name ) );
			}
		}
		
		public void RemoveRenderer()
		{
			if( line_renderer != null )
			{
				DestroyImmediate( line_renderer );
				line_renderer = null;
			}
		}
		
		public void Update()
		{
			if( line_renderer == null )
			{
				line_renderer = GetComponent<LineRenderer>();
			}
			
			if( line_renderer != null && line_renderer.enabled && transform.parent != null )
			{
				line_renderer.SetPosition( 0, ParentTransform.position );
				line_renderer.SetPosition( 1, ChildTransform.position );
			}
		}
	};
}