Shader "Hidden/DamageText"
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{	
			CGPROGRAM
			#include "UnityCG.cginc"

			struct a2v 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				half4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
				half4 color : COLOR;
			};

			sampler2D _MainTex;
			
			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex, i.texcoord);
				return col * i.color;
			}

			#pragma vertex vert
			#pragma fragment frag

			ENDCG 
		}
	}	
}
