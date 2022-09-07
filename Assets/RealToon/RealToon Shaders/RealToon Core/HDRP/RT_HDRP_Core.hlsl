//RealToon HDRP - Core
//MJQStudioWorks

//=========================

float RTD_LVLC_F( float3 Light_Color_f3 )
{

		float4 node_3149_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4 node_3149_p = lerp(float4(float4(Light_Color_f3.rgb,0.0).zy, node_3149_k.wz), float4(float4(Light_Color_f3.rgb,0.0).yz, node_3149_k.xy), step(float4(Light_Color_f3.rgb,0.0).z, float4(Light_Color_f3.rgb,0.0).y));
		float4 node_3149_q = lerp(float4(node_3149_p.xyw, float4(Light_Color_f3.rgb,0.0).x), float4(float4(Light_Color_f3.rgb,0.0).x, node_3149_p.yzx), step(node_3149_p.x, float4(Light_Color_f3.rgb,0.0).x));
		float node_3149_d = node_3149_q.x - min(node_3149_q.w, node_3149_q.y);
		float node_3149_e = 1.0e-10;
		float3 node_3149 = float3(abs(node_3149_q.z + (node_3149_q.w - node_3149_q.y) / (6.0 * node_3149_d + node_3149_e)), node_3149_d / (node_3149_q.x + node_3149_e), node_3149_q.x);

		return saturate(node_3149.b);

}

float4 EL_AT_SC(PositionInputs posInput, float3 V, float4 inputColor)
{
	float4 result = inputColor;

	#if defined(N_F_TRANS_ON) && !defined(N_F_CO_ON)
		
		float3 volColor, volOpacity;
		EvaluateAtmosphericScattering(posInput, V, volColor, volOpacity);

			result.rgb = result.rgb * (1.0 - volOpacity) + volColor * result.a;
				
	#endif

		return result;
}

float4 ComputeScreenPos(float4 positionCS)
{
	float4 o = positionCS * 0.5f;
	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
	o.zw = positionCS.zw;
	return o;
}

//Temporary Solution
real Po_Ir(real4x3 L)
{
	#ifdef APPROXIMATE_POLY_LIGHT_AS_SPHERE_LIGHT

		L[0] = normalize(L[0]);
		L[1] = normalize(L[1]);
		L[2] = normalize(L[2]);
		L[3] = normalize(L[3]);

		real3 F  = ComputeEdgeFactor(L[0], L[1]);
				F += ComputeEdgeFactor(L[1], L[2]);
				F += ComputeEdgeFactor(L[2], L[3]);
				F += ComputeEdgeFactor(L[3], L[0]);

		#if 1
			float l = length(F);
			return max(0.0, (l * l + F.z) / (l + 1.0  * (0.16 * 0.02) ) );
		#endif

	#endif
}

float3 AL_GI( float3 N, float3 positionRWS )
{
	real4 SHCoefficients[7];
	SHCoefficients[0] = unity_SHAr;
	SHCoefficients[1] = unity_SHAg;
	SHCoefficients[2] = unity_SHAb;
	SHCoefficients[3] = unity_SHBr;
	SHCoefficients[4] = unity_SHBg;
	SHCoefficients[5] = unity_SHBb;
	SHCoefficients[6] = unity_SHC;

	return max(float3(0.0, 0.0, 0.0), SampleSH9(SHCoefficients, N));

}

void EncIntNormBu(NormalData normalData, uint2 positionSS, out float4 outNormalBuffer0)
{

    float2 octNormalWS = PackNormalOctQuadEncode(normalData.normalWS);
    float3 packNormalWS = PackFloat2To888(saturate(octNormalWS * 0.5 + 0.5));

    outNormalBuffer0 = float4(packNormalWS, 1.0);
}

float3 calcNorm(float3 pos)
{
	float3 vecTan = normalize(cross(pos, float3(1.01, 1.0, 1.0)));
	float3 vecBitan = normalize(cross(vecTan, pos));

	return normalize(cross(vecTan, vecBitan));
}

//=========================


//EdgDet/SSOL
//Most of the lines are based on unity hdrp example
float SampleClampedDepth(float2 uv)
{
	return SampleCameraDepth(clamp(uv, _ScreenSize.zw, 1.0 - _ScreenSize.zw)).r;
}

//Most of the lines are based on unity hdrp example
float EdgDet(float2 uv, float4 PosCS)
{

	float depth = LoadCameraDepth(PosCS.xy);
	float obj_only = depth != UNITY_RAW_FAR_CLIP_VALUE;

	float halfScaleFloor = floor(_OutlineWidth * 0.5);
	float halfScaleCeil = ceil(_OutlineWidth * 0.5);

	float2 bottomLeftUV = uv - float2(_ScreenSize.zw.x, _ScreenSize.zw.y) * halfScaleFloor;
	float2 topRightUV = uv + float2(_ScreenSize.zw.x, _ScreenSize.zw.y) * halfScaleCeil;
	float2 bottomRightUV = uv + float2(_ScreenSize.zw.x * halfScaleCeil, -_ScreenSize.zw.y * halfScaleFloor);
	float2 topLeftUV = uv + float2(-_ScreenSize.zw.x * halfScaleFloor, _ScreenSize.zw.y * halfScaleCeil);

	float depth0 = SampleCameraDepth(bottomLeftUV);
	float depth1 = SampleCameraDepth(topRightUV);
	float depth2 = SampleCameraDepth(bottomRightUV);
	float depth3 = SampleCameraDepth(topLeftUV);

	float depthDerivative0 = depth1 - depth0;
	float depthDerivative1 = depth3 - depth2;

	float edgeDepth = sqrt(pow(depthDerivative0, 2.0) + pow(depthDerivative1, 2.0)) * 100;

	edgeDepth = edgeDepth > (depth0 * (_DepthThreshold * 0.01)) ? 1 : 0;

	NormalData normalData0, normalData1, normalData2, normalData3;
	DecodeFromNormalBuffer(_ScreenSize.xy * bottomLeftUV, normalData0);
	DecodeFromNormalBuffer(_ScreenSize.xy * topRightUV, normalData1);
	DecodeFromNormalBuffer(_ScreenSize.xy * bottomRightUV, normalData2);
	DecodeFromNormalBuffer(_ScreenSize.xy * topLeftUV, normalData3);

	float3 normalFiniteDifference0 = (normalData1.normalWS - normalData0.normalWS);
	float3 normalFiniteDifference1 = (normalData3.normalWS - normalData2.normalWS);

	float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
	edgeNormal = smoothstep(_NormalMin, _NormalMax, edgeNormal * _NormalThreshold);
	edgeNormal *= obj_only;

	return max(edgeNormal, edgeDepth);
}


//RT NM
float3 RT_NM( float2 uv )
{
	#if N_F_NM_ON

		float3 _NormalMap_var = UnpackNormal( SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap , TRANSFORM_TEX(uv, _NormalMap) ) );
		float3 normalLocal = lerp(float3(0.0,0.0,1.0),_NormalMap_var.rgb,_NormalScale);

		return normalLocal;

	#else

		return float3(0.0,0.0,1.0);

	#endif

}
//

//RT_MCAP
float3 RT_MCAP( float2 uv, float3 normalDirection )
{
	#if N_F_MC_ON 
            
		float2 MUV = (mul( UNITY_MATRIX_V, float4(normalDirection,0.0) ).xyz.rgb.rg * 0.5 + 0.5); 
		float4 _MatCap_var = SAMPLE_TEXTURE2D(_MCap, sampler_MCap , TRANSFORM_TEX(MUV, _MCap));
		float4 _MCapMask_var = SAMPLE_TEXTURE2D(_MCapMask, sampler_MCapMask , TRANSFORM_TEX(uv, _MCapMask));
		float3 MCapOutP = lerp( lerp((float3)1.0,(float3)0.0, _SPECMODE), lerp( lerp((float3)1.0,(float3)0.0, _SPECMODE) ,_MatCap_var.rgb,_MCapIntensity) ,_MCapMask_var.rgb ); 

		return MCapOutP;

	#else
            
		return (float3)1.0;

	#endif
}

//RT_MCAP_SUB1
float3 RT_MCAP_SUB1( float3 MCapOutP, float4 _MainTex_var, float3 _RTD_MVCOL, out float3 RTD_TEX_COL )
{
	#if N_F_MC_ON 

		float3 SPECMode_Sel = lerp( lerp( ((_MainColor.rgb * _MaiColPo) * MCapOutP), ((_MainColor.rgb * _MaiColPo) + (MCapOutP * _SPECIN) ), _SPECMODE) ,  lerp( ( MCapOutP), ( (MCapOutP * _SPECIN) ), _SPECMODE), _MCIALO );
		RTD_TEX_COL = _MainTex_var.rgb * SPECMode_Sel * _RTD_MVCOL;
			
		float3 RTD_MCIALO_IL = RTD_TEX_COL;

		return RTD_MCIALO_IL;

	#else

		RTD_TEX_COL = _MainTex_var.rgb * (_MainColor.rgb * _MaiColPo) * MCapOutP * _RTD_MVCOL;
		float3 RTD_MCIALO_IL = lerp( RTD_TEX_COL , _MainTex_var.rgb * MCapOutP * _RTD_MVCOL, _MCIALO);

		return RTD_MCIALO_IL;

	#endif
}
//

//RT TRANS CO
void RT_TRANS_CO( float2 uv , float4 _MainTex_var , out float RTD_TRAN_OPA_Sli , float RTD_CO , out bool bo_co_val, bool is_rt, inout float3 GLO_OUT)
{

	RTD_TRAN_OPA_Sli = 1.0;
	bo_co_val = false;

	#if N_F_TRANS_ON

		#if N_F_CO_ON

			float4 _SecondaryCutout_var = SAMPLE_TEXTURE2D(_SecondaryCutout, sampler_SecondaryCutout ,TRANSFORM_TEX(uv,_SecondaryCutout));
			float RTD_CO_ON = (float)lerp( (lerp((_MainTex_var.r*_SecondaryCutout_var.r),_SecondaryCutout_var.r,_UseSecondaryCutoutOnly)+lerp(0.5,(-1.0),_Cutout)), saturate(( (1.0 - _Cutout) > 0.5 ? (1.0-(1.0-2.0*((1.0 - _Cutout)-0.5))*(1.0-lerp((_MainTex_var.a*_SecondaryCutout_var.r),_SecondaryCutout_var.a,_UseSecondaryCutoutOnly))) : (2.0*(1.0 - _Cutout)*lerp((_MainTex_var.a*_SecondaryCutout_var.r),_SecondaryCutout_var.a,_UseSecondaryCutoutOnly)) )), _AlphaBasedCutout );
			RTD_CO = RTD_CO_ON;

			//GLOW
			#ifdef N_F_COEDGL_ON

				float _Glow_Edge_Width_Val = (1.0 - _Glow_Edge_Width);
				float _Glow_Edge_Width_Add_Input_Value = (_Glow_Edge_Width_Val + RTD_CO);
				float _Remapping = (_Glow_Edge_Width_Add_Input_Value * 8.0 + -4.0);
				float _Pre_Output = (1.0 - saturate(_Remapping));
				 
				#if SHADERPASS == SHADERPASS_RAYTRACING_INDIRECT || SHADERPASS == SHADERPASS_RAYTRACING_FOWARD || SHADERPASS == SHADERPASS_RAYTRACING_GBUFFER

					float3 _Final_Output = (_Pre_Output * lerp(0.0, _Glow_Color.rgb * 6000.0, saturate(_Cutout * 200.0)));
					GLO_OUT = _Final_Output;

				#else

					float3 _Final_Output = (_Pre_Output * lerp(0.0, _Glow_Color.rgb, saturate(_Cutout * 200.0)));
					GLO_OUT = _Final_Output;

				#endif

			#endif

			if (!is_rt)
			{
				clip(RTD_CO - 0.5);
			}
			else
			{
				bo_co_val = RTD_CO >= 0.5;
			}
            
		#else

			float4 _MaskTransparency_var = SAMPLE_TEXTURE2D(_MaskTransparency, sampler_MaskTransparency ,TRANSFORM_TEX(uv,_MaskTransparency));

			//Backup (Old)
			//float RTD_TRAN_MAS = (smoothstep(clamp(-20.0,1.0,_TransparentThreshold),1.0,_MainTex_var.a) *_MaskTransparency_var.r);
			//RTD_TRAN_OPA_Sli = lerp( RTD_TRAN_MAS, smoothstep(clamp(-20.0,1.0,_TransparentThreshold) , 1.0, _MainTex_var.a)  ,_Opacity);

			RTD_TRAN_OPA_Sli = lerp(smoothstep(clamp(-20.0, 1.0, _TransparentThreshold), 1.0, _MainTex_var.a) * _Opacity, 1.0, _MaskTransparency_var.r);


		#endif

	#endif

}
//

//RT SON
float3 RT_SON( float4 vertexColor , float3 calNorm , float3 normalDirection , out float3 RTD_SON_CHE_1)
{

	RTD_SON_CHE_1 = float3(1.0,1.0,1.0);

	#if N_F_SON_ON

		float RTD_SON_VCBCSON_OO = lerp( _SmoothObjectNormal, (_SmoothObjectNormal*(1.0 - vertexColor.r)), _VertexColorRedControlSmoothObjectNormal );

		float3 RTD_SON_ON_OTHERS = lerp(normalDirection,TransformObjectToWorldNormal(-calNorm), RTD_SON_VCBCSON_OO);

		float3 RTD_SNorm_OO = lerp((float3)1.0, smoothstep(0.0, 0.01, RTD_SON_ON_OTHERS), _ShowNormal);
		RTD_SON_CHE_1 = RTD_SNorm_OO;

		float3 RTD_SON = RTD_SON_ON_OTHERS;

		return RTD_SON;
            
	#else
            
		float3 RTD_SON = normalDirection;
		
		return RTD_SON;
            
	#endif

}
//

//RT_RELGI
float3 RT_RELGI( float3 RTD_SON )
{

	#if N_F_RELGI_ON

		float3 RTD_GI_ST_Sli = (RTD_SON*_GIShadeThreshold);
		float3 RTD_GI_FS_OO = lerp( RTD_GI_ST_Sli, float3(smoothstep( float2(0.0,0.0), float2(0.01,0.01), (RTD_SON.rb*_GIShadeThreshold) ),0.0), _GIFlatShade );

		return RTD_GI_FS_OO;

	#else

		float3 RTD_GI_FS_OO = RTD_SON;

		return RTD_GI_FS_OO;

	#endif

}

//RT_RELGI_SUB1
float3 RT_RELGI_SUB1( PositionInputs posInput , float3 RTD_GI_FS_OO , float3 RTD_SHAT_COL , float3 RTD_MCIALO , float RTD_STIAL , out float RTD_RT_GI_Sha_FO , out float ref_int_val , in float3 ref_dif , bool isNonRT)
{

	RTD_RT_GI_Sha_FO = 1.0;
	ref_int_val = 1.0;
	float3 RTD_SL_OFF_OTHERS = float3(1.0,1.0,1.0);
	float3 IDT = float3(1.0, 1.0, 1.0);
	float4 IDTex = LOAD_TEXTURE2D_X(_IndirectDiffuseTexture, posInput.positionSS);
	float3 ALGI = AL_GI(lerp(float3(0.0, 0.0, 0.0), float3(1.0, 1.0, 1.0), RTD_GI_FS_OO), posInput.positionWS);

	#if N_F_RELGI_ON 

		#if N_F_R_ON

			ref_int_val = _ReflectionIntensity;

		#else

			ref_int_val = 1.0;

		#endif

		if(!isNonRT)
		{

			#if N_F_ESSGI_ON

				if (_IndirectDiffuseMode == INDIRECTDIFFUSEMODE_RAYTRACE)
				{ 
					#if SHADERPASS == SHADERPASS_FORWARD && !defined(N_F_TRANS_ON) || SHADERPASS == SHADERPASS_FORWARD && defined(N_F_CO_ON)

						if (_IndirectDiffuseMode == INDIRECTDIFFUSEMODE_RAYTRACE)
						{
							IDT = IDTex.xyz + ref_dif;
							RTD_RT_GI_Sha_FO = smoothstep( -0.01 , _RTGIShaFallo ,(float)lerp(0.0, 1.0 , IDT * _EnvironmentalLightingIntensity) ); 
							RTD_SL_OFF_OTHERS = lerp( RTD_SHAT_COL , RTD_MCIALO , RTD_STIAL) * IDT * _EnvironmentalLightingIntensity;
						}
						else
						{
							RTD_SL_OFF_OTHERS = lerp( RTD_SHAT_COL , RTD_MCIALO , RTD_STIAL) * (ALGI * ((_EnvironmentalLightingIntensity) * GetCurrentExposureMultiplier()));
						}

					#else

						RTD_SL_OFF_OTHERS = lerp(RTD_SHAT_COL, RTD_MCIALO, RTD_STIAL) * (ALGI * ((_EnvironmentalLightingIntensity)*GetCurrentExposureMultiplier()));

					#endif
				}
				else
				{

					#if !defined(N_F_TRANS_ON) || defined(N_F_CO_ON)

						if (_IndirectDiffuseMode != INDIRECTDIFFUSEMODE_OFF)
						{
							RTD_SL_OFF_OTHERS = lerp(RTD_SHAT_COL, RTD_MCIALO, RTD_STIAL) * lerp(ALGI, (IDTex.xyz * 2.0) * GetInverseCurrentExposureMultiplier(), IDTex.w )  *  GetCurrentExposureMultiplier()  * _EnvironmentalLightingIntensity;

						}
						else if (_IndirectDiffuseMode == INDIRECTDIFFUSEMODE_OFF)
						{

							RTD_SL_OFF_OTHERS = lerp(RTD_SHAT_COL, RTD_MCIALO, RTD_STIAL) * (ALGI * ((_EnvironmentalLightingIntensity)*GetCurrentExposureMultiplier()));

						}

					#else	

						RTD_SL_OFF_OTHERS = lerp(RTD_SHAT_COL, RTD_MCIALO, RTD_STIAL) * (ALGI * ((_EnvironmentalLightingIntensity)*GetCurrentExposureMultiplier()));

					#endif

				}

			#else

				RTD_SL_OFF_OTHERS = lerp(RTD_SHAT_COL, RTD_MCIALO, RTD_STIAL) * (ALGI * ((_EnvironmentalLightingIntensity)*GetCurrentExposureMultiplier()));

			#endif

		}
		else
		{

			RTD_SL_OFF_OTHERS = lerp(RTD_SHAT_COL, RTD_MCIALO, RTD_STIAL) * (ALGI * ((_EnvironmentalLightingIntensity)));

		}

		return RTD_SL_OFF_OTHERS;

	#else

		RTD_SL_OFF_OTHERS = float3(0.0,0.0,0.0);

		return RTD_SL_OFF_OTHERS;

	#endif

}
//

//RT_SCT
float3 RT_SCT( float2 uv , float3 RTD_MCIALO_IL )
{

	#if N_F_SCT_ON
            	
		float4 _ShadowColorTexture_var = SAMPLE_TEXTURE2D(_ShadowColorTexture, sampler_ShadowColorTexture ,TRANSFORM_TEX(uv,_ShadowColorTexture)); 
		float3 RTD_SCT_ON = lerp(_ShadowColorTexture_var.rgb,(_ShadowColorTexture_var.rgb*_ShadowColorTexture_var.rgb),_ShadowColorTexturePower);

		float3 RTD_SCT = RTD_SCT_ON * lerp((_MainColor.rgb * _MaiColPo),1.0,_MCIALO);

		return RTD_SCT;
            
	#else
            
		float3 RTD_SCT = RTD_MCIALO_IL;

		return RTD_SCT;
            
	#endif

}
//

//RT_PT
float RT_PT( float2 RTD_VD_Cal , out float3 RTD_PT_COL )
{

	RTD_PT_COL = float3(1.0,1.0,1.0);

	#if N_F_PT_ON

		float4 _PTexture_var = SAMPLE_TEXTURE2D(_PTexture, sampler_PTexture ,TRANSFORM_TEX(RTD_VD_Cal,_PTexture));  
		float RTD_PT_ON = lerp((1.0 - _PTexturePower),1.0,_PTexture_var.r);
		RTD_PT_COL = _PTCol.rgb;
            
		float RTD_PT = RTD_PT_ON;

		return RTD_PT;
            
	#else
            
		float RTD_PT = 1.0;

		return RTD_PT;
            
	#endif
}
//

//RT_GLO
void RT_GLO( float2 uv , float2 RTD_VD_Cal , float3 halfDirection , float3 normalDirection, float3 viewDirection , float dim_val , out float RTD_GLO , out float3 RTD_GLO_COL)
{

	#if N_F_GLO_ON

		#if N_F_GLOT_ON

			//#ifndef SHADER_API_MOBILE
				float _5992_ang = _GlossTextureRotate;
				float _5992_spd = 1.0;
				float _5992_cos = cos(_5992_spd*_5992_ang);
				float _5992_sin = sin(_5992_spd*_5992_ang);
				float2 _5992_piv = float2(0.5,0.5);
			//#endif

				float3 RTD_GT_FL_Sli = lerp(viewDirection,halfDirection,_GlossTextureFollowLight);
				float3 RefGlo = reflect(-RTD_GT_FL_Sli,normalDirection);

				float3 RTD_GT_FOR_OO = lerp( RefGlo, mul( GetWorldToObjectMatrix() , float4(RefGlo,0.0) ).xyz, _GlossTextureFollowObjectRotation );

			//#ifndef SHADER_API_MOBILE
				float2 glot_rot_cal = (mul(float2((RTD_GT_FOR_OO.r),RTD_GT_FOR_OO.g)-_5992_piv,float2x2( _5992_cos, -_5992_sin, _5992_sin, _5992_cos))+_5992_piv);
				float2 glot_rot_out = (glot_rot_cal*0.5+0.5);
			//#endif

			//#ifdef SHADER_API_MOBILE
				//float4 _GlossTexture_var = tex2Dlod(_GlossTexture,float4(TRANSFORM_TEX( lerp( (float2((-1*RefGlo.r),RefGlo.g)*0.5+0.5) ,RTD_VD_Cal,_PSGLOTEX) , _GlossTexture),0.0,_GlossTextureSoftness));
			//#else
            	float4 _GlossTexture_var = SAMPLE_TEXTURE2D_LOD(_GlossTexture, sampler_GlossTexture , TRANSFORM_TEX(lerp(glot_rot_out,RTD_VD_Cal,_PSGLOTEX),_GlossTexture) , _GlossTextureSoftness );
			//#endif

			float RTD_GT_ON = _GlossTexture_var.r;

			float RTD_GT = RTD_GT_ON;
            
		#else

			float RTD_GLO_MAIN_Sof_Sli = lerp(0.1,1.0,_GlossSoftness);
			float RTD_NDOTH = saturate(dot(halfDirection, normalDirection));
			float RTD_GLO_MAIN = smoothstep( 0.1, RTD_GLO_MAIN_Sof_Sli, pow(RTD_NDOTH,exp2(lerp(-2.0,15.0,_Glossiness))) );

			float RTD_GT = RTD_GLO_MAIN;
            
		#endif

		float RTD_GLO_I_Sli = lerp(0.0, RTD_GT,_GlossIntensity);
		float4 _MaskGloss_var = SAMPLE_TEXTURE2D(_MaskGloss, sampler_MaskGloss , TRANSFORM_TEX(uv, _MaskGloss));


		//
		#ifdef UNITY_COLORSPACE_GAMMA
			_GlossColor = float4(LinearToGamma22(_GlossColor.rgb), _GlossColor.a);
		#endif

		RTD_GLO_COL = (_GlossColor.rgb*_GlossColorPower); 
		//

		float RTD_GLO_MAS = lerp( 0.0, RTD_GLO_I_Sli ,_MaskGloss_var.r);

		RTD_GLO = RTD_GLO_MAS * dim_val;

	#else

		RTD_GLO_COL = (float3)1.0;
		RTD_GLO = 0.0;

            
	#endif

}
//

//RT_RL
float RT_RL(float3 viewDirection , float3 normalDirection , float3 lightColor , out float3 RTD_RL_LARL_OO , out float RTD_RL_MAIN)
{

	RTD_RL_MAIN = 0.0;

	#if N_F_RL_ON

		//
		#ifdef UNITY_COLORSPACE_GAMMA
			_RimLightColor = float4(LinearToGamma22(_RimLightColor.rgb), _RimLightColor.a);
		#endif

		RTD_RL_LARL_OO = lerp( _RimLightColor.rgb, lerp(float3(0.0,0.0,0.0),_RimLightColor.rgb,lightColor), _LightAffectRimLightColor ) * _RimLightColorPower;
		//


		float RTD_RL_S_Sli = lerp(1.70,0.29,_RimLightSoftness);
		RTD_RL_MAIN = lerp(0.0, 1.0 ,smoothstep( 1.71, RTD_RL_S_Sli, pow(abs( 1.0-max(0,dot(normalDirection, viewDirection) ) ), (1.0 - _RimLightUnfill) ) ) );
					
		float RTD_RL_IL_OO = lerp( 0.0, RTD_RL_MAIN, _RimLigInt);

		float RTD_RL_CHE_1 = RTD_RL_IL_OO;

		return RTD_RL_CHE_1;
            
	#else
					
		RTD_RL_LARL_OO = (float3)1.0;

		float RTD_RL_CHE_1 = 0.0;

		return RTD_RL_CHE_1;
            
	#endif

}

//RT_RL_SUB1
float3 RT_RL_SUB1( float3 RTD_SL_CHE_1 , float3 RTD_RL_LARL_OO , float3 RTD_RL_MAIN)
{

	#if N_F_RL_ON

        float3 RTD_RL_ON = lerp(RTD_SL_CHE_1 ,lerp( (lerp(RTD_SL_CHE_1, RTD_RL_LARL_OO, RTD_RL_MAIN) ), RTD_SL_CHE_1, _RimLightInLight) , _RimLigInt);
		float3 RTD_RL = RTD_RL_ON;

		return RTD_RL;
            
	#else
            
		float3 RTD_RL = RTD_SL_CHE_1;

		return RTD_RL;
            
	#endif

}
//

//RT_CLD
float3 RT_CLD( float3 lightDirection )
{

	#if N_F_CLD_ON

        float3 RTD_CLD_CLDFOR_OO = lerp( _CustomLightDirection.rgb, TransformObjectToWorldDir( _CustomLightDirection.xyz ), _CustomLightDirectionFollowObjectRotation );
		float3 RTD_CLD_CLDI_Sli = lerp(lightDirection,RTD_CLD_CLDFOR_OO,_CustomLightDirectionIntensity); 
		float3 RTD_CLD = RTD_CLD_CLDI_Sli;

		return RTD_CLD;
            
	#else
            
		float3 RTD_CLD = lightDirection;

		return RTD_CLD;
            
	#endif

}
//

//RT_ST
float RT_ST ( float2 uv , float3 RTD_NDOTL, float attenuation , float RTD_LVLC , float3 RTD_PT_COL , float3 lightColint , float3 RTD_SCT , float3 RTD_OSC , float RTD_PT , out float3 RTD_SHAT_COL , out float RTD_STIAL , out float RTD_ST_IS , out float3 RTD_ST_LAF)
{
	#if N_F_ST_ON

		float4 _ShadowT_var = SAMPLE_TEXTURE2D(_ShadowT, sampler_ShadowT , TRANSFORM_TEX(uv,_ShadowT));


		//
		#ifdef UNITY_COLORSPACE_GAMMA
			_ShadowTColor = float4(LinearToGamma22(_ShadowTColor.rgb), _ShadowTColor.a);
		#endif

		RTD_SHAT_COL = lerp( RTD_PT_COL, (_ShadowTColor.rgb*_ShadowTColorPower) * RTD_SCT * RTD_OSC, RTD_PT);
		//

		#if SHADERPASS == SHADERPASS_RAYTRACING_INDIRECT || SHADERPASS == SHADERPASS_RAYTRACING_GBUFFER
			float SWEXPO = saturate(GetCurrentExposureMultiplier());
			RTD_ST_LAF = lerp((RTD_SHAT_COL * lerp(GetInverseCurrentExposureMultiplier(), 7.0, SWEXPO)) * RTD_LVLC, (RTD_SHAT_COL * lightColint), _LightAffectShadow);
		#else
			RTD_ST_LAF = lerp((RTD_SHAT_COL * 7) * RTD_LVLC, (RTD_SHAT_COL * lightColint), _LightAffectShadow);
		#endif

		float RTD_ST_H_Sli = lerp(0.0,0.22,_ShadowTHardness);

		float RTD_ST_IS_ON = (float)smoothstep( RTD_ST_H_Sli, 0.22, (_ShowInAmbientLightShadowThreshold*_ShadowT_var.rgb) ); 

		#if N_F_STIAL_ON

			float RTD_ST_ALI_Sli = lerp(1.0,RTD_ST_IS_ON,_ShowInAmbientLightShadowIntensity);
			float RTD_STIAL_ON = (float)lerp(RTD_ST_ALI_Sli,float3(1.0,1.0,1.0),clamp((RTD_LVLC*8.0),0.0,1.0));

			RTD_STIAL = RTD_STIAL_ON;
            
		#else
            
			RTD_STIAL = 1.0;
            
		#endif

		#if N_F_STIS_ON
            
			RTD_ST_IS = lerp(1.0,RTD_ST_IS_ON,_ShowInAmbientLightShadowIntensity);
            
		#else
            
			RTD_ST_IS = 1.0;
            
		#endif

		float RTD_ST_LFAST_OO = (float)lerp(lerp( RTD_NDOTL, (attenuation*RTD_NDOTL), _LightFalloffAffectShadowT ) , 1.0 , _STIL );
		float RTD_ST_In_Sli = lerp( 1.0 ,smoothstep( RTD_ST_H_Sli, 0.22, ((_ShadowT_var.r*(1.0 - _ShadowTShadowThreshold))*(RTD_ST_LFAST_OO *_ShadowTLightThreshold*0.01)) ),_ShadowTIntensity);
		float RTD_ST_ON = RTD_ST_In_Sli;

		float RTD_ST = RTD_ST_ON;

		return RTD_ST;
            
	#else
            
		float RTD_ST = 1.0;
		RTD_SHAT_COL = 1.0;
		RTD_ST_LAF = 1.0;
		RTD_STIAL = 1.0;
		RTD_ST_IS = 1.0;

		return RTD_ST;
            
	#endif
}
//

//RT_SS
float RT_SS( float4 vertexColor , float3 RTD_NDOTL , float attenuation , float dim_val )
{

	#if N_F_SS_ON
 
		float RTD_SS_SSH_Sil = lerp(0.3,1.0,_SelfShadowHardness);
		float RTD_SS_SSTH_Sli = lerp(-1.0, 1.0, _SelfShadowThreshold);

		float RTD_SS_VCGCSSS_OO = lerp( RTD_SS_SSTH_Sli, (RTD_SS_SSTH_Sli*(1.0 - vertexColor.g)), _VertexColorGreenControlSelfShadowThreshold);

		float RTD_SS_SST = smoothstep( RTD_SS_SSH_Sil, 1.0, ((float)RTD_NDOTL * lerp(7.0, RTD_SS_VCGCSSS_OO ,RTD_SS_SSTH_Sli)) );
		float RTD_SS_SSABLSS_OO = lerp( RTD_SS_SST, lerp(RTD_SS_SST,1.0, (1.0 - dim_val)  ), _SelfShadowAffectedByLightShadowStrength );
		float RTD_SS_ON = lerp(1.0,(RTD_SS_SSABLSS_OO*attenuation),_SelfShadowRealtimeShadowIntensity);

		float RTD_SS = RTD_SS_ON;

		return RTD_SS;
            
	#else
            
		float RTD_SS_OFF = lerp(1.0,attenuation,_SelfShadowRealtimeShadowIntensity);

		float RTD_SS = RTD_SS_OFF;

		return RTD_SS;
            
	#endif

}
//

//RT_R
float3 RT_R( float2 uv , float3 viewDirection , float3 normalDirection , float3 EnvRef , float3 RTD_TEX_COL , float3 RTD_R_OFF_OTHERS )
{

	#if N_F_R_ON

		float3 RTD_FR_OFF_OTHERS = EnvRef;

		#if N_F_FR_ON
            
			float2 ref_cal = reflect(viewDirection,normalDirection).rg;
			float2 ref_cal_out = (float2(ref_cal.r,(-1.0*ref_cal.g))*0.5+0.5);
			float4 _FReflection_var = SAMPLE_TEXTURE2D_LOD(_FReflection, sampler_FReflection, TRANSFORM_TEX(ref_cal_out, _FReflection), _Smoothness);
			float3 RTD_FR_ON = _FReflection_var.rgb;

			float3 RTD_FR = RTD_FR_ON;
            
		#else
            
			float3 RTD_FR = RTD_FR_OFF_OTHERS;

		#endif

		float4 _MaskReflection_var = SAMPLE_TEXTURE2D(_MaskReflection, sampler_MaskReflection , TRANSFORM_TEX(uv, _MaskReflection));
		float3 RTD_R_MET_Sli = lerp((float3)1.0,(9.0 * (RTD_TEX_COL - (9.0 * 0.005) ) ) , _RefMetallic);
		float3 RTD_R_MAS = lerp(RTD_R_OFF_OTHERS, (RTD_FR * RTD_R_MET_Sli) ,_MaskReflection_var.r);
		float3 RTD_R_ON = lerp(RTD_R_OFF_OTHERS, RTD_R_MAS ,_ReflectionIntensity);

		float3 RTD_R = RTD_R_ON;

		return RTD_R;
            
	#else
            
		float3 RTD_R = RTD_R_OFF_OTHERS;
        
		return RTD_R;

	#endif

}
//

//RT_SL
float3 RT_SL( float2 uv , float3 RTD_SL_OFF_OTHERS , float3 RTD_TEX_COL , float3 RTD_R , out float3 RTD_SL_CHE_1)
{

	#if N_F_SL_ON

        float3 RTD_SL_HC_OO = lerp( 1.0, RTD_TEX_COL, _SelfLitHighContrast );
		float4 _MaskSelfLit_var = SAMPLE_TEXTURE2D(_MaskSelfLit, sampler_MaskSelfLit ,TRANSFORM_TEX(uv, _MaskSelfLit)); 


		//
		#ifdef UNITY_COLORSPACE_GAMMA
			_SelfLitColor = float4(LinearToGamma22(_SelfLitColor.rgb), _SelfLitColor.a);
		#endif

		#if SHADERPASS == SHADERPASS_RAYTRACING_INDIRECT || SHADERPASS == SHADERPASS_RAYTRACING_GBUFFER
			float SL_IRT_GICEM = GetInverseCurrentExposureMultiplier();
		#else
			float SL_IRT_GICEM = 1.0;
		#endif

		float3 RTD_SL_MAS = lerp(RTD_SL_OFF_OTHERS, ((_SelfLitColor.rgb * RTD_TEX_COL * RTD_SL_HC_OO * SL_IRT_GICEM)*_SelfLitPower),_MaskSelfLit_var.r);
		//
					
					
		float3 RTD_SL_ON = lerp(RTD_SL_OFF_OTHERS,RTD_SL_MAS,_SelfLitIntensity);

		float3 RTD_SL = RTD_SL_ON;

		float3 RTD_R_SEL = lerp(RTD_R,lerp(RTD_R,RTD_TEX_COL*_TEXMCOLINT,_MaskSelfLit_var.r),_SelfLitIntensity);
		RTD_SL_CHE_1 = RTD_R_SEL;

		return RTD_SL;
            
	#else
            
		float3 RTD_SL = RTD_SL_OFF_OTHERS;
		RTD_SL_CHE_1 = RTD_R;

		return RTD_SL;
     
	#endif

}
//

//RT_CA
float3 RT_CA( float3 color )
{

	#if N_F_CA_ON
            
		float3 RTD_CA_ON = lerp(color,dot(color,float3(0.3,0.59,0.11)),(1.0 - _Saturation));
		float3 RTD_CA = RTD_CA_ON;

		return RTD_CA;
            
	#else

		float3 RTD_CA = color;

		return RTD_CA;
            
	#endif

}

//RT_SSAO
float3 RT_SSAO(float2 positionSS)
{
	float3 RT_SSAmOc = 0.0;

	float3 SSAmOc = GetScreenSpaceAmbientOcclusion(positionSS);
	RT_SSAmOc = lerp(_SSAOColor.rgb, 1.0, SSAmOc);

	#ifdef N_F_ESSAO_ON
		return RT_SSAmOc;
	#else
		return 1.0;
	#endif
}
//