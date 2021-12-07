Shader "Custom/Animal Crossing"
{
    Properties
    {
	   [NoScaleOffset]
        _MainTex ("颜色纹理", 2D) = "white" {}
		_BendY("弯曲程度", Range(0,0.1)) = 0.0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

				//获取模型第一套UV
                float2 uv : TEXCOORD0;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

				//定义顶点世界变量
				float4 WordData : TEXCOORD1;
				float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(4)
            };

			//颜色纹理
            sampler2D _MainTex;
			float _BendY;
            v2f vert (appdata v)
            {
                v2f o;

                o.uv = v.uv;
				
				float3 WordPos = mul(unity_ObjectToWorld, v.vertex);
				//获取Y轴坐标
				o.WordData.x = WordPos.y;

				//Y轴扭 = 曲依据XZ坐标变化
				WordPos.y -= pow(WordPos.x, 2)*_BendY;
				WordPos.y -= pow(WordPos.z, 2)*_BendY;

				//获取位移位置
				WordPos -= mul(unity_ObjectToWorld, float4(0, 0, 0, 1));

				//修改世界顶点转回物体自身顶点。
				v.vertex = mul(unity_WorldToObject, WordPos);
				//转换为裁切空间
				o.vertex = UnityObjectToClipPos(v.vertex);
				
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//获取颜色贴图
                fixed4 col = tex2D(_MainTex, i.uv);
				

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
