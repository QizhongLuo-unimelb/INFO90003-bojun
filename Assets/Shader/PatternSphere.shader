Shader "Custom/InsidePatternShader"
{
    Properties
    {
        _MainTex ("Pattern Texture", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 1
        _ScrollSpeed ("Scroll Speed", Float) = 0
        _Brightness ("Brightness", Float) = 1.0
        _FlipX ("Flip X", Float) = 1
        _FlipY ("Flip Y", Float) = 0
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

            sampler2D _MainTex;
            float _Tiling;
            float _ScrollSpeed;
            float _Brightness;
            float _FlipX;
            float _FlipY;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float2 uv = v.uv;

                if (_FlipX > 0.5)
                {
                    uv.x = 1.0 - uv.x;
                }

                if (_FlipY > 0.5)
                {
                    uv.y = 1.0 - uv.y;
                }

                uv *= _Tiling;
                o.uv = uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.x += _Time.y * _ScrollSpeed;

                fixed4 col = tex2D(_MainTex, uv);
                col.rgb *= _Brightness;
                return col;
            }
            ENDCG
        }
    }
}