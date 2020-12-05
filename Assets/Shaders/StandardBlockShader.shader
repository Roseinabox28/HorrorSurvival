Shader "HorrorSurvival/Blocks"
{
    Properties
    {
        _MainTex ("Block Texture Atlas", 2D) = "white" {}
    }



    SubShader
    {
        
        

        Pass 
        {
            Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
            LOD 100
            Lighting On
            CGPROGRAM
                //#pragma surface surf Standard fullforwardshadows
                
                #pragma vertex vertFunction
                #pragma fragment fragFunction
                #pragma target 3.0

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;

                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };

                sampler2D _MainTex;
                float GlobalLightLevel;
                float minGlobalLightLevel;
                float maxGlobalLightLevel;
                float localLightLevel;

                half _Glossiness;
                half _Metallic;
                fixed4 _Color;

                v2f vertFunction (appdata v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.color = v.color;

                    return o;
                }

                fixed4 fragFunction (v2f i) : SV_Target
                {
                    fixed4 col = tex2D (_MainTex, i.uv);

                    //float shade = (maxGlobalLightLevel - minGlobalLightLevel) * GlobalLightLevel + minGlobalLightLevel;
                    float lightLevel = clamp(localLightLevel + i.color.a, 0, 1);

                    clip(col.a - 1);
                    col = lerp(col, float4(0,0,0,1), localLightLevel);

                    return col;
                }

                

                ENDCG
        }
         // Pass to render object as a shadow caster
       
    }
}

