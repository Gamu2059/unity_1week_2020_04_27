Shader "Custom/Ground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        //X方向のシフトとスピードに関するパラメータを追加
        _XShift("Xuv Shift", Range(-1.0, 1.0)) = 0.1
        _XSpeed("X Scroll Speed", Range(1.0, 100.0)) = 10.0

        //Y方向のシフトとスピードに関するパラメータを追加
        _YShift("Yuv Shift", Range(-1.0, 1.0)) = 0.1
        _YSpeed("Y Scroll Speed", Range(1.0, 100.0)) = 10.0

        //ちらつく部分を疑似フォグで補正する
        _FogColor("Fog Color", Color) = (1,1,1,1)
        _FogBeginY("Fog Begin Y", float) = 0.0
        _FogEndY("Fog End Y", float) = 1.0

        _Alpha("Alpha", Range(0., 1.)) = 1.0
    }
    SubShader
    {
        Tags
        {
            //レンダリング順に関する指示
            "Queue"      = "Transparent"
            "RenderType" = "Transparent"
        }

        //透明なテクスチャを使用する場合に必要なプロパティ
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            //追加したパラメータを宣言する
            float _XShift;
            float _YShift;
            float _XSpeed;
            float _YSpeed;

            float4 _FogColor;
            float _FogBeginY;
            float _FogEndY;

            float _Alpha;

            //バーテクスシェーダー（変更なし）
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            //フラグメントシェーダー（変更箇所）
            fixed4 frag (v2f i) : SV_Target
            {
                //Speed
                _XShift = _XShift * _XSpeed;
                _YShift = _YShift * _YSpeed;

                float y = i.uv.y;
                //add Shift
                i.uv.x = i.uv.x + _XShift * _Time;
                i.uv.y = i.uv.y + _YShift * _Time;

                float fogRate = smoothstep(_FogBeginY, _FogEndY, y);
                // float fogRate = step(_FogBeginY, i.uv.y);

                //i.uvの適用
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = col.rgb * (1 - fogRate) + _FogColor.rgb * fogRate;

                UNITY_APPLY_FOG(i.fogCoord, col);
                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }
}