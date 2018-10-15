#ifndef UNITY_STANDARD_UTILS_INCLUDED
#define UNITY_STANDARD_UTILS_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"

// Helper functions, maybe move into UnityCG.cginc

half SpecularStrength(half3 specular)
{
	#if (SHADER_TARGET < 30)
		// SM2.0: instruction count limitation
		// SM2.0: simplified SpecularStrength
		return specular.r; // Red channel - because most metals are either monocrhome or with redish/yellowish tint
	#else
		return max (max (specular.r, specular.g), specular.b);
	#endif
}

// Diffuse/Spec Energy conservation
inline half3 EnergyConservationBetweenDiffuseAndSpecular (half3 albedo, half3 specColor, out half oneMinusReflectivity)
{
	oneMinusReflectivity = 1 - SpecularStrength(specColor);
	#if !UNITY_CONSERVE_ENERGY
		return albedo;
	#elif UNITY_CONSERVE_ENERGY_MONOCHROME
		return albedo * oneMinusReflectivity;
	#else
		return albedo * (half3(1,1,1) - specColor);
	#endif
}

inline half3 DiffuseAndSpecularFromMetallic (half3 albedo, half metallic, out half3 specColor, out half oneMinusReflectivity)
{
	specColor = lerp (unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	// We'll need oneMinusReflectivity, so
	//   1-reflectivity = 1-lerp(dielectricSpec, 1, metallic) = lerp(1-dielectricSpec, 0, metallic)
	// store (1-dielectricSpec) in unity_ColorSpaceDielectricSpec.a, then
	//	 1-reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) = 
	//                  = alpha - metallic * alpha
	half oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;
	oneMinusReflectivity = oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
	return albedo * oneMinusReflectivity;
}

inline half3 PreMultiplyAlpha (half3 diffColor, half alpha, half oneMinusReflectivity, out half outModifiedAlpha)
{
	#if defined(_ALPHAPREMULTIPLY_ON)
		// NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)

		// Transparency 'removes' from Diffuse component
 		diffColor *= alpha;
 		
 		#if (SHADER_TARGET < 30)
 			// SM2.0: instruction count limitation
 			// Instead will sacrifice part of physically based transparency where amount Reflectivity is affecting Transparency
 			// SM2.0: uses unmodified alpha
 			outModifiedAlpha = alpha;
 		#else
	 		// Reflectivity 'removes' from the rest of components, including Transparency
	 		// outAlpha = 1-(1-alpha)*(1-reflectivity) = 1-(oneMinusReflectivity - alpha*oneMinusReflectivity) =
	 		//          = 1-oneMinusReflectivity + alpha*oneMinusReflectivity
	 		outModifiedAlpha = 1-oneMinusReflectivity + alpha*oneMinusReflectivity;
 		#endif
 	#else
 		outModifiedAlpha = alpha;
 	#endif
 	return diffColor;
}

// Same as ParallaxOffset in Unity CG, except:
//  *) precision - half instead of float
half2 ParallaxOffset1Step (half h, half height, half3 viewDir)
{
	h = h * height - height/2.0;
	half3 v = normalize(viewDir);
	v.z += 0.42;
	return h * (v.xy / v.z);
}

half LerpOneTo(half b, half t)
{
	half oneMinusT = 1 - t;
	return oneMinusT + b * t;
}

half3 LerpWhiteTo(half3 b, half t)
{
	half oneMinusT = 1 - t;
	return half3(oneMinusT, oneMinusT, oneMinusT) + b * t;
}

half3 UnpackScaleNormal(half4 packednormal, half bumpScale)
{
	#if defined(UNITY_NO_DXT5nm)
		return packednormal.xyz * 2 - 1;
	#else
		half3 normal;
		normal.xy = (packednormal.wy * 2 - 1);
		#if (SHADER_TARGET >= 30)
			// SM2.0: instruction count limitation
			// SM2.0: normal scaler is not supported
			normal.xy *= bumpScale;
		#endif
		normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
		return normal;
	#endif
}		

half3 BlendNormals(half3 n1, half3 n2)
{
	return normalize(half3(n1.xy + n2.xy, n1.z*n2.z));
}

half3x3 CreateTangentToWorldPerVertex(half3 normal, half3 tangent, half3 flip)
{
	half3 binormal = cross(normal, tangent) * flip;
	return half3x3(tangent, binormal, normal);
}

//-------------------------------------------------------------------------------------
inline half3 BoxProjectedCubemapDirection (half3 worldNormal, float3 worldPos, float4 cubemapCenter, float4 boxMin, float4 boxMax)
{
	// Do we have a valid reflection probe?
	UNITY_BRANCH
	if (cubemapCenter.w > 0.0)
	{
		half3 nrdir = normalize(worldNormal);

		#if 1				
			half3 rbmax = (boxMax.xyz - worldPos) / nrdir;
			half3 rbmin = (boxMin.xyz - worldPos) / nrdir;

			half3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;

		#else // Optimized version
			half3 rbmax = (boxMax.xyz - worldPos);
			half3 rbmin = (boxMin.xyz - worldPos);

			half3 select = step (half3(0,0,0), nrdir);
			half3 rbminmax = lerp (rbmax, rbmin, select);
			rbminmax /= nrdir;
		#endif

		half fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);

		float3 aabbCenter = (boxMax.xyz + boxMin.xyz) * 0.5;
		float3 offset = aabbCenter - cubemapCenter.xyz;
		float3 posonbox = offset + worldPos + nrdir * fa;

		worldNormal = posonbox - aabbCenter;
	}
	return worldNormal;
}


//-------------------------------------------------------------------------------------
// Derivative maps
// http://www.rorydriscoll.com/2012/01/11/derivative-maps/
// For future use.

// Project the surface gradient (dhdx, dhdy) onto the surface (n, dpdx, dpdy)
half3 CalculateSurfaceGradient(half3 n, half3 dpdx, half3 dpdy, half dhdx, half dhdy)
{
	half3 r1 = cross(dpdy, n);
	half3 r2 = cross(n, dpdx);
	return (r1 * dhdx + r2 * dhdy) / dot(dpdx, r1);
}

// Move the normal away from the surface normal in the opposite surface gradient direction
half3 PerturbNormal(half3 n, half3 dpdx, half3 dpdy, half dhdx, half dhdy)
{
	//TODO: normalize seems to be necessary when scales do go beyond the 2...-2 range, should we limit that?
	//how expensive is a normalize? Anything cheaper for this case?
	return normalize(n - CalculateSurfaceGradient(n, dpdx, dpdy, dhdx, dhdy));
}

// Calculate the surface normal using the uv-space gradient (dhdu, dhdv)
half3 CalculateSurfaceNormal(half3 position, half3 normal, half2 gradient, half2 uv)
{
	half3 dpdx = ddx(position);
	half3 dpdy = ddy(position);

	half dhdx = dot(gradient, ddx(uv));
	half dhdy = dot(gradient, ddy(uv));

	return PerturbNormal(normal, dpdx, dpdy, dhdx, dhdy);
}


#endif // UNITY_STANDARD_UTILS_INCLUDED
