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

    // Animação procedural do boneco: uma pose desenhada vira parado, andar, golpe e dano
    public static class RegrasDeAnimacao
    {
        public const float CicloDaRespiracao = 2.4f;
        public const float CicloDoPasso = 0.7f;
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
                    float fase = Tau * tempo / CicloDoPasso;
                    float passo = Sen(fase);
                    p.PernaFrente = 20 * passo;
                    p.PernaTras = -20 * passo;
                    p.BracoFrente = -12 * passo;
                    p.BracoTras = 12 * passo;
                    p.Tronco = 2 * passo;
                    p.Capa = 8 + 4 * Sen(2 * fase);
                    p.Elevacao = Math.Abs(passo) * 0.05f;
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
