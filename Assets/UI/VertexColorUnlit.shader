Shader "Unlit/VertexColorUnlit"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct VertOut
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            VertOut vert(appdata v)
            {
                VertOut o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(VertOut i) : SV_Target
            {
                fixed4 col = i.color;

            return col;
        }
        ENDCG
    }
    }
}