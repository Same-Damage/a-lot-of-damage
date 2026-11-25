using ALotOfDamage.Core;
using UnityEngine;
using VContainer;

namespace ALotOfDamage.UI
{
    // Ícone do menu do mapa que leva à cidade, liberado pela história
    public class PortaoDaCidade : MonoBehaviour
    {
        const int Ordem = 13;

        IPortaoDaCidade portao;
        ConsultarLiberacao liberacao;
        BotaoDeIcone botao;
        float proxima;

        [Inject]
        public void Construir(IPortaoDaCidade portao, ConsultarLiberacao liberacao)
        {
            this.portao = portao;
            this.liberacao = liberacao;
        }

        void Start() => botao = MenuDoMapa.DoCanvas(KitDeInterface.Canvas()).Adicionar(Ordem, IconeDoHud.Cidade, "Cidade", null, portao.IrParaCidade);

        void Update()
        {
            if (Time.unscaledTime < proxima) return;
            proxima = Time.unscaledTime + 1f;
            bool liberada = liberacao.Executar(SistemaDoJogo.Cidade);
            if (botao.Rect.gameObject.activeSelf != liberada) botao.Rect.gameObject.SetActive(liberada);
        }
    }
}
