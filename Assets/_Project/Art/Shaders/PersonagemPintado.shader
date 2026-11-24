// Personagem 3D com cara de pintado: luz em faixas (toon), brilho de borda e contorno.
// Recebe a luz principal e o ambiente do cenário, então muda de cor com a região como o resto do mapa.
Shader "ALotOfDamage/PersonagemPintado"
{
    Properties
    {
        _BaseMap ("Textura", 2D) = "white" {}
        _BaseColor ("Cor", Color) = (1, 1, 1, 1)
        _CorDaSombra ("Cor da sombra", Color) = (0.62, 0.55, 0.68, 1)
        _LimiarDaSombra ("Limiar da sombra", Range(-1, 1)) = 0.05
        _SuavidadeDaSombra ("Suavidade da sombra", Range(0.001, 0.5)) = 0.08
        _ForcaDoAmbiente ("Força do ambiente", Range(0, 1)) = 0.35
        _CorDaBorda ("Cor do brilho de borda", Color) = (1, 0.86, 0.55, 1)
        _ForcaDaBorda ("Força do brilho de borda", Range(0, 2)) = 0.6
        _PotenciaDaBorda ("Largura do brilho de borda", Range(1, 10)) = 4
        _CorDoContorno ("Cor do contorno", Color) = (0.12, 0.08, 0.06, 1)
        _LarguraDoContorno ("Largura do contorno", Range(0, 0.01)) = 0.001
        _RecuoDoContorno ("Recuo do contorno (metros para trás, em tecido fino)", Range(0, 0.2)) = 0
        _CorDaEmissao ("Cor da emissão (poder do item)", Color) = (0, 0, 0, 1)
        _ForcaDaEmissao ("Força da emissão", Range(0, 4)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _CorDaSombra;
            half _LimiarDaSombra;
            half _SuavidadeDaSombra;
            half _ForcaDoAmbiente;
            half4 _CorDaBorda;
            half _ForcaDaBorda;
            half _PotenciaDaBorda;
            half4 _CorDoContorno;
            float _LarguraDoContorno;
            float _RecuoDoContorno;
            half4 _CorDaEmissao;
            half _ForcaDaEmissao;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        ENDHLSL

        Pass
        {
            Name "Pintado"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            struct Entrada
            {
                float4 posicao : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Saida
            {
                float4 posicaoClip : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalMundo : TEXCOORD1;
                float3 posicaoMundo : TEXCOORD2;
                half fog : TEXCOORD3;
            };

            Saida vert(Entrada e)
            {
                Saida s;
                VertexPositionInputs p = GetVertexPositionInputs(e.posicao.xyz);
                s.posicaoClip = p.positionCS;
                s.posicaoMundo = p.positionWS;
                s.normalMundo = TransformObjectToWorldNormal(e.normal);
                s.uv = TRANSFORM_TEX(e.uv, _BaseMap);
                s.fog = ComputeFogFactor(p.positionCS.z);
                return s;
            }

            half4 frag(Saida s) : SV_Target
            {
                half4 cor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, s.uv) * _BaseColor;
                float3 n = normalize(s.normalMundo);
                Light luz = GetMainLight(TransformWorldToShadowCoord(s.posicaoMundo));

                // luz em duas faixas, com a borda entre elas levemente suave
                half ndl = dot(n, luz.direction) * luz.shadowAttenuation;
                half faixa = smoothstep(_LimiarDaSombra - _SuavidadeDaSombra, _LimiarDaSombra + _SuavidadeDaSombra, ndl);
                half3 tom = lerp(_CorDaSombra.rgb, half3(1, 1, 1), faixa) * luz.color;
                tom += SampleSH(n) * _ForcaDoAmbiente;

                // brilho dourado na borda, mais forte do lado iluminado
                float3 v = normalize(GetWorldSpaceViewDir(s.posicaoMundo));
                half borda = pow(saturate(1 - dot(n, v)), _PotenciaDaBorda) * _ForcaDaBorda * (0.35 + 0.65 * faixa);

                half3 final = cor.rgb * tom + _CorDaBorda.rgb * borda;
                // brilho próprio do item (raridade e refino): mais forte nas partes claras do desenho, que pegam o bloom
                half luz_ = dot(cor.rgb, half3(0.3, 0.59, 0.11));
                final += _CorDaEmissao.rgb * _ForcaDaEmissao * (0.35 + 0.65 * luz_);
                final = MixFog(final, s.fog);
                return half4(final, 1);
            }
            ENDHLSL
        }

        // Contorno: a malha inflada pela normal, desenhada só por dentro (casco invertido)
        Pass
        {
            Name "Contorno"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            struct Entrada
            {
                float4 posicao : POSITION;
                float3 normal : NORMAL;
            };

            struct Saida
            {
                float4 posicaoClip : SV_POSITION;
                half fog : TEXCOORD0;
            };

            Saida vert(Entrada e)
            {
                Saida s;
                float3 posicaoMundo = TransformObjectToWorld(e.posicao.xyz);
                float3 normalMundo = TransformObjectToWorldNormal(e.normal);
                // largura constante na tela, independente da distância da câmera
                float distancia = length(GetCameraPositionWS() - posicaoMundo);
                posicaoMundo += normalMundo * _LarguraDoContorno * distancia;
                posicaoMundo += normalize(posicaoMundo - _WorldSpaceCameraPos) * _RecuoDoContorno;
                s.posicaoClip = TransformWorldToHClip(posicaoMundo);
                s.fog = ComputeFogFactor(s.posicaoClip.z);
                return s;
            }

            half4 frag(Saida s) : SV_Target
            {
                return half4(MixFog(_CorDoContorno.rgb, s.fog), 1);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
