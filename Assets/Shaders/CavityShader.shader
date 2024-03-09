Shader "Unlit/CavityShader"
{
    Properties
    {
        _Albedo ("Albedo", Color) = (1,1,1,1)

        // @help: I don't know how to get this from Unity
        // In the internet people some people are using _WorldSpaceLightPos0
        // some other GetMainLight() nither of these for for me...
        _LightDirection ("Light Direction", Vector) = (-1, 0.5, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vertexFunc
            #pragma fragment fragmentFunc
            #pragma exclude_renderers gles xbox360 ps3
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Albedo;
            float3 _LightDirection;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 dirWS : SW_POSITION;
                float3 normal : NORMAL;
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
                float3 dirWS : SW_POSITION;
                float3 normal : NORMAL; 
            };

            VertexOutput vertexFunc(VertexInput input)
            {
                VertexOutput output;
                output.position = TransformObjectToHClip(input.vertex.xyz);
                output.normal = TransformObjectToWorldNormal(input.normal);
                output.dirWS = TransformWorldToViewDir(input.dirWS);

                return output;
            }

            float4 fragmentFunc(VertexOutput input) : SV_Target
            {
                float diffuse = saturate(dot(input.normal, _LightDirection));
                float specular = saturate(dot(input.normal, normalize(_LightDirection * input.dirWS)));
                specular = specular * diffuse;

                float4 color = 1;
                color.rgb = _Albedo * (diffuse + specular);
                
                return color;
            }

            ENDHLSL
        }
    }
}


// HLSLPROGRAM
// #pragma vertex vertexFunc
// #pragma fragment fragmentFunc
// #pragma exclude_renderers gles xbox360 ps3
// #pragma multi_compile_fog

// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// float4 _Albedo;

// // @fixme: Dunno how to get this from Unity
// float3 _LightDirection = normalize(float3(1, 1, -1));

// struct VertexInput
// {
//     float4 vertex : POSITION;
//     float3 normal : NORMAL;
// };

// struct VertexOutput
// {
//     float4 position : SV_POSITION;
//     float3 normal : NORMAL;
// };

// VertexOutput vertexFunc(VertexInput input)
// {
//     VertexOutput output;
//     output.position = TransformObjectToHClip(input.vertex.xyz);
//     output.normal = TransformObjectToWorldNormal(input.normal);

//     return output;
// }

// float4 fragmentFunc(VertexOutput input) : SV_Target
// {
//     // output.color.xyz = input.normal * 0.5 + 0.5;
//     // output.color.w = 1;    
//     float4 color = 0;
//     color.rgb = input.normal;
//     color.w = 1;

//     return color;
// }

// ENDHLSL