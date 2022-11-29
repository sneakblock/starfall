Shader "SandalStudio/ImpactTransparency"
{
	Properties
	{
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_TextureSample0("Texture Sample", 2D) = "white" {}
		[HDR]_Color0("Color", Color) = (1,0.4481132,0.4481132,1)
		[HDR]_Color1("Color 2", Color) = (0,0.1950307,1,1)
		[Toggle(_COLORRCOLOR_ON)] _ColorRColor("2Color/Color", Float) = 0
		_Emissive("Emissive", Float) = 1
		_SpeedXYScaleZPowerW("Speed XY/Scale Z/ Power W", Vector) = (0,0,3.68,5.05)
		[Toggle(_KEYWORD0_ON)] _Keyword0("Noise On/Off", Float) = 0
		[Toggle(_RG_ON)] _RG("R/G", Float) = 0

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha One
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR
				#pragma shader_feature_local _KEYWORD0_ON
				#pragma shader_feature_local _RG_ON
				#pragma shader_feature_local _COLORRCOLOR_ON


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float4 ase_texcoord1 : TEXCOORD1;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float4 _SpeedXYScaleZPowerW;
				uniform sampler2D _TextureSample0;
				uniform float _Emissive;
				uniform float4 _Color0;
				uniform float4 _Color1;
						float2 voronoihash27( float2 p )
						{
							
							p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
							return frac( sin( p ) *43758.5453);
						}
				
						float voronoi27( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
						{
							float2 n = floor( v );
							float2 f = frac( v );
							float F1 = 8.0;
							float F2 = 8.0; float2 mg = 0;
							for ( int j = -1; j <= 1; j++ )
							{
								for ( int i = -1; i <= 1; i++ )
							 	{
							 		float2 g = float2( i, j );
							 		float2 o = voronoihash27( n + g );
									o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
									float d = 0.5 * dot( r, r );
							 		if( d<F1 ) {
							 			F2 = F1;
							 			F1 = d; mg = g; mr = r; id = o;
							 		} else if( d<F2 ) {
							 			F2 = d;
							
							 		}
							 	}
							}
							return F1;
						}
				


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.ase_texcoord3 = v.ase_texcoord1;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float time27 = 3.37;
					float2 voronoiSmoothId27 = 0;
					float2 appendResult44 = (float2(_SpeedXYScaleZPowerW.x , _SpeedXYScaleZPowerW.y));
					float2 panner24 = ( 1.0 * _Time.y * appendResult44 + float2( 0,0 ));
					float2 texCoord22 = i.texcoord.xy * float2( 1,1 ) + panner24;
					float2 coords27 = texCoord22 * _SpeedXYScaleZPowerW.z;
					float2 id27 = 0;
					float2 uv27 = 0;
					float voroi27 = voronoi27( coords27, time27, id27, uv27, 0, voronoiSmoothId27 );
					float4 texCoord42 = i.ase_texcoord3;
					texCoord42.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
					float clampResult37 = clamp( ( pow( ( voroi27 * 3.25 ) , _SpeedXYScaleZPowerW.w ) + texCoord42.z ) , 0.0 , 1.0 );
					#ifdef _KEYWORD0_ON
					float staticSwitch47 = clampResult37;
					#else
					float staticSwitch47 = 1.0;
					#endif
					float2 texCoord1 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode2 = tex2D( _TextureSample0, texCoord1 );
					#ifdef _RG_ON
					float staticSwitch48 = tex2DNode2.g;
					#else
					float staticSwitch48 = tex2DNode2.r;
					#endif
					float smoothstepResult21 = smoothstep( 0.0 , texCoord42.w , staticSwitch48);
					float4 lerpResult12 = lerp( _Color0 , _Color1 , texCoord1.y);
					#ifdef _COLORRCOLOR_ON
					float4 staticSwitch18 = lerpResult12;
					#else
					float4 staticSwitch18 = _Color0;
					#endif
					

					fixed4 col = ( ( ( staticSwitch47 * smoothstepResult21 ) * _Emissive ) * i.color * staticSwitch18 );
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}