----------------------------------------------------------------------------------------------

FBX Exporter for Unity β Version 2018/10/15

Copyright (c) 2015-2017, ACTINIA Software. All rights reserved.

Homepage: http://actinia-software.com

E-Mail: hoetan@actinia-software.com

Twitter: https://twitter.com/hoetan3

GitHub: https://github.com/hoetan

Developer: ほえたん(Hoetan)

----------------------------------------------------------------------------------------------
[Japanese Manual]

■対応OS

 Windows 10 (64bit)
 Windows 8/8.1 Update 1 (64bit)
 Windows 7 ServicePack 1 (64bit) [※Windows7は、Kinect2は動きません。]


■対応3Dエンジン

 Unity Personal/Professional v2017.4.1f1 (64bit)

※必ずこのバージョン以降でシーン(Scene)を開いて下さい。※これ以下だとシーンを正常に開けません。


■対応Visual C++ランタイム

 Microsoft Visual C++ 2015 再頒布可能パッケージ Update 3 (x86とx64): https://www.microsoft.com/ja-jp/download/details.aspx?id=53587

※「DllNotFoundException: Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Plugins/x86_64/FBXExporterForUnity.dll」のエラーがでた場合は、上記のランタイムのインストールが必須です。


■使用ライブラリ(DLL)

 FBX SDK 2019.0.0 VS2015 (64bit): http://www.autodesk.com/products/fbx/overview


■対応Kinect1ドライバ

 Kinect for Windows SDK v1.8 : http://www.microsoft.com/en-us/download/details.aspx?id=40278

※Kinect1のデバイスを使用したい場合は、上記のドライバをインストールしてＰＣを再起動してください。


■対応Kinect2ドライバ

 Kinect for Windows SDK 2.0 (1409): http://www.microsoft.com/en-us/download/confirmation.aspx?id=44561

※Kinect2のデバイスを使用したい場合は、上記のドライバをインストールしてＰＣを再起動してください。


■対応Perception Neuronソフトウェア（ドライバ）

 Axis Neuron: https://neuronmocap.com/downloads


■対応IKinema Orion（アプリケーション）

 IKinema Orion: https://ikinema.com/orion


■使用プログラミングツール(VisualC++)

 Visual Studio Community 2015 with Update 3


■使用Unityアセット

 "Unity-chan!" model: http://unity-chan.com/

 中野シスターズ モデル: http://nakasis.com/

 Kinect Wrapper Package for Unity3D: http://wiki.etc.cmu.edu/unity3d/index.php/Microsoft_Kinect_-_Microsoft_SDK

 KinectForWindows UnityPro 2.0.1410: http://download.microsoft.com/download/F/8/1/F81BC66F-7AA8-4FE4-8317-26A0991162E9/KinectForWindows_UnityPro_2.0.1410.zip

 Perception Neuron: https://neuronmocap.com/

 IKinema Orion: https://ikinema.com/orion


■使用方法

　FBX Exporterを使用したい場合は、Unity Editor上で、「Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scenes/FBXExporterForUnity.unity」のシーンを開いてから実行（再生）してください。
　モーションキャプチャデバイスを使用したい場合は、Unity Editor上で、「Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scenes/UnityAutoMotionCaptureDevicesSelecter.unity」のシーンを開いてから実行（再生）してください。
　新規シーンの場合は、Hierarchyの「Main Camera」に「Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scripts/FBXExporterForUnity.cs」のスクリプトをアタッチしてから実行（再生）してみてください。
　実行（再生）すると、プロジェクトのルートディレクトにデフォルトのファイル名「Unity.fbx」が自動生成されます。
　FBXファイルは、3ds Max 2019 / Maya 2018 / Softimage 2015 / MotionBuilder 2018 / StingrayとFBX Importer for Unityに対応しています。
　Maya用にAnimatorの出力をする場合は、「Apply Root Motion」のチェックを外してから出力するようにしてください。
　使用されたテクスチャは、「Textures」フォルダに、PNG形式に自動変換され出力されます。
　DCCToolsが2019より下位のバージョンで、FBX形式(2019.0.0)を読み込む場合は、「FBX Converter 2013.3」を使用して、古いバージョンのＦＢＸ形式にコンバートしてから読み込んで下さい。


■ライセンス

　このソフトウェアで出力したＦＢＸファイルは、自由に商用・非商用で使用することを許可します。
　複数のＰＣ（パソコン）へのインストールは、台数分のライセンスが必要になりますので、注意してください。


■体験版の使用制限

　体験版では、FBXファイルは、総数90フレーム(※約３秒間）以上のアニメーションは出力されません。製品版では無制限です。
　FBXファイルを書き込み時に、アセットストアのホームページ（宣伝）を自動で開きます。製品版ではスキップされます。


■製品版紹介

　「Asset Store」にて製品版の「FBX Importer for Unity β」と「FBX Exporter for Unity β」と「FBX Importer & Exporter for Unity β」を販売中です。

　・FBX Importer for Unity β
　　http://u3d.as/iit

　・FBX Exporter for Unity β
　　http://u3d.as/ghk

　・FBX Importer & Exporter for Unity β
　　http://u3d.as/iiu


■お問い合わせ

　改善案（アイデア）やバグ報告も、メールにて受け付けております。
　ついでに、このツールの使用目的を、メールで詳細を教えてくれると助かります。（興味深々）
　お仕事も随時募集中です！（ツールの独自機能拡張依頼も可能！）

　E-Mail: hoetan@actinia-software.com


----------------------------------------------------------------------------------------------
[English Manual]

■Compatible OS

Windows 10 (64bit)
Windows 8/8.1 Update 1 (64bit)
Windows 7 Service Pack 1 (64bit) [Windows7: Kinect2 is no support.]


■Compatible 3D Engine

Unity Personal/Professional v2017.4.1f1 (64bit)

*Always open scenes using the version stated above or newer. Scenes will not properly open on older versions than the one stated above.


■Compatible Visual C++ Runtime

Microsoft Visual C++ 2015 Redistributable Update 3 (x86 and x64):
Japanese: https://www.microsoft.com/ja-jp/download/details.aspx?id=53587
English: https://www.microsoft.com/en-US/download/details.aspx?id=53587

*If the following error occurs:"DllNotFoundException: Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Plugins/x86_64/FBXExporterForUnity.dll"

Please download a runtime from one of the links above that best matches your language.


■Compatible Kinect1 Driver

Kinect for Windows SDK v1.8 : http://www.microsoft.com/en-us/download/details.aspx?id=40278

*If you want to use the Kinect1 device, install the above driver and restart your computer before use.


■Compatible Kinect2 Driver

Kinect for Windows SDK 2.0 (1409): http://www.microsoft.com/en-us/download/confirmation.aspx?id=44561

*If you want to use the Kinect1 device, install the above driver and restart your computer before use.


■Compatible Perception Neuron Software(Driver)

Axis Neuron: https://neuronmocap.com/downloads


■Compatible IKinema Orion(Application)

IKinema Orion: https://ikinema.com/orion


■Library Dependency(DLL)

FBX SDK 2019.0.0 VS2015 (64bit): http://www.autodesk.com/products/fbx/overview


■Programming Tool Dependency (Visual C++)

Visual Studio Community 2015 with Update3


■Used UnityAsset

"Unity-chan!" model: http://unity-chan.com/

"Nakano-sisters" model: http://nakasis.com/

Kinect Wrapper Package for Unity3D: http://wiki.etc.cmu.edu/unity3d/index.php/Microsoft_Kinect_-_Microsoft_SDK

KinectForWindows UnityPro 2.0.1410: http://download.microsoft.com/download/F/8/1/F81BC66F-7AA8-4FE4-8317-26A0991162E9/KinectForWindows_UnityPro_2.0.1410.zip

Perception Neuron: https://neuronmocap.com/

IKinema Orion: https://ikinema.com/orion


■How to use

Using the FBX Exporter on Unity Editor: "Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scenes/FBXExporterForUnity.unity" open this scene and play.

Using the Motion Caputre Deveices to FBX on Unity Editor: "Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scenes/UnityAutoMotionCaptureDevicesSelecter.unity" open this scene and play.

*If it is a new scene, attach the following script, “Assets/FBXImporter&ExporterForUnity/FBXExporterForUnity/Scripts/FBXExporterForUnity.cs”, to the “Main Camera” found in the Hierarchy.
Running(playing) the scene will automatically create a file with a default name “Unity.fbx” under the project's root directory.
This FBX file is compatible with: 3ds Max 2019, Maya 2018, Softimage 2015, MotionBuilder 2018, Stingray and FBX Importer for Unity.

When outputting Animation for Maya, please uncheck "Apply Root Motion".

Applied textures are automatically converted into PNG format and output to the “Textures” folder.

If the DCCTools being used is prior to the 2019 version; in order to be able to open the FBX format file (2019.0.0), please convert it to an older version of the FBX format using “FBX Converter 2013.3”.


■License

The FBX files exported through this software are free for personal and commercial use.
Please be aware that it is prohibited to use a single license to install this software on multiple computers. Each license purchased can only be applied to a single computer. Multiple computers will mean a need to purchase an equivalent amount of licenses.


■Limitations for the trial version

In the trial version, exported FBX files can only have animations up to 90 frames (about 3 seconds). There is no frame limitation for the product version.

Everytime a FBX file is saved, the asset store homepage will be automatically opened.
This will not occur for the product version.


■Now on sale

"FBX Importer for Unity β" and "FBX Exporter for Unity β" and "FBX Importer & Exporter for Unity β" is now on sale in the "Asset Store".

FBX Importer for Unity β
http://u3d.as/iit

FBX Exporter for Unity β
http://u3d.as/ghk

FBX Importer & Exporter for Unity β
http://u3d.as/iiu


■Contact Us

Mails about ideas for improving this software and bug reports are welcome.
Mails about how this software is being used are also very welcome.
 
E-Mail: hoetan@actinia-software.com
