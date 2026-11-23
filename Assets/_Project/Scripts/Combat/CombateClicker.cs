using System.Collections.Generic;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace ALotOfDamage.Combat
{
    // Protótipo do combate clicker: auto-ataque + click com combo + ponto fraco + ultimate,
    // com a passiva e a ultimate da classe e o follower lutando junto.
    // HUD em OnGUI só para testar a sensação; trocar por UI de verdade depois.
    public class CombateClicker : MonoBehaviour
    {
        [Header("Jogador")]
        public float intervaloAutomatico = 1f;
        public double multiplicadorCritico = RegrasDePoder.MultiplicadorCriticoPadrao;
        public int cliquesPorSegundoMaximo = 15;

        [Header("Combo")]
        public float tempoDoCombo = 1f;
        public float bonusDeComboPorHit = 0.02f;

        [Header("Ponto fraco")]
        public float pontoFracoACada = 4f;
        public float duracaoDoPontoFraco = 1.2f;
        public double multiplicadorPontoFraco = 10;

        [Header("Ultimate")]
        public float cargaDaUltimatePorClick = 2f;
        public double multiplicadorDaUltimate = 50;

        [Header("Inimigo")]
        public double vidaBaseDoInimigo = 100;
        public double crescimentoDaVida = 1.55;

        public Transform visualDoInimigo;

        int estagio = 1;
        double vidaDoInimigo, vidaMaximaDoInimigo;
        int combo;
        float ultimoClick, timerAutomatico, timerFollower, timerPontoFraco, pontoFracoRestante, ultimate, tremor;
        float forcaDoTremor = 0.08f;
        LimitadorDeCliques limitador;
        readonly List<Popup> popups = new List<Popup>();

        string nomeDoInimigo = "Inimigo";
        string faixa;
        float faixaAte, cliquesTravadosAte, automaticoComecaEm, voltarEm = -1;

        FluxoDeEncontro encontro;
        IRepositorioDeProgresso repositorio;
        CalcularAtributos calcularAtributos;
        RegistrarVitoria registrarVitoria;

        // O progresso não muda durante a luta, então os atributos são calculados uma vez
        AtributosDeCombate atributos;
        double DanoDoClick => atributos.DanoDoClick;
        double DanoAutomatico => atributos.DanoAutomatico;
        double ChanceCritica => atributos.ChanceCritica;
        double Ouro => repositorio.Carregar().Ouro;

        [Inject]
        public void Construir(FluxoDeEncontro encontro, IRepositorioDeProgresso repositorio,
            CalcularAtributos calcularAtributos, RegistrarVitoria registrarVitoria)
        {
            this.encontro = encontro;
            this.repositorio = repositorio;
            this.calcularAtributos = calcularAtributos;
            this.registrarVitoria = registrarVitoria;
        }

        struct Popup { public string texto; public Vector2 pos; public float t; public Color cor; public int tamanho; }

        void Start()
        {
            limitador = new LimitadorDeCliques(cliquesPorSegundoMaximo);
            atributos = calcularAtributos.Executar(encontro.Ativo ? encontro.TipoDoInimigo : null);
            if (encontro.Ativo)
            {
                nomeDoInimigo = encontro.NomeDoInimigo;
                vidaBaseDoInimigo *= encontro.MultiplicadorDeVida;
            }
            NovoInimigo();

            if (!encontro.Ativo) return;
            switch (encontro.Vantagem)
            {
                case Vantagem.Preventivo:
                    MostrarFaixa("ATAQUE PREVENTIVO!");
                    ultimate = 100;
                    Acertar(DanoDoClick * 20, true, new Color(0.4f, 1f, 0.5f), 48);
                    break;
                case Vantagem.Emboscada:
                    MostrarFaixa("EMBOSCADA!");
                    cliquesTravadosAte = Time.time + 1.5f;
                    automaticoComecaEm = Time.time + 2.5f;
                    break;
            }
        }

        void MostrarFaixa(string texto)
        {
            faixa = texto;
            faixaAte = Time.time + 1.8f;
        }

        void NovoInimigo()
        {
            vidaMaximaDoInimigo = RegrasDoClick.VidaDoInimigo(vidaBaseDoInimigo, crescimentoDaVida, estagio);
            vidaDoInimigo = vidaMaximaDoInimigo;
        }

        void Update()
        {
            float agora = Time.time;

            if (voltarEm > 0)
            {
                if (agora >= voltarEm) encontro.Encerrar(true);
                return;
            }

            if (encontro.Ativo && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                encontro.Encerrar(false);
                return;
            }

            timerAutomatico += Time.deltaTime;
            if (agora >= automaticoComecaEm && timerAutomatico >= intervaloAutomatico)
            {
                timerAutomatico = 0;
                Acertar(DanoAutomatico, false, Color.white, 22);
            }

            var follower = atributos.Follower;
            if (follower != null && agora >= automaticoComecaEm)
            {
                timerFollower += Time.deltaTime;
                if (timerFollower >= follower.IntervaloDeAtaque)
                {
                    timerFollower = 0;
                    Acertar(atributos.DanoDoFollower, false, new Color(0.55f, 0.8f, 1f), 20);
                }
            }

            if (combo > 0 && agora - ultimoClick > tempoDoCombo) combo = 0;

            timerPontoFraco += Time.deltaTime;
            if (pontoFracoRestante > 0) pontoFracoRestante -= Time.deltaTime;
            else if (timerPontoFraco >= pontoFracoACada) { timerPontoFraco = 0; pontoFracoRestante = RegrasDePersonagem.DuracaoDoPontoFraco(atributos.Follower, duracaoDoPontoFraco); }

            var ponteiro = Pointer.current;
            if (ponteiro != null && ponteiro.press.wasPressedThisFrame) AoClicar(agora);

            var teclado = Keyboard.current;
            if (teclado != null && teclado.spaceKey.wasPressedThisFrame && ultimate >= 100) DispararUltimate();

            if (tremor > 0) tremor -= Time.deltaTime;
            if (visualDoInimigo)
                visualDoInimigo.localPosition = tremor > 0 ? (Vector3)Random.insideUnitCircle * forcaDoTremor : Vector3.zero;
        }

        ParametrosDoClick Parametros() => new ParametrosDoClick
        {
            DanoBase = DanoDoClick,
            BonusDeComboPorHit = bonusDeComboPorHit,
            MultiplicadorCritico = multiplicadorCritico,
            MultiplicadorPontoFraco = multiplicadorPontoFraco,
        };

        void AoClicar(float agora)
        {
            if (agora < cliquesTravadosAte) return;
            if (!limitador.Registrar(agora)) return;

            combo++;
            ultimoClick = agora;
            ultimate = Mathf.Min(100, ultimate + cargaDaUltimatePorClick);

            bool critico = Random.value < ChanceCritica;
            bool pontoFraco = pontoFracoRestante > 0;
            double dano = RegrasDoClick.Dano(Parametros(), combo, critico, pontoFraco)
                * RegrasDePersonagem.MultiplicadorDeFuria(atributos.Classe, combo);

            Color cor = new Color(1f, 0.85f, 0.3f);
            int tamanho = 26;
            if (pontoFraco) { cor = new Color(0.4f, 1f, 1f); tamanho = 40; ultimate = Mathf.Min(100, ultimate + 10); }
            if (critico) { cor = new Color(1f, 0.3f, 0.3f); tamanho += 10; }

            Acertar(dano, true, cor, tamanho);
        }

        void DispararUltimate()
        {
            ultimate = 0;
            var classe = atributos.Classe;
            double multiplicador = classe?.MultiplicadorDaUltimate ?? multiplicadorDaUltimate;
            if (!string.IsNullOrEmpty(classe?.NomeDaUltimate)) MostrarFaixa(classe.NomeDaUltimate.ToUpperInvariant() + "!");
            Acertar(DanoDoClick * multiplicador * (1 + combo * bonusDeComboPorHit), true, new Color(0.8f, 0.4f, 1f), 56);
            tremor = 0.4f;
            forcaDoTremor = 0.3f;
        }

        void Acertar(double dano, bool doClick, Color cor, int tamanho)
        {
            if (voltarEm > 0) return;
            vidaDoInimigo -= dano;
            if (doClick && tremor <= 0) { tremor = 0.08f; forcaDoTremor = 0.08f; }

            popups.Add(new Popup
            {
                texto = FormatadorDeNumeros.Formatar(dano),
                pos = new Vector2(Screen.width * 0.5f + Random.Range(-80, 80), Screen.height * 0.45f),
                t = 0, cor = cor, tamanho = tamanho
            });

            if (vidaDoInimigo > 0) return;

            if (encontro.Ativo)
            {
                var v = registrarVitoria.Executar(encontro.TipoDoInimigo, vidaMaximaDoInimigo * 0.5);
                string texto = $"VITÓRIA!  +{FormatadorDeNumeros.Formatar(v.Ouro)} ouro";
                if (v.MaterialGanho > 0) texto += $"  +{v.MaterialGanho} {v.Monstro.NomeDoMaterial}";
                if (v.AtingiuMarcoDoBestiario) texto += $"\nBESTIÁRIO: {v.Monstro.Nome} x{v.Derrotas}!";
                MostrarFaixa(texto);
                voltarEm = Time.time + 1.5f;
            }
            else
            {
                // Modo de teste da cena Batalha aberta direto: estágios infinitos
                registrarVitoria.Executar(null, vidaMaximaDoInimigo * 0.5);
                estagio++;
                NovoInimigo();
            }
        }

        void OnGUI()
        {
            var titulo = TemaVisual.EstiloTitulo;
            float l = Screen.width, a = Screen.height;

            // placa do topo esquerdo: estágio ou inimigo e ouro
            var placa = TemaVisual.Painel(new Rect(16, 16, 330, 64), null);
            TemaVisual.TextoComContorno(new Rect(placa.x, placa.y - 4, placa.width, 22),
                encontro.Ativo ? nomeDoInimigo : $"Estágio {estagio}", titulo, TemaVisual.OuroClaro, TextAnchor.UpperLeft, 18);
            TemaVisual.TextoComContorno(new Rect(placa.x, placa.y + 18, placa.width, 22),
                $"Ouro  {FormatadorDeNumeros.Formatar(Ouro)}", titulo, TemaVisual.CorDoTexto, TextAnchor.UpperLeft, 16);

            if (encontro.Ativo && voltarEm < 0 && TemaVisual.Botao(new Rect(l - 150, a - 70, 130, 44), "Fugir [Esc]"))
                encontro.Encerrar(false);

            DesenharVidaDoInimigo(l);

            if (Time.time < faixaAte || voltarEm > 0) DesenharFaixa(l, a);

            if (pontoFracoRestante > 0)
                TemaVisual.TextoComContorno(new Rect(0, 150, l, 40), "PONTO FRACO! CLIQUE!", titulo, TemaVisual.Ciano, TextAnchor.MiddleCenter, 30);

            if (combo > 1) DesenharCombo(l, a);
            DesenharBarraInferior(l, a);
            DesenharPopups();
        }

        void DesenharVidaDoInimigo(float l)
        {
            float largura = Mathf.Min(l * 0.5f, 620);
            var painel = new Rect(l - largura - 20, 16, largura, 70);
            var dentro = TemaVisual.Painel(painel, null);
            var barra = new Rect(dentro.x + 8, dentro.y + 16, dentro.width - 16, 16);
            TemaVisual.Barra(barra, TemaVisual.Fracao(vidaDoInimigo / vidaMaximaDoInimigo), TemaVisual.Vida);
            var titulo = TemaVisual.EstiloTitulo;
            TemaVisual.TextoComContorno(new Rect(dentro.x, dentro.y - 4, dentro.width, 22), nomeDoInimigo, titulo, TemaVisual.OuroClaro, TextAnchor.UpperRight, 16);
            TemaVisual.TextoComContorno(barra,
                $"{FormatadorDeNumeros.Formatar(System.Math.Max(0, vidaDoInimigo))} / {FormatadorDeNumeros.Formatar(vidaMaximaDoInimigo)}",
                titulo, Color.white, TextAnchor.MiddleCenter, 14);
        }

        void DesenharFaixa(float l, float a)
        {
            if (string.IsNullOrEmpty(faixa)) return;
            int quebra = faixa.IndexOf('\n');
            string primeira = quebra < 0 ? faixa : faixa.Substring(0, quebra);
            TemaVisual.FaixaDeHabilidade(l * 0.5f, a * 0.2f, primeira);
            if (quebra >= 0)
                TemaVisual.TextoComContorno(new Rect(0, a * 0.2f + 58, l, 34), faixa.Substring(quebra + 1),
                    TemaVisual.EstiloTitulo, TemaVisual.Ciano, TextAnchor.MiddleCenter, 22);
        }

        // Contador de combo grande à direita, com o acúmulo da passiva embaixo
        void DesenharCombo(float l, float a)
        {
            var numero = TemaVisual.EstiloNumero;
            var r = new Rect(l - 330, a * 0.32f, 300, 70);
            float largura = TemaVisual.LarguraDoTexto(combo.ToString(), numero, 64);
            TemaVisual.TextoComContorno(new Rect(r.xMax - largura - 150, r.y, largura + 10, 70), combo.ToString(), numero, Color.white, TextAnchor.MiddleLeft, 64);
            TemaVisual.TextoComContorno(new Rect(r.xMax - 140, r.y + 14, 150, 50), "golpes\nseguidos", numero, TemaVisual.OuroClaro, TextAnchor.MiddleLeft, 18);

            int furia = RegrasDePersonagem.AcumulosDeFuria(atributos.Classe, combo);
            if (furia > 0)
                TemaVisual.TextoComContorno(new Rect(r.x, r.yMax, r.width, 28), $"{atributos.Classe.NomeDaPassiva} x{furia}",
                    TemaVisual.EstiloTitulo, TemaVisual.Laranja, TextAnchor.MiddleRight, 20);
        }

        // Orbe da ultimate à esquerda, orbe do ponto fraco à direita e a placa do follower no meio
        void DesenharBarraInferior(float l, float a)
        {
            float tamanho = Mathf.Clamp(a * 0.14f, 80, 120);
            float y = a - tamanho - 52;
            bool pronta = ultimate >= 100;
            float pulso = pronta ? 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 8) : 1f;
            TemaVisual.Orbe(new Rect(44, y, tamanho, tamanho), ultimate / 100f, TemaVisual.Laranja * pulso,
                pronta ? "ULTIMATE [Espaço]" : $"Ultimate {ultimate:0}%");

            // barra de atalhos entre os orbes: ultimate, ponto fraco e follower em slots
            var placa = new Rect(tamanho + 90, a - 92, Mathf.Min(l - 2 * tamanho - 300, 560), 76);
            if (placa.width > 220)
            {
                var dentro = TemaVisual.Painel(placa, null);
                float lado = dentro.height + 12;
                var slot = new Rect(dentro.x - 4, dentro.y - 6, lado, lado);
                TemaVisual.Slot(slot, "U", pronta ? TemaVisual.Laranja * pulso : TemaVisual.CorDoTexto * 0.6f);
                slot.x += lado + 6;
                TemaVisual.Slot(slot, "!", pontoFracoRestante > 0 ? TemaVisual.Ciano : TemaVisual.CorDoTexto * 0.6f);
                slot.x += lado + 6;
                var follower = atributos.Follower;
                TemaVisual.Slot(slot, follower != null ? TemaVisual.Inicial(follower.Nome) : "", TemaVisual.Ciano);
                if (follower != null)
                    TemaVisual.TextoComContorno(new Rect(slot.xMax + 10, dentro.y, dentro.xMax - slot.xMax - 10, dentro.height),
                        $"{follower.Nome} luta com você", TemaVisual.EstiloTitulo, TemaVisual.Ciano, TextAnchor.MiddleLeft, 16);
            }

            float fraco = pontoFracoRestante > 0
                ? pontoFracoRestante / Mathf.Max(0.01f, duracaoDoPontoFraco)
                : timerPontoFraco / Mathf.Max(0.01f, pontoFracoACada);
            TemaVisual.Orbe(new Rect(l - tamanho - 190, y, tamanho, tamanho), fraco,
                pontoFracoRestante > 0 ? TemaVisual.Ciano : TemaVisual.Ciano * 0.55f, "Ponto fraco");
        }

        void DesenharPopups()
        {
            var numero = TemaVisual.EstiloNumero;
            for (int i = popups.Count - 1; i >= 0; i--)
            {
                var p = popups[i];
                p.t += Time.deltaTime;
                if (p.t > 0.9f) { popups.RemoveAt(i); continue; }
                popups[i] = p;
                // cresce rápido no começo e some no fim
                float escala = p.t < 0.1f ? Mathf.Lerp(1.4f, 1f, p.t / 0.1f) : 1f;
                var cor = new Color(p.cor.r, p.cor.g, p.cor.b, 1 - p.t / 0.9f);
                TemaVisual.TextoComContorno(new Rect(p.pos.x, p.pos.y - p.t * 90, 500, 80), p.texto, numero, cor,
                    TextAnchor.MiddleLeft, Mathf.RoundToInt(p.tamanho * escala));
            }
        }
    }
}
