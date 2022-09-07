//RealToon HDRP RT - VisiPas
//MJQStudioWorks

#include "Assets/RealToon/RealToon Shaders/RealToon Core/HDRP/RT_HDRP_Other.hlsl"

[shader("closesthit")]
void ClosestHitVisibility(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
{

	UNITY_XR_ASSIGN_VIEW_INDEX(DispatchRaysIndex().z);

	IntersectionVertex currentVertex;
	GetCurrentIntersectionVertex(attributeData, currentVertex);

	FragInputs fragInput;
	BuildFragInputsFromIntersection(currentVertex, fragInput);

	rayIntersection.t = RayTCurrent();

	float3 positionOS = ObjectRayOrigin() + ObjectRayDirection() * rayIntersection.t;
	float3 previousPositionWS = TransformPreviousObjectToWorld(positionOS);
	rayIntersection.velocity = saturate(length(previousPositionWS - fragInput.positionRWS));

}

[shader("anyhit")]
void AnyHitVisibility(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
{

	UNITY_XR_ASSIGN_VIEW_INDEX(DispatchRaysIndex().z);

	IntersectionVertex currentVertex;
	GetCurrentIntersectionVertex(attributeData, currentVertex);

	FragInputs fragInput;
	BuildFragInputsFromIntersection(currentVertex, fragInput);

	float3 viewWS = -WorldRayDirection();

	rayIntersection.t = RayTCurrent();

	PositionInputs posInput;
	posInput.positionWS = fragInput.positionRWS;
	posInput.positionSS = rayIntersection.pixelCoord;

	#if defined(TRANSPARENT_COLOR_SHADOW) && defined(N_F_TRANS_ON)

	float3 positionOS = ObjectRayOrigin() + ObjectRayDirection() * rayIntersection.t;
	float3 previousPositionWS = TransformPreviousObjectToWorld(positionOS);
	rayIntersection.velocity = saturate(length(previousPositionWS - fragInput.positionRWS));

	#if N_F_DCS_ON
						
		rayIntersection.color *= float3(1.0,1.0,1.0);
		IgnoreHit();

	#else

		if(!_TexturePatternStyle)
		{
			float4 objPos = mul ( GetObjectToWorldMatrix(), float4(0.0,0.0,0.0,1.0) );
			float4 projPos = ComputeScreenPos( TransformWorldToHClip( WorldRayOrigin() ) );
			float2 sceneUVs = (projPos.xy / projPos.w);

			float2 RTD_OB_VP_CAL = distance(objPos.xyz, GetCurrentViewPosition()) * _HighlightColorPower;
			float2 RTD_VD_Cal = -(float2((sceneUVs.x * 2.0 - 1.0)*(_ScreenParams.r/_ScreenParams.g), sceneUVs.y * 2.0 - 1.0).xy * RTD_OB_VP_CAL );

			//float2 RTD_TC_TP_OO = lerp( fragInput.texCoord0.xy, RTD_VD_Cal, _TexturePatternStyle );

			float4 _MainTex_var = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(fragInput.texCoord0.xy, _MainTex) );

			#if N_F_CO_ON

				//RT_TRANS_CO
				float RTD_TRAN_OPA_Sli;
				bool bo_co_val;
				float RTD_CO;				
				RT_TRANS_CO( fragInput.texCoord0.xy, _MainTex_var , RTD_TRAN_OPA_Sli , RTD_CO , bo_co_val , true);

				if(!bo_co_val)
				{
					IgnoreHit();
				}

			#else

				#if N_F_TRANS_ON

					float4 _MaskTransparency_var = SAMPLE_TEXTURE2D(_MaskTransparency, sampler_MaskTransparency , TRANSFORM_TEX(fragInput.texCoord0.xy, _MaskTransparency) );

					float RTD_TRAN_MAS = (smoothstep(clamp(-20.0,1.0,_TransparentThreshold),1.0,_MainTex_var.a) *_MaskTransparency_var.r);
					float RTD_TRAN_OPA_Sli = lerp( RTD_TRAN_MAS, smoothstep(clamp(-20.0,1.0,_TransparentThreshold) , 1.0, _MainTex_var.a)  ,_Opacity);

					rayIntersection.color *= ( 1.0 - RTD_TRAN_OPA_Sli);

					IgnoreHit();

				#endif

			#endif
		}
		else
		{
			rayIntersection.color *= float3(0.0,0.0,0.0);
		}

#endif

#else 
	
	#if N_F_DCS_ON

		rayIntersection.color *= float3(1.0,1.0,1.0);
		IgnoreHit();

	#else

		rayIntersection.color *= float3(0.0, 0.0, 0.0);
		AcceptHitAndEndSearch();

	#endif

#endif

}