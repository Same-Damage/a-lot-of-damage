using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ALotOfDamage.Visual;
using VContainer;

namespace ALotOfDamage.Combat
{
    // Combate clicker na arena: auto-ataque + click com combo + ponto fraco + ultimate,
    // com a passiva e a ultimate da classe e o follower lutando junto.
    // Lê input, chama as regras e os casos de uso, e entrega o resultado ao HUD e ao feedback.
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

        public Transform visualDoInimigo;
        public MonstroLowPoly modeloDoInimigo;
        public HeroisDaCena herois;

        static readonly Color CorDoClick = new Color(1f, 0.85f, 0.3f);
        static readonly Color CorDoPontoFraco = new Color(0.4f, 1f, 1f);
        static readonly Color CorDoCritico = new Color(1f, 0.3f, 0.3f);
        static readonly Color CorDaUltimate = new Color(0.8f, 0.4f, 1f);
        static readonly Color CorDoFollower = new Color(0.55f, 0.8f, 1f);
        static readonly Color CorDoPreventivo = new Color(0.4f, 1f, 0.5f);

        FluxoDeEncontro encontro;
        IRepositorioDeProgresso repositorio;
        CalcularAtributos calcularAtributos;
        CalcularInimigo calcularInimigo;
        RegistrarVitoria registrarVitoria;

        HudDaBatalha hud;
        FeedbackDoGolpe feedback;
        LimitadorDeCliques limitador;

        // O progresso não muda durante a luta, então os atributos são calculados uma vez
        AtributosDeCombate atributos;
        InimigoDaBatalha inimigo;
        double vidaDoInimigo;
        int combo;
        float ultimoClick, timerAutomatico, timerFollower, timerPontoFraco, pontoFracoRestante, ultimate;
        float cliquesTravadosAte, automaticoComecaEm, voltarEm = -1;
        double ouroMostrado = -1;

        EscolhaDeHeroi escolhaDeHeroi;
        PersonagemVisual heroi;

        bool EmEncontro => encontro.Ativo;
        string Tipo => EmEncontro ? encontro.TipoDoInimigo : null;
        bool Chefe => EmEncontro && encontro.Chefe;

        [Inject]
        public void Construir(FluxoDeEncontro encontro, IRepositorioDeProgresso repositorio, CalcularAtributos calcularAtributos,
            CalcularInimigo calcularInimigo, RegistrarVitoria registrarVitoria, EscolhaDeHeroi escolhaDeHeroi)
        {
            this.escolhaDeHeroi = escolhaDeHeroi;
            this.encontro = encontro;
            this.repositorio = repositorio;
            this.calcularAtributos = calcularAtributos;
            this.calcularInimigo = calcularInimigo;
            this.registrarVitoria = registrarVitoria;
        }

        void Start()
        {
            if (herois) heroi = herois.Mostrar(escolhaDeHeroi.Atual(herois.Nomes));
            limitador = new LimitadorDeCliques(cliquesPorSegundoMaximo);
            atributos = calcularAtributos.Executar(Tipo);
            hud = new HudDaBatalha(Fugir);
            feedback = gameObject.AddComponent<FeedbackDoGolpe>();
            feedback.alvo = visualDoInimigo;
            hud.PodeFugir(EmEncontro);
            NovoInimigo();

            if (!EmEncontro) return;
            switch (encontro.Vantagem)
            {
                case Vantagem.Preventivo:
                    hud.Faixa.Mostrar("ATAQUE PREVENTIVO!");
                    ultimate = 100;
                    Acertar(atributos.DanoDoClick * 20, TipoDeGolpe.Critico, CorDoPreventivo, 64);
                    break;
                case Vantagem.Emboscada:
                    hud.Faixa.Mostrar("EMBOSCADA!");
                    cliquesTravadosAte = Time.time + 1.5f;
                    automaticoComecaEm = Time.time + 2.5f;
                    break;
            }
        }

        void NovoInimigo()
        {
            inimigo = calcularInimigo.Executar(Tipo, Chefe);
            vidaDoInimigo = inimigo.Vida;
            if (modeloDoInimigo)
            {
                if (!ColorUtility.TryParseHtmlString(inimigo.Cor, out var cor)) cor = Color.magenta;
                if (inimigo.Chefe) cor = Color.Lerp(cor, new Color(0.7f, 0.1f, 0.1f), 0.5f);
                modeloDoInimigo.Montar(inimigo.Forma, cor);
            }
            hud.Inimigo(inimigo.Nome, atributos.Follower?.Nome);
            AtualizarVida();
        }

        void Fugir()
        {
            if (EmEncontro && voltarEm < 0) encontro.Encerrar(false);
        }

        void Update()
        {
            float agora = Time.time;

            if (voltarEm > 0)
            {
                if (Time.unscaledTime >= voltarEm) encontro.Encerrar(true);
                return;
            }

            var teclado = Keyboard.current;
            if (teclado != null && teclado.escapeKey.wasPressedThisFrame) { Fugir(); return; }

            timerAutomatico += Time.deltaTime;
            if (agora >= automaticoComecaEm && timerAutomatico >= intervaloAutomatico)
            {
                timerAutomatico = 0;
                Acertar(atributos.DanoAutomatico, TipoDeGolpe.Automatico, Color.white, 30);
            }

            var follower = atributos.Follower;
            if (follower != null && agora >= automaticoComecaEm)
            {
                timerFollower += Time.deltaTime;
                if (timerFollower >= follower.IntervaloDeAtaque)
                {
                    timerFollower = 0;
                    Acertar(atributos.DanoDoFollower, TipoDeGolpe.Follower, CorDoFollower, 28);
                }
            }

            if (combo > 0 && agora - ultimoClick > tempoDoCombo) combo = 0;

            timerPontoFraco += Time.deltaTime;
            if (pontoFracoRestante > 0) pontoFracoRestante -= Time.deltaTime;
            else if (timerPontoFraco >= pontoFracoACada)
            {
                timerPontoFraco = 0;
                pontoFracoRestante = RegrasDePersonagem.DuracaoDoPontoFraco(atributos.BonusNoPontoFraco, duracaoDoPontoFraco);
            }

            var ponteiro = Pointer.current;
            if (ponteiro != null && ponteiro.press.wasPressedThisFrame && !SobreAInterface()) AoClicar(agora);
            if (teclado != null && teclado.spaceKey.wasPressedThisFrame && ultimate >= 100) DispararUltimate();

            hud.Combo(combo, RegrasDePersonagem.AcumulosDeFuria(atributos.Classe, combo), atributos.Classe?.NomeDaPassiva);
            hud.Ultimate(ultimate);
            hud.PontoFraco(pontoFracoRestante > 0);
            AtualizarOuro();
        }

        static bool SobreAInterface() => EventSystem.current && EventSystem.current.IsPointerOverGameObject();

        ParametrosDoClick Parametros() => new ParametrosDoClick
        {
            DanoBase = atributos.DanoDoClick,
            BonusDeComboPorHit = bonusDeComboPorHit,
            MultiplicadorCritico = multiplicadorCritico,
            MultiplicadorPontoFraco = multiplicadorPontoFraco,
        };

        void AoClicar(float agora)
        {
            if (agora < cliquesTravadosAte) return;
            if (!limitador.Registrar(Time.unscaledTime)) return;

            combo++;
            ultimoClick = agora;
            ultimate = Mathf.Min(100, ultimate + cargaDaUltimatePorClick);

            bool critico = Random.value < atributos.ChanceCritica;
            bool pontoFraco = pontoFracoRestante > 0;
            double dano = RegrasDoClick.Dano(Parametros(), combo, critico, pontoFraco)
                * RegrasDePersonagem.MultiplicadorDeFuria(atributos.Classe, combo);

            var tipo = TipoDeGolpe.Click;
            Color cor = CorDoClick;
            int tamanho = 40;
            if (pontoFraco) { tipo = TipoDeGolpe.PontoFraco; cor = CorDoPontoFraco; tamanho = 56; ultimate = Mathf.Min(100, ultimate + 10); }
            if (critico) { tipo = TipoDeGolpe.Critico; cor = CorDoCritico; tamanho += 14; }

            if (heroi) heroi.Golpear(1.6f);
            Acertar(dano, tipo, cor, tamanho);
        }

        void DispararUltimate()
        {
            ultimate = 0;
            var classe = atributos.Classe;
            double multiplicador = classe?.MultiplicadorDaUltimate ?? multiplicadorDaUltimate;
            if (!string.IsNullOrEmpty(classe?.NomeDaUltimate)) hud.Faixa.Mostrar(classe.NomeDaUltimate.ToUpperInvariant() + "!");
            if (heroi) heroi.Golpear(0.8f);
            Acertar(atributos.DanoDoClick * multiplicador * (1 + combo * bonusDeComboPorHit), TipoDeGolpe.Ultimate, CorDaUltimate, 84);
        }

        void Acertar(double dano, TipoDeGolpe tipo, Color cor, int tamanho)
        {
            if (voltarEm > 0) return;
            vidaDoInimigo -= dano;

            feedback.Golpe(tipo, cor);
            var desvio = new Vector2(Random.Range(-160f, 160f), Random.Range(-40f, 60f));
            hud.Numeros.Mostrar(FormatadorDeNumeros.Formatar(dano), cor, tamanho, desvio);
            AtualizarVida();

            if (vidaDoInimigo > 0) return;

            var v = registrarVitoria.Executar(Tipo, Chefe);
            if (EmEncontro)
            {
                string texto = $"VITÓRIA!  +{FormatadorDeNumeros.Formatar(v.Ouro)} ouro";
                if (v.MaterialGanho > 0) texto += $"  +{v.MaterialGanho} {v.Monstro.NomeDoMaterial}";
                if (v.AtingiuMarcoDoBestiario) texto += $"\nBESTIÁRIO: {v.Monstro.Nome} ×{v.Derrotas}!";
                if (v.AvancouEstagio) texto += $"\nESTÁGIO {v.Estagio}!";
                if (v.ItemGanho != null)
                    texto += $"\nITEM: {v.NomeDoItem} {RegrasDosItens.Nome(v.ItemGanho.Raridade)}" +
                             (v.ItemVendidoPorFaltaDeEspaco ? " (inventário cheio: vendido)" : "");
                hud.Faixa.Fixar(texto);
                voltarEm = Time.unscaledTime + 1.8f;
            }
            else
            {
                // Cena Batalha aberta direto: luta em sequência seguindo a campanha
                if (v.AvancouEstagio) hud.Faixa.Mostrar($"ESTÁGIO {v.Estagio}!");
                NovoInimigo();
            }
        }

        void AtualizarVida() =>
            hud.Vida(vidaDoInimigo, inimigo.Vida,
                $"{FormatadorDeNumeros.Formatar(System.Math.Max(0, vidaDoInimigo))} / {FormatadorDeNumeros.Formatar(inimigo.Vida)}");

        void AtualizarOuro()
        {
            double ouro = repositorio.Carregar().Ouro;
            if (ouro == ouroMostrado) return;
            ouroMostrado = ouro;
            hud.Ouro($"Ouro {FormatadorDeNumeros.Formatar(ouro)}");
        }
    }
}
