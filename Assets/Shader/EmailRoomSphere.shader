Shader "Custom/EmailRoomSphere"
{
    Properties
    {
        _Brightness ("Brightness", Float) = 1.0
        _PaperDensity ("Paper Density", Float) = 1.0
        _WarmLight ("Warm Light", Color) = (1.0, 0.72, 0.42, 1.0)
        _WallColor ("Wall Color", Color) = (0.30, 0.24, 0.19, 1.0)
        _ShadowColor ("Shadow Color", Color) = (0.045, 0.038, 0.035, 1.0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Brightness;
            float _PaperDensity;
            float4 _WarmLight;
            float4 _WallColor;
            float4 _ShadowColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 localDir : TEXCOORD0;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 hash22(float2 p)
            {
                float n = hash21(p);
                return frac(float2(n, hash21(p + n + 17.13)) * float2(13.37, 29.71));
            }

            float rectMask(float2 p, float2 center, float2 halfSize, float softness)
            {
                float2 d = abs(p - center) - halfSize;
                float outside = length(max(d, 0.0));
                float inside = min(max(d.x, d.y), 0.0);
                return 1.0 - smoothstep(0.0, softness, outside + inside);
            }

            float lineMask(float2 p, float2 a, float2 b, float thickness)
            {
                float2 pa = p - a;
                float2 ba = b - a;
                float h = saturate(dot(pa, ba) / dot(ba, ba));
                return 1.0 - smoothstep(thickness, thickness * 1.8, length(pa - ba * h));
            }

            float paperField(float2 uv, float scale, float density)
            {
                float2 gridUv = uv * scale;
                float2 cell = floor(gridUv);
                float2 local = frac(gridUv);
                float rnd = hash21(cell);
                float2 offset = lerp(float2(0.34, 0.36), float2(0.66, 0.64), hash22(cell));
                float angle = (hash21(cell + 9.1) - 0.5) * 0.75;
                float s = sin(angle);
                float c = cos(angle);
                float2 p = mul(float2x2(c, -s, s, c), local - offset) + 0.5;
                float2 size = float2(0.21 + hash21(cell + 2.7) * 0.14, 0.14 + hash21(cell + 5.4) * 0.10);
                float visible = step(1.0 - saturate(density), rnd);
                float page = rectMask(p, 0.5, size, 0.015) * visible;
                float crease = lineMask(p, 0.5 - size * float2(0.72, 0.45), 0.5 + size * float2(0.72, 0.45), 0.008) * page;
                return saturate(page * 0.86 + crease * 0.34);
            }

            float3 shadePaper(float2 uv, float paper)
            {
                float noise = hash21(floor(uv * 96.0)) * 0.12;
                return lerp(float3(0.66, 0.55, 0.44), float3(0.92, 0.82, 0.68) + noise, paper);
            }

            float3 roomSurface(float3 dir)
            {
                dir = normalize(dir);

                float tx = 1e5;
                float ty = 1e5;
                float tz = 1e5;
                if (abs(dir.x) > 0.001) tx = 1.0 / abs(dir.x);
                if (abs(dir.y) > 0.001) ty = 1.0 / abs(dir.y);
                if (dir.z > 0.001) tz = 1.24 / dir.z;

                float t = min(tx, min(ty, tz));
                float3 hit = dir * t;

                float isSide = step(tx, min(ty, tz));
                float isFloor = step(ty, min(tx, tz)) * step(dir.y, 0.0);
                float isCeiling = step(ty, min(tx, tz)) * step(0.0, dir.y);
                float isBack = step(tz, min(tx, ty));

                float2 uv;
                float paper = 0.0;
                float3 baseColor = _WallColor.rgb;

                if (isBack > 0.5)
                {
                    uv = hit.xy * float2(1.0, 0.82);
                    paper = paperField(uv + 12.0, 7.5, _PaperDensity * 0.78);
                    baseColor *= 0.82;
                }
                else if (isFloor > 0.5)
                {
                    uv = hit.xz * float2(0.9, 1.0);
                    paper = paperField(uv + 21.0, 9.0, _PaperDensity * 0.92);
                    baseColor = float3(0.25, 0.20, 0.16);
                }
                else if (isCeiling > 0.5)
                {
                    uv = hit.xz * float2(0.95, 1.08);
                    paper = paperField(uv + 34.0, 8.2, _PaperDensity * 0.98);
                    baseColor = float3(0.20, 0.16, 0.13);
                }
                else
                {
                    uv = hit.zy * float2(0.95, 0.82);
                    paper = paperField(uv + hit.x * 7.0, 8.4, _PaperDensity * 0.9);
                    baseColor *= lerp(0.72, 0.95, saturate(hit.z * 0.6));
                }

                float3 paperColor = shadePaper(uv, paper);
                float3 col = lerp(baseColor, paperColor, paper);

                float vignette = saturate(1.25 - length(dir.xy) * 0.95);
                float doorway = rectMask(hit.xy, float2(0.86, -0.03), float2(0.16, 0.46), 0.015) * isBack;
                float screen = rectMask(hit.xy, float2(-0.78, -0.05), float2(0.18, 0.14), 0.015) * isBack;
                float desk = rectMask(hit.xy, float2(-0.77, -0.27), float2(0.33, 0.08), 0.015) * isBack;
                float glow = saturate(1.0 - length(hit.xy - float2(0.72, -0.34)) * 1.8) * isBack;

                col = lerp(col, float3(1.0, 0.78, 0.45), doorway);
                col += _WarmLight.rgb * glow * 0.55;
                col = lerp(col, float3(0.05, 0.048, 0.045), desk);
                col = lerp(col, float3(0.62, 0.86, 0.95), screen);
                col *= lerp(0.42, 1.0, vignette);
                col = lerp(_ShadowColor.rgb, col, saturate(vignette + doorway * 0.65 + glow * 0.45));

                float cornerShadow = smoothstep(0.45, 1.1, max(abs(hit.x), abs(hit.y)));
                col *= lerp(1.0, 0.55, cornerShadow * (isBack + isSide * 0.7));
                return col;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localDir = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 col = roomSurface(i.localDir);
                col *= _Brightness;
                return fixed4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
}
