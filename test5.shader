Shader "Unlit/CavityShaderT5"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _DepthTreshold ("Depth Edge Treshold", Range(0.001, 1)) = 1
        _NormalTreshold ("Normal Edge Treshold", Range(0.001, 1)) = 1
        _Size ("Size", Range(0, 2)) = 1
        _FalloffTreshold ("Falloff Treshold", Range(0, 100)) = 10
        _FalloffSpeed ("Falloff Speed", Range(0.001, 0.1)) = 0.01
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
            float _FalloffTreshold;
            float _FalloffSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.screenSpace = ComputeScreenPos(o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col;

                float2 screenSpaceUV = i.screenSpace.xy / i.screenSpace.w;
                float depth = SampleSceneDepth(screenSpaceUV);

                float cavity, edge;
                cavity = edge = 0;

                if(depth >= 1 || depth <= 0) return float4(0,0,0,0);

                float3 normals = SampleSceneNormals(screenSpaceUV);
                float3 worldPos = ComputeWorldSpacePosition(screenSpaceUV, depth, UNITY_MATRIX_I_VP);
                float dist = length(worldPos - _WorldSpaceCameraPos);
                depth = Linear01Depth(depth, _ZBufferParams);

                float2 offset = _Size/(_ScreenParams.xy);
                
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
                float falloff =  1 - _FalloffSpeed * (dist - _FalloffTreshold);
                col.a *= _Color.a;
                col.a = clamp(falloff, 0, col.a);
                return col;
            }
            ENDHLSL
        }
    }
}