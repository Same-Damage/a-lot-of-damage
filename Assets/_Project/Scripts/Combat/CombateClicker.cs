using System.Collections.Generic;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace ALotOfDamage.Combat
{
    // Protótipo do combate clicker: auto-ataque + click com combo + ponto fraco + ultimate.
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
        float ultimoClick, timerAutomatico, timerPontoFraco, pontoFracoRestante, ultimate, tremor;
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

            if (combo > 0 && agora - ultimoClick > tempoDoCombo) combo = 0;

            timerPontoFraco += Time.deltaTime;
            if (pontoFracoRestante > 0) pontoFracoRestante -= Time.deltaTime;
            else if (timerPontoFraco >= pontoFracoACada) { timerPontoFraco = 0; pontoFracoRestante = duracaoDoPontoFraco; }

            var ponteiro = Pointer.current;
            if (ponteiro != null && ponteiro.press.wasPressedThisFrame) AoClicar(agora);

            var teclado = Keyboard.current;
            if (teclado != null && teclado.spaceKey.wasPressedThisFrame && ultimate >= 100) DispararUltimate();

            if (tremor > 0) tremor -= Time.deltaTime;
            if (visualDoInimigo)
                visualDoInimigo.localPosition = tremor > 0 ? (Vector3)Random.insideUnitCircle * 0.08f : Vector3.zero;
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
            double dano = RegrasDoClick.Dano(Parametros(), combo, critico, pontoFraco);

            Color cor = new Color(1f, 0.85f, 0.3f);
            int tamanho = 26;
            if (pontoFraco) { cor = new Color(0.4f, 1f, 1f); tamanho = 40; ultimate = Mathf.Min(100, ultimate + 10); }
            if (critico) { cor = new Color(1f, 0.3f, 0.3f); tamanho += 10; }

            Acertar(dano, true, cor, tamanho);
        }

        void DispararUltimate()
        {
            ultimate = 0;
            Acertar(DanoDoClick * multiplicadorDaUltimate * (1 + combo * bonusDeComboPorHit), true, new Color(0.8f, 0.4f, 1f), 56);
        }

        void Acertar(double dano, bool doClick, Color cor, int tamanho)
        {
            if (voltarEm > 0) return;
            vidaDoInimigo -= dano;
            if (doClick) tremor = 0.08f;

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
            var grande = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };

            if (encontro.Ativo)
            {
                GUI.Label(new Rect(20, 20, 600, 30), $"{nomeDoInimigo}   Ouro {FormatadorDeNumeros.Formatar(Ouro)}", grande);
                if (voltarEm < 0 && GUI.Button(new Rect(Screen.width - 140, 20, 120, 40), "Fugir (Esc)"))
                    encontro.Encerrar(false);
            }
            else
            {
                GUI.Label(new Rect(20, 20, 600, 30), $"Estágio {estagio}   Ouro {FormatadorDeNumeros.Formatar(Ouro)}", grande);
            }

            if (Time.time < faixaAte || voltarEm > 0)
                GUI.Label(new Rect(Screen.width * 0.5f - 250, Screen.height * 0.2f, 500, 130), faixa,
                    new GUIStyle(grande) { fontSize = 32, wordWrap = true, alignment = TextAnchor.MiddleCenter });

            float largura = Screen.width * 0.5f;
            Rect barra = new Rect(Screen.width * 0.25f, 70, largura, 24);
            GUI.Box(barra, "");
            GUI.color = new Color(0.85f, 0.2f, 0.25f);
            GUI.Box(new Rect(barra.x, barra.y, largura * (float)System.Math.Max(0, vidaDoInimigo / vidaMaximaDoInimigo), barra.height), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(barra.x + 8, barra.y, largura, 24),
                $"{FormatadorDeNumeros.Formatar(System.Math.Max(0, vidaDoInimigo))} / {FormatadorDeNumeros.Formatar(vidaMaximaDoInimigo)}");

            GUI.Label(new Rect(20, Screen.height - 90, 600, 30), combo > 1 ? $"Combo x{combo}" : "", grande);
            GUI.Label(new Rect(20, Screen.height - 55, 600, 30), ultimate >= 100 ? "ULTIMATE PRONTA! [Espaço]" : $"Ultimate {ultimate:0}%", grande);
            if (pontoFracoRestante > 0)
                GUI.Label(new Rect(Screen.width * 0.5f - 120, 110, 300, 40), "PONTO FRACO! CLIQUE!", new GUIStyle(grande) { fontSize = 28 });

            for (int i = popups.Count - 1; i >= 0; i--)
            {
                var p = popups[i];
                p.t += Time.deltaTime;
                if (p.t > 0.9f) { popups.RemoveAt(i); continue; }
                popups[i] = p;
                var estilo = new GUIStyle(grande) { fontSize = p.tamanho, normal = { textColor = new Color(p.cor.r, p.cor.g, p.cor.b, 1 - p.t / 0.9f) } };
                GUI.Label(new Rect(p.pos.x, p.pos.y - p.t * 90, 400, 60), p.texto, estilo);
            }
        }
    }
}
