/*
 * FBXImporterForUnity.cs
 *
 *  	Developed by ほえたん(Hoetan) -- 2018/10/15
 *  	Copyright (c) 2015-2017, ACTINIA Software. All rights reserved.
 * 		Homepage: http://actinia-software.com
 * 		E-Mail: hoetan@actinia-software.com
 * 		Twitter: https://twitter.com/hoetan3
 * 		GitHub: https://github.com/hoetan
 */

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WWWDownloaderImporter : MonoBehaviour
{
    public bool bWWWDownloadEnd = false;
    public int iWWWDownloadCount = 0;

    public Texture2D texture = null;

    public IEnumerator WWWDownloadToLoadFileTexture(string strURLFileName, bool bUnloadUnusedAssets, System.Action callback)
    {
        bWWWDownloadEnd = false;
        iWWWDownloadCount++;

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(strURLFileName))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                texture = DownloadHandlerTexture.GetContent(uwr);
            }

            uwr.Dispose();
        }

        if (bUnloadUnusedAssets)
        {
            Resources.UnloadUnusedAssets();
        }

        iWWWDownloadCount--;
        if (iWWWDownloadCount <= 0)
        {
            bWWWDownloadEnd = true;
        }

        callback();
    }
}

public class FBXImporterForUnity : MonoBehaviour
{
    public struct FBXSceneInfo
    {
        public IntPtr strApplicationName;
    };

    public struct FBXImportInfo
    {
        public IntPtr strCreator;
        public int iVersion;
    };

    public struct FBXNodeName {
        public IntPtr strParentNodeNames;
        public IntPtr strNodeNames;
    };

    public struct FBXNodeTRS
    {
        public int iFrameStart;
        public int iFrmeEnd;
        public int iCount;
        public IntPtr fLclTranslation;
        public IntPtr fLclRotation;
        public IntPtr fLclScale;
    };

    public struct FBXMesh {
        public int numVertexs;
        public IntPtr fVertexs;
        public int numNormals;
        public IntPtr fNormals;
        public int numUVs;
        public IntPtr fUVs;
        public int numPolygonIndex;
        public IntPtr iPolygonIndexs;
        public int numPolygonMatID;
        public IntPtr iPolygonMatIDs;
        public int numMatID;
    };

    public struct FBXWeight {
        public int iBoneWeightCount;
        public IntPtr iBoneID;
        public IntPtr fBoneWeight;
        public int iBoneNodeNamesCount;
        public IntPtr strBoneNodeNames;
        //public IntPtr matBonePose;
        public IntPtr fGlbTranslation;
        public IntPtr fGlbRotation;
        public IntPtr fGlbScale;
    };


    struct FBXMaterial
    {
        public IntPtr strMaterialName;
        public IntPtr strShadingModelName;
        public IntPtr fAmbient;
        public IntPtr fAmbientFactor;
        public IntPtr fDiffuse;
        public IntPtr fDiffuseFactor;
        public IntPtr fEmissive;
        public IntPtr fEmissiveFactor;
        public IntPtr fBump;
        public IntPtr fBumpFactor;
        public IntPtr fNormalMap;
        public IntPtr fSpecular;
        public IntPtr fSpecularFactor;
        public IntPtr fShininess;
        public IntPtr fReflection;
        public IntPtr fReflectionFactor;
        public IntPtr fTransparentColor;
        public IntPtr fTransparencyFactor;
        public IntPtr fDisplacementColor;
        public IntPtr fDisplacementFactor;
        public IntPtr fVectorDisplacementColor;
        public IntPtr fVectorDisplacementFactor;
        public IntPtr strTexturAmbientName;
        public IntPtr strTextureDiffuseName;
        public IntPtr strTextureEmissiveName;
        public IntPtr strTextureBumpName;
        public IntPtr strTextureNormalMapName;
        public IntPtr strTextureDisplacementName;
        public IntPtr strTextureTransparentName;
        public IntPtr strTextureReflectionName;
        public IntPtr strCgfxTextureTypes;
        public IntPtr strCgfxTextureNames;
    };

    [DllImport("FBXImporterForUnity")]
    public static extern int FBXImporterInit();
    [DllImport("FBXImporterForUnity")]
    public static extern int FBXImporterExit();
    [DllImport("FBXImporterForUnity")]
    public static extern IntPtr FBXImporterLoad(string pcName);
    [DllImport("FBXImporterForUnity")]
    public static extern IntPtr FBXImporterAnimationLoad(string pcName);
    [DllImport("FBXImporterForUnity")]
    public static extern void FBXImporterGeometryConverter(bool bTriangulate);
    [DllImport("FBXImporterForUnity")]
    public static extern void FBXImporterAxisSystem(int iUpVector, int iFrontVector, int iCoordSystem);
    [DllImport("FBXImporterForUnity")]
    public static extern void FBXImporterSystemUnit(int iSystemUnit);
    [DllImport("FBXImporterForUnity")]
    public static extern IntPtr FBXImporterGetNodes();
    [DllImport("FBXImporterForUnity")]
    public static extern IntPtr FBXImporterGetNodeTRS(string pcNodeName);
    [DllImport("FBXImporterForUnity")]
    public static extern IntPtr FBXImporterGetNodeTRSAnimation(string pcNodeName, int iAnimationClip, int iTimeMode);
    [DllImport("FBXImporterForUnity")]
    public static extern IntPtr FBXImporterGetMesh(string pcNodeName, bool bDirectControlPointsMode, bool bNormalSmoothing);
    [DllImport("FBXImporterForUnity")]
    public static extern IntPtr FBXImporterGetWeight(string pcNodeName, int[] piPolygonIndexs, int iPolygonIndexCount, bool bDirectControlPointsMode);
    [DllImport("FBXImporterForUnity")]
    public static extern int FBXImporterGetMaterialCount(string pNodeName);
    [DllImport("FBXImporterForUnity")]
    public static extern IntPtr FBXImporterGetMaterial(string pNodeName, int i, bool bInputTextureFullPath);

    private WWWDownloaderImporter sWWWDownloaderImporter;

    public enum EFBXImportToolTarget
    {
        eAuto,
        eCustom,
        eFBXExporterForUnity,
        e3dsMax,
        eMaya,
        eSoftimage,
        eMotionBuilder,
    };

    public enum EFBXUpVector
    {
        eInit = -1,
        eDefault = 0,
        eXAxis = 1,
        eYAxis = 2,
        eZAxis = 3,
    };

    public enum EFBXFrontVector
    {
        eInit = -1,
        eDefault = 0,
        eParityEven = 1,
        eParityOdd = 2,
    };

    public enum EFBXCoordSystem
    {
        eInit = -1,
        eDefault = 0,
        eLeftHanded = 1,
        eRightHanded = 2,
    };

    public enum EFBXSystemUnit
    {
        eInit = -1,
        eDefault = 0,
        ecm = 1,
        edm = 2,
        eFoot = 3,
        eInch = 4,
        ekm = 5,
        em = 6,
        eMile = 7,
        emm = 8,
        eYard = 9,
    };

    public enum EFBXImportTextrureMode
    {
        eWWWLocalEmbedMedia,
        eWWWLocalProject,
        eWWWLocalStreamingAssets,
        eWWWLocalFullPath,
        eWWWURL,
        eResourceLoad,
        eEditorLocalAssets,
        eEditorAllAssets,
    };

    private bool bTrialMode = true;

    public bool bDebugLog = true;
    public GameObject goTopAttach = null;
    private GameObject goTopAttachNode = null;
    public string strFilename = "Unity.fbx";
    public bool bInMesh = true;
    public bool bInShader = true;
    public bool bInComponent = true;
    public string strImportApplicationName = "";
    public EFBXImportToolTarget eImportToolTarget = EFBXImportToolTarget.eAuto;
    public EFBXUpVector eUpVector = EFBXUpVector.eInit;
    public EFBXFrontVector eFrontVector = EFBXFrontVector.eInit;
    public EFBXCoordSystem eCoordSystem = EFBXCoordSystem.eInit;
    public EFBXSystemUnit eSystemUnit = EFBXSystemUnit.eInit;

    public bool bInDirectControlPointsMode = false;
    public bool bInNormalSmoothing = false;
    public float fGlobalScale = 0.01f;
    public bool bInRootNode = false;
    public string strInRootNodeName = "RootNode";

    public EFBXImportTextrureMode eImportTextureMode = EFBXImportTextrureMode.eWWWLocalEmbedMedia;
    public bool bInputTextureFullPath = true;
    public string strWWWLocalEmbedMediaTexturePath = "";
    public string strWWWLocalProjectTexturePath = "OutputTextures/";
    public string strWWWLocalStreamingAssetsTexturePath = "OutputTextures/";
    public string strWWWLocalFullPathTexturePath = "file://";
    public string strWWWURLTexturePath = "http://";
    public string strResourceLoad = "OutputTextures/";
    public string strEditorLocalAssetsTexturePath = "Assets/OutputTextures/";
    public bool bWWWUnloadUnusedAssets = true;

    public string strShaderTextureAmbientParameter = "";
    public string strShaderTextureDiffuseParameter = "";
    public string strShaderTextureEmissiveParameter = "";
    public string strShaderTextureBumpParameter = "";
    public string strShaderTextureNormalMapParameter = "";
    public string strShaderTextureDisplacementParameter = "";
    public string strShaderTextureTransparentParameter = "";
    public string strShaderTextureReflectionParameter = "";

    public bool bInAnimation = true;
    public bool bAutoAnimationPlay = true;
    public int iAutoAnimationPlayNo = 0;
    public Animation animationAttach;
    public int iDefaultFrameRate = 30;

    private Transform trnsTopAttach;

    private StreamReader sr_shader;

    public struct SShaderMaterial
    {
        public string strMaterailName;
        public string strShaderName;
        public List<string> strShaderParameters;
    };

    public struct SShaderMaterialBase
    {
        public int iShaderMaterialCount;
        public SShaderMaterial[] sShaderMaterials;
    }

    private Dictionary<string, SShaderMaterialBase> dicShaderMaterialBases = new Dictionary<string, SShaderMaterialBase>();

    public enum EFBXTimeMode
    {
        eDefaultMode,
        eFrames120,
        eFrames100,
        eFrames60,
        eFrames50,
        eFrames48,
        eFrames30,
        eFrames30Drop,
        eNTSCDropFrame,
        eNTSCFullFrame,
        ePAL,
        eFrames24,
        eFrames1000,
        eFilmFullFrame,
        eCustom,
        eFrames96,
        eFrames72,
        eFrames59dot94,
    };

    [Serializable]
    public struct SFBXSceneInfo
    {
        public string strApplicationName;
    };

    [Serializable]
    public struct SFBXImportInfo
    {
        public string strCreator;
        public int iVersion;
    };

    [Serializable]
    public struct SAnimationClips
    {
        public string strFilename;
        public int iFrameRate;
        public EFBXTimeMode eTimeMode;
        public WrapMode eWarapMode;
        public string strClipName;
        public AnimationClip animationClip;
        public SFBXImportInfo sImportInfo;
    }

    public SAnimationClips[] listSAnimationClips;
    private int iAnimationClipsCount = 0;

    void Start()
    {
        // Init
        if (bDebugLog) Debug.Log("FBXImporterInit()");
        FBXImporterInit();

        // WWW Downloader
        sWWWDownloaderImporter = gameObject.AddComponent<WWWDownloaderImporter>();

        // In Animation
        if (bInAnimation)
        {
            // New
            if (listSAnimationClips.Length == 0)
            {
                listSAnimationClips = new SAnimationClips[1];
                listSAnimationClips[0].strFilename = "";
                listSAnimationClips[0].strClipName = "";
            }
        }

        // Importer Load
        if (bDebugLog) Debug.Log("FBXImporterLoad()");
        IntPtr pFBXSceneInfo = FBXImporterLoad(strFilename);

        // FBX Scene Info
        if (pFBXSceneInfo != IntPtr.Zero)
        {
            FBXSceneInfo sFBXSceneInfo = new FBXSceneInfo();
            IntPtr pFBXImportInfoT = Marshal.AllocHGlobal(Marshal.SizeOf(sFBXSceneInfo));
            try
            {
                sFBXSceneInfo = (FBXSceneInfo)Marshal.PtrToStructure(pFBXSceneInfo, typeof(FBXSceneInfo));
            }
            finally
            {
                Marshal.FreeHGlobal(pFBXImportInfoT);
            }
            strImportApplicationName = Marshal.PtrToStringAnsi(sFBXSceneInfo.strApplicationName).ToString();
        }

        // Imoprt Tool Target (Auto)
        switch (eImportToolTarget)
        {
            case EFBXImportToolTarget.eAuto:
                switch (strImportApplicationName)
                {
                    case "":
                        eImportToolTarget = EFBXImportToolTarget.eCustom;
                        break;
                    case "FBXExporterForUnity":
                        eImportToolTarget = EFBXImportToolTarget.eFBXExporterForUnity;
                        break;
                    case "3ds Max":
                        eImportToolTarget = EFBXImportToolTarget.e3dsMax;
                        break;
                    case "Maya":
                        eImportToolTarget = EFBXImportToolTarget.eMaya;
                        break;
                    case "Softimage":
                        eImportToolTarget = EFBXImportToolTarget.eSoftimage;
                        break;
                    case "MotionBuilder":
                        eImportToolTarget = EFBXImportToolTarget.eMotionBuilder;
                        break;
                }
                break;
        }

        // Imoprt Tool Target
        switch (eImportToolTarget)
        {
            case EFBXImportToolTarget.eCustom:
                break;
            case EFBXImportToolTarget.eFBXExporterForUnity:
                bInDirectControlPointsMode = true;
                break;
            case EFBXImportToolTarget.e3dsMax:
                bInDirectControlPointsMode = false;
                eUpVector = EFBXUpVector.eYAxis;
                eSystemUnit = EFBXSystemUnit.eDefault;
                break;
            case EFBXImportToolTarget.eMaya:
                bInDirectControlPointsMode = false;
                break;
            case EFBXImportToolTarget.eSoftimage:
                bInDirectControlPointsMode = false;
                break;
            case EFBXImportToolTarget.eMotionBuilder:
                bInDirectControlPointsMode = false;
                break;
        }

        // Converter
        FBXImporterGeometryConverter(true);
        FBXImporterAxisSystem((int)eUpVector, (int)eFrontVector, (int)eCoordSystem);
        FBXImporterSystemUnit((int)eSystemUnit);

        // In Shader
        if (bInMesh)
        {
            if (bInShader)
            {
                FileInfo fi = new FileInfo(strFilename + ".shader.txt");
                if (fi.Exists)
                {
                    sr_shader = fi.OpenText();
                    // Dictionary ShaderMaterialBases
                    dicShaderMaterialBases.Clear();
                    // Shader Count
                    string strLine = sr_shader.ReadLine();
                    string[] strLines_ShaderCount = strLine.Split(',');
                    int iCount = int.Parse(strLines_ShaderCount[1]);
                    // ShaderMaterialBases
                    SShaderMaterialBase sShaderMaterialBase = new SShaderMaterialBase();
                    for (int i = 0; i < iCount; i++)
                    {
                        // Model Name
                        string strLine_ModelName = sr_shader.ReadLine();
                        // {
                        strLine = sr_shader.ReadLine();
                        // Material Count
                        strLine = sr_shader.ReadLine();
                        string[] strLines_MaterialCount = strLine.Split(',');
                        int iMaterialCoiunt = int.Parse(strLines_MaterialCount[1]);
                        sShaderMaterialBase.iShaderMaterialCount = iMaterialCoiunt;
                        sShaderMaterialBase.sShaderMaterials = new SShaderMaterial[iMaterialCoiunt];
                        for (int j = 0; j < iMaterialCoiunt; j++)
                        {
                            // Material Name
                            strLine = sr_shader.ReadLine();
                            string[] strLines_MaterialName = strLine.Split(',');
                            sShaderMaterialBase.sShaderMaterials[j].strMaterailName = strLines_MaterialName[1];
                            // Shader Name
                            strLine = sr_shader.ReadLine();
                            string[] strLines_ShaderName = strLine.Split(',');
                            sShaderMaterialBase.sShaderMaterials[j].strShaderName = strLines_ShaderName[1];
                            // {
                            strLine = sr_shader.ReadLine();
                            // Shader Parameters
                            sShaderMaterialBase.sShaderMaterials[j].strShaderParameters = new List<string>();
                            do
                            {
                                strLine = sr_shader.ReadLine();
                                string strShaderParameter = strLine.Replace("\t", "");
                                sShaderMaterialBase.sShaderMaterials[j].strShaderParameters.Add(strShaderParameter);
                            }
                            while (strLine.IndexOf('}') < 0); // }
                        }
                        // }
                        strLine = sr_shader.ReadLine();
                        dicShaderMaterialBases.Add(strLine_ModelName, sShaderMaterialBase);
                    }
                }
            }
        }

        // Nodes
        IntPtr pFBXNodes = FBXImporterGetNodes();
        FBXNodeName sFBXNodeName = new FBXNodeName();
        IntPtr pFBXNodeNameT = Marshal.AllocHGlobal(Marshal.SizeOf(sFBXNodeName));
        try {
            sFBXNodeName = (FBXNodeName)Marshal.PtrToStructure(pFBXNodes, typeof(FBXNodeName));
        }
        finally {
            Marshal.FreeHGlobal(pFBXNodeNameT);
        }

        // Node Names
        string[] strParentNodeNames = Marshal.PtrToStringAnsi(sFBXNodeName.strParentNodeNames).ToString().Replace("RootNode", "").Split(',');
        string[] strNodeNames = Marshal.PtrToStringAnsi(sFBXNodeName.strNodeNames).ToString().Split(',');

        goTopAttachNode = goTopAttach;
        if (goTopAttachNode == null)
        {
            goTopAttachNode = gameObject;
        }

        // TopAttachNode Move (Root)
        Vector3 vecTopAttachPos = goTopAttachNode.transform.localPosition;
        Quaternion qTopAttachRot = goTopAttachNode.transform.localRotation;
        Vector3 vecTopAttachScl = goTopAttachNode.transform.localScale;
        goTopAttachNode.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        goTopAttachNode.transform.localRotation = Quaternion.identity;
        goTopAttachNode.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        for (int i = 0; i < strNodeNames.Length; i++)
        {
            GameObject goNode = null;
            GameObject goParentNode = null;
            if (strNodeNames[i] == "")
            {
                if (bInRootNode)
                {
                    goNode = new GameObject(strInRootNodeName);
                    goNode.transform.parent = goTopAttachNode.transform;
                }
                else
                {
                    goNode = goTopAttachNode;
                }
            }
            if (goNode == null)
            {
                goNode = new GameObject(strNodeNames[i]);
            }
            if (strParentNodeNames[i] == "")
            {
                if (bInRootNode)
                {
                    goParentNode = goTopAttachNode.transform.FindDeepI(strInRootNodeName).gameObject;
                }
                else
                {
                    goParentNode = goTopAttachNode;
                }
            }
            if (goParentNode == null)
            {
                goParentNode = goTopAttachNode.transform.FindDeepI(strParentNodeNames[i]).gameObject;
            }
            if (goParentNode != null)
            {
                goNode.transform.parent = goParentNode.transform;
            }
            // Import TRS
            ImportTRS(goNode.name, goNode, goTopAttachNode);
        }
        for (int i = 0; i < strNodeNames.Length; i++)
        {
            GameObject goNode;
            if (strNodeNames[i] == "")
            {
                if (bInRootNode)
                {
                    goNode = goTopAttachNode.transform.FindDeepI(strInRootNodeName).gameObject;
                }
                else
                {
                    goNode = goTopAttachNode;
                }
            }
            else
            {
                goNode = goTopAttachNode.transform.FindDeepI(strNodeNames[i]).gameObject;
            }
            // Import Set Mesh
            if (goNode.name == strInRootNodeName)
            {
                /*
                if (eImportToolTarget == EFBXImportToolTarget.e3dsMax)
                {
                    goNode.transform.localRotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
                }
                */
            }
            else
            {
                if (bInMesh)
                {
                    ImportSetMesh(goNode.name, goNode, goTopAttachNode, bInDirectControlPointsMode);
                }
            }
        }
        if (bInAnimation)
        {
            // In Animation
            iAnimationClipsCount = 0;
            foreach (SAnimationClips listSAnimationClip in listSAnimationClips)
            {
                // Animataion Load
                if (bDebugLog) Debug.Log("FBXImporterAnimationLoad()");
                if (listSAnimationClip.strFilename == "")
                {
                    listSAnimationClips[iAnimationClipsCount].strFilename = strFilename;
                }
                if (listSAnimationClip.strClipName == "")
                {
                    listSAnimationClips[iAnimationClipsCount].strClipName = listSAnimationClips[iAnimationClipsCount].strFilename;
                }
                IntPtr pFBXImportInfo = FBXImporterAnimationLoad(listSAnimationClips[iAnimationClipsCount].strFilename);
                if (pFBXImportInfo != IntPtr.Zero)
                {
                    // FBX Import Info
                    FBXImportInfo sFBXImportInfo = new FBXImportInfo();
                    IntPtr pFBXImportInfoT = Marshal.AllocHGlobal(Marshal.SizeOf(sFBXImportInfo));
                    try
                    {
                        sFBXImportInfo = (FBXImportInfo)Marshal.PtrToStructure(pFBXImportInfo, typeof(FBXImportInfo));
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pFBXImportInfoT);
                    }
                    listSAnimationClips[iAnimationClipsCount].sImportInfo.strCreator = Marshal.PtrToStringAnsi(sFBXImportInfo.strCreator).ToString();
                    listSAnimationClips[iAnimationClipsCount].sImportInfo.iVersion = sFBXImportInfo.iVersion;
                    // Converter
                    FBXImporterAxisSystem((int)eUpVector, (int)eFrontVector, (int)eCoordSystem);
                    FBXImporterSystemUnit((int)eSystemUnit);
                    for (int i = 0; i < strNodeNames.Length; i++)
                    {
                        GameObject goNode;
                        if (strNodeNames[i] == "")
                        {
                            if (bInRootNode)
                            {
                                goNode = goTopAttachNode.transform.FindDeepI(strInRootNodeName).gameObject;
                            }
                            else
                            {
                                goNode = goTopAttachNode;
                            }
                        }
                        else
                        {
                            goNode = goTopAttachNode.transform.FindDeepI(strNodeNames[i]).gameObject;
                        }
                        // Import TRS Animation
                        if (goNode.name == strInRootNodeName)
                        {
                            /*
                            if (eImportToolTarget == EFBXImportToolTarget.e3dsMax)
                            {
                                goNode.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                            }
                            */
                        }
                        else
                        {
                            ImportTRSAnimation(iAnimationClipsCount, listSAnimationClip.iFrameRate, (int)listSAnimationClip.eTimeMode, listSAnimationClip.eWarapMode, goNode.name, goNode, goTopAttachNode);
                        }
                    }

                    // Add Animation
                    Animation anim = animationAttach;
                    if (anim == null)
                    {
                        anim = goTopAttachNode.GetComponentInParent<Animation>();
                        if (anim == null)
                        {
                            anim = goTopAttachNode.AddComponent<Animation>();
                        }
                    }

                    // Add Animation Clips
                    if (anim != null)
                    {
                        if (listSAnimationClips[iAnimationClipsCount].animationClip != null && listSAnimationClips[iAnimationClipsCount].strClipName != "")
                        {
                            anim.AddClip(listSAnimationClips[iAnimationClipsCount].animationClip, listSAnimationClips[iAnimationClipsCount].strClipName);
                            if (anim.clip == null)
                            {
                                anim.clip = listSAnimationClips[iAnimationClipsCount].animationClip;
                            }
                        }
                    }
                }
                iAnimationClipsCount++;
            }
        }

        // TopAttachNode Move (Retrun)
        goTopAttachNode.transform.localPosition = vecTopAttachPos;
        goTopAttachNode.transform.localRotation = qTopAttachRot;
        goTopAttachNode.transform.localScale = vecTopAttachScl;

        // Renderer Disabled
        Renderer[] rends = goTopAttachNode.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in rends)
        {
            rend.enabled = false;
        }
        SkinnedMeshRenderer[] skinrends = goTopAttachNode.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (Renderer skinrend in skinrends)
        {
            skinrend.enabled = false;
        }

        // Exit
        if (bDebugLog) Debug.Log("FBXImporterExit()");
        FBXImporterExit();

        // Trial
        if (bDebugLog && bTrialMode) Debug.LogWarning("[FBX Importer for Unity] Free Trial version is limited over 150 frames(30fps = 5sec) animations! and Open Commercial WEB Accsess!");
    }

    void Update()
    {
        if (sWWWDownloaderImporter.bWWWDownloadEnd)
        {
            // Renderer Enabled
            Renderer[] rends = goTopAttachNode.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in rends)
            {
                rend.enabled = true;
            }
            SkinnedMeshRenderer[] skinrends = goTopAttachNode.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (Renderer skinrend in skinrends)
            {
                skinrend.enabled = true;
            }
            sWWWDownloaderImporter.bWWWDownloadEnd = false;

            // Auto Animation Play
            if (bAutoAnimationPlay)
            {
                Animation anim = animationAttach;
                if (anim == null)
                {
                    anim = goTopAttachNode.GetComponentInParent<Animation>();
                }
                if (anim != null && listSAnimationClips != null)
                {
                    if (iAutoAnimationPlayNo <= 0)
                    {
                        foreach (SAnimationClips listSAnimationClip in listSAnimationClips)
                        {
                            if (listSAnimationClip.animationClip != null && listSAnimationClip.strClipName != "")
                            {
                                //
                                anim.clip = listSAnimationClip.animationClip;
                                anim.Play(listSAnimationClip.strClipName);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (iAutoAnimationPlayNo <= listSAnimationClips.Length)
                        {
                            if (listSAnimationClips[iAutoAnimationPlayNo - 1].animationClip != null)
                            {
                                //
                                anim.clip = listSAnimationClips[iAutoAnimationPlayNo - 1].animationClip;
                                anim.Play(listSAnimationClips[iAutoAnimationPlayNo - 1].strClipName);
                            }
                        }
                    }
                }
            }
        }
    }

    void ImportSetMesh(string strNodeName, GameObject goNode, GameObject goTopAttachNode, bool bDirectControlPointsMode)
    {
        // Get Mesh
        IntPtr pFBXMesh = FBXImporterGetMesh(strNodeName, bInDirectControlPointsMode, bInNormalSmoothing);
        if (pFBXMesh != IntPtr.Zero)
        {
            FBXMesh sFBXMesh = new FBXMesh();
            IntPtr pFBXMeshT = Marshal.AllocHGlobal(Marshal.SizeOf(sFBXMesh));
            try
            {
                sFBXMesh = (FBXMesh)Marshal.PtrToStructure(pFBXMesh, typeof(FBXMesh));
            }
            finally
            {
                Marshal.FreeHGlobal(pFBXMeshT);
            }
            if (bDirectControlPointsMode)
            {
                sFBXMesh.numVertexs = sFBXMesh.numVertexs / 3;
                sFBXMesh.numNormals = sFBXMesh.numNormals / 3;
                sFBXMesh.numUVs = sFBXMesh.numUVs / 3;
            }

            // Vertex
            float[] fVertexs = new float[sFBXMesh.numVertexs * 3];
            Marshal.Copy(sFBXMesh.fVertexs, fVertexs, 0, sFBXMesh.numVertexs * 3);
            Vector3[] vecVertexs = new Vector3[sFBXMesh.numVertexs];
            int j = 0;
            for (int i = 0; i < sFBXMesh.numVertexs; i++)
            {
                vecVertexs[i].x = -fVertexs[j] * fGlobalScale;
                j++;
                vecVertexs[i].y = fVertexs[j] * fGlobalScale;
                j++;
                vecVertexs[i].z = fVertexs[j] * fGlobalScale;
                j++;
            }

            // Normal
            float[] fNormals = new float[sFBXMesh.numNormals * 3];
            Marshal.Copy(sFBXMesh.fNormals, fNormals, 0, sFBXMesh.numNormals * 3);
            Vector3[] vecNormals = new Vector3[sFBXMesh.numNormals];
            j = 0;
            for (int i = 0; i < sFBXMesh.numNormals; i++)
            {
                vecNormals[i].x = -fNormals[j];
                j++;
                vecNormals[i].y = fNormals[j];
                j++;
                vecNormals[i].z = fNormals[j];
                j++;
            }

            // UV
            float[] fUVs = new float[sFBXMesh.numUVs * 2];
            Marshal.Copy(sFBXMesh.fUVs, fUVs, 0, sFBXMesh.numUVs * 2);
            Vector2[] vecUVs = new Vector2[sFBXMesh.numUVs];
            j = 0;
            for (int i = 0; i < sFBXMesh.numUVs; i++)
            {
                vecUVs[i].x = fUVs[j];
                j++;
                vecUVs[i].y = fUVs[j];
                j++;
            }

            // Polygon (MaterialID)
            int[] iPolygonMatIDs = new int[sFBXMesh.numPolygonMatID];
            Marshal.Copy(sFBXMesh.iPolygonMatIDs, iPolygonMatIDs, 0, sFBXMesh.numPolygonMatID);
            int numPolygonIndex = sFBXMesh.numPolygonIndex;
            int[] iPolygonIndexs = new int[numPolygonIndex];
            Marshal.Copy(sFBXMesh.iPolygonIndexs, iPolygonIndexs, 0, numPolygonIndex);
            if (bDirectControlPointsMode)
            {
                sFBXMesh.numPolygonMatID = sFBXMesh.numPolygonMatID / 3;
            }

            // Set Mesh
            goNode.AddComponent<MeshRenderer>();
            goNode.AddComponent<MeshFilter>();
            Mesh sMesh = new Mesh();
            sMesh.name = goNode.name;
            goNode.GetComponent<MeshFilter>().mesh = sMesh;
            sMesh.Clear();
            sMesh.vertices = vecVertexs;
            sMesh.normals = vecNormals;
            sMesh.uv = vecUVs;
            sMesh.subMeshCount = sFBXMesh.numMatID;
            List<List<int>> listListMatIDIndeices = new List<List<int>>();
            for (int i = 0; i < sFBXMesh.numMatID; i++)
            {
                List<int> iListMatIDIndeices = new List<int>();
                listListMatIDIndeices.Add(iListMatIDIndeices);
            }

            // Polygon (Material Index)
            if (bDirectControlPointsMode)
            {
                j = 0;
                for (int i = 0; i < sFBXMesh.numPolygonMatID; i++)
                {
                    int iMatID = iPolygonMatIDs[i];
                    if (iMatID < listListMatIDIndeices.Count)
                    {
                        listListMatIDIndeices[iMatID].Add(iPolygonIndexs[j + 0]);
                        listListMatIDIndeices[iMatID].Add(iPolygonIndexs[j + 2]);
                        listListMatIDIndeices[iMatID].Add(iPolygonIndexs[j + 1]);
                        j = j + 3;
                    }
                }
            }
            else
            {
                j = 0;
                for (int i = 0; i < sFBXMesh.numPolygonMatID; i++)
                {
                    int iMatID = iPolygonMatIDs[i];
                    if (iMatID < listListMatIDIndeices.Count)
                    {
                        listListMatIDIndeices[iMatID].Add(j + 0);
                        listListMatIDIndeices[iMatID].Add(j + 2);
                        listListMatIDIndeices[iMatID].Add(j + 1);
                        j = j + 3;
                    }
                }
            }

            // Triangles (Material Index)
            j = 0;
            for (int i = 0; i < sFBXMesh.numMatID; i++)
            {
                int[] iMatIDIndeices = listListMatIDIndeices[i].ToArray();
                //if (iMatIDIndeices.Length > 0)
                {
                    sMesh.SetTriangles(iMatIDIndeices, j);
                    j++;
                }
            }

            // Get Weight
            IntPtr pFBXWeight = FBXImporterGetWeight(strNodeName, iPolygonIndexs, numPolygonIndex, bInDirectControlPointsMode);
            if (pFBXWeight != IntPtr.Zero)
            {
                FBXWeight sFBXWeight = new FBXWeight();
                IntPtr pFBXWeightT = Marshal.AllocHGlobal(Marshal.SizeOf(sFBXWeight));
                try
                {
                    sFBXWeight = (FBXWeight)Marshal.PtrToStructure(pFBXWeight, typeof(FBXWeight));
                }
                finally
                {
                    Marshal.FreeHGlobal(pFBXWeightT);
                }

                if (bDirectControlPointsMode)
                {
                    sFBXWeight.iBoneWeightCount = sFBXWeight.iBoneWeightCount / 3;
                }

                // Weight (Bone)
                int[] iBoneID = new int[sFBXWeight.iBoneWeightCount * 4];
                Marshal.Copy(sFBXWeight.iBoneID, iBoneID, 0, sFBXWeight.iBoneWeightCount * 4);
                float[] fBoneWeight = new float[sFBXWeight.iBoneWeightCount * 4];
                Marshal.Copy(sFBXWeight.fBoneWeight, fBoneWeight, 0, sFBXWeight.iBoneWeightCount * 4);
                BoneWeight[] boneWeights = new BoneWeight[sFBXWeight.iBoneWeightCount];
                for (int i = 0; i < sFBXWeight.iBoneWeightCount; i++)
                {
                    BoneWeight boneWeight = new BoneWeight();
                    boneWeight.boneIndex0 = iBoneID[(i * 4) + 0];
                    boneWeight.boneIndex1 = iBoneID[(i * 4) + 1];
                    boneWeight.boneIndex2 = iBoneID[(i * 4) + 2];
                    boneWeight.boneIndex3 = iBoneID[(i * 4) + 3];
                    boneWeight.weight0 = fBoneWeight[(i * 4) + 0];
                    boneWeight.weight1 = fBoneWeight[(i * 4) + 1];
                    boneWeight.weight2 = fBoneWeight[(i * 4) + 2];
                    boneWeight.weight3 = fBoneWeight[(i * 4) + 3];
                    boneWeights[i] = boneWeight;
                }
                sMesh.boneWeights = boneWeights;

                // Bind Pose (Bone)
                //var goNode2 = new GameObject(goNode.name);
                SkinnedMeshRenderer sSkinnedMeshRenderer = goNode.AddComponent<SkinnedMeshRenderer>();
                string strBoneName = Marshal.PtrToStringAnsi(sFBXWeight.strBoneNodeNames).ToString();
                if (strBoneName != "")
                {
                    string[] strBoneNodeNames = strBoneName.Split(',');
                    //float[] matBonePose = new float[strBoneNodeNames.Length * 16];
                    //Marshal.Copy(sFBXWeight.matBonePose, matBonePose, 0, strBoneNodeNames.Length * 16);
                    Matrix4x4[] bindposes = new Matrix4x4[strBoneNodeNames.Length];
                    Transform[] bones = new Transform[strBoneNodeNames.Length];
                    sSkinnedMeshRenderer.bones = new Transform[strBoneNodeNames.Length];
                    float[] fGlbTranslation = new float[3 * strBoneNodeNames.Length];
                    Marshal.Copy(sFBXWeight.fGlbTranslation, fGlbTranslation, 0, 3 * strBoneNodeNames.Length);
                    float[] fGlbRotation = new float[4 * strBoneNodeNames.Length];
                    Marshal.Copy(sFBXWeight.fGlbRotation, fGlbRotation, 0, 4 * strBoneNodeNames.Length);
                    float[] fGlbScale = new float[3 * strBoneNodeNames.Length];
                    Marshal.Copy(sFBXWeight.fGlbScale, fGlbScale, 0, 3 * strBoneNodeNames.Length);
                    for (int i = 0; i < strBoneNodeNames.Length; i++)
                    {
                        /*
                        Matrix4x4 matBindPose = new Matrix4x4();
                        matBindPose.m00 = matBonePose[(i * 16) + 0];
                        matBindPose.m01 = matBonePose[(i * 16) + 1];
                        matBindPose.m02 = matBonePose[(i * 16) + 2];
                        matBindPose.m03 = matBonePose[(i * 16) + 3];
                        matBindPose.m10 = matBonePose[(i * 16) + 4];
                        matBindPose.m11 = matBonePose[(i * 16) + 5];
                        matBindPose.m12 = matBonePose[(i * 16) + 6];
                        matBindPose.m13 = matBonePose[(i * 16) + 7];
                        matBindPose.m20 = matBonePose[(i * 16) + 8];
                        matBindPose.m21 = matBonePose[(i * 16) + 9];
                        matBindPose.m22 = matBonePose[(i * 16) + 10];
                        matBindPose.m23 = matBonePose[(i * 16) + 11];
                        matBindPose.m30 = matBonePose[(i * 16) + 12] * fGlobalScale;
                        matBindPose.m31 = matBonePose[(i * 16) + 13] * fGlobalScale;
                        matBindPose.m32 = matBonePose[(i * 16) + 14] * fGlobalScale;
                        matBindPose.m33 = matBonePose[(i * 16) + 15];
                        */
                        Transform bone = goTopAttachNode.transform.FindDeepI(strBoneNodeNames[i]).transform;
                        bone.transform.position = new Vector3(-fGlbTranslation[(i * 3) + 0] * fGlobalScale, fGlbTranslation[(i * 3) + 1] * fGlobalScale, fGlbTranslation[(i * 3) + 2] * fGlobalScale);
                        bone.transform.rotation = new Quaternion(-fGlbRotation[(i * 4) + 0], fGlbRotation[(i * 4) + 1], fGlbRotation[(i * 4) + 2], -fGlbRotation[(i * 4) + 3]);
                        bone.transform.localScale = new Vector3(fGlbScale[(i * 3) + 0], fGlbScale[(i * 3) + 1], fGlbScale[(i * 3) + 2]);
                        bones[i] = bone;                    
                        bindposes[i] = bones[i].worldToLocalMatrix;
                    }
                    //sSkinnedMeshRenderer.rootBone = goTopAttachNode.transform;
                    sSkinnedMeshRenderer.bones = bones;
                    sSkinnedMeshRenderer.sharedMesh = sMesh;
                    sSkinnedMeshRenderer.sharedMesh.bindposes = bindposes;
                    //sSkinnedMeshRenderer.sharedMesh.RecalculateNormals();
                    //sSkinnedMeshRenderer.sharedMesh.RecalculateBounds();
                }
            }
            // Import TRS
            ImportTRS(goNode.name, goNode, goTopAttachNode);

            // Set Shader
            SetShader(goNode);
        }
    }

    void SetShader(GameObject goNode)
    {
        Renderer rend = goNode.GetComponent<Renderer>();
        SkinnedMeshRenderer skinRend = goNode.GetComponent<SkinnedMeshRenderer>();
        if (rend != null || skinRend != null)
        {
            Material[] mats;
            SShaderMaterialBase sShaderMaterialBase;
            if (dicShaderMaterialBases.TryGetValue(goNode.name, out sShaderMaterialBase) && bInShader)
            {
                mats = new Material[sShaderMaterialBase.iShaderMaterialCount];
                int k = 0;
                for (int i = 0; i < sShaderMaterialBase.iShaderMaterialCount; i++)
                {
                    Shader shader = Shader.Find(sShaderMaterialBase.sShaderMaterials[i].strShaderName);
                    if (shader != null)
                    {
                        mats[k] = new Material(shader);
                        mats[k].name = sShaderMaterialBase.sShaderMaterials[i].strMaterailName;
                        foreach (string strParameter in sShaderMaterialBase.sShaderMaterials[i].strShaderParameters)
                        {
                            string[] strParameters = strParameter.Split(',');
                            switch (strParameters[0])
                            {
                                case "Color":
                                    mats[k].SetColor(strParameters[1], new Color(float.Parse(strParameters[2]), float.Parse(strParameters[3]), float.Parse(strParameters[4]), float.Parse(strParameters[5])));
                                    break;
                                case "Vector":
                                    mats[k].SetVector(strParameters[1], new Vector4(float.Parse(strParameters[2]), float.Parse(strParameters[3]), float.Parse(strParameters[4]), float.Parse(strParameters[5])));
                                    break;
                                case "Float":
                                    mats[k].SetFloat(strParameters[1], float.Parse(strParameters[2]));
                                    break;
                                case "Range":
                                    mats[k].SetFloat(strParameters[1], float.Parse(strParameters[2]));
                                    break;
                                case "TexEnv":
                                    string strTextureName = "";
                                    if (strParameters[2].IndexOf(".fbm") >= 0 && bInputTextureFullPath)
                                    {
                                        bool bFirst = false;
                                        bool bInsert = false;
                                        string[] strTextureNames = strParameters[2].Split('/');
                                        for (int j = 0; j < strTextureNames.Length; j++)
                                        {
                                            if (strTextureNames[j].IndexOf(".fbm") >= 0)
                                            {
                                                bInsert = true;
                                                bFirst = true;
                                            }
                                            if (bInsert)
                                            {
                                                if (!bFirst)
                                                {
                                                    strTextureName += "/";
                                                }
                                                strTextureName += strTextureNames[j];
                                                bFirst = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string[] strTextureNames = strParameters[2].Split('/');
                                        strTextureName = strTextureNames[strTextureNames.Length - 1];
                                    }
                                    // Set Texture (Shader: On)
                                    SetTexture(mats[k], strParameters[1], strTextureName, true);
                                    break;
                            }
                        }
                        k++;
                    }
                    else
                    {
                        Debug.LogError("[Shader not found] " + sShaderMaterialBase.sShaderMaterials[i].strShaderName);
                    }
                }
                if (skinRend != null)
                {
                    if (k > 1)
                    {
                        skinRend.materials = mats;
                    }
                    else
                    {
                        skinRend.material = mats[0];
                    }
                }
                else
                {
                    if (k > 1)
                    {
                        rend.materials = mats;
                    }
                    else
                    {
                        rend.material = mats[0];
                    }
                }
            }
            else
            {
                // Set Material
                SetMaterial(goNode);
            }
        }
    }
    void SetMaterial(GameObject goNode)
    {
        Renderer rend = goNode.GetComponent<Renderer>();
        SkinnedMeshRenderer skinRend = goNode.GetComponent<SkinnedMeshRenderer>();
        if (rend != null || skinRend != null)
        {
            int iMaterialCount = FBXImporterGetMaterialCount(goNode.name);
            Material[] mats = new Material[iMaterialCount];
            int k = 0;
            for (int i = 0; i < iMaterialCount; i++)
            {
                IntPtr pFBXMaterial = FBXImporterGetMaterial(goNode.name, i, bInputTextureFullPath);
                if (pFBXMaterial != IntPtr.Zero)
                {
                    FBXMaterial sFBXMaterial = new FBXMaterial();
                    IntPtr pFBXMaterialT = Marshal.AllocHGlobal(Marshal.SizeOf(sFBXMaterial));
                    try
                    {
                        sFBXMaterial = (FBXMaterial)Marshal.PtrToStructure(pFBXMaterial, typeof(FBXMaterial));
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pFBXMaterialT);
                    }

                    if (Marshal.PtrToStringAnsi(sFBXMaterial.strMaterialName).ToString() == "CgfxShader")
                    {
                        /*
                        string strCgfxTexturTypes = "";
                        string strCgfxTextureNames = "";
                        if (sFBXMaterial.strCgfxTextureTypes != null)
                        {
                            strCgfxTexturTypes = Marshal.PtrToStringAnsi(sFBXMaterial.strCgfxTextureTypes);
                        }
                        if (sFBXMaterial.strCgfxTextureNames != null)
                        {
                            strCgfxTextureNames = Marshal.PtrToStringAnsi(sFBXMaterial.strCgfxTextureNames);
                        }
                        */
                        Debug.LogWarning("CgfxShader:" + sFBXMaterial.strMaterialName);
                        continue;
                    }
                    else
                    {
                        float[] fDiffuse = new float[3];
                        fDiffuse[0] = 0.0f;
                        fDiffuse[1] = 0.0f;
                        fDiffuse[2] = 0.0f;
                        Marshal.Copy(sFBXMaterial.fDiffuse, fDiffuse, 0, 3);

                        float[] fTransparent = new float[1];
                        fTransparent[0] = 0.0f;
                        Marshal.Copy(sFBXMaterial.fTransparentColor, fTransparent, 0, 1);

                        mats[k] = new Material(Shader.Find("Standard"));
                        mats[k].color = new Color(fDiffuse[0], fDiffuse[1], fDiffuse[2], 1.0f - fTransparent[0]);

                        string strTexturAmbientName = Marshal.PtrToStringAnsi(sFBXMaterial.strTexturAmbientName);
                        string strTextureDiffuseName = Marshal.PtrToStringAnsi(sFBXMaterial.strTextureDiffuseName);
                        string strTextureEmissiveName = Marshal.PtrToStringAnsi(sFBXMaterial.strTextureEmissiveName);
                        string strTextureBumpName = Marshal.PtrToStringAnsi(sFBXMaterial.strTextureBumpName);
                        string strTextureNormalMapName = Marshal.PtrToStringAnsi(sFBXMaterial.strTextureNormalMapName);
                        string strTextureDisplacementName = Marshal.PtrToStringAnsi(sFBXMaterial.strTextureDisplacementName);
                        string strTextureTransparentName = Marshal.PtrToStringAnsi(sFBXMaterial.strTextureTransparentName);
                        string strTextureReflectionName = Marshal.PtrToStringAnsi(sFBXMaterial.strTextureReflectionName);

                        // Set Texture (Shader: Off)
                        SetTexture(mats[k], "MainTexture", strTextureDiffuseName, false);
                        SetTexture(mats[k], strShaderTextureAmbientParameter, strTexturAmbientName, false);
                        SetTexture(mats[k], strShaderTextureDiffuseParameter, strTextureDiffuseName, false);
                        SetTexture(mats[k], strShaderTextureEmissiveParameter, strTextureEmissiveName, false);
                        SetTexture(mats[k], strShaderTextureBumpParameter, strTextureBumpName, false);
                        SetTexture(mats[k], strShaderTextureNormalMapParameter, strTextureNormalMapName, false);
                        SetTexture(mats[k], strShaderTextureDisplacementParameter, strTextureDisplacementName, false);
                        SetTexture(mats[k], strShaderTextureTransparentParameter, strTextureTransparentName, false);
                        SetTexture(mats[k], strShaderTextureReflectionParameter, strTextureReflectionName, false);
                    }
                }
                k++;
            }
            if (skinRend != null)
            {
                if (rend != null)
                {
                    /*
                    Destroy(goNode.GetComponent<MeshFilter>());
                    Destroy(rend);
                    */
                }
                if (k > 1)
                {
                    skinRend.materials = mats;
                }
                else
                {
                    skinRend.material = mats[0];
                }

            }
            else
            {
                if (rend != null)
                {
                    if (k > 1)
                    {
                        rend.materials = mats;
                    }
                    else
                    {
                        rend.material = mats[0];
                    }
                }
            }
        }
    }

    string GetTexturePathName(string strTexturePathName)
    {
        if (strTexturePathName != "")
        {
            // Texture Name Ext
            if (File.Exists(strTexturePathName))
            {
                return strTexturePathName;
            }
            else
            {
                if (File.Exists(strTexturePathName + ".png"))
                {
                    return strTexturePathName += ".png";
                }
                else
                {
                    if (File.Exists(strTexturePathName + ".tga"))
                    {
                        return strTexturePathName += ".tga";
                    }
                    else
                    {
                        if (File.Exists(strTexturePathName + ".jpg"))
                        {
                            return strTexturePathName += ".jpg";
                        }
                    }
                }
            }
        }
        return strTexturePathName;
    }

    string GetTextureExtName(string strTextureName, string strTextureExtName)
    {
        if (strTextureName.IndexOf(".") >= 0)
        {
            return strTextureName;
        }
        return strTextureName + strTextureExtName;
    }

    void SetTexture(Material mat, string strParameter, string strTextureName, bool bShader)
    {
        if (strTextureName != null)
        {
            if (strTextureName != "")
            {
                // \\ to /
                strTextureName = strTextureName.Replace("\\", "/");
                // Resource Texture Name (None EXT)
                string strDataPath = "";
                string strResourceTextureName = "";
                if (bShader)
                {
                    strResourceTextureName = strTextureName;
                }
                else
                {
                    string[] strResourceTextureNameTemp = strTextureName.Split('.');
                    for (int i = 0; i < strResourceTextureNameTemp.Length - 1; i++)
                    {
                        strResourceTextureName += strResourceTextureNameTemp[i];
                        if (i < strResourceTextureNameTemp.Length - 2)
                        {
                            strResourceTextureName += ".";
                        }
                    }
                }
                // ImportTextureMode Init
                switch (eImportTextureMode)
                {
                    case EFBXImportTextrureMode.eWWWLocalEmbedMedia:
                    case EFBXImportTextrureMode.eWWWLocalProject:
                        strDataPath = Application.dataPath.Replace("/Assets", "");
                        string[] strFilePaths = strDataPath.Split('/');
                        strDataPath = "";
                        int iDataPathLength = strFilePaths.Length;
                        if (strFilePaths[iDataPathLength - 1].LastIndexOf("_Data") > 0)
                        {
                            iDataPathLength = iDataPathLength - 1;
                        }
                        for (int i = 0; i < iDataPathLength; i++)
                        {
                            strDataPath += strFilePaths[i] + "/";
                        }
                        break;
                }
                // ImportTextureMode Exec
                string strTexturePathName = "";
                switch (eImportTextureMode)
                {
                    case EFBXImportTextrureMode.eWWWLocalEmbedMedia:
                        strTexturePathName = GetTexturePathName(strDataPath + strWWWLocalEmbedMediaTexturePath + strResourceTextureName);
                        StartCoroutine(sWWWDownloaderImporter.WWWDownloadToLoadFileTexture("file://" + strTexturePathName, bWWWUnloadUnusedAssets, () =>
                        {
                            if (strParameter == "MainTexture")
                            {
                                mat.mainTexture = sWWWDownloaderImporter.texture;
                            }
                            else
                            {
                                mat.SetTexture(strParameter, sWWWDownloaderImporter.texture);
                            }
                        }));
                        break;
                    case EFBXImportTextrureMode.eWWWLocalProject:
                        strTexturePathName = GetTexturePathName(strDataPath + strWWWLocalProjectTexturePath + strResourceTextureName);
                        StartCoroutine(sWWWDownloaderImporter.WWWDownloadToLoadFileTexture("file://" + strTexturePathName, bWWWUnloadUnusedAssets, () =>
                        {
                            if (strParameter == "MainTexture")
                            {
                                mat.mainTexture = sWWWDownloaderImporter.texture;
                            }
                            else
                            {
                                mat.SetTexture(strParameter, sWWWDownloaderImporter.texture);
                            }
                        }));
                        break;
                    case EFBXImportTextrureMode.eWWWLocalStreamingAssets:
                        strTexturePathName = GetTexturePathName(Application.streamingAssetsPath + "/" + strWWWLocalStreamingAssetsTexturePath + strResourceTextureName);
                        StartCoroutine(sWWWDownloaderImporter.WWWDownloadToLoadFileTexture("file://" + strTexturePathName, bWWWUnloadUnusedAssets, () =>
                        {
                            if (strParameter == "MainTexture")
                            {
                                mat.mainTexture = sWWWDownloaderImporter.texture;
                            }
                            else
                            {
                                mat.SetTexture(strParameter, sWWWDownloaderImporter.texture);
                            }
                        }));
                        break;
                    case EFBXImportTextrureMode.eWWWLocalFullPath:
                        strTexturePathName = GetTexturePathName(strWWWLocalFullPathTexturePath + strResourceTextureName);
                        StartCoroutine(sWWWDownloaderImporter.WWWDownloadToLoadFileTexture(strTexturePathName, bWWWUnloadUnusedAssets, () =>
                        {
                            if (strParameter == "MainTexture")
                            {
                                mat.mainTexture = sWWWDownloaderImporter.texture;
                            }
                            else
                            {
                                mat.SetTexture(strParameter, sWWWDownloaderImporter.texture);
                            }
                        }));
                        break;
                    case EFBXImportTextrureMode.eWWWURL:
                        strTexturePathName = strWWWURLTexturePath + GetTextureExtName(strResourceTextureName, ".png");
                        StartCoroutine(sWWWDownloaderImporter.WWWDownloadToLoadFileTexture(strTexturePathName, bWWWUnloadUnusedAssets, () =>
                        {
                            if (strParameter == "MainTexture")
                            {
                                mat.mainTexture = sWWWDownloaderImporter.texture;
                            }
                            else
                            {
                                mat.SetTexture(strParameter, sWWWDownloaderImporter.texture);
                            }
                        }));
                        break;
                    case EFBXImportTextrureMode.eResourceLoad:
                        if (strParameter == "MainTexture")
                        {
                            mat.mainTexture = Resources.Load(strResourceLoad + strResourceTextureName) as Texture2D;
                        }
                        else
                        {
                            mat.SetTexture(strParameter, Resources.Load(strResourceLoad + strResourceTextureName) as Texture2D);
                        }
                        if (sWWWDownloaderImporter.iWWWDownloadCount <= 0)
                        {
                            sWWWDownloaderImporter.bWWWDownloadEnd = true;
                        }
                        break;
                    case EFBXImportTextrureMode.eEditorLocalAssets:
#if UNITY_EDITOR
                        strTexturePathName = strEditorLocalAssetsTexturePath + GetTextureExtName(strResourceTextureName, ".png");
                        Texture2D texture = AssetDatabase.LoadAssetAtPath(strTexturePathName, typeof(Texture2D)) as Texture2D;
                        if (texture != null)
                        {
                            if (strParameter == "MainTexture")
                            {
                                mat.mainTexture = texture;
                            }
                            else
                            {
                                mat.SetTexture(strParameter, texture);
                            }
                        }
#endif
                        if (sWWWDownloaderImporter.iWWWDownloadCount <= 0)
                        {
                            sWWWDownloaderImporter.bWWWDownloadEnd = true;
                        }
                        break;
                    case EFBXImportTextrureMode.eEditorAllAssets:
#if UNITY_EDITOR
                        strTexturePathName = GetTextureExtName(strResourceTextureName, ".png");
                        foreach (string assetGuid in AssetDatabase.FindAssets("t:Texture2D"))
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                            texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
                            if (texture != null && strTexturePathName != null)
                            {
                                if (strTexturePathName.IndexOf(texture.name, 0) >= 0)
                                {
                                    if (strParameter == "MainTexture")
                                    {
                                        mat.mainTexture = texture;
                                    }
                                    else
                                    {
                                        mat.SetTexture(strParameter, texture);
                                    }
                                    break;
                                }
                            }
                        }
#endif
                        if (sWWWDownloaderImporter.iWWWDownloadCount <= 0)
                        {
                            sWWWDownloaderImporter.bWWWDownloadEnd = true;
                        }
                        break;
                }
            }
        }
        else
        {
            if (sWWWDownloaderImporter.iWWWDownloadCount <= 0)
            {
                sWWWDownloaderImporter.bWWWDownloadEnd = true;
            }
        }
    }

    void ImportTRS(string strNodeName, GameObject goNode, GameObject goTopAttachNode)
    {
        // TRS
        IntPtr pFBXNodeTRS = FBXImporterGetNodeTRS(strNodeName);
        if (pFBXNodeTRS != IntPtr.Zero)
        {
            FBXNodeTRS sFBXNodeTRS = new FBXNodeTRS();
            IntPtr pFBXNodeTRST = Marshal.AllocHGlobal(Marshal.SizeOf(sFBXNodeTRS));
            try
            {
                sFBXNodeTRS = (FBXNodeTRS)Marshal.PtrToStructure(pFBXNodeTRS, typeof(FBXNodeTRS));
            }
            finally
            {
                Marshal.FreeHGlobal(pFBXNodeTRST);
            }
            float[] fLclTranslation = new float[3];
            Marshal.Copy(sFBXNodeTRS.fLclTranslation, fLclTranslation, 0, 3);
            float[] fLclRotation = new float[4];
            Marshal.Copy(sFBXNodeTRS.fLclRotation, fLclRotation, 0, 4);
            float[] fLclScale = new float[3];
            Marshal.Copy(sFBXNodeTRS.fLclScale, fLclScale, 0, 3);

            goNode.transform.localPosition = new Vector3(-fLclTranslation[0] * fGlobalScale, fLclTranslation[1] * fGlobalScale, fLclTranslation[2] * fGlobalScale);
            goNode.transform.localRotation = new Quaternion(-fLclRotation[0], fLclRotation[1], fLclRotation[2], -fLclRotation[3]);
            goNode.transform.localScale = new Vector3(fLclScale[0], fLclScale[1], fLclScale[2]);
        }
    }

    void ImportTRSAnimation(int iAnimationClip, int iFrameRate, int iTimeMode, WrapMode eWrapMode, string strNodeName, GameObject goNode, GameObject goTopAttachNode)
    {
        IntPtr pFBXNodeTRSAnim = FBXImporterGetNodeTRSAnimation(strNodeName, iAnimationClip, iTimeMode);
        if (pFBXNodeTRSAnim != IntPtr.Zero)
        {
            FBXNodeTRS sFBXNodeTRSAnim = new FBXNodeTRS();
            IntPtr pFBXNodeTRST = Marshal.AllocHGlobal(Marshal.SizeOf(sFBXNodeTRSAnim));
            try
            {
                sFBXNodeTRSAnim = (FBXNodeTRS)Marshal.PtrToStructure(pFBXNodeTRSAnim, typeof(FBXNodeTRS));
            }
            finally
            {
                Marshal.FreeHGlobal(pFBXNodeTRST);
            }
            float[] fLclTranslation = new float[3 * sFBXNodeTRSAnim.iCount];
            Marshal.Copy(sFBXNodeTRSAnim.fLclTranslation, fLclTranslation, 0, 3 * sFBXNodeTRSAnim.iCount);
            float[] fLclRotation = new float[4 * sFBXNodeTRSAnim.iCount];
            Marshal.Copy(sFBXNodeTRSAnim.fLclRotation, fLclRotation, 0, 4 * sFBXNodeTRSAnim.iCount);
            float[] fLclScale = new float[3 * sFBXNodeTRSAnim.iCount];
            Marshal.Copy(sFBXNodeTRSAnim.fLclScale, fLclScale, 0, 3 * sFBXNodeTRSAnim.iCount);

            // Keyframeの生成.
			if (iFrameRate == 0)
			{
				iFrameRate = iDefaultFrameRate;
			}
			float fTime = (float)sFBXNodeTRSAnim.iFrameStart / iFrameRate;
            Keyframe[] key_localPositionX = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localPositionY = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localPositionZ = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localRotationX = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localRotationY = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localRotationZ = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localRotationW = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localScaleX = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localScaleY = new Keyframe[sFBXNodeTRSAnim.iCount];
            Keyframe[] key_localScaleZ = new Keyframe[sFBXNodeTRSAnim.iCount];
            for (int i = 0; i < sFBXNodeTRSAnim.iCount; i++)
            {
                key_localPositionX[i] = new Keyframe(fTime, -fLclTranslation[(i * 3) + 0] * fGlobalScale, 0.0f, 0.0f);
                key_localPositionY[i] = new Keyframe(fTime, fLclTranslation[(i * 3) + 1] * fGlobalScale, 0.0f, 0.0f);
                key_localPositionZ[i] = new Keyframe(fTime, fLclTranslation[(i * 3) + 2] * fGlobalScale, 0.0f, 0.0f);
                key_localRotationX[i] = new Keyframe(fTime, -fLclRotation[(i * 4) + 0], 0.0f, 0.0f);
                key_localRotationY[i] = new Keyframe(fTime, fLclRotation[(i * 4) + 1], 0.0f, 0.0f);
                key_localRotationZ[i] = new Keyframe(fTime, fLclRotation[(i * 4) + 2], 0.0f, 0.0f);
                key_localRotationW[i] = new Keyframe(fTime, -fLclRotation[(i * 4) + 3], 0.0f, 0.0f);
                key_localScaleX[i] = new Keyframe(fTime, fLclScale[(i * 3) + 0], 0.0f, 0.0f);
                key_localScaleY[i] = new Keyframe(fTime, fLclScale[(i * 3) + 1], 0.0f, 0.0f);
                key_localScaleZ[i] = new Keyframe(fTime, fLclScale[(i * 3) + 2], 0.0f, 0.0f);
                fTime += (float)1.0f / iFrameRate;
            }
            // AnimationCurveの生成.
            AnimationCurve curve_localPositionX = new AnimationCurve(key_localPositionX);
            AnimationCurve curve_localPositionY = new AnimationCurve(key_localPositionY);
            AnimationCurve curve_localPositionZ = new AnimationCurve(key_localPositionZ);
            AnimationCurve curve_localRotationX = new AnimationCurve(key_localRotationX);
            AnimationCurve curve_localRotationY = new AnimationCurve(key_localRotationY);
            AnimationCurve curve_localRotationZ = new AnimationCurve(key_localRotationZ);
            AnimationCurve curve_localRotationW = new AnimationCurve(key_localRotationW);
            AnimationCurve curve_localScaleX = new AnimationCurve(key_localScaleX);
            AnimationCurve curve_localScaleY = new AnimationCurve(key_localScaleY);
            AnimationCurve curve_localScaleZ = new AnimationCurve(key_localScaleZ);
            // AnimationCurveの追加.
            string strRelativePath = "/" + goNode.name;
            Transform transParent = goNode.transform.parent;
            while (transParent != null)
            {
                strRelativePath = "/" + transParent.name + strRelativePath;
                transParent = transParent.parent;
            }
            // Animation Clip
            AnimationClip animationClip = null;
            if (listSAnimationClips != null)
            {
                if (listSAnimationClips.Length > iAnimationClip)
                {
                    animationClip = listSAnimationClips[iAnimationClip].animationClip;
                    if (animationClip == null)
                    {
                        animationClip = new AnimationClip();
                        animationClip.name = listSAnimationClips[iAnimationClip].strClipName;
                        animationClip.legacy = true;
                        animationClip.frameRate = iFrameRate;
                        animationClip.wrapMode = eWrapMode;
                        listSAnimationClips[iAnimationClip].animationClip = animationClip;
                    }
                }
            }
            if (animationClip != null)
            {
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localPosition.x", curve_localPositionX);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localPosition.y", curve_localPositionY);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localPosition.z", curve_localPositionZ);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localRotation.x", curve_localRotationX);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localRotation.y", curve_localRotationY);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localRotation.z", curve_localRotationZ);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localRotation.w", curve_localRotationW);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localScale.x", curve_localScaleX);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localScale.y", curve_localScaleY);
                animationClip.SetCurve(strRelativePath, typeof(Transform), "localScale.z", curve_localScaleZ);
            }
        }
    }
}
