Shader "Unlit/CavityShaderT4"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _DepthTreshold ("Depth Edge Treshold", Range(0.001, 1)) = 1
        _NormalTreshold ("Normal Edge Treshold", Range(0.001, 1)) = 1
        _Size ("Size", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

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
            float _DepthTreshold;
            float _NormalTreshold;
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
                depth = Linear01Depth(depth, _ZBufferParams);

                float cavity, edge;
                cavity = edge = 0;

                if(depth >= 1 || depth <= 0) return float4(0,0,0,0);

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

                float depthU = Linear01Depth(SampleSceneDepth(screenSpaceUV + offset.y), _ZBufferParams);
                float depthD = Linear01Depth(SampleSceneDepth(screenSpaceUV - offset.y), _ZBufferParams);
                float depthL = Linear01Depth(SampleSceneDepth(screenSpaceUV - offset.x), _ZBufferParams);
                float depthR = Linear01Depth(SampleSceneDepth(screenSpaceUV + offset.x), _ZBufferParams);

                float depthDiff = 0;
                depthDiff += depthU - depth;
                depthDiff += depthD - depth;
                depthDiff += depthL - depth;
                depthDiff += depthR - depth;

                if(depthDiff/4 < _DepthTreshold/_ScreenParams.y) depthDiff = 0;
                else depthDiff = 1;

                if(1-normalDiff < _NormalTreshold) normalDiff = 0;
                else normalDiff = 1;

                col.rgba = max(normalDiff, depthDiff);
                col.rgb = _Color * depthDiff;
                //USE SSAO? FOR COLOR GRADING
                //USE DISTANCE FALLOFF?
                return col;
            }
            ENDHLSL
        }
    }
}