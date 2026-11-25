using System;

namespace ALotOfDamage.Core
{
    // Luta contra o chefe da guilda: conta o que o jogador faz, com tempo fixo. O chefe não morre na arena;
    // a vida mostrada é a da fase em que a guilda estava ao começar
    public class LutaDaGuilda
    {
        readonly DesafioDaGuilda desafio;
        readonly ParametrosDaGuilda p;
        double passou;
        string titulo;
        int segundoDoTitulo = -1;

        public LutaDaGuilda(DesafioDaGuilda desafio, ParametrosDaGuilda p)
        {
            this.desafio = desafio;
            this.p = p;
        }

        public ContagemDaLuta Contagem { get; } = new ContagemDaLuta();
        public bool Acabou => passou >= p.SegundosDaLuta;
        public double Pontos => RegrasDaGuilda.Pontos(p, Contagem, p.SegundosDaLuta);
        public double Vida => desafio.VidaDaFase;
        public double VidaRestante => Math.Max(0, desafio.VidaDaFase - desafio.PontosNaFase - Pontos);

        public InimigoDaBatalha Inimigo => new InimigoDaBatalha { Nome = p.NomeDoChefe, Vida = desafio.VidaDaFase, Chefe = true, Forma = FormaDoMonstro.Golem, Cor = "#5B3FA8" };

        // Refeito só quando o segundo muda
        public string Titulo
        {
            get
            {
                int s = (int)Math.Ceiling(Math.Max(0, p.SegundosDaLuta - passou));
                if (s != segundoDoTitulo)
                {
                    segundoDoTitulo = s;
                    titulo = $"{p.NomeDoChefe}  ·  fase {desafio.Fase}  ·  {s} s";
                }
                return titulo;
            }
        }

        public void Avancar(double segundos) => passou += segundos;

        public void Clique(int combo, bool pontoFraco)
        {
            if (Acabou) return;
            Contagem.Cliques++;
            if (pontoFraco) Contagem.PontosFracos++;
            Contagem.MaiorCombo = Math.Max(Contagem.MaiorCombo, combo);
        }

        public void Ultimate() { if (!Acabou) Contagem.Ultimates++; }
    }
}
