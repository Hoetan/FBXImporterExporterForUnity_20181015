----------------------------------------------------------------------------------------------

FBX Exporter for Unity �� Version 2018/10/15

Copyright (c) 2015-2017, ACTINIA Software. All rights reserved.

Homepage: http://actinia-software.com

E-Mail: hoetan@actinia-software.com

Twitter: https://twitter.com/hoetan3

GitHub: https://github.com/hoetan

Developer: �ق�����(Hoetan)

----------------------------------------------------------------------------------------------
[Japanese Manual]

���Ή�OS

 Windows 10 (64bit)
 Windows 8/8.1 Update 1 (64bit)
 Windows 7 ServicePack 1 (64bit) [��Windows7�́AKinect2�͓����܂���B]


���Ή�3D�G���W��

 Unity Personal/Professional v2017.4.1f1 (64bit)

���K�����̃o�[�W�����ȍ~�ŃV�[��(Scene)���J���ĉ������B������ȉ����ƃV�[���𐳏�ɊJ���܂���B


���Ή�Visual C++�����^�C��

 Microsoft Visual C++ 2015 �ĔЕz�\�p�b�P�[�W Update 3 (x86��x64): https://www.microsoft.com/ja-jp/download/details.aspx?id=53587

���uDllNotFoundException: Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Plugins/x86_64/FBXExporterForUnity.dll�v�̃G���[���ł��ꍇ�́A��L�̃����^�C���̃C���X�g�[�����K�{�ł��B


���g�p���C�u����(DLL)

 FBX SDK 2019.0.0 VS2015 (64bit): http://www.autodesk.com/products/fbx/overview


���Ή�Kinect1�h���C�o

 Kinect for Windows SDK v1.8 : http://www.microsoft.com/en-us/download/details.aspx?id=40278

��Kinect1�̃f�o�C�X���g�p�������ꍇ�́A��L�̃h���C�o���C���X�g�[�����Ăo�b���ċN�����Ă��������B


���Ή�Kinect2�h���C�o

 Kinect for Windows SDK 2.0 (1409): http://www.microsoft.com/en-us/download/confirmation.aspx?id=44561

��Kinect2�̃f�o�C�X���g�p�������ꍇ�́A��L�̃h���C�o���C���X�g�[�����Ăo�b���ċN�����Ă��������B


���Ή�Perception Neuron�\�t�g�E�F�A�i�h���C�o�j

 Axis Neuron: https://neuronmocap.com/downloads


���Ή�IKinema Orion�i�A�v���P�[�V�����j

 IKinema Orion: https://ikinema.com/orion


���g�p�v���O���~���O�c�[��(VisualC++)

 Visual Studio Community 2015 with Update 3


���g�pUnity�A�Z�b�g

 "Unity-chan!" model: http://unity-chan.com/

 ����V�X�^�[�Y ���f��: http://nakasis.com/

 Kinect Wrapper Package for Unity3D: http://wiki.etc.cmu.edu/unity3d/index.php/Microsoft_Kinect_-_Microsoft_SDK

 KinectForWindows UnityPro 2.0.1410: http://download.microsoft.com/download/F/8/1/F81BC66F-7AA8-4FE4-8317-26A0991162E9/KinectForWindows_UnityPro_2.0.1410.zip

 Perception Neuron: https://neuronmocap.com/

 IKinema Orion: https://ikinema.com/orion


���g�p���@

�@FBX Exporter���g�p�������ꍇ�́AUnity Editor��ŁA�uAssets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scenes/FBXExporterForUnity.unity�v�̃V�[�����J���Ă�����s�i�Đ��j���Ă��������B
�@���[�V�����L���v�`���f�o�C�X���g�p�������ꍇ�́AUnity Editor��ŁA�uAssets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scenes/UnityAutoMotionCaptureDevicesSelecter.unity�v�̃V�[�����J���Ă�����s�i�Đ��j���Ă��������B
�@�V�K�V�[���̏ꍇ�́AHierarchy�́uMain Camera�v�ɁuAssets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scripts/FBXExporterForUnity.cs�v�̃X�N���v�g���A�^�b�`���Ă�����s�i�Đ��j���Ă݂Ă��������B
�@���s�i�Đ��j����ƁA�v���W�F�N�g�̃��[�g�f�B���N�g�Ƀf�t�H���g�̃t�@�C�����uUnity.fbx�v��������������܂��B
�@FBX�t�@�C���́A3ds Max 2019 / Maya 2018 / Softimage 2015 / MotionBuilder 2018 / Stingray��FBX Importer for Unity�ɑΉ����Ă��܂��B
�@Maya�p��Animator�̏o�͂�����ꍇ�́A�uApply Root Motion�v�̃`�F�b�N���O���Ă���o�͂���悤�ɂ��Ă��������B
�@�g�p���ꂽ�e�N�X�`���́A�uTextures�v�t�H���_�ɁAPNG�`���Ɏ����ϊ�����o�͂���܂��B
�@DCCTools��2019��艺�ʂ̃o�[�W�����ŁAFBX�`��(2019.0.0)��ǂݍ��ޏꍇ�́A�uFBX Converter 2013.3�v���g�p���āA�Â��o�[�W�����̂e�a�w�`���ɃR���o�[�g���Ă���ǂݍ���ŉ������B


�����C�Z���X

�@���̃\�t�g�E�F�A�ŏo�͂����e�a�w�t�@�C���́A���R�ɏ��p�E�񏤗p�Ŏg�p���邱�Ƃ������܂��B
�@�����̂o�b�i�p�\�R���j�ւ̃C���X�g�[���́A�䐔���̃��C�Z���X���K�v�ɂȂ�܂��̂ŁA���ӂ��Ă��������B


���̌��ł̎g�p����

�@�̌��łł́AFBX�t�@�C���́A����90�t���[��(����R�b�ԁj�ȏ�̃A�j���[�V�����͏o�͂���܂���B���i�łł͖������ł��B
�@FBX�t�@�C�����������ݎ��ɁA�A�Z�b�g�X�g�A�̃z�[���y�[�W�i��`�j�������ŊJ���܂��B���i�łł̓X�L�b�v����܂��B


�����i�ŏЉ�

�@�uAsset Store�v�ɂĐ��i�ł́uFBX Importer for Unity ���v�ƁuFBX Exporter for Unity ���v�ƁuFBX Importer & Exporter for Unity ���v��̔����ł��B

�@�EFBX Importer for Unity ��
�@�@http://u3d.as/iit

�@�EFBX Exporter for Unity ��
�@�@http://u3d.as/ghk

�@�EFBX Importer & Exporter for Unity ��
�@�@http://u3d.as/iiu


�����₢���킹

�@���P�āi�A�C�f�A�j��o�O�񍐂��A���[���ɂĎ󂯕t���Ă���܂��B
�@���łɁA���̃c�[���̎g�p�ړI���A���[���ŏڍׂ������Ă����Ə�����܂��B�i�����[�X�j
�@���d����������W���ł��I�i�c�[���̓Ǝ��@�\�g���˗����\�I�j

�@E-Mail: hoetan@actinia-software.com


----------------------------------------------------------------------------------------------
[English Manual]

��Compatible OS

Windows 10 (64bit)
Windows 8/8.1 Update 1 (64bit)
Windows 7 Service Pack 1 (64bit) [Windows7: Kinect2 is no support.]


��Compatible 3D Engine

Unity Personal/Professional v2017.4.1f1 (64bit)

*Always open scenes using the version stated above or newer. Scenes will not properly open on older versions than the one stated above.


��Compatible Visual C++ Runtime

Microsoft Visual C++ 2015 Redistributable Update 3 (x86 and x64):
Japanese: https://www.microsoft.com/ja-jp/download/details.aspx?id=53587
English: https://www.microsoft.com/en-US/download/details.aspx?id=53587

*If the following error occurs:"DllNotFoundException: Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Plugins/x86_64/FBXExporterForUnity.dll"

Please download a runtime from one of the links above that best matches your language.


��Compatible Kinect1 Driver

Kinect for Windows SDK v1.8 : http://www.microsoft.com/en-us/download/details.aspx?id=40278

*If you want to use the Kinect1 device, install the above driver and restart your computer before use.


��Compatible Kinect2 Driver

Kinect for Windows SDK 2.0 (1409): http://www.microsoft.com/en-us/download/confirmation.aspx?id=44561

*If you want to use the Kinect1 device, install the above driver and restart your computer before use.


��Compatible Perception Neuron Software(Driver)

Axis Neuron: https://neuronmocap.com/downloads


��Compatible IKinema Orion(Application)

IKinema Orion: https://ikinema.com/orion


��Library Dependency(DLL)

FBX SDK 2019.0.0 VS2015 (64bit): http://www.autodesk.com/products/fbx/overview


��Programming Tool Dependency (Visual C++)

Visual Studio Community 2015 with Update3


��Used UnityAsset

"Unity-chan!" model: http://unity-chan.com/

"Nakano-sisters" model: http://nakasis.com/

Kinect Wrapper Package for Unity3D: http://wiki.etc.cmu.edu/unity3d/index.php/Microsoft_Kinect_-_Microsoft_SDK

KinectForWindows UnityPro 2.0.1410: http://download.microsoft.com/download/F/8/1/F81BC66F-7AA8-4FE4-8317-26A0991162E9/KinectForWindows_UnityPro_2.0.1410.zip

Perception Neuron: https://neuronmocap.com/

IKinema Orion: https://ikinema.com/orion


��How to use

Using the FBX Exporter on Unity Editor: "Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scenes/FBXExporterForUnity.unity" open this scene and play.

Using the Motion Caputre Deveices to FBX on Unity Editor: "Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scenes/UnityAutoMotionCaptureDevicesSelecter.unity" open this scene and play.

*If it is a new scene, attach the following script, �gAssets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scripts/FBXExporterForUnity.cs�h, to the �gMain Camera�h found in the Hierarchy.
Running(playing) the scene will automatically create a file with a default name �gUnity.fbx�h under the project's root directory.
This FBX file is compatible with: 3ds Max 2019, Maya 2018, Softimage 2015, MotionBuilder 2018, Stingray and FBX Importer for Unity.

When outputting Animation for Maya, please uncheck "Apply Root Motion".

Applied textures are automatically converted into PNG format and output to the �gTextures�h folder.

If the DCCTools being used is prior to the 2019 version; in order to be able to open the FBX format file (2019.0.0), please convert it to an older version of the FBX format using �gFBX Converter 2013.3�h.


��License

The FBX files exported through this software are free for personal and commercial use.
Please be aware that it is prohibited to use a single license to install this software on multiple computers. Each license purchased can only be applied to a single computer. Multiple computers will mean a need to purchase an equivalent amount of licenses.


��Limitations for the trial version

In the trial version, exported FBX files can only have animations up to 90 frames (about 3 seconds). There is no frame limitation for the product version.

Everytime a FBX file is saved, the asset store homepage will be automatically opened.
This will not occur for the product version.


��Now on sale

"FBX Importer for Unity ��" and "FBX Exporter for Unity ��" and "FBX Importer & Exporter for Unity ��" is now on sale in the "Asset Store".

FBX Importer for Unity ��
http://u3d.as/iit

FBX Exporter for Unity ��
http://u3d.as/ghk

FBX Importer & Exporter for Unity ��
http://u3d.as/iiu


��Contact Us

Mails about ideas for improving this software and bug reports are welcome.
Mails about how this software is being used are also very welcome.
 
E-Mail: hoetan@actinia-software.com
