Shader "Custom/BillboardRingXZ"
{
    Properties
    {
        _Color ("Ring Color", Color) = (0,1,0,1)
        _RingRadius ("Ring Radius", Float) = 1
        _LineWidth ("Line Width (World Units)", Float) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _Color;
            float _RingRadius;
            float _LineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // For XZ quad, get distance from world center in XZ
                float2 posXZ = i.worldPos.xz;
                float dist = length(posXZ);

                float halfWidth = _LineWidth * 0.5;
                float alpha = smoothstep(halfWidth, 0, abs(dist - _RingRadius));

                return float4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}
