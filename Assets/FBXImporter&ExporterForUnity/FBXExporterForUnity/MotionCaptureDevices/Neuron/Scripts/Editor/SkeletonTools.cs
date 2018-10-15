using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Neuron;

public class SkeletonTools: EditorWindow
{
	// target
	List<GameObject> 				objects					= new List<GameObject>();
	List<string>					prefixes				= new List<string>();
	string							default_prefix			= "Robot_";
	// gui
	Vector2							scroll_position			= new Vector2( 0.0f, 0.0f );
	// collider
	string							collider_template_path	= "Assets/Neuron/Prefabs/Robot Colliders Template.prefab";
	Color							bone_collider_color		= Color.yellow;
	int								bone_collider_layer		= 0;
	string							default_layer_name		= "Body";
	// bone line	
	Color 							bone_line_color 		= Color.blue;
	float 							bone_line_parent_width 	= 0.03f;
	float 							bone_line_child_width 	= 0.008f;
	int								bone_line_render_order	= 10;
	
	[MenuItem( "Neuron/Skeleton Tools" )]
	static void Init ()
	{
		SkeletonTools window = (SkeletonTools)EditorWindow.GetWindow( typeof( SkeletonTools ) );
		window.Show();
		
		window.OnSelectionChange();
	}
	
	void OnFocus()
	{
		OnSelectionChange();
	}
	
	bool CheckSelectedObjects()
	{
		if( objects.Count > 0 )
		{
			return true;
		}
		
		Debug.LogWarning( "[SkeletonToolsForAnimator] No target objects." );
		return false;
	}
	
	void OnInspectorUpdate()
	{
		this.Repaint();
	}
	
	void OnSelectionChange()
	{
		List<GameObject> new_objects = new List<GameObject>();
		List<string> new_prefixes = new List<string>();
		
		for( int i = 0; i < Selection.transforms.Length; ++i )
		{
			Transform t = Selection.transforms[i];
			NeuronAnimatorInstance animator_driver = t.GetComponent<NeuronAnimatorInstance>();
			NeuronTransformsInstance transform_driver = t.GetComponent<NeuronTransformsInstance>();
			
			if( animator_driver != null && t.GetComponent<Animator>() != null || transform_driver != null )
			{
				new_objects.Add( t.gameObject );
				new_prefixes.Add( transform_driver != null ? default_prefix : null );
			}
		}
		
		if( new_objects.Count > 0 )
		{
			objects = new_objects;
			prefixes = new_prefixes;
		}
	}
	
	void Update()
	{
		int layer_body = LayerMask.NameToLayer( default_layer_name );
		if( layer_body >= 0 && bone_collider_layer == 0 )
		{
			bone_collider_layer = layer_body;	
		}
	}
	
	void OnGUI()
	{	
		float wFix = 0.5f;
		scroll_position = GUILayout.BeginScrollView( scroll_position, GUILayout.Width( Screen.width ), GUILayout.Height( Screen.height - 20 ) );
		
		GUILayout.Label( "Various tools to add or remove components from every bone.", EditorStyles.wordWrappedMiniLabel );
		
		// show targets
		GUILayout.Label( "Target Objects:", EditorStyles.boldLabel );
		{		
			if( objects.Count > 0 )
			{
				for( int i = 0; i < objects.Count; ++i )
				{
					GUILayout.BeginHorizontal();
					objects[i] = EditorGUILayout.ObjectField( objects[i], typeof( GameObject ), true, GUILayout.Width( Screen.width * wFix ) ) as GameObject;
					if( prefixes[i] != null )
					{
						prefixes[i] = EditorGUILayout.TextField( prefixes[i] );
					}
					GUILayout.EndHorizontal();
					
					// set null
					if( objects[i] == null )
					{
						objects.RemoveAt( i );
						prefixes.RemoveAt( i );
					}
				}
				
				if( GUILayout.Button( "Clear" ) )
				{
					objects.Clear();
				}
			}
			else
			{
				GUILayout.Label( "Select objects with Component \"NeuronAnimatorDriver\" or \"NeuronTransformsDriver\".", EditorStyles.wordWrappedMiniLabel );
			}
		}
		
		// show tools
		if( objects.Count > 0 )
		{
			// collider tools
			GUILayout.Label( "Colliders Settings:", EditorStyles.boldLabel );
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bone Collider Layer: ");
				bone_collider_layer = EditorGUILayout.LayerField( bone_collider_layer, GUILayout.Width( Screen.width * wFix ) );
				GUILayout.EndHorizontal();
				
				if( GUILayout.Button( "Add Colliders" ) && CheckSelectedObjects() )
				{
					for( int i = 0; i < objects.Count; ++i )
					{
						GameObject obj = objects[i];
						string prefix = prefixes[i];
						
						if( obj != null )
						{
							if( prefix == null )
							{
								Neuron.BoneColliders.AddSkeletonColliders( obj.GetComponent<Animator>(), collider_template_path, bone_collider_layer );
							}
							else
							{
								Neuron.BoneColliders.AddSkeletonColliders( obj.transform, prefix, collider_template_path, bone_collider_layer );
							}
						}
					}
				}
			
				if( GUILayout.Button( "Remove Colliders" ) && CheckSelectedObjects() )
				{
					for( int i = 0; i < objects.Count; ++i )
					{
						GameObject obj = objects[i];
						string prefix = prefixes[i];
						
						if( obj != null )
						{
							if( prefix == null )
							{
								Neuron.BoneColliders.RemoveSkeletonColliders( obj.GetComponent<Animator>() );
							}
							else
							{
								Neuron.BoneColliders.RemoveSkeletonColliders( obj.transform, prefix );
							}
						}
					}
				}
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bone Collider Color: ");
				bone_collider_color = EditorGUILayout.ColorField( bone_collider_color, GUILayout.Width( Screen.width * wFix ) );
				GUILayout.EndHorizontal();
				
				if( GUILayout.Button( "Show Colliders" ) && CheckSelectedObjects() )
				{
					for( int i = 0; i < objects.Count; ++i )
					{
						GameObject obj = objects[i];
						string prefix = prefixes[i];
						
						if( obj != null )
						{
							if( prefix == null )
							{
								Neuron.BoneColliders.AddSkeletonColliderRenderers( obj.GetComponent<Animator>(), bone_collider_color );
							}
							else
							{
								Neuron.BoneColliders.AddSkeletonColliderRenderers( obj.transform, prefix, bone_collider_color );
							}
						}
					}
				}
				
				if( GUILayout.Button( "Hide Colliders" ) && CheckSelectedObjects() )
				{
					for( int i = 0; i < objects.Count; ++i )
					{
						GameObject obj = objects[i];
						string prefix = prefixes[i];
						
						if( obj != null )
						{
							if( prefix == null )
							{
								Neuron.BoneColliders.RemoveSkeletonColliderRenderers( obj.GetComponent<Animator>() );
							}
							else
							{
								Neuron.BoneColliders.RemoveSkeletonColliderRenderers( obj.transform, prefix );
							}
						}
					}
				}
			}
		
			// rigidbody tools
			GUILayout.Label( "Rigidbodies Settings:", EditorStyles.boldLabel );
			{			
				if( GUILayout.Button( "Add Rigidbodies" ) && CheckSelectedObjects() )
				{
					for( int i = 0; i < objects.Count; ++i )
					{
						GameObject obj = objects[i];
						string prefix = prefixes[i];
						
						if( obj != null )
						{
							if( prefix == null )
							{
								Neuron.BoneRigidbodies.AddSkeletonRigidBodies( obj.GetComponent<Animator>() );
							}
							else
							{
								Neuron.BoneRigidbodies.AddSkeletonRigidBodies( obj.transform, prefix );
							}
						}
					}
				}
				
				if( GUILayout.Button( "Remove Rigidbodies" ) && CheckSelectedObjects() )
				{
					for( int i = 0; i < objects.Count; ++i )
					{
						GameObject obj = objects[i];
						string prefix = prefixes[i];
						
						if( obj != null )
						{
							if( prefix == null )
							{
								Neuron.BoneRigidbodies.RemoveSkeletonRigidBodies( obj.GetComponent<Animator>() );
							}
							else
							{
								Neuron.BoneRigidbodies.RemoveSkeletonRigidBodies( obj.transform, prefix );
							}
						}
					}
				}
			}
			
			// bone line tools				
			GUILayout.Label( "BoneLine Settings:", EditorStyles.boldLabel );
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bone Line Color: ");
				bone_line_color = EditorGUILayout.ColorField( bone_line_color, GUILayout.Width( Screen.width * wFix ) );
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bone Line Parent Width: ");
				bone_line_parent_width = EditorGUILayout.Slider( bone_line_parent_width ,0.001f, 0.1f, GUILayout.Width( Screen.width * wFix ) );
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bone Line Child Width: ");
				bone_line_child_width = EditorGUILayout.Slider( bone_line_child_width,0.001f, 0.1f, GUILayout.Width( Screen.width * wFix ) );
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bone Line Render Order: ");
				bone_line_render_order = int.Parse( EditorGUILayout.TextField( bone_line_render_order.ToString(), GUILayout.Width( Screen.width * wFix ) ) );
				GUILayout.EndHorizontal();
				
				if( GUILayout.Button( "Add BoneLines" ) && CheckSelectedObjects() )
				{
					for( int i = 0; i < objects.Count; ++i )
					{
						GameObject obj = objects[i];
						string prefix = prefixes[i];
						
						if( obj != null )
						{
							if( prefix == null )
							{
								Neuron.BoneLines.AddSkeletonBoneLines( obj.GetComponent<Animator>(), bone_line_color, 10, bone_line_parent_width, bone_line_child_width );
							}
							else
							{
								Neuron.BoneLines.AddSkeletonBoneLines( obj.transform, prefix, bone_line_color, 10, bone_line_parent_width, bone_line_child_width );
							}
						}
					}
				}
				
				if( GUILayout.Button( "Remove BoneLines" ) && CheckSelectedObjects() )
				{
					for( int i = 0; i < objects.Count; ++i )
					{
						GameObject obj = objects[i];
						string prefix = prefixes[i];
						
						if( obj != null )
						{
							if( prefix == null )
							{
								Neuron.BoneLines.RemoveSkeletonBoneLines( obj.GetComponent<Animator>() );
							}
							else
							{
								Neuron.BoneLines.RemoveSkeletonBoneLines( obj.transform, prefix );
							}
						}
					}
				}				
			}
		}
		
		GUILayout.EndScrollView();
	}
}