// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'
// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'
// Upgrade NOTE: replaced 'unity_World2Shadow' with 'unity_WorldToShadow'

#ifndef UNITY_DEFERRED_LIBRARY_INCLUDED
#define UNITY_DEFERRED_LIBRARY_INCLUDED

// Deferred lighting / shading helpers


// --------------------------------------------------------
// Vertex shader

struct unity_v2f_deferred {
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
	float3 ray : TEXCOORD1;
};

float _LightAsQuad;

unity_v2f_deferred vert_deferred (float4 vertex : POSITION, float3 normal : NORMAL)
{
	unity_v2f_deferred o;
	o.pos = UnityObjectToClipPos(vertex);
	o.uv = ComputeScreenPos (o.pos);
	o.ray = mul (UNITY_MATRIX_MV, vertex).xyz * float3(-1,-1,1);
	
	// normal contains a ray pointing from the camera to one of near plane's
	// corners in camera space when we are drawing a full screen quad.
	// Otherwise, when rendering 3D shapes, use the ray calculated here.
	o.ray = lerp(o.ray, normal, _LightAsQuad);
	
	return o;
}


// --------------------------------------------------------
// Shared uniforms


sampler2D_float _CameraDepthTexture;

float4 _LightDir;
float4 _LightPos;
float4 _LightColor;
float4 unity_LightmapFade;
CBUFFER_START(UnityPerCamera2)
// float4x4 _CameraToWorld;
CBUFFER_END
float4x4 unity_WorldToLight;
sampler2D _LightTextureB0;

#if defined (POINT_COOKIE)
samplerCUBE _LightTexture0;
#else
sampler2D _LightTexture0;
#endif

#if defined (SHADOWS_SCREEN)
sampler2D _ShadowMapTexture;
#endif


// --------------------------------------------------------
// Shadow/fade helpers

#include "UnityShadowLibrary.cginc"


float UnityDeferredComputeFadeDistance(float3 wpos, float z)
{
	float sphereDist = distance(wpos, unity_ShadowFadeCenterAndType.xyz);
	return lerp(z, sphereDist, unity_ShadowFadeCenterAndType.w);
}

half UnityDeferredComputeShadow(float3 vec, float fadeDist, float2 uv)
{
	#if defined(SHADOWS_DEPTH) || defined(SHADOWS_SCREEN) || defined(SHADOWS_CUBE)
	float fade = fadeDist * _LightShadowData.z + _LightShadowData.w;
	fade = saturate(fade);
	#endif
	
	#if defined(SPOT)
	#if defined(SHADOWS_DEPTH)
	float4 shadowCoord = mul (unity_WorldToShadow[0], float4(vec,1));
	return saturate(UnitySampleShadowmap (shadowCoord) + fade);
	#endif //SHADOWS_DEPTH
	#endif
	
	#if defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
	#if defined(SHADOWS_SCREEN)
	return saturate(tex2D (_ShadowMapTexture, uv).r + fade);
	#endif
	#endif //DIRECTIONAL || DIRECTIONAL_COOKIE
	
	#if defined (POINT) || defined (POINT_COOKIE)
	#if defined(SHADOWS_CUBE)
	return UnitySampleShadowmap (vec);
	#endif //SHADOWS_CUBE
	#endif
	
	return 1.0;
}


// --------------------------------------------------------
// Common lighting data calculation (direction, attenuation, ...)


void UnityDeferredCalculateLightParams (
	unity_v2f_deferred i,
	out float3 outWorldPos,
	out float2 outUV,
	out half3 outLightDir,
	out float outAtten,
	out float outFadeDist)
{
	i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
	float2 uv = i.uv.xy / i.uv.w;
	
	// read depth and reconstruct world position
	float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
	depth = Linear01Depth (depth);
	float4 vpos = float4(i.ray * depth,1);
	float3 wpos = mul (unity_CameraToWorld, vpos).xyz;

	float fadeDist = UnityDeferredComputeFadeDistance(wpos, vpos.z);
	
	// spot light case
	#if defined (SPOT)	
		float3 tolight = _LightPos.xyz - wpos;
		half3 lightDir = normalize (tolight);
		
		float4 uvCookie = mul (unity_WorldToLight, float4(wpos,1));
		float atten = tex2Dproj (_LightTexture0, UNITY_PROJ_COORD(uvCookie)).w;
		atten *= uvCookie.w < 0;
		float att = dot(tolight, tolight) * _LightPos.w;
		atten *= tex2D (_LightTextureB0, att.rr).UNITY_ATTEN_CHANNEL;
		
		atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);
	
	// directional light case		
	#elif defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
		half3 lightDir = -_LightDir.xyz;
		float atten = 1.0;
		
		atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);
		
		#if defined (DIRECTIONAL_COOKIE)
		atten *= tex2D (_LightTexture0, mul(unity_WorldToLight, half4(wpos,1)).xy).w;
		#endif //DIRECTIONAL_COOKIE
	
	// point light case	
	#elif defined (POINT) || defined (POINT_COOKIE)
		float3 tolight = wpos - _LightPos.xyz;
		half3 lightDir = -normalize (tolight);
		
		float att = dot(tolight, tolight) * _LightPos.w;
		float atten = tex2D (_LightTextureB0, att.rr).UNITY_ATTEN_CHANNEL;
		
		atten *= UnityDeferredComputeShadow (tolight, fadeDist, uv);
		
		#if defined (POINT_COOKIE)
		atten *= texCUBE(_LightTexture0, mul(unity_WorldToLight, half4(wpos,1)).xyz).w;
		#endif //POINT_COOKIE	
	#else
		half3 lightDir = 0;
		float atten = 0;
	#endif

	outWorldPos = wpos;
	outUV = uv;
	outLightDir = lightDir;
	outAtten = atten;
	outFadeDist = fadeDist;
}

#endif // UNITY_DEFERRED_LIBRARY_INCLUDED
