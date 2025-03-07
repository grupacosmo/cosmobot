Shader "Unlit/CavityShaderT2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Valley ("Valley", Range(0, 2)) = 1
        _Ridge ("Ridge", Range(0, 2)) = 1
        _Size ("Size", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct appdata
            {
                float3 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenSpace : TEXCOORD0;
            };

            float4 _Color;
            float _Valley;
            float _Ridge;
            float _Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.screenSpace = ComputeScreenPos(o.vertex);
                return o;
            }

            float EdgeClamp (float edge, float control)
            {
                if (edge < 0.5 / control) return edge * (1 - edge * control);
                return 0.25 / control;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col;

                float2 screenSpaceUV = i.screenSpace.xy / i.screenSpace.w;
                float depth = SampleSceneDepth(screenSpaceUV);

                float cavity, edge;
                cavity = edge = 0;

                if(depth >= 1 || depth <= 0) return float4(0,0,0,1);

                float3 worldPos = ComputeWorldSpacePosition(screenSpaceUV, depth, UNITY_MATRIX_I_VP);
                float3 normals = SampleSceneNormals(screenSpaceUV);

                float2 offset = _Size/_ScreenParams.xy;
                
                float3 normalU = SampleSceneNormals(screenSpaceUV + offset.y);
                float3 normalD = SampleSceneNormals(screenSpaceUV - offset.y);
                float3 normalL = SampleSceneNormals(screenSpaceUV - offset.x);
                float3 normalR = SampleSceneNormals(screenSpaceUV + offset.x);

                float normalDiff = 0;
                normalDiff += dot(normals, normalU);
                normalDiff += dot(normals, normalD);
                normalDiff += dot(normals, normalL);
                normalDiff += dot(normals, normalR);

                normalDiff /= 4;

                float depthU = SampleSceneDepth(screenSpaceUV + offset.y);
                float depthD = SampleSceneDepth(screenSpaceUV - offset.y);
                float depthL = SampleSceneDepth(screenSpaceUV - offset.x);
                float depthR = SampleSceneDepth(screenSpaceUV + offset.x);

                float depthDiff = 0;
                depthDiff += depthU - depth;
                depthDiff += depthD - depth;
                depthDiff += depthL - depth;
                depthDiff += depthR - depth;

                depthDiff /= 4;
                
                col.rgb = _Color;
                col.rgb += clamp(depthDiff * (_Ridge * 50), 0, 1);
                col.rgb += (1-normalDiff) * (_Valley);
                return col;
            }
            ENDHLSL
        }
    }
}