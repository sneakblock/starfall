//RealToon HDRP - Other
//MJQStudioWorks

float4 ComputeScreenPos(float4 positionCS)
{
	float4 o = positionCS * 0.5f;
	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
	o.zw = positionCS.zw;
	return o;
}

//=========================

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

//RT TRANS CO
void RT_TRANS_CO( float2 uv , float4 _MainTex_var , out float RTD_TRAN_OPA_Sli , float RTD_CO , out bool bo_co_val, bool is_rt)
{

	RTD_TRAN_OPA_Sli = 1.0;
	bo_co_val = false;

	#if N_F_TRANS_ON

		#if N_F_CO_ON

			float4 _SecondaryCutout_var = SAMPLE_TEXTURE2D(_SecondaryCutout, sampler_SecondaryCutout ,TRANSFORM_TEX(uv,_SecondaryCutout));
			float RTD_CO_ON = (float)lerp( (lerp((_MainTex_var.r*_SecondaryCutout_var.r),_SecondaryCutout_var.r,_UseSecondaryCutoutOnly)+lerp(0.5,(-1.0),_Cutout)), saturate(( (1.0 - _Cutout) > 0.5 ? (1.0-(1.0-2.0*((1.0 - _Cutout)-0.5))*(1.0-lerp((_MainTex_var.a*_SecondaryCutout_var.r),_SecondaryCutout_var.a,_UseSecondaryCutoutOnly))) : (2.0*(1.0 - _Cutout)*lerp((_MainTex_var.a*_SecondaryCutout_var.r),_SecondaryCutout_var.a,_UseSecondaryCutoutOnly)) )), _AlphaBasedCutout );
			RTD_CO = RTD_CO_ON;

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

//RT CO ONLY
void RT_CO_ONLY(float2 uv)
{

	#if N_F_TRANS_ON

		#if N_F_CO_ON

			float4 _MainTex_var = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex , TRANSFORM_TEX(uv.xy, _MainTex) ); 

			float4 _SecondaryCutout_var = SAMPLE_TEXTURE2D(_SecondaryCutout, sampler_SecondaryCutout ,TRANSFORM_TEX(uv.xy,_SecondaryCutout));
			float RTD_CO_ON = (float)lerp( (lerp((_MainTex_var.r*_SecondaryCutout_var.r),_SecondaryCutout_var.r,_UseSecondaryCutoutOnly)+lerp(0.5,(-1.0),_Cutout)), saturate(( (1.0 - _Cutout) > 0.5 ? (1.0-(1.0-2.0*((1.0 - _Cutout)-0.5))*(1.0-lerp((_MainTex_var.a*_SecondaryCutout_var.r),_SecondaryCutout_var.a,_UseSecondaryCutoutOnly))) : (2.0*(1.0 - _Cutout)*lerp((_MainTex_var.a*_SecondaryCutout_var.r),_SecondaryCutout_var.a,_UseSecondaryCutoutOnly)) )), _AlphaBasedCutout );
			float RTD_CO = RTD_CO_ON;

			clip(RTD_CO - 0.5);

		#endif

	#endif
}

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

//=========================

