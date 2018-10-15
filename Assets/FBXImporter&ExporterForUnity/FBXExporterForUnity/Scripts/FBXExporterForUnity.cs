/*
 * FBXExporterForUnity.cs
 *
 *  	Developed by ほえたん(Hoetan) -- 2018/10/15
 *  	Copyright (c) 2015-2017, ACTINIA Software. All rights reserved.
 * 		Homepage: http://actinia-software.com
 * 		E-Mail: hoetan@actinia-software.com
 * 		Twitter: https://twitter.com/hoetan3
 * 		GitHub: https://github.com/hoetan
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FBXExporterForUnity : MonoBehaviour
{
    [DllImport("FBXExporterForUnity")]
	public static extern int FBXExporterInit();
    [DllImport("FBXExporterForUnity")]
	public static extern int FBXExporterExit();
    [DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSave(string pcName, string pcVersion, int iRenamingMode, int iFileFormat, string pcFileFormat2, bool bEmbedMedia, bool bASCIIFBX);
	[DllImport("FBXExporterForUnity")]
	public static extern void FBXExporterGeometryConverter(bool bTriangulate);
	[DllImport("FBXExporterForUnity")]
	public static extern void FBXExporterAxisSystem(int iUpVector, int iFrontVector, int iCoordSystem);
	[DllImport("FBXExporterForUnity")]
	public static extern void FBXExporterSystemUnit(int iSystemUnit);
	[DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetNode(string pcAttachParentNodeName, string pcNodeName, float[] fLclTranslation, float[] fLclRotation, float[] fLclScale);
    [DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetAnimationStackBaseLayer(string pcStack, string pcLayer);
	[DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetAnimationTime(float fTime);
	[DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetAnimationTRS(string pcNodeName, float[] fTranslation, float[] fRotation, float[] fScale, bool bCreate);
	[DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetAnimationsTRS(string pcNodeName, int FrameCount, float[] fTime, float[] fTranslation, float[] fRotation, float[] fScale, bool bCreate);
	[DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetNodeMesh(string pcAttachParentNodeName, string pcNodeMeshName, float[] fVertex, float[] fNormal, float[] fUV, int iVertexCount, int[] iVertexIndex, int[] iVertexIndexMaterialID, int iVertexIndexCount, float[] fLclTranslation, float[] fLclRotation, float[] fLclScale, bool bDirectControlPointsMode, bool bNormalSmoothing);
    [DllImport("FBXExporterForUnity")]
    public static extern bool FBXExporterSetNodeMeshMorphFirst();
    [DllImport("FBXExporterForUnity")]
    public static extern bool FBXExporterSetNodeMeshMorph(string pcMorphName, int iVerticesCount, float[] fVertexMorph, float[] fNormalMorph);
    [DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetSkinBoneName(string pcNodeMeshName, int i, string pcBoneName);
	[DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetSkinBoneWeight(string pcNodeMeshName, string pcRootNodeMeshName, int[] iBoneIndex, float[] fBoneWeight, int iVertexCount);
    [DllImport("FBXExporterForUnity")]
	public static extern bool FBXExporterSetBindPose(string pcNodeMeshName, string pcRootNodeMeshName);
    [DllImport("FBXExporterForUnity")]
    public static extern bool FBXExporterAddMaterial(string pcAddNodeName, string strMaterialName, float[] fAmbient, float[] fAnimbentFactor, float[] fDiffuse, float[] fDiffuseFactor, float[] fEmissive, float[] fEmissiveFactor, float[] fBump, float[] fBumpFactor, float[] fNormalMap, float[] fTransparentColor, float[] fTransparencyFactor, float[] fDisplacementColor, float[] fDisplacementFactor, float[] fVectorDisplacementColor, float[] fVectorDisplacementFactor, string strTextureAmbientName, string strTextureDiffuseName, string strTextureEmissiveName, string strTextureBumpName, string strTextureNormalMapName, string strTextureTransparentName, string strTextureDisplacementName, string strTextureReflectionName);
    [DllImport("FBXExporterForUnity")]
    public static extern bool FBXExporterSetShaderTextureName(int i, string pcTextureName);
    [DllImport("FBXExporterForUnity")]
    public static extern bool FBXExporterAddShaderMaterial(string pcAddNodeName, string strShaderFileName, string strShaderName, string strShaderTextureName, int iTextureCount);
 
    //private WWWDownloaderExporter sWWWDownloaderExporter;

    public enum EFBXExportToolTarget
	{
		eCustom,
		eFBXImporterForUnity,
		eUnity,
		eStingray,
		eDCCTools,
	};

    public enum EFileFormat
    {
        eFBX,
        eCollada_DAE,
        eAlias_OBJ,
        eAutoCAD_DXF,
    };

    public enum EFBXExportFileVersion
    {
        eDefault,
        eFBX_2011_00_COMPATIBLE,
        eFBX_2012_00_COMPATIBLE,
        eFBX_2013_00_COMPATIBLE,
        eFBX_2014_00_COMPATIBLE,
        eFBX_2016_00_COMPATIBLE,
        eFBX_2018_00_COMPATIBLE,
        eFBX_2019_00_COMPATIBLE,
    };

    public enum EFBXRenamingMode
    {
        eNone,
        eMAYA_TO_FBX5,
        eMAYA_TO_FBX_MB75,
        eMAYA_TO_FBX_MB70,
        eFBXMB75_TO_FBXMB70,
        eFBX_TO_FBX,
        eMAYA_TO_FBX,
        eFBX_TO_MAYA,
        eLW_TO_FBX,
        eFBX_TO_LW,
        eXSI_TO_FBX,
        eFBX_TO_XSI,
        eMAX_TO_FBX,
        eFBX_TO_MAX,
        eMB_TO_FBX,
        eFBX_TO_MB,
        eDAE_TO_FBX,
        eFBX_TO_DAE
    };

    public enum EFBXFileFormat
	{
		eDefault = -1,
		eBinary = 0,
		eASCII = 1,
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
		em= 6,
		eMile = 7,
		emm = 8,
		eYard = 9,
	};

	public enum EFBXImportTextrureMode
	{
		eAutoImportAssets,
		eLocalEmbedMedia,
		eLocalProject,
		eLocalStreamingAssets,
		eLocalAssets,
		eLocalFullPath,
		//eWWWFullPath,
		//eWWWURL,
	};

	public struct SAnimBufferTRS
	{
		public List<float> listTime;
		public List<float> listLclPosition;
		public List<float> listLclRotation;
		public List<float> listLclScale;
	};

	internal enum ShaderPropertyType
	{
		Color,
		Vector,
		Float,
		Range,
		TexEnv
	};

    private bool bTrialMode = true;

    public bool bDebugLog = true;
	public string strFilename = "Unity";
    public string strFilenameEXT = "";
    public EFBXExportToolTarget eExportToolTarget = EFBXExportToolTarget.eCustom;
    public EFileFormat eFileFormat = EFileFormat.eFBX;
    public EFBXExportFileVersion eFBXExportFileVersion = EFBXExportFileVersion.eDefault;
    public EFBXRenamingMode eEFBXRenamingMode = EFBXRenamingMode.eNone;
    public EFBXFileFormat eFBXFileFormat = EFBXFileFormat.eDefault;
    public EFBXUpVector eUpVector = EFBXUpVector.eInit;
	public EFBXFrontVector eFrontVector = EFBXFrontVector.eInit;
	public EFBXCoordSystem eCoordSystem = EFBXCoordSystem.eInit;
	public EFBXSystemUnit eSystemUnit = EFBXSystemUnit.eInit;

	public bool bOutDirectControlPointsMode = true;
	public bool bOutNormalSmoothing = true;
    public bool bOutRootBoneInverseMode = true;
    public float fGlobalScale = 100.0f;
	public bool bOutRootNode = false;
	public string strOutRootNodeName = "Root";
	public bool bObjectRename = true;
	public string strObjectRenameFormat = "{0}_{1}";
	public string[] strNoOutputInHierarchyObjectNames;

	public EFBXImportTextrureMode eImportTextrureMode = EFBXImportTextrureMode.eAutoImportAssets;
	public bool bImportEmbedMediaTexturePath = true;
	public string strLocalEmbedMediaTexturePath = "";
	public string strLocalProjectTexturePath = "InputTextures/";
	public string strLocalStreamingAssetsTexturePath = "InputTextures/";
	public string strLocalAssetsTexturePath = "Assets/InputTextures/";
	public string strLocalFullPathTexturePath = "";
	//public string strWWWFullPathTexturePath = "file://";
	//public string strWWWURLTexturePath = "http://";

	public bool bEmbedMedia = true;
	public bool bOutMesh = true;
    public bool bOutBlendShape = false;
	public bool bOutShader = true;
    public bool bOutShaderCGFX = true;
    public bool bOutComponent = true;
    public bool bOutAnimation = true;
	public bool bOutAnimationCustomFrame = false;

	public bool bAnimationBufferRecMode = true;
	public bool bEditorAutoStop = false;
	public string strStack = "Take0";
	public string strLayer = "Layer0";
    public int iFrame = 0;
	public float fTime = 0.0f;
	public float fTimeStart = 0.0f;
	public float fTimeRecStart = 0.0f;
	public float fTimeRecEnd = 5.0f;
    public bool bFixedUpdateMode = true;
    public int iFrameRate = 30;
    public bool bEnableSync = true;
    public int iFixedFrameRate = 30;
    public int iTargetFrameRate = 30;
	public int iVSyncCount = 1;
	public bool bOutRealTime = false;

	public bool bOutTexture = true;
	public bool bConvertAllPNG = true;
	public bool bOverWriteTexture = true;
	public string strOutputTexturePath = "OutputTextures/";

    public string strShaderTextureAmbientParameter = "";
    public string strShaderTextureDiffuseParameter = "_MainTex";
    public string strShaderTextureEmissiveParameter = "";
    public string strShaderTextureNormalMapParameter = "";
    public string strShaderTextureBumpMapParameter = "";
    public string strShaderTextureTransparentParameter = "";
    public string strShaderTextureDisplacementParameter = "";
    public string strShaderTextureReflectionParameter = "";

    private bool bFirst = false;
	private bool bStartupFBX = true;
	private bool bNowFBX = false;
	private bool bEndFBX = false;
	private bool bSave = false;
	
	private GameObject[] goRoot;
	private Dictionary<string, string> dicOutputTextureNames = new Dictionary<string, string>();

	private Dictionary<string, SAnimBufferTRS> dicAnimBufferTRSs = new Dictionary<string, SAnimBufferTRS>();

    #if UNITY_EDITOR
    private StreamWriter sw_shader_all;
    private StreamWriter sw_shader;
    private int iSW_ShaderCount = 0;
    #endif
    private FileStream sw_component_bin;
    private BinaryFormatter bf_component_bin;
    private StreamWriter sw_component_all;
    private StreamWriter sw_component;
    private int iSW_ComponentCount = 0;

    private Dictionary<string, string> dicObjectNames = new Dictionary<string, string>();

	void Start() {
        if (enabled)
        {
            // WWW Downloader
            //sWWWDownloaderExporter = gameObject.AddComponent<WWWDownloaderExporter>();

            // Sync Target Frame Rate or VSync
            if (bEnableSync)
            {
                Time.fixedDeltaTime = 1.0f / iFixedFrameRate;
                Application.targetFrameRate = iTargetFrameRate;
                QualitySettings.vSyncCount = iVSyncCount;
            }

            // Process
            Process();
        }
    }

    void FixedUpdate()
    {
        if (bFixedUpdateMode)
        {
            Process();
        }
    }

    void Update()
	{
        // Process
        if (!bFixedUpdateMode)
        {
            Process();
        }
	}

	void Process() {
		if (!bFirst) {
			if (bStartupFBX && !bEndFBX) {
				StartFBX();
			}
		}
		else {
			// Out Animation
			if (bOutAnimation) {
				if (bOutRealTime) {
					fTime += Time.deltaTime;
				}
				else {
					fTime += (1.0f / iFrameRate);
				}
				if (bNowFBX && !bEndFBX) {
                    // Export AnimTrans from Root (Create Off)
                    iFrame++;
                    if (bDebugLog) Debug.Log("ExportAnimTransformRoot(Create Off) Time:" + fTime + " Frame:" + iFrame);
					if (!bAnimationBufferRecMode)
					{
						FBXExporterSetAnimationTime(fTime);
					}
					ExportAnimTransformRoot(goRoot, false);
                }
			}

			// End
			if (fTimeRecEnd >= 0.0f)
			{
				if (fTime >= fTimeRecEnd)
				{
					bEndFBX = true;
				}
			}
			if (!bOutAnimation || bEndFBX)
			{
				if (!bOutAnimation)
				{
					if (bOutAnimationCustomFrame) {
						return;
					}
				}
				if (!bSave) {
					OnApplicationQuit();
				}
			}
		}
	}

    void StartFBX()
    {
        bool bOutFBX = true;
        if (bOutFBX)
        {
            bSave = false;
            dicOutputTextureNames.Clear();
            dicAnimBufferTRSs.Clear();
            dicObjectNames.Clear();

            // Init
            if (bDebugLog) Debug.Log("FBXExporterInit()");
            FBXExporterInit();

            // Export Tool Target
            switch (eExportToolTarget)
            {
                case EFBXExportToolTarget.eCustom:
                    break;
                case EFBXExportToolTarget.eFBXImporterForUnity:
                    bOutShaderCGFX = true;
                    bOutDirectControlPointsMode = true;
                    bOutRootBoneInverseMode = true;
                    break;
                case EFBXExportToolTarget.eUnity:
                    bOutShaderCGFX = false;
                    bOutDirectControlPointsMode = false;
                    bOutRootBoneInverseMode = false;
                    break;
                case EFBXExportToolTarget.eStingray:
                    bOutShaderCGFX = false;
                    bOutDirectControlPointsMode = true;
                    bOutRootBoneInverseMode = false;
                    break;
                case EFBXExportToolTarget.eDCCTools:
                    bOutShaderCGFX = false;
                    bOutDirectControlPointsMode = true;
                    bOutNormalSmoothing = true;
                    bOutRootBoneInverseMode = false;
                    break;
            }

            // Export Scene Root
            if (bDebugLog) Debug.Log("ExportSceneRoot()");
            fTime = fTimeStart;
            FBXExporterSetAnimationTime(fTime);
            ExportSceneRoot(bOutRootNode);

            // Filename EXT
            if (eFileFormat == EFileFormat.eFBX)
            {
                strFilenameEXT += ".fbx";
            }
            if (eFileFormat == EFileFormat.eCollada_DAE)
            {
                strFilenameEXT += ".dae";
            }
            if (eFileFormat == EFileFormat.eAlias_OBJ)
            {
                strFilenameEXT += ".obj";
            }
            /*
            if (eFileFormat == EFileFormat.e3DStudio_3DS)
            {
                strFilenameEXT += ".3ds";
            }
            */
            if (eFileFormat == EFileFormat.eAutoCAD_DXF)
            {
                strFilenameEXT += ".dxf";
            }

            // Export Trans from Root (Mesh Off & Rename)
            goRoot = transform.FindRootObject();
            if (bDebugLog) Debug.Log("ExportTransformRoot(Mesh Off) Time:" + fTime);
            ExportTransformRoot(goRoot, false, bObjectRename);
            if (bOutMesh)
            {
                #if UNITY_EDITOR
                if (bOutShader)
                {
                    FileInfo fi;
                    fi = new FileInfo(strFilename + strFilenameEXT + ".shader_data.txt");
                    sw_shader = fi.CreateText();
                    iSW_ShaderCount = 0;
                }
                #endif
                if (bOutComponent)
                {
                    Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
                    sw_component_bin = File.Open(strFilename + strFilenameEXT + ".component.bin", FileMode.Create);
                    FileInfo fi;
                    fi = new FileInfo(strFilename + strFilenameEXT + ".component_data.txt");
                    sw_component = fi.CreateText();
                    bf_component_bin = new BinaryFormatter();
                    iSW_ComponentCount = 0;
                }

                // Export Trans from Root (Mesh On)
                if (bDebugLog) Debug.Log("ExportTransformRoot(Mesh On) Time:" + fTime);
                ExportTransformRoot(goRoot, true, false);
            }

            // Export AnimTrans from Root (Create On)
            if (bOutAnimation || bOutAnimationCustomFrame)
            {
                if (bDebugLog) Debug.Log("ExportAnimTransformRoot(Create On) Time:" + fTime);
                FBXExporterSetAnimationStackBaseLayer(strStack, strLayer);
                ExportAnimTransformRoot(goRoot, true);
                bNowFBX = true;
            }
            bFirst = true;
        }
    }

    void OnApplicationQuit() {
		if (bFirst) {
			bool bOutFBX = true;
			if (!bSave && bOutFBX) {
				fTime = 0.0f;
				bFirst = false;
				bSave = true;

				// Export All Nodes AnimBuffer TRSs
				if (bAnimationBufferRecMode)
				{
					ExportAllNodesAnimBufferTRSs();
				}

				// Converter
				FBXExporterGeometryConverter(true);
				FBXExporterAxisSystem((int)eUpVector, (int)eFrontVector, (int)eCoordSystem);
				FBXExporterSystemUnit((int)eSystemUnit);

				// Save
				if (bDebugLog) Debug.Log("FBXExporterSave()");
                string[] strVersions = { "", "FBX201100", "FBX201200", "FBX201300", "FBX201400", "FBX201600", "FBX201800", "FBX201900" };
                string strFilename2 = strFilename;
                string strFileFormat2 = "";
                if (eFileFormat == EFileFormat.eFBX)
                {
                    strFilename2 += ".fbx";
                    strFileFormat2 = "";
                }
                if (eFileFormat == EFileFormat.eCollada_DAE)
                {
                    strFilename2 += ".dae";
                    strFileFormat2 = "Collada DAE(*.dae)";
                }
                if (eFileFormat == EFileFormat.eAlias_OBJ)
                {
                    strFilename2 += ".obj";
                    strFileFormat2 = "Alias OBJ(*.obj)";
                }
                /*
                if (eFileFormat == EFileFormat.e3DStudio_3DS)
                {
                    strFilename2 += ".3ds";
                    strFileFormat2 = "3D Studio 3DS(*.3ds)";
                }
                */
                if (eFileFormat == EFileFormat.eAutoCAD_DXF)
                {
                    strFilename2 += ".dxf";
                    strFileFormat2 = "AutoCAD DXF(*.dxf)";
                }
                FBXExporterSave(strFilename2, strVersions[(int)eFBXExportFileVersion], (int)eEFBXRenamingMode, (int)eFBXFileFormat, (string)strFileFormat2, bEmbedMedia, (eFBXFileFormat == EFBXFileFormat.eASCII));

                // Out Shader
                #if UNITY_EDITOR
                if (bOutShader)
				{
                    if (sw_shader != null)
                    {
                        sw_shader.Close();
                        FileInfo fi = new FileInfo(strFilename + strFilenameEXT + ".shader.txt");
                        sw_shader_all = fi.CreateText();
                        sw_shader_all.WriteLine("ShaderCount," + iSW_ShaderCount);
                        FileInfo fi2 = new FileInfo(strFilename + strFilenameEXT + ".shader_data.txt");
                        StreamReader sr_shader = fi2.OpenText();
                        sw_shader_all.Write(sr_shader.ReadToEnd());
                        sw_shader_all.Flush();
                        sw_shader_all.Close();
                        sr_shader.Close();
                        File.Delete(strFilename + strFilenameEXT + ".shader_data.txt");
                    }
				}
				// Out Component
				if (bOutComponent)
				{
                    if (sw_component != null)
                    {
                        sw_component.Close();
                        FileInfo fi = new FileInfo(strFilename + strFilenameEXT + ".component.txt");
                        sw_component_all = fi.CreateText();
                        sw_component_all.WriteLine("ComponentCount," + iSW_ComponentCount);
                        FileInfo fi2 = new FileInfo(strFilename + strFilenameEXT + ".component_data.txt");
                        StreamReader sr_component = fi2.OpenText();
                        sw_component_all.Write(sr_component.ReadToEnd());
                        sw_component_all.Flush();
                        sw_component_all.Close();
                        sr_component.Close();
                        sw_component_bin.Close();
                        File.Delete(strFilename + strFilenameEXT + ".component_data.txt");
                    }
                }
                #endif

                // Exit
                if (bDebugLog) Debug.Log("FBXExporterExit()");
				FBXExporterExit();
				if (bDebugLog && bTrialMode) Debug.LogWarning("[FBX Exporter for Unity] Free Trial version is limited over 150 frames(30fps = 5sec) animations! and Open Commercial WEB Accsess!");
				bNowFBX = false;
				bEndFBX = true;

				// Editor Auto Stop
                #if UNITY_EDITOR
				if (bEditorAutoStop)
				{
					EditorApplication.isPlaying = false;
				}
                #endif
			}
		}
	}

	void RenameObject(GameObject go, string strFormat)
	{
		string strRename = go.name;
		string strName = "";
		bool bLoop = true;
		int i = 0;
		do {
			if (dicObjectNames.TryGetValue(strRename, out strName)) {
				i++;
				strRename = string.Format (strFormat, go.name, i);
			} else {
				dicObjectNames.Add (strRename, go.name);
				go.name = strRename;
				bLoop = false;
			}
		} while (bLoop);
	}

	bool NoOutputInHierarchyObjectName(string strName)
	{
		foreach (string strNoOutputInHierarchyObjectName in strNoOutputInHierarchyObjectNames)
		{
			if (strName == strNoOutputInHierarchyObjectName)
			{
				return true;
			}
		}
		return false;
	}
	
	void ExportAnimTransformChildsFromParent(Transform transParent, bool bCreate)
    {
        if (NoOutputInHierarchyObjectName(transParent.name))
        {
            return;
        }
        for (int i = 0; i < transParent.transform.childCount; i++)
        {
			Transform trnsChild = transParent.transform.GetChild(i);
			if (!trnsChild.gameObject.activeInHierarchy) {
				continue;
			}
			if (NoOutputInHierarchyObjectName(trnsChild.name))
			{
				continue;
			}
			float[] fTranslation = new float[3];
			fTranslation[0] = -trnsChild.localPosition.x * fGlobalScale;
			fTranslation[1] = trnsChild.localPosition.y * fGlobalScale;
			fTranslation[2] = trnsChild.localPosition.z * fGlobalScale;
			float[] fRotation = new float[4];
			fRotation[0] = -trnsChild.localRotation.x;
			fRotation[1] = trnsChild.localRotation.y;
			fRotation[2] = trnsChild.localRotation.z;
			fRotation[3] = -trnsChild.localRotation.w;
			float[] fScale = new float[3];
            fScale[0] = trnsChild.localScale.x;
            fScale[1] = trnsChild.localScale.y;
            fScale[2] = trnsChild.localScale.z;
			if (bAnimationBufferRecMode)
			{
				SAnimBufferTRS sAnimBufferTRS;
				if (dicAnimBufferTRSs.TryGetValue(trnsChild.name, out sAnimBufferTRS))
				{
					sAnimBufferTRS.listTime.Add(fTime);
					sAnimBufferTRS.listLclPosition.Add(fTranslation[0]);
					sAnimBufferTRS.listLclPosition.Add(fTranslation[1]);
					sAnimBufferTRS.listLclPosition.Add(fTranslation[2]);
					sAnimBufferTRS.listLclRotation.Add(fRotation[0]);
					sAnimBufferTRS.listLclRotation.Add(fRotation[1]);
					sAnimBufferTRS.listLclRotation.Add(fRotation[2]);
					sAnimBufferTRS.listLclRotation.Add(fRotation[3]);
					sAnimBufferTRS.listLclScale.Add(fScale[0]);
					sAnimBufferTRS.listLclScale.Add(fScale[1]);
					sAnimBufferTRS.listLclScale.Add(fScale[2]);
				}
				else
				{
					sAnimBufferTRS.listTime = new List<float>();
					sAnimBufferTRS.listTime.Add(fTime);
					sAnimBufferTRS.listLclPosition = new List<float>();
					sAnimBufferTRS.listLclPosition.Add(fTranslation[0]);
					sAnimBufferTRS.listLclPosition.Add(fTranslation[1]);
					sAnimBufferTRS.listLclPosition.Add(fTranslation[2]);
					sAnimBufferTRS.listLclRotation = new List<float>();
					sAnimBufferTRS.listLclRotation.Add(fRotation[0]);
					sAnimBufferTRS.listLclRotation.Add(fRotation[1]);
					sAnimBufferTRS.listLclRotation.Add(fRotation[2]);
					sAnimBufferTRS.listLclRotation.Add(fRotation[3]);
					sAnimBufferTRS.listLclScale = new List<float>();
					sAnimBufferTRS.listLclScale.Add(fScale[0]);
					sAnimBufferTRS.listLclScale.Add(fScale[1]);
					sAnimBufferTRS.listLclScale.Add(fScale[2]);
					dicAnimBufferTRSs.Add(trnsChild.name, sAnimBufferTRS);
				}
			}
			else
			{
                FBXExporterSetAnimationTRS(trnsChild.name, fTranslation, fRotation, fScale, bCreate);
			}
            ExportAnimTransformChildsFromParent(trnsChild, bCreate);
        }
    }

    void ExportAnimTransformRoot(GameObject[] goRoot, bool bCreate)
    {
        for (int i = 0; i < goRoot.Length; i++)
        {
			Transform trnsRoot = goRoot[i].transform;
			if (!trnsRoot.gameObject.activeInHierarchy) {
				continue;
			}
			if (NoOutputInHierarchyObjectName(trnsRoot.name))
			{
				continue;
			}
			float[] fRootTranslation = new float[3];
			fRootTranslation[0] = -trnsRoot.localPosition.x * fGlobalScale;
			fRootTranslation[1] = trnsRoot.localPosition.y * fGlobalScale;
			fRootTranslation[2] = trnsRoot.localPosition.z * fGlobalScale;
			float[] fRootRotation = new float[4];
			fRootRotation[0] = -trnsRoot.localRotation.x;
			fRootRotation[1] = trnsRoot.localRotation.y;
			fRootRotation[2] = trnsRoot.localRotation.z;
			fRootRotation[3] = -trnsRoot.localRotation.w;
			float[] fRootScale = new float[3];
			fRootScale[0] = trnsRoot.localScale.x;
			fRootScale[1] = trnsRoot.localScale.y;
			fRootScale[2] = trnsRoot.localScale.z;
			if (bAnimationBufferRecMode)
			{
				SAnimBufferTRS sAnimBufferTRS;
				if (dicAnimBufferTRSs.TryGetValue(trnsRoot.name, out sAnimBufferTRS))
				{
					sAnimBufferTRS.listTime.Add(fTime);
					sAnimBufferTRS.listLclPosition.Add(fRootTranslation[0]);
					sAnimBufferTRS.listLclPosition.Add(fRootTranslation[1]);
					sAnimBufferTRS.listLclPosition.Add(fRootTranslation[2]);
					sAnimBufferTRS.listLclRotation.Add(fRootRotation[0]);
					sAnimBufferTRS.listLclRotation.Add(fRootRotation[1]);
					sAnimBufferTRS.listLclRotation.Add(fRootRotation[2]);
					sAnimBufferTRS.listLclRotation.Add(fRootRotation[3]);
					sAnimBufferTRS.listLclScale.Add(fRootScale[0]);
					sAnimBufferTRS.listLclScale.Add(fRootScale[1]);
					sAnimBufferTRS.listLclScale.Add(fRootScale[2]);
				}
				else 
				{
					sAnimBufferTRS.listTime = new List<float>();
					sAnimBufferTRS.listTime.Add(fTime);
					sAnimBufferTRS.listLclPosition = new List<float>();
					sAnimBufferTRS.listLclPosition.Add(fRootTranslation[0]);
					sAnimBufferTRS.listLclPosition.Add(fRootTranslation[1]);
					sAnimBufferTRS.listLclPosition.Add(fRootTranslation[2]);
					sAnimBufferTRS.listLclRotation = new List<float>();
					sAnimBufferTRS.listLclRotation.Add(fRootRotation[0]);
					sAnimBufferTRS.listLclRotation.Add(fRootRotation[1]);
					sAnimBufferTRS.listLclRotation.Add(fRootRotation[2]);
					sAnimBufferTRS.listLclRotation.Add(fRootRotation[3]);
					sAnimBufferTRS.listLclScale = new List<float>();
					sAnimBufferTRS.listLclScale.Add(fRootScale[0]);
					sAnimBufferTRS.listLclScale.Add(fRootScale[1]);
					sAnimBufferTRS.listLclScale.Add(fRootScale[2]);
					dicAnimBufferTRSs.Add(trnsRoot.name, sAnimBufferTRS);
				}
			}
			else
			{
                FBXExporterSetAnimationTRS(trnsRoot.name, fRootTranslation, fRootRotation, fRootScale, bCreate);
			}
			ExportAnimTransformChildsFromParent(trnsRoot, bCreate);
        }
	}

	void ExportAllNodesAnimBufferTRSs()
	{
		foreach (string key in dicAnimBufferTRSs.Keys)
		{
            if (NoOutputInHierarchyObjectName(key))
            {
                continue;
            }
            SAnimBufferTRS sAnimBufferTRS = dicAnimBufferTRSs[key];
			float[] fTimes = sAnimBufferTRS.listTime.ToArray();
			float[] fTranslations = sAnimBufferTRS.listLclPosition.ToArray();
			float[] fRotations = sAnimBufferTRS.listLclRotation.ToArray();
			float[] fScales = sAnimBufferTRS.listLclScale.ToArray();
			FBXExporterSetAnimationsTRS(key, fTimes.Length, fTimes, fTranslations, fRotations, fScales, true);
		}
	}
    
    void ExportTransformChildsFromParent(Transform transParent, bool bMesh, bool bRename)
    {
        if (NoOutputInHierarchyObjectName(transParent.name))
        {
            return;
        }
        for (int i = 0; i < transParent.transform.childCount; i++) {
			Transform trnsChild = transParent.transform.GetChild(i);
			if (!trnsChild.gameObject.activeInHierarchy) {
				continue;
			}
			if (NoOutputInHierarchyObjectName(trnsChild.name))
			{
				continue;
			}
			ExportSetVetexIndex(transParent.name, trnsChild.gameObject, bMesh, bRename);
            ExportTransformChildsFromParent(trnsChild, bMesh, bRename);
		}
	}

	void ExportTransformRoot(GameObject[] goRoot, bool bMesh, bool bRename) {
		for (int i = 0; i < goRoot.Length; i++) {
			if (!goRoot[i].gameObject.activeInHierarchy) {
				continue;
			}
			if (NoOutputInHierarchyObjectName(goRoot[i].name))
			{
				continue;
			}
			ExportSetVetexIndex(strOutRootNodeName, goRoot[i].gameObject, bMesh, bRename);
			for (int j = 0; j < goRoot[i].transform.childCount; j++) {
				Transform trnsChild = goRoot[i].transform.GetChild(j);
				if (!trnsChild.gameObject.activeInHierarchy) {
					continue;
				}
				ExportSetVetexIndex(goRoot[i].name, trnsChild.gameObject, bMesh, bRename);
				ExportTransformChildsFromParent(trnsChild, bMesh, bRename);
			}
		}
	}

	void ExportSceneRoot(bool bOutRootNode)
	{
		if (bOutRootNode) {
			// Transform
			float[] fLclTranslation = new float[3];
			fLclTranslation [0] = -0.0f;
			fLclTranslation [1] = 0.0f;
			fLclTranslation [2] = 0.0f;
			float[] fLclRotation = new float[4];
			Quaternion qRot = Quaternion.Euler (0.0f, 0.0f, 0.0f);
			fLclRotation [0] = -qRot.x;
			fLclRotation [1] = qRot.y;
			fLclRotation [2] = qRot.z;
			fLclRotation [3] = -qRot.w;
			float[] fLclScale = new float[3];
			fLclScale [0] = 1.0f;
			fLclScale [1] = 1.0f;
			fLclScale [2] = 1.0f;
			FBXExporterSetNode ("", strOutRootNodeName, fLclTranslation, fLclRotation, fLclScale);
		}
		else {
			strOutRootNodeName = "";
		}
	}

    void ExportSetVetexIndex(string strParentNodeName, GameObject goChild, bool bMesh, bool bRename)
    {
        // Mesh
        if (bMesh)
		{
            if (bOutComponent)
            {
                Component[] components = goChild.GetComponents<Component>();
                sw_component.WriteLine(goChild.name);
                sw_component.WriteLine("{");
                for (int j = 0; j < components.Length; j++)
                {
                    if (components[j] != null)
                    {
                        string strComponentType = components[j].GetType().ToString();
                        if (strComponentType.IndexOf("UnityEngine.Transform") < 0)
                        {
                            sw_component.WriteLine("\t" + strComponentType);
                            sw_component.WriteLine("\tBinaryPosition," + sw_component_bin.Position);
                            System.Reflection.FieldInfo[] fields = components[j].GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                            sw_component.WriteLine("\tField {");
                            for (int f = 0; f < fields.Length; f++)
                            {
                                string strField = fields[f].ToString();
                                if (strField.IndexOf("UnityEngine.Component") < 0)
                                {
                                    if (fields[f].GetValue(components[j]) != null)
                                    {
                                        sw_component.WriteLine("\t\t" + fields[f].ToString() + "," + fields[f].GetValue(components[j]).ToString());
                                        bf_component_bin.Serialize(sw_component_bin, fields[f]);
                                    }
                                }
                            }
                            sw_component.WriteLine("\t}");
                            System.Reflection.PropertyInfo[] propertys = components[j].GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                            sw_component.WriteLine("\tProperty {");
                            for (int p = 0; p < propertys.Length; p++)
                            {
                                string strField = propertys[p].ToString();
                                if (strField.IndexOf("UnityEngine.Component") < 0 && strField.IndexOf("UnityEngine.Transform") < 0)
                                {
                                    if (propertys[p].GetValue(components[j], null) != null)
                                    {
                                        sw_component.WriteLine("\t\t" + propertys[p].ToString() + "," + propertys[p].GetValue(components[j], null).ToString());
                                        bf_component_bin.Serialize(sw_component_bin, propertys[p]);
                                    }
                                }
                            }
                            sw_component.WriteLine("\t}");
                        }
                    }
                }
                sw_component.WriteLine("}");
                sw_component.Flush();
                iSW_ComponentCount++;
            }

            //
            SkinnedMeshRenderer meshSkin = goChild.GetComponent<SkinnedMeshRenderer>();
            MeshFilter meshF = goChild.GetComponent<MeshFilter>();
            if (meshSkin || meshF)
            {
				Mesh mesh = null;
                if (meshSkin)
                {
                	mesh = meshSkin.sharedMesh;
				}
                else
                {
                    mesh = meshF.mesh;
				}
                if (mesh)
                {
					// Vertex
                    float[] fVertex = new float[mesh.vertices.Length * 3];
                    Vector3[] vertices = mesh.vertices;
                    int i = 0;
					foreach (Vector3 vertex in vertices)
                    {
						fVertex[i + 0] = -vertex.x * fGlobalScale;
						fVertex[i + 1] = vertex.y * fGlobalScale;
						fVertex[i + 2] = vertex.z * fGlobalScale;
                        i = i + 3;
                    }

                    // Normal
                    float[] fNormal = new float[mesh.vertices.Length * 3];
                    Vector3[] normals = mesh.normals;
                    i = 0;
                    foreach (Vector3 nomral in normals)
                    {
						fNormal[i + 0] = -nomral.x;
						fNormal[i + 1] = nomral.y;
						fNormal[i + 2] = nomral.z;
                        i = i + 3;
                    }

                    // UV
                    float[] fUV = new float[mesh.vertices.Length * 2];
                    Vector2[] uvs = mesh.uv;
                    i = 0;
                    foreach (Vector3 uv in uvs)
                    {
                        fUV[i + 0] = uv.x;
                        fUV[i + 1] = uv.y;
                        i = i + 2;
                    }

                    // Index
                    int iIndexCount = 0;
                    for (i = 0; i < mesh.subMeshCount; i++)
                    {
                        int[] iVIndexss = mesh.GetTriangles(i);
                        iIndexCount += iVIndexss.Length;
                    }
                    int[] iVIndexs = new int[iIndexCount * 3];
                    int[] iVIndexIDs = new int[iIndexCount];
                    int k = 0;
                    int m = 0;
                    for (i = 0; i < mesh.subMeshCount; i++)
                    {
                        int[] iVIndexss = mesh.GetTriangles(i);
                        for (int j = 0; j < (iVIndexss.Length / 3); j++)
                        {
                            iVIndexs[k + (j * 3) + 0] = iVIndexss[(j * 3) + 0];
                            iVIndexs[k + (j * 3) + 1] = iVIndexss[(j * 3) + 2];
                            iVIndexs[k + (j * 3) + 2] = iVIndexss[(j * 3) + 1];
                            // MaterialID
                            iVIndexIDs[m + j] = i;
                        }
                        k = k + iVIndexss.Length;
                        m = m + iVIndexss.Length / 3;
                    }

                    // Transform
                    float[] fLclTranslation = new float[3];
					fLclTranslation[0] = -goChild.transform.localPosition.x * fGlobalScale;
					fLclTranslation[1] = goChild.transform.localPosition.y * fGlobalScale;
					fLclTranslation[2] = goChild.transform.localPosition.z * fGlobalScale;
					float[] fLclRotation = new float[4];
					fLclRotation[0] = -goChild.transform.localRotation.x;
					fLclRotation[1] = goChild.transform.localRotation.y;
					fLclRotation[2] = goChild.transform.localRotation.z;
					fLclRotation[3] = -goChild.transform.localRotation.w;
					float[] fLclScale = new float[3];
					fLclScale[0] = goChild.transform.localScale.x;
					fLclScale[1] = goChild.transform.localScale.y;
					fLclScale[2] = goChild.transform.localScale.z;

                    FBXExporterSetNodeMesh(strParentNodeName, goChild.name, fVertex, fNormal, fUV, fVertex.Length, iVIndexs, iVIndexIDs, iVIndexs.Length, fLclTranslation, fLclRotation, fLclScale, bOutDirectControlPointsMode, bOutNormalSmoothing);

                    // BlendShape
                    if (bOutBlendShape)
                    {
                        if (mesh.blendShapeCount > 0)
                        {
                            FBXExporterSetNodeMeshMorphFirst();
                            Vector3[] vecVertexMorph = new Vector3[mesh.vertices.Length];
                            Vector3[] vecNormalMorph = new Vector3[mesh.vertices.Length];
                            //Vector3[] vecTangentMorph = new Vector3[mesh.vertices.Length];
                            for (i = 0; i < mesh.blendShapeCount; i++)
                            {
                                for (m = 0; m < mesh.GetBlendShapeFrameCount(i); m++)
                                {
                                    string strBlendShapeName = mesh.GetBlendShapeName(i);
                                    Debug.Log(strBlendShapeName);
                                    int iBlendShapeID = mesh.GetBlendShapeIndex(strBlendShapeName);
                                    mesh.GetBlendShapeFrameVertices(iBlendShapeID, m, vecVertexMorph, vecNormalMorph, null);
                                    float[] fVertexMorph = new float[mesh.vertices.Length * 3];
                                    float[] fNormalMorph = new float[mesh.vertices.Length * 3];
                                    k = 0;
                                    for (int j = 0; j < mesh.vertices.Length * 3; j = j + 3)
                                    {
                                        fVertexMorph[j] = vecVertexMorph[k].x;
                                        fVertexMorph[j + 1] = vecVertexMorph[k].y;
                                        fVertexMorph[j + 2] = vecVertexMorph[k].z;
                                        fNormalMorph[j] = vecNormalMorph[k].x;
                                        fNormalMorph[j + 1] = vecNormalMorph[k].y;
                                        fNormalMorph[j + 2] = vecNormalMorph[k].z;
                                        k++;
                                    }
                                    FBXExporterSetNodeMeshMorph(strBlendShapeName, mesh.vertices.Length, fVertexMorph, fNormalMorph);
                                }
                            }
                        }
                    }

                    // Material
                    Renderer rendrMain = goChild.GetComponent<Renderer>();
                    if (rendrMain)
                    {
                        Material[] matMains;
                        if (meshSkin)
                        {
                            matMains = meshSkin.sharedMaterials;
                        }
                        else
                        {
                            matMains = goChild.GetComponent<Renderer>().materials;
                        }
                        #if UNITY_EDITOR
                        if (bOutShader)
						{
							sw_shader.WriteLine(goChild.name);
							sw_shader.WriteLine("{");
							sw_shader.WriteLine("\tMaterialCount," + matMains.Length);
						}
                        #endif
                        for (i = 0; i < matMains.Length; i++)
                        {
                            Material material = matMains[i];
                            if (material)
                            {
                                float[] fAmbient = new float[3];
                                fAmbient[0] = 0.0f;
                                fAmbient[1] = 0.0f;
                                fAmbient[2] = 0.0f;
                                float[] fAmbientFactor = new float[1];
                                fAmbientFactor[0] = 0.0f;
                                float[] fDiffuse = new float[3];
                                fDiffuse[0] = 1.0f;
                                fDiffuse[1] = 1.0f;
                                fDiffuse[2] = 1.0f;
                                float[] fDiffuseFactor = new float[1];
                                fDiffuseFactor[0] = 1.0f;
                                float[] fEmissive = new float[3];
                                fEmissive[0] = 0.0f;
                                fEmissive[1] = 0.0f;
                                fEmissive[2] = 0.0f;
                                float[] fEmissiveFactor = new float[1];
                                float[] fBump = new float[3];
                                fBump[0] = 0.0f;
                                fBump[1] = 0.0f;
                                fBump[2] = 0.0f;
                                float[] fBumpFactor = new float[1];
                                fBumpFactor[0] = 0.0f;
                                float[] fNormalMap = new float[3];
                                fNormalMap[0] = 0.0f;
                                fNormalMap[1] = 0.0f;
                                fNormalMap[2] = 0.0f;
                                float[] fTransparentColor = new float[3];
                                fTransparentColor[0] = 0.0f;
                                fTransparentColor[1] = 0.0f;
                                fTransparentColor[2] = 0.0f;
                                float[] fTransparencyFactor = new float[1];
                                fTransparencyFactor[0] = 0.0f;
                                float[] fDisplacementColor = new float[3];
                                fDisplacementColor[0] = 0.0f;
                                fDisplacementColor[1] = 0.0f;
                                fDisplacementColor[2] = 0.0f;
                                float[] fDisplacementFactor = new float[1];
                                fDisplacementFactor[0] = 0.0f;
                                float[] fVectorDisplacementColor = new float[3];
                                fVectorDisplacementColor[0] = 0.0f;
                                fVectorDisplacementColor[1] = 0.0f;
                                fVectorDisplacementColor[2] = 0.0f;
                                float[] fVectorDisplacementFactor = new float[1];
                                fVectorDisplacementFactor[0] = 0.0f;
                                if (material.HasProperty("_Color"))
                                {
                                    //Color colAmbient = material.GetColor("_Color");
                                    fAmbient[0] = 0.0f;//colAmbient.r;
                                    fAmbient[1] = 0.0f;//colAmbient.g;
                                    fAmbient[2] = 0.0f;//colAmbient.b;
                                    Color colDiffuse = material.GetColor("_Color");
                                    fDiffuse[0] = colDiffuse.r;
                                    fDiffuse[1] = colDiffuse.g;
                                    fDiffuse[2] = colDiffuse.b;
                                    //Color colEmissive = material.GetColor("_Color");
                                    fEmissive[0] = 0.0f;//colEmissive.r;
                                    fEmissive[1] = 0.0f;//colEmissive.g;
                                    fEmissive[2] = 0.0f;//colEmissive.b;
                                    fTransparencyFactor[0] = 0.0f;
                                }

                                // Texture
                                string strDestPathTextureAmbient = "";
                                if (material.HasProperty(strShaderTextureAmbientParameter))
                                {
                                    strDestPathTextureAmbient = ConvertTexture(material.GetTexture(strShaderTextureAmbientParameter) as Texture2D);
                                }
                                string strDestPathTextureDiffuse = "";
                                if (material.HasProperty(strShaderTextureDiffuseParameter))
                                {
                                    strDestPathTextureDiffuse = ConvertTexture(material.GetTexture(strShaderTextureDiffuseParameter) as Texture2D);
                                }
                                string strDestPathTexturEmissive = "";
                                if (material.HasProperty(strShaderTextureEmissiveParameter))
                                {
                                    strDestPathTexturEmissive = ConvertTexture(material.GetTexture(strShaderTextureEmissiveParameter) as Texture2D);
                                }
                                string strDestPathTextureBumpMap = "";
                                if (material.HasProperty(strShaderTextureBumpMapParameter))
                                {
                                    strDestPathTextureBumpMap = ConvertTexture(material.GetTexture(strShaderTextureBumpMapParameter) as Texture2D);
                                }
                                string strDestPathTextureNormalMap = "";
                                if (material.HasProperty(strShaderTextureNormalMapParameter))
                                {
                                    strDestPathTextureNormalMap = ConvertTexture(material.GetTexture(strShaderTextureNormalMapParameter) as Texture2D);
                                }
                                string strDestPathTextureTransparent = "";
                                if (material.HasProperty(strShaderTextureTransparentParameter))
                                {
                                    strDestPathTextureTransparent = ConvertTexture(material.GetTexture(strShaderTextureTransparentParameter) as Texture2D);
                                }
                                string strDestPathTextureDisplacement = "";
                                if (material.HasProperty(strShaderTextureDisplacementParameter))
                                {
                                    strDestPathTextureDisplacement = ConvertTexture(material.GetTexture(strShaderTextureDisplacementParameter) as Texture2D);
                                }
                                string strDestPathTextureReflection = "";
                                if (material.HasProperty(strShaderTextureReflectionParameter))
                                {
                                    strDestPathTextureReflection = ConvertTexture(material.GetTexture(strShaderTextureReflectionParameter) as Texture2D);
                                }
                                FBXExporterAddMaterial(goChild.name, material.name, fAmbient, fAmbientFactor, fDiffuse, fDiffuseFactor, fEmissive, fEmissiveFactor, fBump, fBumpFactor, fNormalMap, fTransparentColor, fTransparencyFactor, fDisplacementColor, fDisplacementFactor, fVectorDisplacementColor, fVectorDisplacementFactor, strDestPathTextureAmbient, strDestPathTextureDiffuse, strDestPathTexturEmissive, strDestPathTextureBumpMap, strDestPathTextureNormalMap, strDestPathTextureTransparent, strDestPathTextureDisplacement, strDestPathTextureReflection);
                                #if UNITY_EDITOR
                                if (bOutShader)
                                {
                                    sw_shader.WriteLine("\tMaterialName," + material.name);
									Shader shader = material.shader;
									sw_shader.WriteLine("\tShaderName," + shader.name);
									sw_shader.WriteLine("\t{");
                                    int iShaderTextureCount = 0;
									for (int j = 0; j < ShaderUtil.GetPropertyCount(shader); j++) {
										string strPropertyName = ShaderUtil.GetPropertyName(shader, j);
										switch (ShaderUtil.GetPropertyType(shader, j))
										{
											case ShaderUtil.ShaderPropertyType.Color:
											{
												Color col = material.GetColor(strPropertyName);
												sw_shader.WriteLine("\t\tColor," + strPropertyName + "," + col.r + "," + col.g + "," + col.b + "," + col.a);
												break;
											}
											case ShaderUtil.ShaderPropertyType.Vector:
											{
												Vector4 vec = material.GetVector(strPropertyName);
												sw_shader.WriteLine("\t\tVector," + strPropertyName + "," + vec.x + "," + vec.y + "," + vec.z + "," + vec.w);
												break;
											}
											case ShaderUtil.ShaderPropertyType.Float:
											{
												float f = material.GetFloat(strPropertyName);
												sw_shader.WriteLine("\t\tFloat," + strPropertyName + "," + f);
												break;
											}
											case ShaderUtil.ShaderPropertyType.Range:
											{
												float f = material.GetFloat(strPropertyName);
												sw_shader.WriteLine("\t\tRange," + strPropertyName + "," + f);
												break;
											}
											case ShaderUtil.ShaderPropertyType.TexEnv:
											{
												string strShaderDestPathTexture = ConvertTexture(material.GetTexture(strPropertyName) as Texture2D);
												sw_shader.WriteLine("\t\tTexEnv," + strPropertyName + "," + strShaderDestPathTexture);
                                                FBXExporterSetShaderTextureName(iShaderTextureCount, strShaderDestPathTexture);
                                                iShaderTextureCount++;
                                                break;
											}
										}
									}
                                    if (bOutShaderCGFX)
                                    {
                                        FBXExporterAddShaderMaterial(goChild.name, "", shader.name, shader.name + "Tex", iShaderTextureCount);
                                    }
                                    sw_shader.WriteLine("\t}");
								}
                                #endif
							}
                        }
                        #if UNITY_EDITOR
                        if (bOutShader)
						{
							sw_shader.WriteLine("}");
							sw_shader.Flush();
							iSW_ShaderCount++;
						}
                        #endif
                    }

                    // Skin Mesh
                    if (meshSkin)
                    {
						if (meshSkin.bones.Length > 0)
						{
							for (i = 0; i < meshSkin.bones.Length; i++)
	                        {
								FBXExporterSetSkinBoneName(goChild.name, i, meshSkin.bones[i].name);
	                        }
							if (bOutDirectControlPointsMode)
							{
		                        int iBoneWeightCount = mesh.boneWeights.Length;
		                        BoneWeight[] boneweight = mesh.boneWeights;
		                        int[] iBoneIndex = new int[4 * iBoneWeightCount];
		                        float[] fBoneWeight = new float[4 * iBoneWeightCount];
								for (i = 0; i < iBoneWeightCount; i++)
		                        {
		                            iBoneIndex[(i * 4) + 0] = boneweight[i].boneIndex0;
		                            iBoneIndex[(i * 4) + 1] = boneweight[i].boneIndex1;
		                            iBoneIndex[(i * 4) + 2] = boneweight[i].boneIndex2;
		                            iBoneIndex[(i * 4) + 3] = boneweight[i].boneIndex3;
		                            fBoneWeight[(i * 4) + 0] = boneweight[i].weight0;
		                            fBoneWeight[(i * 4) + 1] = boneweight[i].weight1;
		                            fBoneWeight[(i * 4) + 2] = boneweight[i].weight2;
		                            fBoneWeight[(i * 4) + 3] = boneweight[i].weight3;
								}
								if (iBoneWeightCount > 0) {
                                    if (bOutRootBoneInverseMode)
                                    {
                                        string strChildRootName = goChild.name;
                                        GameObject goChildT = goChild;
                                        bool bLoop = true;
                                        while (bLoop)
                                        {
                                            strChildRootName = goChildT.name;
                                            goChildT = goChildT.transform.parent.gameObject;
                                            foreach (GameObject goRootT in goRoot)
                                            {
                                                if (goRootT.name == goChildT.name)
                                                {
                                                    strChildRootName = goChildT.name;
                                                    bLoop = false;
                                                }
                                            }
                                        }
                                        FBXExporterSetSkinBoneWeight(goChild.name, strChildRootName, iBoneIndex, fBoneWeight, mesh.vertices.Length);
                                        FBXExporterSetBindPose(goChild.name, strChildRootName);
                                    }
                                    else
                                    {
                                        FBXExporterSetSkinBoneWeight(goChild.name, "", iBoneIndex, fBoneWeight, mesh.vertices.Length);
                                        FBXExporterSetBindPose(goChild.name, "");
                                    }
                                }
							}
							else
							{
								int iBoneWeightCount = iVIndexs.Length;
								BoneWeight[] boneweight = mesh.boneWeights;
								int[] iBoneIndex = new int[4 * iBoneWeightCount];
								float[] fBoneWeight = new float[4 * iBoneWeightCount];
								for (i = 0; i < iBoneWeightCount; i++)
								{
									int index = iVIndexs[i];
									iBoneIndex[(i * 4) + 0] = boneweight[index].boneIndex0;
									iBoneIndex[(i * 4) + 1] = boneweight[index].boneIndex1;
									iBoneIndex[(i * 4) + 2] = boneweight[index].boneIndex2;
									iBoneIndex[(i * 4) + 3] = boneweight[index].boneIndex3;
									fBoneWeight[(i * 4) + 0] = boneweight[index].weight0;
									fBoneWeight[(i * 4) + 1] = boneweight[index].weight1;
									fBoneWeight[(i * 4) + 2] = boneweight[index].weight2;
									fBoneWeight[(i * 4) + 3] = boneweight[index].weight3;
								}
								if (iBoneWeightCount > 0) {
                                    if (bOutRootBoneInverseMode)
                                    {
                                        string strChildRootName = goChild.name;
                                        GameObject goChildT = goChild;
                                        bool bLoop = true;
                                        while (bLoop)
                                        {
                                            strChildRootName = goChildT.name;
                                            goChildT = goChildT.transform.parent.gameObject;
                                            foreach (GameObject goRootT in goRoot)
                                            {
                                                if (goRootT.name == goChildT.name)
                                                {
                                                    strChildRootName = goChildT.name;
                                                    bLoop = false;
                                                }
                                            }
                                        }
                                        FBXExporterSetSkinBoneWeight(goChild.name, strChildRootName, iBoneIndex, fBoneWeight, iBoneWeightCount);
                                        FBXExporterSetBindPose(goChild.name, strChildRootName);
                                    }
                                    else
                                    {
                                        FBXExporterSetSkinBoneWeight(goChild.name, "", iBoneIndex, fBoneWeight, iBoneWeightCount);
                                        FBXExporterSetBindPose(goChild.name, "");
                                    }
                                }
                            }
						}
					}
				}
			}
		}
		else
		{
			// Transform
			float[] fLclTranslation = new float[3];
			fLclTranslation[0] = -goChild.transform.localPosition.x * fGlobalScale;
			fLclTranslation[1] = goChild.transform.localPosition.y * fGlobalScale;
			fLclTranslation[2] = goChild.transform.localPosition.z * fGlobalScale;
			float[] fLclRotation = new float[4];
			fLclRotation[0] = -goChild.transform.localRotation.x;
			fLclRotation[1] = goChild.transform.localRotation.y;
			fLclRotation[2] = goChild.transform.localRotation.z;
			fLclRotation[3] = -goChild.transform.localRotation.w;
			float[] fLclScale = new float[3];
			fLclScale[0] = goChild.transform.localScale.x;
			fLclScale[1] = goChild.transform.localScale.y;
			fLclScale[2] = goChild.transform.localScale.z;
			if (bRename)
			{
				RenameObject(goChild, strObjectRenameFormat);
			}
			FBXExporterSetNode(strParentNodeName, goChild.name, fLclTranslation, fLclRotation, fLclScale);
		}
	}

    Texture2D NormalMap(string strDestPathTexture, Texture2D source, float strength)
    {
        strength = Mathf.Clamp(strength, 0.0F, 1.0F);

        Texture2D normalTexture;
        float xLeft;
        float xRight;
        float yUp;
        float yDown;
        float yDelta;
        float xDelta;

        normalTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);

        for (int y = 0; y < normalTexture.height; y++)
        {
            for (int x = 0; x < normalTexture.width; x++)
            {
                xLeft = source.GetPixel(x - 1, y).grayscale * strength;
                xRight = source.GetPixel(x + 1, y).grayscale * strength;
                yUp = source.GetPixel(x, y - 1).grayscale * strength;
                yDown = source.GetPixel(x, y + 1).grayscale * strength;
                xDelta = ((xLeft - xRight) + 1) * 0.5f;
                yDelta = ((yUp - yDown) + 1) * 0.5f;
                normalTexture.SetPixel(x, y, new Color(xDelta, yDelta, 1.0f, yDelta));
            }
        }
        normalTexture.Apply();

        //Code for exporting the image to assets folder
        System.IO.File.WriteAllBytes(strDestPathTexture, normalTexture.EncodeToPNG());

        return normalTexture;
    }

    string ConvertTexture(Texture2D tex2d)
	{
		string strDestPathTexture = "";
		if (tex2d != null)
		{
			string strTexture = tex2d.name;
			if (strTexture != "")
			{
				string strEmbedMediaPath = "";
				if (bImportEmbedMediaTexturePath)
				{
					strEmbedMediaPath = strFilename.Replace(".fbx", "").Replace(".FBX", "") + ".fbm/";
				}
				if (!dicOutputTextureNames.TryGetValue(strTexture, out strDestPathTexture))
				{
					string strAssetPathDiffuseTex = "";
					string strDestDiffuseTex = "";
					string strDestDiffuseTex2 = "";
					string strDataPath = "";
					switch (eImportTextrureMode)
					{
					case EFBXImportTextrureMode.eLocalEmbedMedia:
					case EFBXImportTextrureMode.eLocalProject:
					case EFBXImportTextrureMode.eLocalStreamingAssets:
						strDataPath = Application.dataPath.Replace("/Assets", "");
						string[] strFilePaths = strDataPath.Split('/');
						strDataPath = "";
						int iDataPathLength = strFilePaths.Length;
						if (strFilePaths[iDataPathLength - 1].LastIndexOf("_Data") > 0)
						{
							iDataPathLength = iDataPathLength - 1;
						}
						for (int j = 0; j < iDataPathLength; j++)
						{
							strDataPath += strFilePaths[j] + "/";
						}
						break;
					}
					switch (eImportTextrureMode)
					{
					case EFBXImportTextrureMode.eAutoImportAssets:
#if UNITY_EDITOR
						strAssetPathDiffuseTex = AssetDatabase.GetAssetPath(tex2d);
						string[] strAssetPathDiffuseTexs = strAssetPathDiffuseTex.Split('/');
						string strAssetPathDiffuseTex2 = "";
						for (int j = 0; j < strAssetPathDiffuseTexs.Length - 1; j++)
						{
							strAssetPathDiffuseTex2 += strAssetPathDiffuseTexs[j] + "/";
						}
						string[] strDestDiffuseTexs = strAssetPathDiffuseTexs[strAssetPathDiffuseTexs.Length - 1].Split('.');
						strDestDiffuseTex = strDestDiffuseTexs[0];
						strDestDiffuseTex2 = strAssetPathDiffuseTexs[strAssetPathDiffuseTexs.Length - 1];
#else
						strAssetPathDiffuseTex = strDataPath + strLocalEmbedMediaTexturePath + strEmbedMediaPath + strTexture;
						strDestDiffuseTex = tex2d.name;
						strDestDiffuseTex2 = tex2d.name;
#endif
						break;
					case EFBXImportTextrureMode.eLocalEmbedMedia:
						strAssetPathDiffuseTex = strDataPath + strLocalEmbedMediaTexturePath + strEmbedMediaPath + strTexture;
						strDestDiffuseTex = tex2d.name;
						strDestDiffuseTex2 = tex2d.name;
						break;
					case EFBXImportTextrureMode.eLocalProject:
						strAssetPathDiffuseTex = strDataPath + strLocalProjectTexturePath + strEmbedMediaPath + strTexture;
						strDestDiffuseTex = tex2d.name;
						strDestDiffuseTex2 = tex2d.name;
						break;
					case EFBXImportTextrureMode.eLocalStreamingAssets:
						strAssetPathDiffuseTex = Application.streamingAssetsPath + "/" + strLocalStreamingAssetsTexturePath + strEmbedMediaPath + strTexture;
						strDestDiffuseTex = tex2d.name;
						strDestDiffuseTex2 = tex2d.name;
						break;
					case EFBXImportTextrureMode.eLocalAssets:
						strAssetPathDiffuseTex = strDataPath + strLocalAssetsTexturePath + strTexture;
						strDestDiffuseTex = tex2d.name;
						strDestDiffuseTex2 = tex2d.name;
						break;
					case EFBXImportTextrureMode.eLocalFullPath:
						strAssetPathDiffuseTex = strLocalFullPathTexturePath + strEmbedMediaPath + strTexture;
						strDestDiffuseTex = tex2d.name;
						strDestDiffuseTex2 = tex2d.name;
						break;
					}
					string strOutPath = Directory.GetCurrentDirectory() + "/" + strOutputTexturePath + strEmbedMediaPath;
					if (!Directory.Exists(strOutPath))
					{
						Directory.CreateDirectory(strOutPath);
					}
					if (bConvertAllPNG)
					{
#if UNITY_EDITOR
						string assetPath = AssetDatabase.GetAssetPath(tex2d);
						TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
						//if (importer.isReadable == false)
						{
							//importer.textureType = TextureImporterType.Default;
							importer.isReadable = true;
							AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
						}
                        if (importer.textureType == TextureImporterType.NormalMap)
                        {
                            strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex + ".png";
                            NormalMap(strDestPathTexture, tex2d, 1.0f);
                            if (bDebugLog) Debug.Log("Output PNG Normal Texture:<" + strDestPathTexture + ">");
                        }
                        else
                        {
                            Color[] pix = tex2d.GetPixels();
                            Texture2D tex2d2 = new Texture2D(tex2d.width, tex2d.height, TextureFormat.RGBA32, true);
                            tex2d2.SetPixels(pix);
                            tex2d2.Apply();
                            byte[] bytes = tex2d2.EncodeToPNG();
                            strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex;
                            bool bExists = bOverWriteTexture;
                            if (File.Exists(strDestPathTexture + ".png") && !bOverWriteTexture)
                            {
                                bExists = false;
                            }
                            if (bOutTexture && bExists)
                            {
                                if (bDebugLog) Debug.Log("Output PNG Texture:<" + strDestPathTexture + ".png>");
                                File.WriteAllBytes(strDestPathTexture + ".png", bytes);
                            }
                        }
                        strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex;
#else
                        Color[] pix = tex2d.GetPixels();
                        Texture2D tex2d2 = new Texture2D(tex2d.width, tex2d.height, TextureFormat.RGBA32, true);
                        tex2d2.SetPixels(pix);
                        tex2d2.Apply();
                        byte[] bytes = tex2d2.EncodeToPNG();
                        strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex;
                        bool bExists = bOverWriteTexture;
                        if (File.Exists(strDestPathTexture + ".png") && !bOverWriteTexture)
                        {
                            bExists = false;
                        }
                        if (bOutTexture && bExists)
                        {
                            if (bDebugLog) Debug.Log("Output PNG Texture:<" + strDestPathTexture + ".png>");
                            File.WriteAllBytes(strDestPathTexture + ".png", bytes);
                        }
                        strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex;
                        /*
						strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex2;
						if (bOutTexture) {
							// strOutPath + strAssetPathDiffuseTexs[strAssetPathDiffuseTexs.Length - 1]
							if (File.Exists(strAssetPathDiffuseTex + ".png"))
							{
								strAssetPathDiffuseTex += ".png";
								strDestPathTexture += ".png";
							}
							else 
							{
								if (File.Exists(strAssetPathDiffuseTex + ".tga"))
								{
									strAssetPathDiffuseTex += ".tga";
									strDestPathTexture += ".tga";
								}
								else
								{
									if (File.Exists(strAssetPathDiffuseTex + ".jpg"))
									{
										strAssetPathDiffuseTex += ".jpg";
										strDestPathTexture += ".jpg";
									}
								}
							}
							if (bDebugLog) Debug.Log("Input Texture:<" + strAssetPathDiffuseTex + "> Output Texture:<" + strDestPathTexture + ">");
                            if (File.Exists(strAssetPathDiffuseTex))
                            {
                                File.Copy(strAssetPathDiffuseTex, strDestPathTexture, bOverWriteTexture);
                            }
						}
						strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex;
                        */
#endif
					}
					else
					{
						strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex2;
						if (bOutTexture)
						{
							// strOutPath + strAssetPathDiffuseTexs[strAssetPathDiffuseTexs.Length - 1]
							if (File.Exists(strAssetPathDiffuseTex + ".png"))
							{
								strAssetPathDiffuseTex += ".png";
								strDestPathTexture += ".png";
							}
							else
							{
								if (File.Exists(strAssetPathDiffuseTex + ".tga"))
								{
									strAssetPathDiffuseTex += ".tga";
									strDestPathTexture += ".tga";
								}
								else
								{
									if (File.Exists(strAssetPathDiffuseTex + ".jpg"))
									{
										strAssetPathDiffuseTex += ".jpg";
										strDestPathTexture += ".jpg";
									}
								}
							}
							if (bDebugLog) Debug.Log("Input Texture:<" + strAssetPathDiffuseTex + "> Output Texture:<" + strDestPathTexture + ">");
                            if (File.Exists(strAssetPathDiffuseTex))
                            {
                                File.Copy(strAssetPathDiffuseTex, strDestPathTexture, bOverWriteTexture);
                            }
						}
						strDestPathTexture = strOutputTexturePath + strEmbedMediaPath + strDestDiffuseTex;
					}
					dicOutputTextureNames.Add(strTexture, strDestPathTexture);
				}
			}
		}
		return strDestPathTexture;
	}

}

public static class ComponentExtension
{
    public static bool IsSerializable(this object obj)
    {
        if (obj is ISerializable)
            return true;
        return false;// Attribute.IsDefined(obj.GetType(), typeof(SerializableAttribute));
    }
}

public static class TransformExtension
{
	public static GameObject[] FindRootObject (this Transform transform) {
		return Array.FindAll (GameObject.FindObjectsOfType<GameObject> (), (item) => item.transform.parent == null);
	}
}
