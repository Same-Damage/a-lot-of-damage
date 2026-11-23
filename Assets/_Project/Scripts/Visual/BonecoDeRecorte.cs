using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Personagem pintado, cortado em partes e animado por cÃ³digo a partir de RegrasDeAnimacao.
    // A hierarquia de ossos Ã© montada pelo ConstrutorDeCenas a partir do montagem.json da arte.
    public class BonecoDeRecorte : MonoBehaviour
    {
        public Transform corpo;   // pai de todos os ossos; virar para a esquerda/direita Ã© escala X dele
        public Transform tronco, cabeca, capa, bracoFrente, antebracoFrente, espada, bracoTras, antebracoTras, pernaFrente, pernaTras;
        public float duracaoDoGolpe = 0.35f;
        public float duracaoDoDano = 0.3f;
        [Tooltip("Para que lado o personagem estÃ¡ desenhado")]
        public bool desenhoOlhaParaDireita = true;
        public bool olhandoParaDireita = true;
        [Tooltip("Segundos para misturar uma pose na outra ao trocar de estado")]
        public float duracaoDaTransicao = 0.15f;

        EstadoDoBoneco estado = EstadoDoBoneco.Parado;
        EstadoDoBoneco estadoDeBase = EstadoDoBoneco.Parado;
        float desde, duracao;
        Vector3 posicaoDoTronco;
        Core.Pose atual, deOnde;

        public bool Andando
        {
            set
            {
                var novo = value ? EstadoDoBoneco.Andando : EstadoDoBoneco.Parado;
                if (novo == estadoDeBase) return;
                estadoDeBase = novo;
                if (estado == EstadoDoBoneco.Parado || estado == EstadoDoBoneco.Andando) Trocar(novo, 0);
            }
        }

        public bool OlhandoParaDireita
        {
            get => olhandoParaDireita;
            set
            {
                olhandoParaDireita = value;
                AplicarDirecao();
            }
        }

        void Start()
        {
            if (tronco) posicaoDoTronco = tronco.localPosition;
            AplicarDirecao();
        }

        void AplicarDirecao()
        {
            if (!corpo) return;
            var s = corpo.localScale;
            s.x = Mathf.Abs(s.x) * RegrasDeAnimacao.EscalaHorizontal(desenhoOlhaParaDireita, olhandoParaDireita);
            corpo.localScale = s;
        }

        public void Golpear(float velocidade = 1f) => Trocar(EstadoDoBoneco.Golpeando, duracaoDoGolpe / Mathf.Max(0.1f, velocidade));

        public void LevarDano()
        {
            if (estado != EstadoDoBoneco.Golpeando) Trocar(EstadoDoBoneco.LevandoDano, duracaoDoDano);
        }

        void Trocar(EstadoDoBoneco novo, float duracaoDoEstado)
        {
            // a nova pose parte de onde o corpo estÃ¡, em vez de pular
            deOnde = atual;
            estado = novo;
            desde = Time.unscaledTime;
            duracao = duracaoDoEstado;
        }

        void Update()
        {
            float tempo = Time.unscaledTime - desde;
            float progresso = duracao > 0 ? tempo / duracao : 0;
            if (duracao > 0 && progresso >= 1) Trocar(estadoDeBase, 0);

            // parado respira no relÃ³gio global (nÃ£o reinicia a cada troca); o resto conta do inÃ­cio do estado
            var p = RegrasDeAnimacao.Pose(estado, estado == EstadoDoBoneco.Parado ? Time.unscaledTime : tempo, progresso);
            // o golpe entra seco, sem mistura, para nÃ£o perder o impacto
            if (tempo < duracaoDaTransicao && estado != EstadoDoBoneco.Golpeando) p = RegrasDeAnimacao.Misturar(deOnde, p, tempo / duracaoDaTransicao);
            atual = p;
            // As poses sÃ£o pensadas para um desenho olhando para a direita; desenho para a esquerda usa o espelho
            float sinal = desenhoOlhaParaDireita ? 1 : -1;
            Girar(tronco, sinal * p.Tronco);
            Girar(cabeca, sinal * p.Cabeca);
            Girar(capa, sinal * p.Capa);
            Girar(bracoFrente, sinal * p.BracoFrente);
            Girar(antebracoFrente, sinal * p.AntebracoFrente);
            Girar(espada, sinal * p.Espada);
            Girar(bracoTras, sinal * p.BracoTras);
            Girar(antebracoTras, sinal * p.AntebracoTras);
            Girar(pernaFrente, sinal * p.PernaFrente);
            Girar(pernaTras, sinal * p.PernaTras);
            if (tronco) tronco.localPosition = posicaoDoTronco + new Vector3(0, p.Elevacao, 0);
        }

        static void Girar(Transform osso, float angulo)
        {
            if (osso) osso.localRotation = Quaternion.Euler(0, 0, angulo);
        }
    }
}
