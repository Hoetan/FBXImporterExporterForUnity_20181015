/************************************************************************************
 Copyright: Copyright 2014 Beijing Noitom Technology Ltd. All Rights reserved.
 Pending Patents: PCT/CN2014/085659 PCT/CN2014/071006

 Licensed under the Perception Neuron SDK License Beta Version (the 窶廰icense");
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
	public static class BoneLines
	{		
		#if UNITY_EDITOR
	
		static Material GetBoneLineMaterial( Color color, int render_queue_plus )
		{
			// create material
			string path = "Assets/Neuron/Resources/Shaders/BoneLineShader.shader";
			Shader shader = UnityEditor.AssetDatabase.LoadAssetAtPath( path, typeof( Shader ) ) as Shader;
			if( shader != null )
			{
				Material mat = new Material( shader );
				if( mat != null )
				{
					mat.color = color;
					mat.renderQueue += render_queue_plus;
					return mat;
				}
			}

			Debug.LogError( string.Format( "[NeuronSkeletonTools] Can not find material for bone line at {0}.", path ) );
			return null;
		}
		
		// add bone line by animator
		static NSH.OnAddBoneComponent<BoneLine>		delegate_add_bone_line = new NSH.OnAddBoneComponent<BoneLine>( AddBoneLine );
		
		static void AddBoneLine( HumanBodyBones bone, BoneLine bone_line, params object[] args )
		{
			if( args.Length < 3 || bone == HumanBodyBones.Hips )
			{
				return;
			}
			
			Material mat = (Material)args[0];
			float parent_width = (float)args[1];
			float child_width = (float)args[2];
			
			if( bone >= HumanBodyBones.LeftThumbProximal && bone <= HumanBodyBones.RightLittleDistal )
			{
				bone_line.AddRenderer( mat, parent_width / 3.0f, child_width / 3.0f, bone_line.transform.parent, bone_line.transform );
			}
			else
			{
				bone_line.AddRenderer( mat, parent_width, child_width, bone_line.transform.parent, bone_line.transform );
			}
		}
		
		public static void AddSkeletonBoneLines( Animator animator, Color color, int render_queue_plus, float parent_width, float child_width )
		{		
			Material line_material = GetBoneLineMaterial( color, render_queue_plus );
			if( line_material )
			{
				int counter = NSH.AddBonesComponents<BoneLine>( animator, delegate_add_bone_line, line_material, parent_width, child_width );

				// add bone line for spine1 and spine2 which we do not assign to animator
				Transform t = animator.GetBoneTransform( HumanBodyBones.Chest );
				Transform child = t;
				t = t.parent;
				while( t != null && t.GetComponent<BoneLine>() == null )
				{
					BoneLine bone_line = t.gameObject.AddComponent<BoneLine>();
					child = t;
					t = t.parent;
					bone_line.AddRenderer( line_material, parent_width, child_width, t, child );
					
					counter++;
				}

				// add bone line for head_end
				t = animator.GetBoneTransform( HumanBodyBones.Head );
				if( t != null && t.childCount > 0 )
				{
					// get end_end
					child = t.GetChild( 0 );
					if( child != null )
					{
						BoneLine boneline = child.gameObject.AddComponent<BoneLine>();
						boneline.AddRenderer( line_material, parent_width, child_width, t, child );
						counter++;
					}
				}

				Debug.Log( string.Format( "[NeuronUtilities] {0} Bone lines added to {1}.", counter, animator.gameObject.name ), animator.gameObject );
			}
		}
		
		// add bone line by transform
		static NSH.OnAddBoneComponentTransform<BoneLine> delegate_add_bone_line_transform = new NSH.OnAddBoneComponentTransform<BoneLine>( AddBoneLineTransform );
		
		static void AddBoneLineTransform( NeuronBones bone, BoneLine bone_line, params object[] args )
		{
			if( args.Length < 3 || bone == NeuronBones.Hips )
			{
				return;
			}
			
			Material mat = (Material)args[0];
			float parent_width = (float)args[1];
			float child_width = (float)args[2];
			
			if( bone >= NeuronBones.LeftHandThumb1 && bone <= NeuronBones.LeftHandPinky3
			   ||  bone >= NeuronBones.RightHandThumb1 && bone <= NeuronBones.RightHandPinky3 )
			{
				bone_line.AddRenderer( mat, parent_width / 3.0f, child_width / 3.0f, bone_line.transform.parent, bone_line.transform );
			}
			else
			{
				bone_line.AddRenderer( mat, parent_width, child_width, bone_line.transform.parent, bone_line.transform );
			}
		}
		
		public static void AddSkeletonBoneLines( Transform root, string prefix, Color color, int render_queue_plus, float parent_width, float child_width )
		{
			Material line_material = GetBoneLineMaterial( color, render_queue_plus );
			if( line_material )
			{
				int counter = NSH.AddBonesComponentsTransform<BoneLine>( root, prefix, delegate_add_bone_line_transform, line_material, parent_width, child_width );
				NeuronTransformsInstance instance = root.GetComponent<NeuronTransformsInstance>();
				instance.Bind( root, prefix );
				Transform t = instance.transforms[(int)NeuronBones.Head];
				if( t != null && t.childCount > 0 )
				{
					// get end_end
					Transform child = t.GetChild( 0 );
					if( child != null )
					{
						BoneLine boneline = child.gameObject.AddComponent<BoneLine>();
						boneline.AddRenderer( line_material, parent_width, child_width, t, child );
						counter++;
					}
				}

				Debug.Log( string.Format( "[NeuronUtilities] {0} Bone lines added to {1}.", counter, root.name ), root );
			}
		}
		
		#endif
		
		// remove bone line by animator
		//static NSH.OnRemoveBoneComponent<BoneLine>	delegate_remove_bone_line = new NSH.OnRemoveBoneComponent<BoneLine>( RemoveBoneLine );
		
		static void RemoveBoneLine( HumanBodyBones bone, BoneLine bone_line, params object[] args )
		{
			bone_line.RemoveRenderer();
		}
		
		public static void RemoveSkeletonBoneLines( Animator animator )
		{
			BoneLine[] bonelines = animator.GetComponentsInChildren<BoneLine>();
			for( int i = 0; i < bonelines.Length; ++i )
			{
				bonelines[i].RemoveRenderer();
				GameObject.DestroyImmediate( bonelines[i] );
			}

			Resources.UnloadUnusedAssets();
			Debug.Log( string.Format( "[NeuronUtilities] {0} Bone lines removed from {1}.", bonelines.Length, animator.gameObject.name ), animator.gameObject );
		}
		
		// remove bone line by transform
		//static NSH.OnRemoveBoneComponentTransform<BoneLine>	delegate_remove_bone_line_tranform = new NSH.OnRemoveBoneComponentTransform<BoneLine>( RemoveBoneLineTransform );
		
		static void RemoveBoneLineTransform( NeuronBones bone, BoneLine bone_line, params object[] args )
		{
			bone_line.RemoveRenderer();
		}
		
		public static void RemoveSkeletonBoneLines( Transform root, string prefix )
		{
			BoneLine[] bonelines = root.GetComponentsInChildren<BoneLine>();
			for( int i = 0; i < bonelines.Length; ++i )
			{
				bonelines[i].RemoveRenderer();
				GameObject.DestroyImmediate( bonelines[i] );
			}

			Resources.UnloadUnusedAssets();
			Debug.Log( string.Format( "[NeuronUtilities] {0} Bone lines removed from {1}.", bonelines.Length, root.gameObject.name ), root.gameObject );

			//int counter = NSH.RemoveBonesComponentsTransform<BoneLine>( root, prefix, delegate_remove_bone_line_tranform );
			//Resources.UnloadUnusedAssets();
			//Debug.Log( string.Format( "[NeuronUtilities] {0} Bone lines removed from {1}.", counter, root.name ), root );
		}
	}
}

