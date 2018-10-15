/*
 * Kinect2ModelControllerV2.cs - Handles rotating the bones of a model to match 
 * 			rotations derived from the bone positions given by the kinect2
 * 
 * 		Developed by Peter Kinney -- 2011/06/30
 *  	Modified by ほえたん(Hoetan) -- 2016/02/01
 *  	Copyright (c) 2015-2016, ACTINIA Software. All rights reserved.
 * 		Homepage: http://actinia-software.com
 * 		E-Mail: hoetan@actinia-software.com
 * 		Twitter: https://twitter.com/hoetan3
 * 		GitHub: https://github.com/hoetan
 */

using UnityEngine;
using RootSystem = System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kinect2 = Windows.Kinect;

public class Kinect2ModelControllerV2 : MonoBehaviour {
	
	//Assignments for a bitmask to control which bones to look at and which to ignore
	public enum BoneMask
	{
		None = 0x0,
		//EMPTY = 0x1,
		Spine = 0x2,
		Shoulder_Center = 0x4,
		Head = 0x8,
		Shoulder_Left = 0x10,
		Elbow_Left = 0x20,
		Wrist_Left = 0x40,
		Hand_Left = 0x80,
		Shoulder_Right = 0x100,
		Elbow_Right = 0x200,
		Wrist_Right = 0x400,
		Hand_Right = 0x800,
		Hips = 0x1000,
		Knee_Left = 0x2000,
		Ankle_Left = 0x4000,
		Foot_Left = 0x8000,
		//EMPTY = 0x10000,
		Knee_Right = 0x20000,
		Ankle_Right = 0x40000,
		Foot_Right = 0x80000,
		All = 0xEFFFE,
		Torso = 0x1000000 | Spine | Shoulder_Center | Head, //the leading bit is used to force the ordering in the editor
		Left_Arm = 0x1000000 | Shoulder_Left | Elbow_Left | Wrist_Left | Hand_Left,
		Right_Arm = 0x1000000 |  Shoulder_Right | Elbow_Right | Wrist_Right | Hand_Right,
		Left_Leg = 0x1000000 | Hips | Knee_Left | Ankle_Left | Foot_Left,
		Right_Leg = 0x1000000 | Hips | Knee_Right | Ankle_Right | Foot_Right,
		R_Arm_Chest = Right_Arm | Spine,
		No_Feet = All & ~(Foot_Left | Foot_Right),
		Upper_Body = Head |Elbow_Left | Wrist_Left | Hand_Left | Elbow_Right | Wrist_Right | Hand_Right
	}

	private Kinect2.KinectSensor _Sensor;
	private Kinect2.MultiSourceFrameReader _Reader;
	private Kinect2.Body[] _Bodies;
	private Kinect2.Body _Body;

    public bool bDebugLog = true;

    public AutoMotionCaptureDevicesSelecter sAutoMotionCaptureDevicesSelecter;
    public FBXExporterForUnity sFBXExporterForUnity;
	public float fFBXRecStart = 3.0f;

	public GameObject Root;
	public GameObject SpineBase;
	public GameObject SpineMid;
	public GameObject Neck;
	public GameObject Head;
	public GameObject CollarLeft;
	public GameObject ShoulderLeft;
	public GameObject ElbowLeft;
	public GameObject WristLeft;
	public GameObject HandLeft;
	public GameObject CollarRight;
	public GameObject ShoulderRight;
	public GameObject ElbowRight;
	public GameObject WristRight;
	public GameObject HandRight;
	public GameObject HipOverride;
	public GameObject HipLeft;
	public GameObject KneeLeft;
	public GameObject AnkleLeft;
	public GameObject FootLeft;
	public GameObject HipRight;
	public GameObject KneeRight;
	public GameObject AnkleRight;
	public GameObject FootRight;
	public GameObject SpineShoulder;
	public GameObject HandTipLeft;
	public GameObject ThumbLeft;
	public GameObject HandTipRight;
	public GameObject ThumbRight;

	public int player;
	public BoneMask Mask = BoneMask.All;
	public bool animated = true;
	public float blendWeight = 1;

    public bool bFixedUpdateMode = true;
    public int iFPS = 60;
    public float fLerpPosition = 0.2f;
    public float fLerpRotation = 0.2f;

    public float fRotRootX = 0.0f;
	
	private GameObject[] _bones; //internal handle for the bones of the model
	private Vector3[] _vecbones;
	private Vector3[] _vecbones2;
	private Quaternion[] _qbones;
	private Quaternion[] _qbones2;

	private uint _nullMask = 0x0;
	
	private Quaternion[] _baseRotation; //starting orientation of the joints
	private Vector3[] _boneDir; //in the bone's local space, the direction of the bones
	private Vector3[] _boneUp; //in the bone's local space, the up vector of the bone
	private Vector3 _hipRight; //right vector of the hips
	private Vector3 _chestRight; //right vectory of the chest	
	
	// Use this for initialization
	void Start () {
        if (sFBXExporterForUnity != null)
        {
            sFBXExporterForUnity.bOutAnimation = false;
            sFBXExporterForUnity.bOutAnimationCustomFrame = true;
        }
		_Sensor = Kinect2.KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            if (!_Sensor.IsOpen)
            {
                if (bDebugLog) Debug.Log("[Kinect2] KinectSensor Open");
                _Sensor.Open();
                _Reader = _Sensor.OpenMultiSourceFrameReader(/*Kinect2.FrameSourceTypes.Color |
				                                             Kinect2.FrameSourceTypes.Depth |
				                                             Kinect2.FrameSourceTypes.Infrared |
				                                             */
                                                             Kinect2.FrameSourceTypes.Body);
                _Reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
            else
            {
                if (bDebugLog) Debug.Log("[Kinect2] KinectSensor Opened");
                _Reader = _Sensor.OpenMultiSourceFrameReader(/*Kinect2.FrameSourceTypes.Color |
				                                             Kinect2.FrameSourceTypes.Depth |
				                                             Kinect2.FrameSourceTypes.Infrared |
				                                             */
                                                             Kinect2.FrameSourceTypes.Body);
                _Reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }
		//store bones in a list for easier access, everything except Hip_Center will be one
		//higher than the corresponding Kinect2.NuiSkeletonPositionIndex (because of the hip_override)
		_bones = new GameObject[(int)Kinect2.JointType.ThumbRight + 1] {
			null, SpineMid, SpineShoulder, Neck,
			CollarLeft, ShoulderLeft, ElbowLeft, WristLeft,
			CollarRight, ShoulderRight, ElbowRight, WristRight,
			HipOverride, HipLeft, KneeLeft, AnkleLeft,
			null, HipRight, KneeRight, AnkleRight,
			Head, HandLeft, HandRight, FootLeft, FootRight};
			//SpineShoulder, HandTipLeft, ThumbLeft, HandTipRight, ThumbRight, FootLeft, FootRight};

		_vecbones = new Vector3[(int)Kinect2.JointType.ThumbRight + 1];
		_vecbones2 = new Vector3[(int)Kinect2.JointType.ThumbRight + 1];
		_qbones = new Quaternion[(int)Kinect2.JointType.ThumbRight + 1];
		_qbones2 = new Quaternion[(int)Kinect2.JointType.ThumbRight + 1];

		//determine which bones are not available
		for(int ii = 0; ii < _bones.Length; ii++)
		{
			if(_bones[ii] == null)
			{
				_nullMask |= (uint)(1 << ii);
			}
		}
		
		//store the base rotations and bone directions (in bone-local space)
		_baseRotation = new Quaternion[(int)Kinect2.JointType.ThumbRight + 1];
		_boneDir = new Vector3[(int)Kinect2.JointType.ThumbRight + 1];
		
		//first save the special rotations for the hip and spine
		_hipRight = HipRight.transform.position - HipLeft.transform.position;
		_hipRight = HipOverride.transform.InverseTransformDirection(_hipRight);
		
		_chestRight = ShoulderRight.transform.position - ShoulderLeft.transform.position;
		_chestRight = SpineMid.transform.InverseTransformDirection(_chestRight);
		
		//get direction of all other bones
		for( int ii = 0; ii < (int)Kinect2.JointType.ThumbRight- 4; ii++)
		{
			if((_nullMask & (uint)(1 << ii)) <= 0)
			{
				//if the bone is the end of a limb, get direction from this bone to one of the extras (hand or foot).
				if(ii % 4 == 3 && ((_nullMask & (uint)(1 << (ii/4) + (int)Kinect2.JointType.ThumbRight - 4)) <= 0))
				{
					_boneDir[ii] = _bones[(ii/4) + (int)Kinect2.JointType.ThumbRight - 4].transform.position - _bones[ii].transform.position;
				}
				//if the bone is the hip_override (at boneindex Hip_Left, get direction from average of left and right hips
				else if(ii == (int)Kinect2.JointType.HipLeft && HipLeft != null && HipRight != null)
				{
					_boneDir[ii] = ((HipRight.transform.position + HipLeft.transform.position) / 2.0f) - HipOverride.transform.position;
				}
				//otherwise, get the vector from this bone to the next.
				else if((_nullMask & (uint)(1 << ii+1)) <= 0)
				{
					_boneDir[ii] = _bones[ii+1].transform.position - _bones[ii].transform.position;
				}
				else
				{
					continue;
				}
				//Since the spine of the kinect data is ~40 degrees back from the hip,
				//check what angle the spine is at and rotate the saved direction back to match the data
				if(ii == (int)Kinect2.JointType.SpineMid)
				{
					float angle = Vector3.Angle(transform.up,_boneDir[ii]);
					_boneDir[ii] = Quaternion.AngleAxis(angle,transform.right) * _boneDir[ii];
				}
				//transform the direction into local space.
				_boneDir[ii] = _bones[ii].transform.InverseTransformDirection(_boneDir[ii]);
			}
		}
		//make _chestRight orthogonal to the direction of the spine.
		_chestRight -= Vector3.Project(_chestRight, _boneDir[(int)Kinect2.JointType.SpineMid]);
		//make _hipRight orthogonal to the direction of the hip override
		Vector3.OrthoNormalize(ref _boneDir[(int)Kinect2.JointType.HipLeft],ref _hipRight);
		// Root
		Root.transform.localRotation = Quaternion.Euler(fRotRootX, 0.0f, 0.0f);
	}
	
	void Reader_MultiSourceFrameArrived(object sender, Kinect2.MultiSourceFrameArrivedEventArgs e)
	{
		var reference = e.FrameReference.AcquireFrame();
		using (var frame = reference.BodyFrameReference.AcquireFrame()) {
			if (frame != null) {
				_Bodies = new Kinect2.Body[frame.BodyFrameSource.BodyCount];
				
				frame.GetAndRefreshBodyData (_Bodies);
				foreach(Kinect2.Body body in _Bodies)
				{
					if(body.IsTracked)
					{
                        if (sAutoMotionCaptureDevicesSelecter != null)
                        {
                            sAutoMotionCaptureDevicesSelecter.bInitMoveTransform = true;
                        }
                        fFBXRecStart -= Time.deltaTime;
						if (fFBXRecStart <= 0.0) {
                            if (sFBXExporterForUnity != null)
                            {
                                sFBXExporterForUnity.bOutAnimation = true;
                            }
						}
						_Body = body;
						for( int ii = 0; ii < (int)Kinect2.JointType.ThumbRight - 4; ii++)
						{
							if( ((uint)Mask & (uint)(1 << ii) ) > 0 && (_nullMask & (uint)(1 << ii)) <= 0 )
							{
								RotateJoint(body, ii);
							}
						}
					}
				}
				frame.Dispose();
			}
		}
	}

	void Update ()
	{
        if (!bFixedUpdateMode) {
            if (_Body != null) {
				if (_Body.IsTracked) {
					for (int ii = 0; ii < (int)Kinect2.JointType.ThumbRight - 4; ii++) {
						RotateJointUpdate (ii);
					}
				}
			}
		}
	}

	void FixedUpdate ()
	{
        if (bFixedUpdateMode) {
            if (_Body != null) {
				if (_Body.IsTracked) {
					for (int ii = 0; ii < (int)Kinect2.JointType.ThumbRight - 4; ii++) {
						RotateJointUpdate (ii);
					}
				}
			}
		}
	}

	private static Vector3 GetVector3FromJoint(Kinect2.Joint joint)
	{
		return new Vector3(joint.Position.X, joint.Position.Y + 1.0f, -joint.Position.Z + 2.0f);
	}

	void RotateJoint(Kinect2.Body body, int bone) {
		//if blendWeight is 0 there is no need to compute the rotations
		if( blendWeight <= 0 ){ return; }
		Vector3 upDir = new Vector3();
		Vector3 rightDir = new Vector3();
		
		if(bone == (int)Kinect2.JointType.SpineMid)
		{
			upDir = ((HipLeft.transform.position + HipRight.transform.position) / 2.0f) - HipOverride.transform.position;
			rightDir = HipRight.transform.position - HipLeft.transform.position;
		}

		//if the model is not animated, reset rotations to fix twisted joints
		if(!animated){_bones[bone].transform.localRotation = _baseRotation[bone];}
		//if the required bone data from the kinect isn't available, return
		Kinect2.Joint? boneJoint = body.Joints[(Kinect2.JointType)bone];
		if( !boneJoint.HasValue )
		{
			return;
		}
		//get the target direction of the bone in world space
		//for the majority of bone it's bone - 1 to bone, but Hip_Override and the outside
		//shoulders are determined differently.
		
		Vector3 dir = _boneDir[bone];
		Vector3 target;
		
		//if bone % 4 == 0 then it is either an outside shoulder or the hip override
		if(bone % 4 == 0)
		{
			//hip override is at Hip_Left
			if(bone == (int)Kinect2.JointType.HipLeft)
			{
				//target = vector from hip_center to average of hips left and right
				target = ((GetVector3FromJoint(body.Joints[Kinect2.JointType.HipLeft]) + GetVector3FromJoint(body.Joints[Kinect2.JointType.HipRight])) / 2.0f) - GetVector3FromJoint(body.Joints[Kinect2.JointType.SpineMid]);
			}
			//otherwise it is one of the shoulders
			else
			{
				//target = vector from shoulder_center to bone
				target = GetVector3FromJoint(body.Joints[(Kinect2.JointType)bone]) - GetVector3FromJoint(body.Joints[Kinect2.JointType.SpineShoulder]);
			}
		}
		else
		{
			//target = vector from previous bone to bone
			target = GetVector3FromJoint(body.Joints[(Kinect2.JointType)bone]) - GetVector3FromJoint(body.Joints[(Kinect2.JointType)bone-1]);
		}
		//transform it into bone-local space (independant of the transform of the controller)
		target = transform.TransformDirection(target);
		target = _bones[bone].transform.InverseTransformDirection(target);
		//create a rotation that rotates dir into target
		Quaternion quat = Quaternion.FromToRotation(dir,target);
		//if bone is the spine, add in the rotation along the spine
		if(bone == (int)Kinect2.JointType.SpineMid)
		{
			//rotate the chest so that it faces forward (determined by the shoulders)
			dir = _chestRight;
			target = GetVector3FromJoint(body.Joints[Kinect2.JointType.ShoulderRight]) - GetVector3FromJoint(body.Joints[Kinect2.JointType.ShoulderLeft]);
			
			target = transform.TransformDirection(target);
			target = _bones[bone].transform.InverseTransformDirection(target);
			target -= Vector3.Project(target,_boneDir[bone]);

			quat *= Quaternion.FromToRotation(dir,target);

			_vecbones[bone] = GetVector3FromJoint(body.Joints[Kinect2.JointType.SpineMid]);
		}
		//if bone is the hip override, add in the rotation along the hips
		else if(bone == (int)Kinect2.JointType.HipLeft)
		{
			//rotate the hips so they face forward (determined by the hips)
			dir = _hipRight;
			target = GetVector3FromJoint(body.Joints[Kinect2.JointType.HipRight]) - GetVector3FromJoint(body.Joints[Kinect2.JointType.HipLeft]);
			
			target = transform.TransformDirection(target);
			target = _bones[bone].transform.InverseTransformDirection(target);
			target -= Vector3.Project(target,_boneDir[bone]);
			
			quat *= Quaternion.FromToRotation(dir,target);
		}
		
		//reduce the effect of the rotation using the blend parameter
		Quaternion quat2 = Quaternion.Lerp(Quaternion.identity, quat, blendWeight);
		//apply the rotation to the local rotation of the bone
		_qbones[bone] = _bones[bone].transform.localRotation * quat2;

		if(bone == (int)Kinect2.JointType.SpineMid)
		{
			restoreBone(_bones[(int)Kinect2.JointType.HipLeft],_boneDir[(int)Kinect2.JointType.HipLeft],upDir);
			restoreBone(_bones[(int)Kinect2.JointType.HipLeft],_hipRight,rightDir);
		}
		
		return;
	}

	void RotateJointUpdate(int bone)
	{
		if (_bones[bone] != null) {
			if(bone == (int)Kinect2.JointType.SpineMid)
			{
                Vector3 vecPos;
                if (bFixedUpdateMode)
                {
                    vecPos = Vector3.Slerp(_vecbones2[bone], _vecbones[bone], fLerpPosition * (120 / iFPS));
                }
                else
                {
                    vecPos = Vector3.Slerp(_vecbones2[bone], _vecbones[bone], fLerpPosition * Time.deltaTime * iFPS);
                }
                _vecbones2[bone] = vecPos;
				_bones[bone].transform.position = vecPos;
			}
			if (_qbones[bone].x == 0.0f && _qbones[bone].y == 0.0f && _qbones[bone].z == 0.0f && _qbones[bone].w == 0.0f)
			{
				return;
			}
            Quaternion quat;
            if (bFixedUpdateMode)
            {
                quat = Quaternion.Slerp(_qbones2[bone], _qbones[bone], fLerpRotation * (120 / iFPS));
            }
            else
            {
                quat = Quaternion.Slerp(_qbones2[bone], _qbones[bone], fLerpRotation * Time.deltaTime * iFPS);
            }
            if (float.IsNaN(quat.x) && float.IsNaN(quat.y) && float.IsNaN(quat.z) && float.IsNaN(quat.w))
			{
				return;
			}
			_qbones2 [bone] = quat;
			_bones[bone].transform.localRotation = quat;

			//save initial rotation
			_baseRotation[bone] = _bones[bone].transform.localRotation;
		}
	}
	
	void restoreBone(GameObject bone,Vector3 dir, Vector3 target)
	{
		//transform target into bone-local space (independant of the transform of the controller)
		//target = transform.TransformDirection(target);
		target = bone.transform.InverseTransformDirection(target);
		//create a rotation that rotates dir into target
		Quaternion quat = Quaternion.FromToRotation(dir,target);
		bone.transform.localRotation *= quat;
	}
}
