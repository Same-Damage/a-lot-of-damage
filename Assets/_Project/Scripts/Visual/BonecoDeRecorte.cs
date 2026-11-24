using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Personagem pintado, cortado em partes e animado por código a partir de RegrasDeAnimacao.
    // A hierarquia de ossos é montada pelo ConstrutorDeCenas a partir do montagem.json da arte.
    public class BonecoDeRecorte : PersonagemVisual
    {
        public Transform corpo;   // pai de todos os ossos; virar para a esquerda/direita é escala X dele
        public Transform tronco, cabeca, capa, bracoFrente, antebracoFrente, espada, bracoTras, antebracoTras, pernaFrente, pernaTras;
        public float duracaoDoGolpe = 0.35f;
        public float duracaoDoDano = 0.3f;
        [Tooltip("Para que lado o personagem está desenhado")]
        public bool desenhoOlhaParaDireita = true;
        public bool olhandoParaDireita = true;
        [Tooltip("Segundos para misturar uma pose na outra ao trocar de estado")]
        public float duracaoDaTransicao = 0.15f;

        EstadoDoBoneco estado = EstadoDoBoneco.Parado;
        EstadoDoBoneco estadoDeBase = EstadoDoBoneco.Parado;
        float desde, duracao;
        Vector3 posicaoDoTronco, posicaoDaPernaFrente, posicaoDaPernaTras;
        Core.Pose atual, deOnde;
        float larguraDoCorpo = 1, alturaDoCorpo = 1, viradaDesde = -10;
        float guinadaAlvo, guinadaAtual;

        // Graus que o corpo gira em direção ao movimento (RegrasDeAnimacao.Guinada)
        public float Guinada { set => guinadaAlvo = value; }

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
                if (value == olhandoParaDireita) return;
                viradaDesde = Time.unscaledTime;
                olhandoParaDireita = value;
            }
        }

        void Start()
        {
            if (tronco) posicaoDoTronco = tronco.localPosition;
            if (pernaFrente) posicaoDaPernaFrente = pernaFrente.localPosition;
            if (pernaTras) posicaoDaPernaTras = pernaTras.localPosition;
            if (corpo) { larguraDoCorpo = Mathf.Abs(corpo.localScale.x); alturaDoCorpo = corpo.localScale.y; }
        }

        float SinalAtual() => RegrasDeAnimacao.EscalaHorizontal(desenhoOlhaParaDireita, olhandoParaDireita);

        // Largura (com o lado), giro em direção ao movimento e balanço do corpo inteiro
        void AplicarDirecao(float balanco)
        {
            if (!corpo) return;
            float sinal = SinalAtual();
            float t = (Time.unscaledTime - viradaDesde) / RegrasDeAnimacao.DuracaoDaVirada;
            var s = corpo.localScale;
            s.x = larguraDoCorpo * sinal * RegrasDeAnimacao.EscalaDaVirada(t);
            s.y = alturaDoCorpo * RegrasDeAnimacao.AlturaDaVirada(t);
            corpo.localScale = s;

            guinadaAtual = Mathf.Lerp(guinadaAtual, guinadaAlvo, 1 - Mathf.Exp(-10 * Time.unscaledDeltaTime));
            corpo.localRotation = Quaternion.Euler(0, sinal * guinadaAtual, sinal * balanco);
        }

        public override void Mover(Vector3 velocidadeNoMundo, float velocidadeMaxima, Vector2 entrada)
        {
            // pela velocidade real: o joystick parado com ruído não fica ligando e desligando o andar
            Andando = velocidadeNoMundo.sqrMagnitude > 0.25f;
            Guinada = RegrasDeAnimacao.Guinada(entrada.x, entrada.y);
            if (Mathf.Abs(entrada.x) > 0.1f) OlhandoParaDireita = entrada.x > 0;
        }

        public override void Golpear(float velocidade = 1f) => Trocar(EstadoDoBoneco.Golpeando, duracaoDoGolpe / Mathf.Max(0.1f, velocidade));

        public override void LevarDano()
        {
            if (estado != EstadoDoBoneco.Golpeando) Trocar(EstadoDoBoneco.LevandoDano, duracaoDoDano);
        }

        void Trocar(EstadoDoBoneco novo, float duracaoDoEstado)
        {
            // a nova pose parte de onde o corpo está, em vez de pular
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

            // parado respira no relógio global (não reinicia a cada troca); o resto conta do início do estado
            var p = RegrasDeAnimacao.Pose(estado, estado == EstadoDoBoneco.Parado ? Time.unscaledTime : tempo, progresso);
            // o golpe entra seco, sem mistura, para não perder o impacto
            if (tempo < duracaoDaTransicao && estado != EstadoDoBoneco.Golpeando) p = RegrasDeAnimacao.Misturar(deOnde, p, tempo / duracaoDaTransicao);
            atual = p;
            // As poses são pensadas para um desenho olhando para a direita; desenho para a esquerda usa o espelho
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
            if (pernaFrente) pernaFrente.localPosition = posicaoDaPernaFrente + new Vector3(0, p.PernaFrenteSobe, 0);
            if (pernaTras) pernaTras.localPosition = posicaoDaPernaTras + new Vector3(0, p.PernaTrasSobe, 0);
            AplicarDirecao(p.Balanco);
        }

        static void Girar(Transform osso, float angulo)
        {
            if (osso) osso.localRotation = Quaternion.Euler(0, 0, angulo);
        }
    }
}
