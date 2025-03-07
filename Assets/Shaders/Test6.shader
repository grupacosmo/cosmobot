Shader "Unlit/CavityShaderT6"
{
    Properties
    {
        //_Color ("Color", Color) = (1,1,1,1)
        _Radius ("Radius", Range(0, 5)) = 1
        _AngleSens ("Angle Sensitivity", Range(1, 5)) = 2.5
        _EdgeMultiplier ("Edge Intensity Multiplier", Range(0, 10)) = 0.6
        _Sharpness ("Sharpness", Range(0, 1)) = 0.9
        _Intensity ("Intensity", Range(0, 10)) = 1
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
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _Color;
            float _Radius;
            float _AngleSens;
            float _EdgeMultiplier;
            float _Sharpness;
            float _Intensity;

            float2 SampleSceneNormalBuffer(float2 uv, float3x3 viewMatrix)
            {
                float3 normalWorldSpace = SampleSceneNormals(uv);
                float3 normalViewSpace = (mul(viewMatrix, normalWorldSpace));
                return normalViewSpace.xy;
            }

            float CalculateCurvature(float2 left, float2 right, float2 down, float2 up)
            {
                float diffX = left.x - right.x;
                float diffY = up.y - down.y;
                float diff = diffX + diffY;

                float curvature = 0.5 + sign(diff) * pow(abs(diff * _EdgeMultiplier), _AngleSens);
                return clamp(curvature, 0, 1);
            }

            float curavtureAtPoint(float2 uv, float3x3 viewMatrix)
            {
                float2 axisX = float2(1.0 / _ScreenParams.x, 0);
                float2 axisY = float2(0, 1.0 / _ScreenParams.y);

                float2 left = SampleSceneNormalBuffer(uv + axisX, viewMatrix);
                float2 right = SampleSceneNormalBuffer(uv - axisX, viewMatrix);
                float2 down = SampleSceneNormalBuffer(uv - axisY, viewMatrix);
                float2 up = SampleSceneNormalBuffer(uv + axisY, viewMatrix);

                return CalculateCurvature(left, right, down, up);
            }

            float Curvature(float2 uv)
            {
                float3x3 viewMatrix = (float3x3)UNITY_MATRIX_V;
                float weightTotal = 0;
                float curvature = 0;

                float sharp = clamp(1 - _Sharpness, 0.0001, 1);

                for(int i = -_Radius; i <= _Radius; i++)
                {
                    for(int j = -_Radius; j <= _Radius; j++)
                    {
                        float2 offset = float2(i,j);
                        float2 uvOffset = offset / _ScreenParams.xy;
                        float weight = 1 / (dot(offset, offset) + sharp);
                        weightTotal += weight;
                        curvature += weight * curavtureAtPoint(uv + uvOffset, viewMatrix);
                    }
                }

                curvature /= weightTotal;
                return curvature;
            }

            float4 frag (Varyings i) : SV_Target
            {
                float4 col = float4(1,1,1,1);
                col.rgb = Curvature(i.texcoord);
                
                //Base
                // if(col.r > 0.49 && col.r < 0.51) col.a = 0;
                //col.rgb *= _Color;

                
                
                //SoftLight
                float3 base = SampleSceneColor(i.texcoord);
                float3 result1 = 2 * base * col.rgb + base * base * (1 - 2 * col.rgb);
                float3 result2 = sqrt(base) * (2 * col.rgb - 1) + 2 * base * (1 - col.rgb);
                float3 zeroOrOne = step(0.5, col.rgb);
                col.rgb = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
                col.rgb = lerp(base, col.rgb, _Intensity);
                
                return col;
            }

            ENDHLSL
        }
    }
}