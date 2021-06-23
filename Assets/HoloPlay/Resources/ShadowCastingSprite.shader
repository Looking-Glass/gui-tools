Shader "Shadow Casting Sprite" {
	Properties {
		_Cutoff("Cutoff", Range(0,1)) = 0.5
		_Color ("Tint", Color) = (1,1,1,1)
		_MainTex ("Sprite Texture", 2D) = "white" {}
	}
	SubShader {
		Tags 
		{ 
			"RenderType"="Opaque"
			"IgnoreProjector"="True" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		LOD 200

		Cull Off
		Lighting On
		//ZWrite Off
		Blend One OneMinusSrcAlpha

		CGPROGRAM
		// Lambert lighting model, and enable shadows on all light types
		#pragma surface surf Lambert addshadow fullforwardshadows

		#pragma target 2.0
		#pragma multi_compile_instancing

		sampler2D _MainTex;
		fixed4 _Color;
		fixed _Cutoff;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			clip(o.Alpha - _Cutoff);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
