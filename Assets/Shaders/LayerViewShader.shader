Shader "Custom/LayerViewShader"
{
    Properties
    {
        _Tex1Color ("Texture 1 Color", Color) = (1,1,1,1)
        _Tex1 ("Texture 1", 2D) = "white" {}
        _Tex2Color ("Texture 2 Color", Color) = (1,1,1,1)
        _Tex2 ("Texture 2", 2D) = "white" {}
        _Tex3Color ("Texture 3 Color", Color) = (1,1,1,1)
        _Tex3 ("Texture 3", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            //レンダリング順に関する指示
            "Queue"      = "Transparent"
            "RenderType" = "Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _Tex1, _Tex2, _Tex3;
            float4 _Tex1_ST;
            float4 _Tex1Color, _Tex2Color, _Tex3Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Tex1);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col1 = tex2D(_Tex1, i.uv) * _Tex1Color;
                float4 col2 = tex2D(_Tex2, i.uv) * _Tex2Color;
                float4 col3 = tex2D(_Tex3, i.uv) * _Tex3Color;

                // SrcAlpha OneMinusSrcAlpha みたいなブレンドをする
                float4 col = col1 * (1 - col2.a) + col2 * col2.a;
                col = col * (1 - col3.a) + col3 * col3.a;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
