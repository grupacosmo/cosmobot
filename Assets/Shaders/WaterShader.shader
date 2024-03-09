Shader "Unlit/CavityShader"
{
    Properties
    {
        _Albedo ("Albedo", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalRenderPipeline" 
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vertexFunc
            #pragma fragment fragmentFunc
            #pragma exclude_renderers gles xbox360 ps3
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Albedo;

            struct VertexInput
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VertexOutput vertexFunc(VertexInput input)
            {
                VertexOutput output;
                output.pos = TransformObjectToHClip(input.pos.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 fragmentFunc(VertexOutput input) : SV_Target
            {
            
                return _Albedo;
            }

            ENDHLSL
        }
    }
}
