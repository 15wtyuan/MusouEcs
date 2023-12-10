Shader "Scene/MusouSprite"
{
    Properties
    {
        _MainTex ("Main Texture Array", 2DArray) = "white" { }
        _TexIndex ("Tex Index", float) = 0
        _Color("Color", color) = (1, 1, 1, 1)
        _Rect("Rect", vector) = (1, 1, 1, 1)

        //闪白
        _BlankColor("Blank Color", color) = (1, 1, 1, 1)
        _BlankOpacity ("Blank Opacity", float) = 0.5

        //描边
        _OutlineWidth ("Outline Width", float) = 0
        _OutlineColor ("Outline Color", Color) = (1.0, 00, 00, 1.0)
        _OutlineOutValue ("Outline Out Alpha Value", Range(0, 1)) = 1
        _OutLineInValue ("OutLine In Alpha Value", Range(0, 1)) = 0
    }
    
    SubShader
    {
        LOD 100
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Fog
        {
            Mode Off
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2DARRAY(_MainTex);
            fixed4 _Color;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _TexIndex)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Rect)
            UNITY_DEFINE_INSTANCED_PROP(float, _Blank)
            UNITY_DEFINE_INSTANCED_PROP(float, _BlankOpacity)
            UNITY_INSTANCING_BUFFER_END(Props)

            fixed4 _BlankColor;

            float4 _MainTex_ST;
            half4 _MainTex_TexelSize;

            float _OutlineWidth;
            float4 _OutlineColor;
            float _OutlineOutValue;
            float _OutLineInValue;
            float _IsOutLine;
            
            struct appdata_t
            {
                float4 vertex: POSITION;
                fixed4 color: COLOR;
                float4 texcoord: TEXCOORD0;

                float4 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex: SV_POSITION;
                fixed4 color: COLOR;
                half2 uv  : TEXCOORD0;

                half2 left : TEXCOORD1;
                half2 right : TEXCOORD2;
                half2 up : TEXCOORD3;
                half2 down : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            v2f vert(appdata_t v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

                o.vertex = o.vertex + v.normal * _OutlineWidth;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;

                float4 rect = UNITY_ACCESS_INSTANCED_PROP(Props, _Rect);
                o.uv.x = 1 / rect.x * (rect.w - 1) + (o.uv.x / rect.x);
                o.uv.y = 1 / rect.y * (rect.z - 1) + (o.uv.y / rect.y);

                o.left = o.uv + half2(-1, 0) * _MainTex_TexelSize.xy * _OutlineWidth;
                o.right = o.uv + half2(1, 0) * _MainTex_TexelSize.xy * _OutlineWidth;
                o.up = o.uv + half2(0, 1) * _MainTex_TexelSize.xy * _OutlineWidth;
                o.down = o.uv + half2(0, -1) * _MainTex_TexelSize.xy * _OutlineWidth;
                return o;
            }

            fixed3 blendNormal(fixed3 base, fixed3 blend, float opacity) {
	            return (blend * opacity + base * (1.0 - opacity));
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); //   necessary only if any instanced properties are going to be accessed in the fragment Shader.
                float2 uv = i.uv;
                float texIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _TexIndex);
                fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(uv, texIndex)) * _Color;
                
                float transparent = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.left, texIndex)).a + UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.right, texIndex)).a + UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.up, texIndex)).a + UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.down, texIndex)).a;
                float isOutline = step(col.a, _OutLineInValue);
                col = (1 - isOutline) * col + isOutline * step(_OutlineOutValue, transparent) * _OutlineColor;
                col.a = clamp(0 , 1, col.a);

                float isBlank = UNITY_ACCESS_INSTANCED_PROP(Props, _Blank);
                if(isBlank == 1)
                {
                    float blankOpacity = UNITY_ACCESS_INSTANCED_PROP(Props, _BlankOpacity);
                    col.rgb = blendNormal(col.rgb, _BlankColor.rgb, blankOpacity);
                }

				return col;
            }
            ENDCG
        }
    }
}
