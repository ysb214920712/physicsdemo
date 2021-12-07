Shader "Custom/LiquidWobble"
{
    Properties
    {
        _LiquidColor ("液体颜色", Color) = (1, 1, 1, 1)
        _NoiseTex ("液体纹理", 2D) = "white" {}
        _FillAmount ("液体填充量/液面高度", Range(-10, 10)) = 0.0
        [HideInInspector] _WobbleX ("WobbleX", Range(-1, 1)) = 0.0
        [HideInInspector] _WobbleZ ("WobbleZ", Range(-1, 1)) = 0.0
        _LiquidTopColor ("液面颜色", Color) = (1, 1, 1, 1)
        _LiquidFoamColor ("液面泡沫颜色", Color) = (1, 1, 1, 1)
        _FoamLineWidth ("液面泡沫高度", Range(0, 0.1)) = 0.0
        _LiquidRimColor ("液体边缘颜色", Color) = (1, 1, 1, 1)
        _LiquidRimPower ("液体边缘宽度", Range(0, 10)) = 0.0
        _LiquidRimIntensity ("液体边缘强度", Range(0.0, 3.0)) = 1.0

        _BottleColor ("瓶子颜色", Color) = (0.5, 0.5, 0.5, 0.3)
        _BottleThickness ("瓶子厚度", Range(0, 1)) = 0.1

        _BottleRimColor ("瓶边缘颜色", Color) = (1, 1, 1, 1)
        _BottleRimPower ("瓶边缘宽度", Range(0, 1)) = 0.0
        _BottleRimIntensity ("瓶边缘强度", Range(0.0, 3.0)) = 1.0

        _BottleSpecular ("瓶子镜面反射", Range(0, 1)) = 0.5
        _BottleGloss ("瓶子玻璃光泽", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "DisableBatching" = "True"
            "Queue"="Transparent"
            "RenderType"="Transparent" 
        }
        
        // 此Pass用于绘制流体
        Pass
        {
            Tags {"RenderType" = "Opaque" "Queue" = "Geometry"}

            Zwrite On
            Cull Off
            AlphaToMask On // 透明

            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 viewDir : COLOR;
                float3 normal : COLOR2;
                float fillEdge : TEXCOORD2;
            };

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _FillAmount, _WobbleX, _WobbleZ;
            float4 _LiquidTopColor, _LiquidRimColor, _LiquidFoamColor, _LiquidColor;
            float _FoamLineWidth, _LiquidRimPower, _LiquidRimIntensity;

            float4 RotateAroundYInDegrees (float4 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, sina, -sina, cosa);
                return float4(vertex.yz, mul(m, vertex.xz)).xzyw;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                //获取顶点世界坐标
                float3 worldPos = mul (unity_ObjectToWorld, v.vertex.xyz);
                //绕XY旋转
                float3 worldPosX = RotateAroundYInDegrees(float4(worldPos, 0), 360);
                //旋转XZ
                float3 worldPosZ = float3 (worldPosX.y, worldPosX.z, worldPosX.x);
                //使用脚本传递的正弦波,结合旋转和世界坐标
                float3 worldPosAdjusted = worldPos + (worldPosX * _WobbleX) + (worldPosZ * _WobbleZ);
                //获取液面高度
                o.fillEdge = worldPosAdjusted.y + _FillAmount;

                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                // 采样纹理
                float4 col = tex2D(_NoiseTex, i.uv) * _LiquidColor;
                // 应用雾
                UNITY_APPLY_FOG(i.fogCoord, col);

                // 轮廓光
                float dotProduct = 1 - pow(dot(i.normal, i.viewDir), _LiquidRimPower);
                float4 RimResult = _LiquidRimColor * smoothstep(0.5, 1.0, dotProduct) * _LiquidRimIntensity;

                // 泡沫边缘
                float4 foam = step(i.fillEdge, 0.5) - step(i.fillEdge, (0.5 - _FoamLineWidth));
                float4 foamColored = foam * _LiquidFoamColor;

                // 液体复位
                float4 result = step(i.fillEdge, 0.5) - foam;
                float4 resultColored = result * col;

                // 结合后作用于纹理
                float4 finalResult = resultColored + foamColored;
                finalResult.rgb += RimResult;

                // 背面和顶部颜色
                float4 topColor = _LiquidTopColor * (foam + result);
                // 正面返回finalResult, 背面返回topColor
                return facing > 0 ? finalResult : topColor;
            }

            ENDCG
        }

        // 此Pass绘制玻璃瓶
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewDir : COLOR;
                float3 normal : COLOR2;
                float2 uv : TEXCOORD0;
                float3 lightDir : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 viewDirWorld : TEXCOORD3;
                UNITY_FOG_COORDS(3)
            };

            float4 _BottleColor, _BottleRimColor;
            float _BottleThickness, _BottleRim, _BottleRimPower, _BottleRimIntensity;
            float _BottleSpecular, _BottleGloss;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.xyz += _BottleThickness * v.normal;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.normal = v.normal;

                float4 posWorld = mul (unity_ObjectToWorld, v.vertex);
                o.viewDirWorld = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);
                o.normalDir = UnityObjectToWorldNormal (v.normal);
                o.lightDir = normalize(_WorldSpaceLightPos0.xyz);

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                // 镜面反射
                i.normalDir = normalize(i.normalDir);
                float specularPow = exp2 ((1 - _BottleGloss) * 10.0 + 1.0);
                fixed4 specularColor = fixed4 (_BottleSpecular, _BottleSpecular, _BottleSpecular, _BottleSpecular);


                float3 halfVector = normalize (i.lightDir + i.viewDirWorld);
                fixed4 specularCol = pow (max (0, dot (halfVector, i.normalDir)), specularPow) * specularColor;

                // 轮廓光
                float dotProduct = 1 - pow(dot (i.normal, i.viewDir), _BottleRimPower);
                fixed4 RimCol = _BottleRimColor * smoothstep(0.5, 1.0, dotProduct) * _BottleRimIntensity;

                fixed4 finalCol = RimCol + _BottleColor + specularCol;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return finalCol;
            }

            ENDCG
        }
        
    }
    FallBack "Diffuse"
}
