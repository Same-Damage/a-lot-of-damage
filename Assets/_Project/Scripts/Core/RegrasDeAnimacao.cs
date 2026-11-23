using System;

namespace ALotOfDamage.Core
{
    public enum EstadoDoBoneco { Parado, Andando, Golpeando, LevandoDano }

    // Ângulos (graus, sentido anti-horário) de cada osso, a partir da pose desenhada (olhando para a direita)
    public struct Pose
    {
        public float Tronco, Cabeca, Capa;
        public float BracoFrente, AntebracoFrente, Espada;
        public float BracoTras, AntebracoTras;
        public float PernaFrente, PernaTras;
        public float Elevacao;   // sobe e desce do corpo, em unidades
    }

    // Ângulos das peças de uma ave voando (graus, anti-horário), a partir do desenho de asas abertas
    public struct PoseDeVoo
    {
        public float AsaFrente, AsaTras, Cabeca, Cauda;
        public float Elevacao;
    }

    // Pose de um monstro low-poly: Largura/Altura amassam o corpo (1 = normal), ângulos em graus
    public struct PoseDeMonstro
    {
        public float Largura, Altura, Elevacao;
        public float PernasA, PernasB;   // pares de pernas que andam juntos (diagonais no quadrúpede)
        public float Bracos, Cabeca, Cauda, Inclinacao;
    }

    // Animação procedural do boneco: uma pose desenhada vira parado, andar, golpe e dano
    public static class RegrasDeAnimacao
    {
        public const float CicloDaRespiracao = 2.4f;
        public const float CicloDoPasso = 0.56f;
        public const float FimDoPreparo = 0.35f;
        public const float FimDoCorte = 0.55f;
        const float EspadaEmRepouso = -20f;
        const float Tau = (float)(Math.PI * 2);

        static float Sen(float x) => (float)Math.Sin(x);
        static float Suave(float a, float b, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            t = t * t * (3 - 2 * t);
            return a + (b - a) * t;
        }

        public const float CicloDaBatida = 0.45f;

        // Batida de asas: a asa da frente (à esquerda no desenho) desce com ângulo positivo e a de trás, negativo;
        // o corpo sobe quando as asas empurram para baixo
        public static PoseDeVoo Voo(float tempo)
        {
            float batida = Sen(Tau * tempo / CicloDaBatida);
            return new PoseDeVoo
            {
                AsaFrente = 32 * batida,
                AsaTras = -32 * batida,
                Cabeca = 3 * Sen(Tau * tempo / CicloDaBatida + 1f),
                Cauda = 6 * Sen(Tau * tempo / CicloDaBatida - 1f),
                Elevacao = 0.06f * batida,
            };
        }

        public const float CicloDoPulo = 0.55f;
        public const float CicloDoTrote = 0.42f;
        public const float CicloDoGolem = 0.95f;

        public static PoseDeMonstro Monstro(FormaDoMonstro forma, float tempo, bool andando)
        {
            var p = new PoseDeMonstro { Largura = 1, Altura = 1 };
            switch (forma)
            {
                case FormaDoMonstro.Gosma:
                    if (andando)
                    {
                        // pulinhos: achatada no chão, esticada no alto do pulo
                        float f = tempo / CicloDoPulo % 1f;
                        float arco = Sen((float)Math.PI * f);
                        p.Elevacao = 0.35f * arco;
                        p.Altura = 0.78f + 0.37f * arco;
                    }
                    else p.Altura = 1 + 0.05f * Sen(Tau * tempo / 1.6f);
                    break;
                case FormaDoMonstro.Quadrupede:
                    if (andando)
                    {
                        float fase = Tau * tempo / CicloDoTrote;
                        p.PernasA = 28 * Sen(fase);
                        p.PernasB = -p.PernasA;
                        p.Elevacao = 0.035f * (float)Math.Cos(2 * fase);
                        p.Cabeca = 4 * Sen(2 * fase);
                        p.Cauda = 15 * Sen(fase);
                    }
                    else
                    {
                        p.Cauda = 12 * Sen(Tau * tempo / 1.1f);
                        p.Altura = 1 + 0.015f * Sen(Tau * tempo / CicloDaRespiracao);
                    }
                    break;
                case FormaDoMonstro.Golem:
                    if (andando)
                    {
                        // passos pesados: balança de um lado para o outro e os braços vão ao contrário das pernas
                        float fase = Tau * tempo / CicloDoGolem;
                        p.PernasA = 18 * Sen(fase);
                        p.PernasB = -p.PernasA;
                        p.Bracos = -20 * Sen(fase);
                        p.Inclinacao = 4 * Sen(fase);
                        p.Elevacao = 0.05f * (float)Math.Cos(2 * fase);
                    }
                    else
                    {
                        p.Bracos = 3 * Sen(Tau * tempo / 3f);
                        p.Altura = 1 + 0.012f * Sen(Tau * tempo / 3f);
                    }
                    break;
            }
            // amassar em altura alarga para os lados, mantendo o volume
            p.Largura = 1 / (float)Math.Sqrt(p.Altura);
            return p;
        }

        // Transição entre duas poses (t de 0 a 1), para trocar de estado sem salto
        public static Pose Misturar(Pose a, Pose b, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            float L(float x, float y) => x + (y - x) * t;
            return new Pose
            {
                Tronco = L(a.Tronco, b.Tronco), Cabeca = L(a.Cabeca, b.Cabeca), Capa = L(a.Capa, b.Capa),
                BracoFrente = L(a.BracoFrente, b.BracoFrente), AntebracoFrente = L(a.AntebracoFrente, b.AntebracoFrente),
                Espada = L(a.Espada, b.Espada),
                BracoTras = L(a.BracoTras, b.BracoTras), AntebracoTras = L(a.AntebracoTras, b.AntebracoTras),
                PernaFrente = L(a.PernaFrente, b.PernaFrente), PernaTras = L(a.PernaTras, b.PernaTras),
                Elevacao = L(a.Elevacao, b.Elevacao),
            };
        }

        // Escala X do corpo: espelha só quando o personagem olha para o lado contrário do desenho
        public static int EscalaHorizontal(bool desenhoOlhaParaDireita, bool olhandoParaDireita) =>
            desenhoOlhaParaDireita == olhandoParaDireita ? 1 : -1;

        // tempo: segundos desde o início do estado (para ciclos); progresso: 0..1 (golpe e dano)
        public static Pose Pose(EstadoDoBoneco estado, float tempo, float progresso)
        {
            progresso = Math.Max(0, Math.Min(1, progresso));
            float respiracao = Sen(Tau * tempo / CicloDaRespiracao);
            var p = new Pose
            {
                Tronco = respiracao * 0.6f,
                Cabeca = respiracao * 1.2f,
                Capa = respiracao * 2.5f,
                BracoFrente = respiracao * 2f,
                BracoTras = -respiracao * 2f,
                Espada = EspadaEmRepouso + respiracao * 1.5f,
                Elevacao = respiracao * 0.012f,
            };

            switch (estado)
            {
                case EstadoDoBoneco.Andando:
                {
                    // fase 0: pernas se cruzando; um quarto de ciclo depois, a perna da frente tocando o chão
                    float fase = Tau * tempo / CicloDoPasso;
                    float passo = Sen(fase);
                    float ritmo = (float)Math.Cos(2 * fase);   // duas batidas por ciclo, uma por pé
                    p.PernaFrente = 24 * passo;
                    p.PernaTras = -24 * passo;
                    p.BracoFrente = -14 * passo;
                    p.BracoTras = 14 * passo;
                    p.AntebracoFrente = -6 - 4 * passo;
                    p.AntebracoTras = -6 + 4 * passo;
                    p.Espada = EspadaEmRepouso - 4 * passo;
                    // inclinado para a frente, com um leve balanço a cada pé
                    p.Tronco = -4 + 1.5f * ritmo;
                    p.Cabeca = 2 - 1.5f * ritmo;
                    // a capa fica para trás e ondula atrasada em relação ao corpo
                    p.Capa = 14 + 5 * Sen(2 * fase - 1.2f);
                    // mais alto quando as pernas cruzam, mais baixo quando o pé toca
                    p.Elevacao = 0.045f * ritmo;
                    break;
                }
                case EstadoDoBoneco.Golpeando:
                    if (progresso < FimDoPreparo)
                    {
                        float t = progresso / FimDoPreparo;
                        p.BracoFrente = Suave(0, -150, t);
                        p.AntebracoFrente = Suave(0, 30, t);
                        p.Espada = Suave(EspadaEmRepouso, -40, t);
                        p.Tronco = Suave(0, -6, t);
                    }
                    else if (progresso < FimDoCorte)
                    {
                        float t = (progresso - FimDoPreparo) / (FimDoCorte - FimDoPreparo);
                        p.BracoFrente = Suave(-150, 70, t);
                        p.AntebracoFrente = Suave(30, -10, t);
                        p.Espada = Suave(-40, 10, t);
                        p.Tronco = Suave(-6, 10, t);
                        p.Capa = Suave(0, 12, t);
                    }
                    else
                    {
                        float t = (progresso - FimDoCorte) / (1 - FimDoCorte);
                        p.BracoFrente = Suave(70, 0, t);
                        p.AntebracoFrente = Suave(-10, 0, t);
                        p.Espada = Suave(10, EspadaEmRepouso, t);
                        p.Tronco = Suave(10, 0, t);
                        p.Capa = Suave(12, 0, t);
                    }
                    break;
                case EstadoDoBoneco.LevandoDano:
                {
                    float forca = 1 - progresso;
                    p.Tronco = 12 * forca;   // positivo inclina para a esquerda: para trás de quem olha para a direita
                    p.Cabeca = 10 * forca;
                    p.BracoFrente = 15 * forca;
                    p.BracoTras = 15 * forca;
                    break;
                }
            }
            return p;
        }
    }
}
