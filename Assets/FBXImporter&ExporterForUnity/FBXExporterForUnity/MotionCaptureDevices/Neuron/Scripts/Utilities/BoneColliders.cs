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
using System.Collections.Generic;
using UnityEngine;

namespace Neuron
{
	public static class BoneColliders
	{
		#if UNITY_EDITOR
		public static void AddSkeletonColliders( Animator animator, string prefab_path, int layer )
		{
			RemoveSkeletonColliders( animator );
			
			// validate input
			if( animator == null )
			{
				Debug.LogError( "[NeuronBoneColliders] Invalid animator" );
				return;
			}
			
			// load prefabs
			GameObject template = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( prefab_path );
			if( template == null )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not load asset \"{0}\"", prefab_path ) );
				return;
			}
			
			Animator template_animator = template.GetComponent<Animator>();
			if( template_animator == null )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not find valid animator from template" ) );
				return;
			}
			
			// prepare source object
			GameObject src_object = (GameObject)GameObject.Instantiate( template, animator.transform.position, animator.transform.rotation );
			Animator src_animator = src_object.GetComponent<Animator>();
						
			// move colliders
			int counter = 0;
			for( HumanBodyBones i = 0; i < HumanBodyBones.LastBone; ++i ) 
			{
				Transform src_t = src_animator.GetBoneTransform( i );
				Transform dest_t = animator.GetBoneTransform( i );
				
				if( src_t != null && dest_t != null )
				{
					Collider src_c = null;
					for( int child_index = 0; child_index < src_t.childCount; ++child_index )
					{
						Transform t = src_t.GetChild( child_index );
						src_c = t.GetComponent<Collider>();
						if( src_c != null )
						{
							Vector3 cache_pos = src_c.transform.localPosition;
							Quaternion cache_rot = src_c.transform.localRotation;

							src_c.transform.parent = dest_t;
							src_c.transform.localPosition = cache_pos;
							src_c.transform.localRotation = cache_rot;
							src_c.gameObject.layer = layer;
							src_c.gameObject.name = string.Format( "COL_{0}", dest_t.gameObject.name );

							++counter;

							--child_index;
						}
					}
				}
			}
			
			Debug.Log( string.Format( "[NeuronBoneColliders] {0} Colliders added to {1}.", counter, animator.gameObject.name ), animator.gameObject );
			
			GameObject.DestroyImmediate( src_object );
			Resources.UnloadUnusedAssets();
		}
		
		public static void AddSkeletonColliders( Transform root, string prefix, string prefab_path, int layer )
		{
			RemoveSkeletonColliders( root, prefix );
			
			// validate input
			Transform[] dest_bones = new Transform[(int)NeuronBones.NumOfBones];
			int bound_count = NeuronHelper.Bind( root, dest_bones, prefix, false );
			if( bound_count < (int)NeuronBones.NumOfBones )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not bind neuron bones to {0}", root.name ), root );
				return;
			}
			
			// load prefab
			GameObject template = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( prefab_path );
			if( template == null )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not load asset \"{0}\"", prefab_path ) );
				return;
			}
			
			Animator template_animator = template.GetComponent<Animator>();
			if( template_animator == null )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not find valid animator from template" ) );
				return;
			}
			
			// prepare source object
			GameObject src_object = (GameObject)GameObject.Instantiate( template, root.position, root.rotation );
			Transform[] src_bones = new Transform[(int)NeuronBones.NumOfBones];
			bound_count = NeuronHelper.Bind( src_object.transform, src_bones, "Robot_", false );
			if( bound_count < (int)NeuronBones.NumOfBones )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not bind neuron bones to template {0}", src_object.name ), src_object );
			}
			
			// move colliders
			int counter = 0;
			for( int i = 0; i < (int)NeuronBones.NumOfBones; ++i ) 
			{
				Transform src_t = src_bones[i];
				Transform dest_t = dest_bones[i];
				
				if( src_t != null && dest_t != null )
				{
					Collider src_c = null;
					for( int child_index = 0; child_index < src_t.childCount; ++child_index )
					{
						Transform t = src_t.GetChild( child_index );
						src_c = t.GetComponent<Collider>();
						if( src_c != null )
						{
							Vector3 cache_pos = src_c.transform.localPosition;
							Quaternion cache_rot = src_c.transform.localRotation;

							src_c.transform.parent = dest_t;
							src_c.transform.localPosition = cache_pos;
							src_c.transform.localRotation = cache_rot;
							src_c.gameObject.layer = layer;
							src_c.gameObject.name = string.Format( "COL_{0}", dest_t.gameObject.name );

							++counter;

							--child_index;
						}
					}
				}
			}
			
			Debug.Log( string.Format( "[NeuronBoneColliders] {0} Colliders added to {1}.", counter, root.name ), root );
			
			// clean source object
			GameObject.DestroyImmediate( src_object );
			Resources.UnloadUnusedAssets();
		}
		
		#endif
		
		public static void RemoveSkeletonColliders( Animator animator )
		{
			if( animator == null )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Invalid animator" ) );
				return;
			}
			
			int counter = 0;
			for( HumanBodyBones i = 0; i < HumanBodyBones.LastBone; ++i ) 
			{	
				Transform bone_t = animator.GetBoneTransform( i );
				if( bone_t != null )
				{
					List<GameObject> to_delete = new List<GameObject>();
					for( int child_index = 0; child_index < bone_t.childCount; ++child_index )
					{
						Collider collider = bone_t.GetChild( child_index ).GetComponent<Collider>();
						if( collider != null )
						{
							to_delete.Add( collider.gameObject );
						}
					}
					
					counter += to_delete.Count;
					foreach( GameObject obj in to_delete )
					{
						GameObject.DestroyImmediate( obj );
					}
				}
			}
			
			Debug.Log( string.Format( "[NeuronBoneColliders] {0} Colliders removed from {1}.", counter, animator.gameObject.name ), animator.gameObject );
		}
		
		public static void RemoveSkeletonColliders( Transform root, string prefix )
		{
			Transform[] bones = new Transform[(int)NeuronBones.NumOfBones];
			int bound_count = NeuronHelper.Bind( root, bones, prefix, false );
			if( bound_count < (int)NeuronBones.NumOfBones )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not bind neuron bones to {0}", root.name ), root );
				return;
			}
			
			int counter = 0;
			for( int i = 0; i < (int)NeuronBones.NumOfBones; ++i ) 
			{	
				Transform bone_t = bones[i];
				if( bone_t != null )
				{
					List<GameObject> to_delete = new List<GameObject>();
					for( int child_index = 0; child_index < bone_t.childCount; ++child_index )
					{
						Collider collider = bone_t.GetChild( child_index ).GetComponent<Collider>();
						if( collider != null )
						{
							to_delete.Add( collider.gameObject );
						}
					}
					
					counter += to_delete.Count;
					foreach( GameObject obj in to_delete )
					{
						GameObject.DestroyImmediate( obj );
					}
				}
			}
			
			Debug.Log( string.Format( "[NeuronBoneColliders] {0} Colliders removed from {1}.", counter, root.name ), root );
		}
		
		#if UNITY_EDITOR
		public static void AddSkeletonColliderRenderers( Animator animator, Color color )
		{
			RemoveSkeletonColliderRenderers( animator );
		
			if( animator == null )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Invalid animator" ) );
				return;
			}

			Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>( "Assets/Neuron/Resources/Materials/BoneColliderMaterial.mat" );
			if( mat != null )
			{
				mat.color = color;
			}
			else
			{
				Debug.LogError( string.Format( "[NeuronBonColliders] Can not find \"Assets/Neuron/Resources/Materials/BoneColliderMaterial.mat\"." ) );
			}
			
			int counter = 0;
			GameObject mesh_capsule_obj = GameObject.CreatePrimitive( PrimitiveType.Capsule );
			GameObject mesh_cube_obj = GameObject.CreatePrimitive( PrimitiveType.Cube );
			
			Mesh mesh_capsule = mesh_capsule_obj.GetComponent<MeshFilter>().sharedMesh;
			Mesh mesh_cube = mesh_cube_obj.GetComponent<MeshFilter>().sharedMesh;
			
			for( HumanBodyBones i = 0; i < HumanBodyBones.LastBone; ++i ) 
			{	
				Transform bone_t = animator.GetBoneTransform( i );
				if( bone_t != null )
				{
					for( int child_index = 0; child_index < bone_t.childCount; ++child_index )
					{
						Collider collider = bone_t.GetChild( child_index ).GetComponent<Collider>();
						if( collider != null )
						{
							MeshFilter mesh_filter = collider.gameObject.AddComponent<MeshFilter>();
							if( collider.GetType() == typeof( CapsuleCollider ) )
							{
								mesh_filter.mesh = mesh_capsule;
							}
							
							if( collider.GetType() == typeof( BoxCollider ) )
							{
								mesh_filter.mesh = mesh_cube;
							}
							
							MeshRenderer mesh_renderer = collider.gameObject.AddComponent<MeshRenderer>();
							mesh_renderer.material = mat;
							
							++counter;
						}
					}
				}
			}
			
			Debug.Log( string.Format( "[NeuronBoneColliders] {0} Collider renderers added to {1}.", counter, animator.gameObject.name ), animator.gameObject );
			
			GameObject.DestroyImmediate( mesh_capsule_obj );
			GameObject.DestroyImmediate( mesh_cube_obj );
		}
		
		public static void AddSkeletonColliderRenderers( Transform root, string prefix, Color color )
		{
			RemoveSkeletonColliderRenderers( root, prefix );
			
			Transform[] dest_bones = new Transform[(int)NeuronBones.NumOfBones];
			int bound_count = NeuronHelper.Bind( root, dest_bones, prefix, false );
			if( bound_count < (int)NeuronBones.NumOfBones )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not bind neuron bones to {0}", root.name ), root );
				return;
			}

			Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>( "Assets/Neuron/Resources/Materials/BoneColliderMaterial.mat" );
			if( mat != null )
			{
				mat.color = color;
			}
			else
			{
				Debug.LogError( string.Format( "[NeuronBonColliders] Can not find \"Assets/Neuron/Resources/Materials/BoneColliderMaterial.mat\"." ) );
			}
			
			int counter = 0;
			GameObject mesh_capsule_obj = GameObject.CreatePrimitive( PrimitiveType.Capsule );
			GameObject mesh_cube_obj = GameObject.CreatePrimitive( PrimitiveType.Cube );
			
			Mesh mesh_capsule = mesh_capsule_obj.GetComponent<MeshFilter>().sharedMesh;
			Mesh mesh_cube = mesh_cube_obj.GetComponent<MeshFilter>().sharedMesh;
			
			for( int i = 0; i < (int)NeuronBones.NumOfBones; ++i ) 
			{	
				Transform bone_t = dest_bones[i];
				if( bone_t != null )
				{
					for( int child_index = 0; child_index < bone_t.childCount; ++child_index )
					{
						Collider collider = bone_t.GetChild( child_index ).GetComponent<Collider>();
						if( collider != null )
						{
							MeshFilter mesh_filter = collider.gameObject.AddComponent<MeshFilter>();
							if( collider.GetType() == typeof( CapsuleCollider ) )
							{
								mesh_filter.mesh = mesh_capsule;
							}
							
							if( collider.GetType() == typeof( BoxCollider ) )
							{
								mesh_filter.mesh = mesh_cube;
							}
							
							MeshRenderer mesh_renderer = collider.gameObject.AddComponent<MeshRenderer>();
							mesh_renderer.material = mat;
							
							++counter;
						}
					}
				}
			}
			
			Debug.Log( string.Format( "[NeuronBoneColliders] {0} Collider renderers added to {1}", counter, root.name ), root );
			
			GameObject.DestroyImmediate( mesh_capsule_obj );
			GameObject.DestroyImmediate( mesh_cube_obj );
		}
		
		#endif
		
		public static void RemoveSkeletonColliderRenderers( Animator animator )
		{
			if( animator == null )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Invalid animator" ) );
				return;
			}
			
			int counter = 0;
			for( HumanBodyBones i = 0; i < HumanBodyBones.LastBone; ++i ) 
			{	
				Transform bone_t = animator.GetBoneTransform( i );
				if( bone_t != null )
				{
					for( int child_index = 0; child_index < bone_t.childCount; ++child_index )
					{
						Collider collider = bone_t.GetChild( child_index ).GetComponent<Collider>();
						if( collider != null )
						{
							MeshFilter mesh_filter = collider.GetComponent<MeshFilter>();
							MeshRenderer mesh_renderer = collider.GetComponent<MeshRenderer>();
							
							if( mesh_renderer != null )
							{
								GameObject.DestroyImmediate( mesh_renderer );
							}
							if( mesh_filter != null )
							{
								GameObject.DestroyImmediate( mesh_filter );
							}
							
							++counter;
						}
					}
				}
			}
			
			Debug.Log( string.Format( "[NeuronBoneColliders] {0} Collider renderers removed from {1}.", counter, animator.gameObject.name ), animator.gameObject );
		}
		
		public static void RemoveSkeletonColliderRenderers( Transform root, string prefix )
		{
			Transform[] bones = new Transform[(int)NeuronBones.NumOfBones];
			int bound_count = NeuronHelper.Bind( root, bones, prefix, false );
			if( bound_count < (int)NeuronBones.NumOfBones )
			{
				Debug.LogError( string.Format( "[NeuronBoneColliders] Can not bind neuron bones to {0}", root.name ), root );
				return;
			}
			
			int counter = 0;
			for( int i = 0; i < (int)NeuronBones.NumOfBones; ++i ) 
			{	
				Transform bone_t = bones[i];
				if( bone_t != null )
				{
					for( int child_index = 0; child_index < bone_t.childCount; ++child_index )
					{
						Collider collider = bone_t.GetChild( child_index ).GetComponent<Collider>();
						if( collider != null )
						{
							MeshFilter mesh_filter = collider.GetComponent<MeshFilter>();
							MeshRenderer mesh_renderer = collider.GetComponent<MeshRenderer>();
							
							if( mesh_renderer != null )
							{
								GameObject.DestroyImmediate( mesh_renderer );
							}
							if( mesh_filter != null )
							{
								GameObject.DestroyImmediate( mesh_filter );
							}
							
							++counter;
						}
					}
				}
			}
			
			Debug.Log( string.Format( "[NeuronBoneColliders] {0} Collider renderers removed from {1}", counter, root.name ), root );
		}
	}
}