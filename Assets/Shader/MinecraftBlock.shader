Shader "Custom/MinecraftBlock"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.36, 0.68, 0.24, 1)
        _SideColor ("Side Color", Color) = (0.46, 0.30, 0.16, 1)
        _BottomColor ("Bottom Color", Color) = (0.28, 0.18, 0.10, 1)
        _MortarColor ("Mortar Color", Color) = (0.18, 0.18, 0.18, 1)
        _PixelScale ("Pixel Scale", Float) = 8
        _BlockScale ("Block Scale", Float) = 1
        _Pattern ("Pattern", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _TopColor;
            float4 _SideColor;
            float4 _BottomColor;
            float4 _MortarColor;
            float _PixelScale;
            float _BlockScale;
            float _Pattern;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            float2 planarUv(float3 positionWS, float3 normalWS)
            {
                float3 n = abs(normalize(normalWS));
                float2 uv = positionWS.xz;

                if (n.x > n.y && n.x > n.z)
                {
                    uv = positionWS.zy;
                }
                else if (n.z > n.y)
                {
                    uv = positionWS.xy;
                }

                return uv / max(0.01, _BlockScale);
            }

            float3 grassColor(float2 uv, float3 normalWS)
            {
                float topFace = smoothstep(0.42, 0.72, normalize(normalWS).y);
                float bottomFace = smoothstep(0.42, 0.72, -normalize(normalWS).y);
                float sideFace = 1.0 - max(topFace, bottomFace);
                float2 pixel = floor(uv * _PixelScale);
                float fleck = hash21(pixel);
                float checker = fmod(pixel.x + pixel.y, 2.0);

                float3 grass = _TopColor.rgb;
                grass *= lerp(0.82, 1.18, fleck);
                grass = lerp(grass, float3(0.20, 0.46, 0.15), step(0.72, fleck) * 0.55);

                float3 dirt = _SideColor.rgb * lerp(0.75, 1.22, fleck);
                float greenCap = step(0.68, frac(uv.y * 2.0 + hash21(pixel.xx))) * step(frac(uv.y * 2.0), 0.23);
                dirt = lerp(dirt, grass, greenCap * 0.65);

                float3 bottom = _BottomColor.rgb * lerp(0.75, 1.1, checker);
                return grass * topFace + dirt * sideFace + bottom * bottomFace;
            }

            float3 stoneColor(float2 uv)
            {
                float2 block = floor(uv * _PixelScale * 0.5);
                float2 cell = frac(uv * _PixelScale * 0.5);
                float mortar = step(cell.x, 0.08) + step(cell.y, 0.08);
                float crack = step(abs(cell.x - cell.y), 0.035) * step(0.72, hash21(block + 11.0));
                float shade = lerp(0.72, 1.24, hash21(block));
                float3 stone = _TopColor.rgb * shade;
                stone += (hash21(floor(uv * _PixelScale * 2.0)) - 0.5) * 0.08;
                return lerp(stone, _MortarColor.rgb, saturate(mortar + crack * 0.7));
            }

            float3 woodColor(float2 uv, float3 normalWS)
            {
                float3 n = abs(normalize(normalWS));
                float verticalFace = 1.0 - smoothstep(0.58, 0.82, n.y);
                float2 scaledUv = uv * float2(1.0, 1.6);
                float2 plank = floor(scaledUv * float2(2.0, 0.75));
                float2 cell = frac(scaledUv * float2(2.0, 0.75));
                float seam = step(cell.x, 0.075) + step(cell.x, 0.925) + step(cell.y, 0.055);
                float2 pixel = floor(uv * _PixelScale * float2(1.0, 2.0));
                float grain = hash21(float2(pixel.y, floor(pixel.x * 0.35)));
                float stripe = step(0.62, frac(cell.y * 7.0 + hash21(plank) * 0.6));
                float knot = smoothstep(0.28, 0.02, length(cell - float2(0.52, 0.54))) * step(0.76, hash21(plank + 23.0));

                float3 wood = lerp(_SideColor.rgb, _TopColor.rgb, grain * 0.55 + stripe * 0.25);
                wood = lerp(wood, _BottomColor.rgb, knot * 0.75);
                wood = lerp(wood, _MortarColor.rgb, saturate(seam) * 0.72);
                wood *= lerp(1.0, 0.82, verticalFace * step(0.5, fmod(plank.x, 2.0)) * 0.35);
                return wood;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float2 uv = planarUv(input.positionWS, normalWS);
                float3 color = grassColor(uv, normalWS);
                if (_Pattern > 1.5)
                {
                    color = woodColor(uv, normalWS);
                }
                else if (_Pattern > 0.5)
                {
                    color = stoneColor(uv);
                }

                float light = 0.58 + saturate(dot(normalWS, normalize(float3(0.35, 0.85, 0.28)))) * 0.42;
                return half4(saturate(color * light), 1.0);
            }
            ENDHLSL
        }
    }
}
