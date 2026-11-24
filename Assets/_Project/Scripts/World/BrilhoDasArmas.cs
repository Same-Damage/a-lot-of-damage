using ALotOfDamage.Core;
using ALotOfDamage.Visual;
using UnityEngine;
using VContainer;

namespace ALotOfDamage.World
{
    // Mostra nos heróis a arma equipada (ConsultarVisualDoHeroi): sem item a mão fica vazia; com item, cor da raridade e brilho do refino.
    // Confere de tempos em tempos, porque o equipamento muda pelo inventário e pela forja.
    public class BrilhoDasArmas : MonoBehaviour
    {
        public float intervalo = 0.5f;

        ConsultarVisualDoHeroi consulta;
        PecaDeEquipamento[] pecas;
        float proximaConsulta;
        VisualDoHeroi atual;

        // Muda a cada troca do visual: quem copia o herói (a vitrine do inventário) sabe que precisa copiar de novo
        public int Versao { get; private set; }

        [Inject]
        public void Construir(ConsultarVisualDoHeroi consulta) => this.consulta = consulta;

        void Start() => pecas = GetComponentsInChildren<PecaDeEquipamento>(true);

        void Update()
        {
            if (consulta == null || Time.unscaledTime < proximaConsulta) return;
            proximaConsulta = Time.unscaledTime + intervalo;
            var v = consulta.Executar();
            if (Versao > 0 && v.Equals(atual)) return;
            atual = v;
            Versao++;
            foreach (var p in pecas) Aplicar(p, v);
        }

        // Também usado nas cópias do herói (a cópia não leva o MaterialPropertyBlock)
        public void AplicarEm(GameObject raiz)
        {
            foreach (var p in raiz.GetComponentsInChildren<PecaDeEquipamento>(true)) Aplicar(p, atual);
        }

        static void Aplicar(PecaDeEquipamento p, VisualDoHeroi v)
        {
            if (!p.ehArma) return;
            p.gameObject.SetActive(v.TemArma);
            if (!v.TemArma) return;
            ColorUtility.TryParseHtmlString(RegrasDosItens.CorDaRaridade(v.RaridadeDaArma), out var cor);
            p.Mostrar(cor, v.Brilho);
        }
    }
}
