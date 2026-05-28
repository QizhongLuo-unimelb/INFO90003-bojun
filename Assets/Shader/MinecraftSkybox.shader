Shader "Custom/MinecraftSkybox"
{
    Properties
    {
        _SkyTop ("Sky Top", Color) = (0.25, 0.62, 1.0, 1)
        _SkyHorizon ("Sky Horizon", Color) = (0.62, 0.82, 1.0, 1)
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 1)
        _CloudShadow ("Cloud Shadow", Color) = (0.72, 0.82, 0.90, 1)
        _CloudScale ("Cloud Scale", Float) = 10
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _SkyTop;
            float4 _SkyHorizon;
            float4 _CloudColor;
            float4 _CloudShadow;
            float _CloudScale;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float horizon = saturate(dir.y * 0.65 + 0.55);
                float3 color = lerp(_SkyHorizon.rgb, _SkyTop.rgb, horizon);

                float2 cloudUv = floor((dir.xz / max(0.06, dir.y + 0.34)) * _CloudScale);
                float cloudBand = smoothstep(0.05, 0.24, dir.y) * (1.0 - smoothstep(0.62, 0.88, dir.y));
                float cloud = step(0.61, hash21(cloudUv)) * cloudBand;
                float cloudEdge = step(0.50, hash21(cloudUv + float2(3.0, 9.0))) * cloudBand * 0.35;
                color = lerp(color, _CloudShadow.rgb, cloudEdge);
                color = lerp(color, _CloudColor.rgb, cloud);

                float sun = step(0.994, dot(dir, normalize(float3(0.38, 0.72, 0.25))));
                color = lerp(color, float3(1.0, 0.96, 0.72), sun);

                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
