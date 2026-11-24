using System.Text;
using ALotOfDamage.Core;
using UnityEngine;
using VContainer;

namespace ALotOfDamage.UI
{
    // Ao abrir o mapa, mostra o que o herói ganhou lutando sozinho enquanto o jogador estava fora (até 12 h) e entrega com "Coletar".
    // Depois disso, marca a presença de tempos em tempos e ao pausar ou fechar: o tempo fora conta a partir da última marca.
    public class JanelaDaRecompensaAfk : MonoBehaviour
    {
        [Tooltip("No mapa a janela aparece; na batalha o componente só marca a presença")]
        public bool mostrarJanela = true;
        public float intervaloDaPresenca = 30f;

        ConsultarRecompensaAfk consultar;
        ColetarRecompensaAfk coletar;
        RegistrarPresenca presenca;
        ICatalogo catalogo;

        RectTransform janela;
        bool aguardandoColeta;
        float proximaPresenca;

        [Inject]
        public void Construir(ConsultarRecompensaAfk consultar, ColetarRecompensaAfk coletar, RegistrarPresenca presenca, ICatalogo catalogo)
        {
            this.consultar = consultar;
            this.coletar = coletar;
            this.presenca = presenca;
            this.catalogo = catalogo;
        }

        void Start()
        {
            var r = consultar.Executar();
            if (mostrarJanela && r.TemRecompensa)
            {
                aguardandoColeta = true;
                Montar(r);
            }
            else coletar.Executar();   // sem nada a mostrar, só recomeça a contar daqui
        }

        void Montar(Core.RecompensaAfk r)
        {
            var canvas = KitDeInterface.Canvas();
            janela = (RectTransform)KitDeInterface.Painel("Recompensa AFK", canvas).transform;
            KitDeInterface.Ancorar(janela, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 460));
            janela.SetAsLastSibling();

            var titulo = KitDeInterface.Texto("Título", janela, 38, KitDeInterface.Dourado, TextAnchor.MiddleCenter, true);
            KitDeInterface.Ancorar((RectTransform)titulo.transform, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(700, 50));
            titulo.text = "Bem-vindo de volta!";

            var texto = KitDeInterface.Texto("Recompensa", janela, 26, KitDeInterface.TextoClaro, TextAnchor.UpperCenter);
            KitDeInterface.Ancorar((RectTransform)texto.transform, new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(680, 240));
            texto.text = Descrever(r);

            var botao = KitDeInterface.Botao("Coletar", janela, "Coletar", Coletar, 28);
            KitDeInterface.Ancorar((RectTransform)botao.transform, new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(260, 70));
        }

        string Descrever(Core.RecompensaAfk r)
        {
            var sb = new StringBuilder();
            int minutos = (int)(r.Segundos / 60);
            sb.Append($"Seu herói lutou por {minutos / 60} h {minutos % 60:00} min\n");
            sb.Append($"e venceu {FormatadorDeNumeros.Formatar(r.Vitorias)} batalhas.\n\n");
            sb.Append($"<color=#FFCC52>+{FormatadorDeNumeros.Formatar(r.Ouro)} ouro</color>");
            foreach (var m in r.Materiais) sb.Append($"   +{m.Valor} {catalogo.NomeDoMaterial(m.Id)}");
            if (r.Itens > 0) sb.Append($"\n+{r.Itens} {(r.Itens == 1 ? "item" : "itens")}");
            return sb.ToString();
        }

        void Coletar()
        {
            var r = coletar.Executar();
            aguardandoColeta = false;
            if (janela) Destroy(janela.gameObject);
            Faixa.DoCanvas(KitDeInterface.Canvas()).Mostrar($"+{FormatadorDeNumeros.Formatar(r.Ouro)} ouro", 1.5f);
            proximaPresenca = Time.unscaledTime + intervaloDaPresenca;
        }

        void Update()
        {
            if (aguardandoColeta || Time.unscaledTime < proximaPresenca) return;
            proximaPresenca = Time.unscaledTime + intervaloDaPresenca;
            presenca.Executar();
        }

        // Celular manda o app para o fundo sem fechar: o tempo fora começa aqui
        void OnApplicationPause(bool pausado)
        {
            if (pausado && !aguardandoColeta && presenca != null) presenca.Executar();
        }

        void OnApplicationQuit()
        {
            if (!aguardandoColeta && presenca != null) presenca.Executar();
        }
    }
}
