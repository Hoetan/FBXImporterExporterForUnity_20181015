----------------------------------------------------------------------------------------------

FBX Importer for Unity �� Version 2018/10/15

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
 Windows 7 ServicePack 1 (64bit)


���Ή�3D�G���W��

 Unity Personal/Professional v2017.4.1f1 (64bit)

���K�����̃o�[�W�����ȍ~�ŃV�[��(Scene)���J���ĉ������B������ȉ����ƃV�[���𐳏�ɊJ���܂���B


���Ή�Visual C++�����^�C��

 Microsoft Visual C++ 2015 �ĔЕz�\�p�b�P�[�W Update 3 (x86��x64): https://www.microsoft.com/ja-jp/download/details.aspx?id=53587

���uDllNotFoundException: Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Plugins/x86_64/FBXExporterForUnity.dll�v�̃G���[���ł��ꍇ�́A��L�̃����^�C���̃C���X�g�[�����K�{�ł��B


���g�p���C�u����(DLL)

 FBX SDK 2019.0.0 VS2015 (64bit): http://www.autodesk.com/products/fbx/overview


���g�p�v���O���~���O�c�[��(VisualC++)

 Visual Studio Community 2015 with Update3


���g�p���@

�@FBX Importer���g�p�������ꍇ�́AUnity Editor��ŁA�uAssets/FBXImporter&ExporterForUnity/FBXImporterForUnity/Scenes/FBXImporterForUnity.unity�v�̃V�[�����J���Ă�����s�i�Đ��j���Ă��������B
�@�V�K�V�[���̏ꍇ�́AHierarchy�ɐV�K�ɁuGameObject�v���쐬���āA�uAssets/FBXImporter&ExporterForUnity/FBXImporterForUnity/Scripts/FBXImporterForUnity.cs�v�̃X�N���v�g���uGameObject�v�ɃA�^�b�`���Ă�����s�i�Đ��j���Ă݂Ă��������B
�@���s����ƁA�v���W�F�N�g�̃��[�g�f�B���N�g�ɂ���t�@�C�����uUnity.fbx�v��ǂݍ��݂܂��B
�@FBX�t�@�C���́A3ds Max 2019 / Maya 2018 / Softimage 2015/ MotionBuilder 2018��FBX Exporter for Unity�ɑΉ����Ă��܂��B


�����C�Z���X

�@���̃\�t�g�E�F�A�œ��͂����e�a�w�t�@�C���́A���R�ɏ��p�E�񏤗p�Ŏg�p���邱�Ƃ������܂��B
�@�����̂o�b�i�p�\�R���j�ւ̃C���X�g�[���́A�䐔���̃��C�Z���X���K�v�ɂȂ�܂��̂ŁA���ӂ��Ă��������B


���̌��ł̎g�p����

�@�̌��łł́AFBX�t�@�C���́A����90�t���[��(����R�b�ԁj�ȏ�̃A�j���[�V�����͓��͂���܂���B���i�łł͖������ł��B
�@FBX�t�@�C����ǂݍ��ݎ��ɁA�A�Z�b�g�X�g�A�̃z�[���y�[�W�i��`�j�������ŊJ���܂��B���i�łł̓X�L�b�v����܂��B


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

*If the following error occurs:"DllNotFoundException: Assets/FBXImporter&ExporterForUnity/FBXImporterForUnity/Plugins/x86_64/FBXImporterForUnity.dll"

Please download a runtime from one of the links above that best matches your language.


��Library Dependency(DLL)

FBX SDK 2019.0.0 VS2015 (64bit): http://www.autodesk.com/products/fbx/overview


��Programming Tool Dependency (Visual C++)

Visual Studio Community 2015 with Update3


��How to use

Using the FBX Importer on Unity Editor: "Assets/FBXImporter&ExporterForUnity/FBXImporterForUnity/Scenes/FBXImporterForUnity.unity" open this scene and play.

*If it is a new scene, create "GameObject"in the Hierarchy and attach the following script, �gAssets/FBXImporter&ExporterForUnity/FBXImporterForUnity/Scripts/FBXImporterForUnity.cs�h.
Running(playing) the scene will automatically load the file �gUnity.fbx�h located in the project's root directory.
This FBX file is compatible with: FBXExporterForUnity, 3ds Max 2019, Maya 2018, Softimage 2015, MotionBuilder 2018 and FBX Exporter for Unity.


��License

The FBX files exported through this software are free for personal and commercial use.
Please be aware that it is prohibited to use a single license to install this software on multiple computers. Each license purchased can only be applied to a single computer. Multiple computers will mean a need to purchase an equivalent amount of licenses.


��Limitations for the trial version

In the trial version, a maximum of only 90 frames will be imported from a FBX animation file. There is no frame limitation for the product version.

Everytime a FBX file is loaded, the asset store homepage will be automatically opened.
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
