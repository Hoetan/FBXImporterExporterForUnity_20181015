Shader "Neuron/Utilities/BoneLineShader" {

	Properties {
	    _Color ("Color", Color) = (1,1,1)
	}
	 
	SubShader {
		ZTest Always
		Tags { "Queue" = "Transparent+100" }
	    Color [_Color]
	    Pass {}
	}
}