using System;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Liga o SincronizarProgresso ao jogo: abre a conta ao iniciar, envia de tempos em tempos e ao pausar,
    // e no mapa pergunta qual progresso vale quando os dois lados mudaram
    public class SincroniaDaNuvem : MonoBehaviour
    {
        [Tooltip("No mapa mostra as telas; na batalha só envia")]
        public bool mostrarTelas = true;
        public float intervaloDoEnvio = 300f;
        public float esperaMaxima = 4f;     // depois disso o jogo segue offline

        SincronizarProgresso sincronia;
        ICatalogo catalogo;

        RectTransform espera, conflito;
        Text avisoDoConflito;
        float proximoEnvio, fimDaEspera;
        int trocas;
        bool escolhendo;

        // Telas por cima de tudo: o teclado do jogo não pode abrir janelas por baixo
        public static bool Bloqueando { get; private set; }

        [Inject]
        public void Construir(SincronizarProgresso sincronia, ICatalogo catalogo)
        {
            this.sincronia = sincronia;
            this.catalogo = catalogo;
        }

        void Start()
        {
            trocas = sincronia.Trocas;
            proximoEnvio = Time.unscaledTime + intervaloDoEnvio;
            if (sincronia.Situacao != SituacaoDaNuvem.NaoIniciada) return;
            Seguir(sincronia.Abrir());
            if (!mostrarTelas || sincronia.Situacao != SituacaoDaNuvem.Conectando) return;
            fimDaEspera = Time.unscaledTime + esperaMaxima;
            espera = Tela("Sincronizando", new Vector2(520, 120));
            Centralizado(espera, "Sincronizando...", 30, KitDeInterface.Dourado, -34, 50);
        }

        void OnDestroy() => Bloqueando = false;

        // Um erro fora do esperado aparece no log em vez de sumir na Task
        static async void Seguir(System.Threading.Tasks.Task t)
        {
            try { await t; }
            catch (Exception e) { Debug.LogException(e); }
        }

        void Update()
        {
            if (sincronia == null) return;
            if (espera && (sincronia.Situacao != SituacaoDaNuvem.Conectando || Time.unscaledTime > fimDaEspera))
            {
                Destroy(espera.gameObject);
                espera = null;
            }

            // o progresso é outro: tudo que a cena já leu ficou velho
            if (mostrarTelas && sincronia.Trocas != trocas)
            {
                trocas = sincronia.Trocas;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }

            if (mostrarTelas && sincronia.Situacao == SituacaoDaNuvem.Conflito && !conflito && !escolhendo) MontarConflito();
            if (conflito && !escolhendo && sincronia.Situacao != SituacaoDaNuvem.Conflito)
            {
                Destroy(conflito.gameObject);
                conflito = null;
            }
            Bloqueando = espera || conflito;

            if (Time.unscaledTime < proximoEnvio) return;
            proximoEnvio = Time.unscaledTime + intervaloDoEnvio;
            Seguir(sincronia.EnviarSeMudou());
        }

        void OnApplicationPause(bool pausado)
        {
            if (pausado && sincronia != null) Seguir(sincronia.EnviarSeMudou());
        }

        static RectTransform Tela(string nome, Vector2 tamanho)
        {
            var fundo = KitDeInterface.Painel(nome, KitDeInterface.Canvas(), new Color(0, 0, 0, 0.6f), false);
            var raiz = (RectTransform)fundo.transform;
            KitDeInterface.Esticar(raiz);
            raiz.SetAsLastSibling();
            var painel = (RectTransform)KitDeInterface.Painel("Painel", raiz).transform;
            KitDeInterface.Ancorar(painel, new Vector2(0.5f, 0.5f), Vector2.zero, tamanho);
            return raiz;
        }

        static RectTransform PainelDa(RectTransform tela) => (RectTransform)tela.Find("Painel");

        static Text Centralizado(RectTransform tela, string texto, int tamanho, Color cor, float y, float altura)
        {
            var t = KitDeInterface.Texto("Texto", PainelDa(tela), tamanho, cor, TextAnchor.UpperCenter);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            KitDeInterface.Ancorar((RectTransform)t.transform, new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(PainelDa(tela).sizeDelta.x - 100, altura));
            t.text = texto;
            return t;
        }

        void MontarConflito()
        {
            var nuvem = sincronia.ProgressoEmConflito;
            var save = sincronia.EmConflito;
            conflito = Tela("Conflito da nuvem", new Vector2(1120, 620));
            var painel = PainelDa(conflito);
            Centralizado(conflito, "Qual progresso vale?", 38, KitDeInterface.Dourado, -30, 50);
            Centralizado(conflito, "Você jogou em dois aparelhos. Escolha um: o outro fica guardado neste aparelho como cópia.", 24, KitDeInterface.TextoClaro, -92, 70);
            Lado(painel, -250, "Este aparelho", sincronia.ProgressoLocal, "agora", () => Escolher(false), true);
            string quando = new DateTime(Math.Max(0, save.SalvoEmUtc), DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            Lado(painel, 250, "Nuvem", nuvem, $"{quando}\n{save.Dispositivo}", () => Escolher(true), nuvem != null);
            avisoDoConflito = Centralizado(conflito, "", 20, KitDeInterface.Vermelho, -560, 30);
        }

        void Lado(RectTransform painel, float x, string titulo, DadosDoProgresso d, string quando, Action escolher, bool pode)
        {
            var caixa = (RectTransform)KitDeInterface.Painel(titulo, painel, new Color(0, 0, 0, 0.35f), false).transform;
            KitDeInterface.Ancorar(caixa, new Vector2(0.5f, 1), new Vector2(x, -180), new Vector2(460, 360));
            var t = KitDeInterface.Texto("Titulo", caixa, 30, KitDeInterface.Dourado, TextAnchor.UpperCenter, true);
            KitDeInterface.Ancorar((RectTransform)t.transform, new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(420, 40));
            t.text = titulo;
            var texto = KitDeInterface.Texto("Resumo", caixa, 24, KitDeInterface.TextoClaro, TextAnchor.UpperCenter);
            KitDeInterface.Ancorar((RectTransform)texto.transform, new Vector2(0.5f, 1), new Vector2(0, -72), new Vector2(420, 190));
            texto.text = d == null ? "Não foi possível ler."
                : $"Estágio {d.Estagio} (maior {d.EstagioMaximo})\nPoder {FormatadorDeNumeros.Formatar(CalcularPoder.De(d, catalogo))}\nOuro {FormatadorDeNumeros.Formatar(d.Ouro)}\n{quando}";
            if (!pode) return;
            var b = KitDeInterface.Botao("Usar", caixa, "Usar este", escolher, 26);
            KitDeInterface.Ancorar((RectTransform)b.transform, new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(280, 64));
        }

        async void Escolher(bool usarNuvem)
        {
            if (escolhendo) return;
            escolhendo = true;
            try { await sincronia.ResolverConflito(usarNuvem); }
            catch (Exception e) { Debug.LogException(e); }
            if (!this) return;
            escolhendo = false;
            if (sincronia.Situacao == SituacaoDaNuvem.Conflito)
            {
                // a nuvem pode ter mudado de novo: mostra os números atuais
                if (conflito) Destroy(conflito.gameObject);
                conflito = null;
                MontarConflito();
                avisoDoConflito.text = string.IsNullOrEmpty(sincronia.Motivo) ? "Não deu certo, tente de novo." : $"Não deu certo: {sincronia.Motivo}";
                return;
            }
            if (conflito) Destroy(conflito.gameObject);
            conflito = null;
        }
    }
}
