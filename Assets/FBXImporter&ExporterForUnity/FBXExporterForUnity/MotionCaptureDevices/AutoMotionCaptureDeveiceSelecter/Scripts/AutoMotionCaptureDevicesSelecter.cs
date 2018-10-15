/*
 * AutoMotionCaptureDevicesSelecter.cs
 * 
 *  	Developed by ほえたん(Hoetan) -- 2017/10/05
 *  	Copyright (c) 2015-2016, ACTINIA Software. All rights reserved.
 * 		Homepage: http://actinia-software.com
 * 		E-Mail: hoetan@actinia-software.com
 * 		Twitter: https://twitter.com/hoetan3
 * 		GitHub: https://github.com/hoetan
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuronDataReaderWraper;

using Kinect;
using Kinect2 = Windows.Kinect;
using Neuron;

using LiveAnimator;

public class AutoMotionCaptureDevicesSelecter : MonoBehaviour
{

    public enum EAutoMotionCaptureDevicesSelecter
    {
        eAuto,
        ePerceptionNeuron_1,
        ePerceptionNeuron_2,
        ePerceptionNeuron_3,
        ePerceptionNeuron_4,
        ePerceptionNeuron_5,
        ePerceptionNeuron_6,
        ePerceptionNeuron_7,
        ePerceptionNeuron_8,
        eKinect1,
        eKinect2,
        eIKinemaOrion_Male,
        eIKinemaOrion_Woman,
        eNone,
    }

    [Serializable]
    public struct SNeuronConnection
    {
        public NeuronSource sNeuronSource;
        public string address; // 127.0.0.1
        public int port; // 7001
        public int commandServerPort; // 7007
        public NeuronConnection.SocketType socketType; // TCP
        public int actorID; // 0

        public GameObject prefabDeviceJoints;
        public GameObject goAttach;
        public Vector3 vecPosition; // 0 0 0
        public Vector3 vecRotation; // 0 0 0
        public Vector3 vecScale; // 1 1 1
    };

    static public bool bDebugLog = true;

    public bool bCheckedDevices = false;
    public bool bInitDevice = false;
    public int iInitReadyFrame = 120;
    public bool bInitTransform = false;
    public bool bInitMoveTransform = false;
    public bool bInitEnableAnimator = false;
    public bool bAllInited = false;

    public string strFBXExporterForUnity = "Main Camera";
    public FBXExporterForUnity sFBXExporterForUnity;
    public GameObject[] goLiveAnimators;

    public bool bDisplayDeveiceJoints = false;

    public bool bLateUpdateMode = false;
    public bool bFixedUpdateMode = true;
    public int iFPS = 60;
    public float fLerpPosition = 0.2f;
    public float fLerpRotation = 0.2f;

    public EAutoMotionCaptureDevicesSelecter eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eAuto;

    public SNeuronConnection[] sNeuronConnections;

    public GameObject prefabKinect1_DeviceJoints;
    public GameObject goKinect1_Attach;
    public Vector3 vecKinect1_Position = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 vecKinect1_Rotation = new Vector3(0.0f, 180.0f, 0.0f);
    public Vector3 vecKinect1_Scale = Vector3.one;

    public GameObject prefabKinect2_DeviceJoints;
    public GameObject goKinect2_Attach;
    public Vector3 vecKinect2_Position = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 vecKinect2_Rotation = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 vecKinect2_Scale = Vector3.one;

    public GameObject prefabIKinemaMale_DeviceJoints;
    public GameObject prefabIKinemaWoman_DeviceJoints;

    void Start()
    {
        // FBX Exporter for Unity (Sync Animation Custom Frame)
        if (sFBXExporterForUnity != null)
        {
            if (sFBXExporterForUnity.enabled)
            {
                sFBXExporterForUnity.bOutAnimation = false;
                sFBXExporterForUnity.bOutAnimationCustomFrame = true;
            }
        }
        else
        {
            if (GameObject.Find(strFBXExporterForUnity) != null)
            {
                sFBXExporterForUnity = GameObject.Find(strFBXExporterForUnity).GetComponent<FBXExporterForUnity>();
            }
        }

        switch (eAutoMotionCaptureDevicesSelecter)
        {
            case EAutoMotionCaptureDevicesSelecter.eAuto:
                {
                    // Perception Neuron(1-5)
                    int iSelect = 0;
                    foreach (SNeuronConnection sNeuronConnection in sNeuronConnections)
                    {
                        sNeuronConnections[iSelect].sNeuronSource = NeuronConnection.CreateConnection(sNeuronConnection.address, sNeuronConnection.port, sNeuronConnection.commandServerPort, sNeuronConnection.socketType);
                        if (sNeuronConnections[iSelect].sNeuronSource != null)
                        {
                            eAutoMotionCaptureDevicesSelecter = (EAutoMotionCaptureDevicesSelecter)(EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_1 + iSelect);
                            NeuronConnection.DestroyConnection(sNeuronConnections[iSelect].sNeuronSource);
                            bCheckedDevices = true;
                            if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [Perception Neuron_" + (iSelect + 1) + "]");
                            return;
                        }
                        iSelect++;
                    }

                    // Kinect1
                    int hr = NativeMethods.NuiInitialize(NuiInitializeFlags.UsesDepthAndPlayerIndex | NuiInitializeFlags.UsesSkeleton | NuiInitializeFlags.UsesColor);
                    if (hr == 0)
                    {
                        eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eKinect1;
                        NativeMethods.NuiShutdown();
                        bCheckedDevices = true;
                        if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [Kinect1]");
                        return;
                    }

                    // Kinect2
                    Kinect2.KinectSensor _Sensor = Kinect2.KinectSensor.GetDefault();
                    if (_Sensor != null)
                    {
                        //if (!_Sensor.IsOpen)
                        {
                            _Sensor.Open();
                            if (_Sensor.IsOpen)
                            {
                                _Sensor.IsAvailableChanged += (sender, evt) =>
                                {
                                    if (!bCheckedDevices)
                                    {
                                        if (_Sensor.IsAvailable)
                                        {
                                            eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eKinect2;
                                            bCheckedDevices = true;
                                            if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [Kinect2]");
                                            return;
                                        }
                                    }
                                };
                            }
                        }
                    }
                    break;
                }
            case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_1:
            case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_2:
            case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_3:
            case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_4:
            case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_5:
            case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_6:
            case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_7:
            case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_8:
                {
                    // Perception Neuron(1-5)
                    int iSelect = (int)eAutoMotionCaptureDevicesSelecter - (int)EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_1;
                    sNeuronConnections[iSelect].sNeuronSource = NeuronConnection.CreateConnection(sNeuronConnections[iSelect].address, sNeuronConnections[iSelect].port, sNeuronConnections[iSelect].commandServerPort, sNeuronConnections[iSelect].socketType);
                    if (sNeuronConnections[iSelect].sNeuronSource != null)
                    {
                        eAutoMotionCaptureDevicesSelecter = (EAutoMotionCaptureDevicesSelecter)(EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_1 + iSelect);
                        NeuronConnection.DestroyConnection(sNeuronConnections[iSelect].sNeuronSource);
                        bCheckedDevices = true;
                        if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [Perception Neuron_" + (iSelect + 1) + "]");
                        return;
                    }
                    break;
                }
            case EAutoMotionCaptureDevicesSelecter.eKinect1:
                {
                    // Kinect1
                    int hr = NativeMethods.NuiInitialize(NuiInitializeFlags.UsesDepthAndPlayerIndex | NuiInitializeFlags.UsesSkeleton | NuiInitializeFlags.UsesColor);
                    if (hr == 0)
                    {
                        eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eKinect1;
                        NativeMethods.NuiShutdown();
                        bCheckedDevices = true;
                        if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [Kinect1]");
                        return;
                    }
                    break;
                }
            case EAutoMotionCaptureDevicesSelecter.eKinect2:
                {
                    // Kinect2
                    Kinect2.KinectSensor _Sensor = Kinect2.KinectSensor.GetDefault();
                    if (_Sensor != null)
                    {
                        if (!_Sensor.IsOpen)
                        {
                            _Sensor.Open();
                            if (_Sensor.IsOpen)
                            {
                                _Sensor.IsAvailableChanged += (sender, evt) =>
                                {
                                    if (!bCheckedDevices)
                                    {
                                        if (_Sensor.IsAvailable)
                                        {
                                            eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eKinect2;
                                            bCheckedDevices = true;
                                            if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [Kinect2]");
                                            return;
                                        }
                                    }
                                };
                            }
                        }
                        else
                        {
                            eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eKinect2;
                            bCheckedDevices = true;
                            if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [Kinect2] -Sensor Opened-");
                            return;
                        }
                    }
                    break;
                }
            case EAutoMotionCaptureDevicesSelecter.eIKinemaOrion_Male:
                {
                    eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eIKinemaOrion_Male;
                    bCheckedDevices = true;
                    if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [IKinema Orion_Male]");
                    return;
                }
            case EAutoMotionCaptureDevicesSelecter.eIKinemaOrion_Woman:
                {
                    eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eIKinemaOrion_Woman;
                    bCheckedDevices = true;
                    if (bDebugLog) Debug.Log("Auto Devices Selecter Enabled [IKinema Orion_Woman]");
                    return;
                }
        }
        eAutoMotionCaptureDevicesSelecter = EAutoMotionCaptureDevicesSelecter.eNone;
    }

    void FixedUpdate()
    {
        if (!bInitDevice)
        {
            if (bCheckedDevices)
            {
                bInitDevice = true;
                switch (eAutoMotionCaptureDevicesSelecter)
                {
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_1:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_2:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_3:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_4:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_5:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_6:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_7:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_8:
                        int iDeviceSelect = eAutoMotionCaptureDevicesSelecter - EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_1;
                        int iSelect = 0;
                        foreach (SNeuronConnection sNeuronConnection in sNeuronConnections)
                        {
                            if (iDeviceSelect == iSelect)
                            {
                                GameObject goTopPerceptionNeuron = null;
                                if (sNeuronConnection.goAttach == null)
                                {
                                    goTopPerceptionNeuron = Instantiate(sNeuronConnection.prefabDeviceJoints, Vector3.zero, Quaternion.identity) as GameObject;
                                    sNeuronConnections[iSelect].goAttach = this.gameObject;
                                    goTopPerceptionNeuron.transform.parent = this.gameObject.transform;
                                    goTopPerceptionNeuron.transform.localPosition = sNeuronConnections[iSelect].goAttach.transform.localPosition;
                                    goTopPerceptionNeuron.transform.localRotation = sNeuronConnections[iSelect].goAttach.transform.localRotation;
                                    goTopPerceptionNeuron.transform.localScale = sNeuronConnections[iSelect].goAttach.transform.localScale;
                                }
                                else
                                {
                                    goTopPerceptionNeuron = Instantiate(sNeuronConnection.prefabDeviceJoints, Vector3.zero, Quaternion.identity) as GameObject;
                                    goTopPerceptionNeuron.transform.parent = sNeuronConnection.goAttach.transform;
                                    goTopPerceptionNeuron.transform.localPosition = sNeuronConnection.goAttach.transform.localPosition;
                                    goTopPerceptionNeuron.transform.localRotation = sNeuronConnection.goAttach.transform.localRotation;
                                    goTopPerceptionNeuron.transform.localScale = sNeuronConnection.goAttach.transform.localScale;
                                }
                                if (goTopPerceptionNeuron != null)
                                {
                                    NeuronAnimatorInstance sNeuronAnimatorInstance = goTopPerceptionNeuron.GetComponent<NeuronAnimatorInstance>();
                                    sNeuronAnimatorInstance.address = sNeuronConnection.address;
                                    sNeuronAnimatorInstance.port = sNeuronConnection.port;
                                    //sNeuronAnimatorInstance.commandServerPort = sNeuronConnection.commandServerPort;
                                    sNeuronAnimatorInstance.socketType = sNeuronConnection.socketType;
                                    sNeuronAnimatorInstance.actorID = sNeuronConnection.actorID;
                                    sNeuronAnimatorInstance.connectToAxis = true;
                                    sNeuronAnimatorInstance.bLateUpdateMode = bLateUpdateMode;
                                    sNeuronAnimatorInstance.bFixedUpdateMode = bFixedUpdateMode;
                                    sNeuronAnimatorInstance.iFPS = iFPS;
                                    sNeuronAnimatorInstance.fLerpPosition = fLerpPosition;
                                    sNeuronAnimatorInstance.fLerpRotation = fLerpRotation;
                                    sNeuronAnimatorInstance.enabled = true;
                                    if (bDisplayDeveiceJoints)
                                    {
                                        goTopPerceptionNeuron.FindDeepE("NeuronRobot_Mesh", true).SetActive(true);
                                    }
                                }
                            }
                            iSelect++;
                        }
                        break;
                    case EAutoMotionCaptureDevicesSelecter.eKinect1:
                        if (goKinect1_Attach == null)
                        {
                            GameObject goTopKinect1 = Instantiate(prefabKinect1_DeviceJoints, Vector3.zero, Quaternion.identity) as GameObject;
                            goKinect1_Attach = this.gameObject;
                            goTopKinect1.transform.parent = goKinect1_Attach.transform;
                            goTopKinect1.transform.localPosition = goKinect1_Attach.transform.localPosition;
                            goTopKinect1.transform.localRotation = goKinect1_Attach.transform.localRotation;
                            goTopKinect1.transform.localScale = goKinect1_Attach.transform.localScale;
                            Kinect1ModelControllerV2 sKinect1ModelControllerV2 = goTopKinect1.GetComponentInChildren<Kinect1ModelControllerV2>();
                            if (sKinect1ModelControllerV2 != null)
                            {
                                sKinect1ModelControllerV2.sAutoMotionCaptureDevicesSelecter = this;
                                sKinect1ModelControllerV2.sFBXExporterForUnity = sFBXExporterForUnity;
                                sKinect1ModelControllerV2.bFixedUpdateMode = bFixedUpdateMode;
                                sKinect1ModelControllerV2.iFPS = iFPS;
                                sKinect1ModelControllerV2.fLerpPosition = fLerpPosition;
                                sKinect1ModelControllerV2.fLerpRotation = fLerpRotation;
                                if (bDisplayDeveiceJoints)
                                {
                                    goTopKinect1.FindDeepE("NeuronRobot_Mesh", true).SetActive(true);
                                }
                            }
                        }
                        else
                        {
                            GameObject goTopKinect1 = Instantiate(prefabKinect1_DeviceJoints, Vector3.zero, Quaternion.identity) as GameObject;
                            goTopKinect1.transform.parent = goKinect1_Attach.transform;
                            goTopKinect1.transform.localPosition = goKinect1_Attach.transform.localPosition;
                            goTopKinect1.transform.localRotation = goKinect1_Attach.transform.localRotation;
                            goTopKinect1.transform.localScale = goKinect1_Attach.transform.localScale;
                            Kinect1ModelControllerV2 sKinect1ModelControllerV2 = goTopKinect1.GetComponentInChildren<Kinect1ModelControllerV2>();
                            if (sKinect1ModelControllerV2 != null)
                            {
                                sKinect1ModelControllerV2.sAutoMotionCaptureDevicesSelecter = this;
                                sKinect1ModelControllerV2.sFBXExporterForUnity = sFBXExporterForUnity;
                                sKinect1ModelControllerV2.bFixedUpdateMode = bFixedUpdateMode;
                                sKinect1ModelControllerV2.iFPS = iFPS;
                                sKinect1ModelControllerV2.fLerpPosition = fLerpPosition;
                                sKinect1ModelControllerV2.fLerpRotation = fLerpRotation;
                                if (bDisplayDeveiceJoints)
                                {
                                    goTopKinect1.FindDeepE("NeuronRobot_Mesh", true).SetActive(true);
                                }
                            }
                        }
                        break;
                    case EAutoMotionCaptureDevicesSelecter.eKinect2:
                        if (goKinect2_Attach == null)
                        {
                            GameObject goTopKinect2 = Instantiate(prefabKinect2_DeviceJoints, Vector3.zero, Quaternion.identity) as GameObject;
                            goKinect2_Attach = this.gameObject;
                            goTopKinect2.transform.parent = goKinect2_Attach.transform;
                            goTopKinect2.transform.localPosition = goKinect2_Attach.transform.localPosition;
                            goTopKinect2.transform.localRotation = goKinect2_Attach.transform.localRotation;
                            goTopKinect2.transform.localScale = goKinect2_Attach.transform.localScale;
                            Kinect2ModelControllerV2 sKinect2ModelControllerV2 = goTopKinect2.GetComponentInChildren<Kinect2ModelControllerV2>();
                            if (sKinect2ModelControllerV2 != null)
                            {
                                sKinect2ModelControllerV2.sAutoMotionCaptureDevicesSelecter = this;
                                sKinect2ModelControllerV2.sFBXExporterForUnity = sFBXExporterForUnity;
                                sKinect2ModelControllerV2.bFixedUpdateMode = bFixedUpdateMode;
                                sKinect2ModelControllerV2.iFPS = iFPS;
                                sKinect2ModelControllerV2.fLerpPosition = fLerpPosition;
                                sKinect2ModelControllerV2.fLerpRotation = fLerpRotation;
                                if (bDisplayDeveiceJoints)
                                {
                                    goTopKinect2.FindDeepE("NeuronRobot_Mesh", true).SetActive(true);
                                }
                            }
                        }
                        else
                        {
                            GameObject goTopKinect2 = Instantiate(prefabKinect2_DeviceJoints, Vector3.zero, Quaternion.identity) as GameObject;
                            goTopKinect2.transform.parent = goKinect2_Attach.transform;
                            goTopKinect2.transform.localPosition = goKinect2_Attach.transform.localPosition;
                            goTopKinect2.transform.localRotation = goKinect2_Attach.transform.localRotation;
                            goTopKinect2.transform.localScale = goKinect2_Attach.transform.localScale;
                            Kinect2ModelControllerV2 sKinect2ModelControllerV2 = goTopKinect2.GetComponentInChildren<Kinect2ModelControllerV2>();
                            if (sKinect2ModelControllerV2 != null)
                            {
                                sKinect2ModelControllerV2.sAutoMotionCaptureDevicesSelecter = this;
                                sKinect2ModelControllerV2.sFBXExporterForUnity = sFBXExporterForUnity;
                                sKinect2ModelControllerV2.bFixedUpdateMode = bFixedUpdateMode;
                                sKinect2ModelControllerV2.iFPS = iFPS;
                                sKinect2ModelControllerV2.fLerpPosition = fLerpPosition;
                                sKinect2ModelControllerV2.fLerpRotation = fLerpRotation;
                                if (bDisplayDeveiceJoints)
                                {
                                    goTopKinect2.FindDeepE("NeuronRobot_Mesh", true).SetActive(true);
                                }
                            }
                        }
                        break;
                    case EAutoMotionCaptureDevicesSelecter.eIKinemaOrion_Male:
                        {
                            GameObject goTopIKinema = null;
                            goTopIKinema = Instantiate(prefabIKinemaMale_DeviceJoints, Vector3.zero, Quaternion.identity) as GameObject;
                            goTopIKinema.transform.parent = this.gameObject.transform;
                        }
                        break;
                    case EAutoMotionCaptureDevicesSelecter.eIKinemaOrion_Woman:
                        {
                            GameObject goTopIKinema = null;
                            goTopIKinema = Instantiate(prefabIKinemaWoman_DeviceJoints, Vector3.zero, Quaternion.identity) as GameObject;
                            goTopIKinema.transform.parent = this.gameObject.transform;
                        }
                        break;
                }
            }
        }
        else
        {
            if (!bInitTransform)
            {
                bInitTransform = true;
                switch (eAutoMotionCaptureDevicesSelecter)
                {
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_1:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_2:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_3:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_4:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_5:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_6:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_7:
                    case EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_8:
                        int iDeviceSelect = eAutoMotionCaptureDevicesSelecter - EAutoMotionCaptureDevicesSelecter.ePerceptionNeuron_1;
                        int iSelect = 0;
                        foreach (SNeuronConnection sNeuronConnection in sNeuronConnections)
                        {
                            if (iDeviceSelect == iSelect)
                            {
                                if (sNeuronConnection.goAttach != null)
                                {
                                    sNeuronConnection.goAttach.transform.localPosition = sNeuronConnection.vecPosition;
                                    sNeuronConnection.goAttach.transform.localRotation = Quaternion.Euler(sNeuronConnection.vecRotation);
                                    sNeuronConnection.goAttach.transform.localScale = sNeuronConnection.vecScale;
                                }
                            }
                            iSelect++;
                        }
                        //iInitReadyFrame = 120;
                        bInitMoveTransform = true;
                        break;
                    case EAutoMotionCaptureDevicesSelecter.eKinect1:
                        if (goKinect1_Attach != null)
                        {
                            goKinect1_Attach.transform.localPosition = vecKinect1_Position;
                            goKinect1_Attach.transform.localRotation = Quaternion.Euler(vecKinect1_Rotation);
                            goKinect1_Attach.transform.localScale = vecKinect1_Scale;
                        }
                        //iInitReadyFrame = 120;
                        //bInitMoveTransform = true;
                        break;
                    case EAutoMotionCaptureDevicesSelecter.eKinect2:
                        if (goKinect2_Attach != null)
                        {
                            goKinect2_Attach.transform.localPosition = vecKinect2_Position;
                            goKinect2_Attach.transform.localRotation = Quaternion.Euler(vecKinect2_Rotation);
                            goKinect2_Attach.transform.localScale = vecKinect2_Scale;
                        }
                        //iInitReadyFrame = 120;
                        //bInitMoveTransform = true;
                        break;
                }
            }
            else
            {
                if (!bInitEnableAnimator)
                {
                    if (bInitMoveTransform)
                    {
                        bInitEnableAnimator = true;
                        foreach (GameObject goLiveAnimator in goLiveAnimators)
                        {
                            HumanPoseRetargetLiveAnimator sHumanPoseRetargetLiveAnimator = goLiveAnimator.GetComponent<HumanPoseRetargetLiveAnimator>();
                            if (sHumanPoseRetargetLiveAnimator != null)
                            {
                                sHumanPoseRetargetLiveAnimator.bFixedUpdateMode = bFixedUpdateMode;
                                sHumanPoseRetargetLiveAnimator.iFPS = iFPS;
                                sHumanPoseRetargetLiveAnimator.fLerpPosition = fLerpPosition;
                                sHumanPoseRetargetLiveAnimator.fLerpRotation = fLerpRotation;
                            }
                            HumanBodyBonesLiveAnimator HumanBodyBonesLiveAnimators= goLiveAnimator.GetComponent<HumanBodyBonesLiveAnimator>();
                            if (HumanBodyBonesLiveAnimators != null)
                            {
                                HumanBodyBonesLiveAnimators.bFixedUpdateMode = bFixedUpdateMode;
                                HumanBodyBonesLiveAnimators.iFPS = iFPS;
                                HumanBodyBonesLiveAnimators.fLerpPosition = fLerpPosition;
                                HumanBodyBonesLiveAnimators.fLerpRotation = fLerpRotation;
                            }
                            HumanoidAvatarLiveAnimator sHumanoidAvatarLiveAnimator = goLiveAnimator.GetComponent<HumanoidAvatarLiveAnimator>();
                            if (sHumanoidAvatarLiveAnimator != null)
                            {
                                sHumanoidAvatarLiveAnimator.bFixedUpdateMode = bFixedUpdateMode;
                                sHumanoidAvatarLiveAnimator.iFPS = iFPS;
                                sHumanoidAvatarLiveAnimator.fLerpPosition = fLerpPosition;
                                sHumanoidAvatarLiveAnimator.fLerpRotation = fLerpRotation;
                                sHumanoidAvatarLiveAnimator.bInitedEnableAnimator = true;
                            }
                        }
                    }
                }
                else
                {
                    if (iInitReadyFrame >= 0)
                    {
                        iInitReadyFrame--;
                        foreach (GameObject goLiveAnimator in goLiveAnimators)
                        {
                            if (goLiveAnimator != null)
                            {
                                if (goLiveAnimator.activeInHierarchy)
                                {
                                    HumanPoseRetargetLiveAnimator sHumanPoseRetargetLiveAnimator = goLiveAnimator.GetComponent<HumanPoseRetargetLiveAnimator>();
                                    if (sHumanPoseRetargetLiveAnimator != null)
                                    {
                                        if (iInitReadyFrame <= 0)
                                        {
                                            sHumanPoseRetargetLiveAnimator.bInitReady = true;
                                        }
                                    }
                                    HumanoidAvatarLiveAnimator sHumanoidAvatarLiveAnimator = goLiveAnimator.GetComponent<HumanoidAvatarLiveAnimator>();
                                    if (sHumanoidAvatarLiveAnimator != null)
                                    {
                                        if (iInitReadyFrame <= 0)
                                        {
                                            sHumanoidAvatarLiveAnimator.bInitReady = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!bAllInited)
                    {
                        bool bAllInitedCheck = true;
                        foreach (GameObject goLiveAnimator in goLiveAnimators)
                        {
                            if (goLiveAnimator != null)
                            {
                                if (goLiveAnimator.activeInHierarchy)
                                {
                                    HumanPoseRetargetLiveAnimator sHumanPoseRetargetLiveAnimator = goLiveAnimator.GetComponent<HumanPoseRetargetLiveAnimator>();
                                    if (sHumanPoseRetargetLiveAnimator != null)
                                    {
                                        if (sHumanPoseRetargetLiveAnimator.enabled)
                                        {
                                            if (!sHumanPoseRetargetLiveAnimator.bInited)
                                            {
                                                bAllInitedCheck = false;
                                            }
                                        }
                                    }
                                    HumanoidAvatarLiveAnimator sHumanoidAvatarLiveAnimator = goLiveAnimator.GetComponent<HumanoidAvatarLiveAnimator>();
                                    if (sHumanoidAvatarLiveAnimator != null)
                                    {
                                        if (sHumanoidAvatarLiveAnimator.enabled)
                                        {
                                            if (!sHumanoidAvatarLiveAnimator.bInited)
                                            {
                                                bAllInitedCheck = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (bAllInitedCheck)
                        {
                            bAllInited = true;
                            if (sFBXExporterForUnity != null)
                            {
                                sFBXExporterForUnity.bOutAnimation = true;
                            }
                        }
                    }
                }
            }
        }
    }
    private void OnApplicationQuit()
    {
        if (bDebugLog) Debug.Log("AutoMotionCaptureDevicesSelecter.OnApplicationQuit");
        // Perception Neuron(1-5)
        int i = 0;
        foreach (SNeuronConnection sNeuronConnection in sNeuronConnections)
        {
            if (sNeuronConnection.sNeuronSource != null)
            {
                NeuronConnection.DestroyConnection(sNeuronConnection.sNeuronSource);
                if (bDebugLog) Debug.Log("[Perception Neuron_" + (i + 1) + "] DestroyConnection");
            }
            i++;
        }
        // Kinect1
        NativeMethods.NuiShutdown();
        if (bDebugLog) Debug.Log("[Kinect1] KinectSensor Shutdown");
        // Kinect2
        Kinect2.KinectSensor _Sensor = Kinect2.KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
                if (bDebugLog) Debug.Log("[Kinect2] KinectSensor Close");
            }
            else
            {
                if (bDebugLog) Debug.Log("[Kinect2] KinectSensor Closed");
            }
        }
    }
}
