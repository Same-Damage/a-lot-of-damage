using ALotOfDamage.Core;
using ALotOfDamage.Visual;
using UnityEngine;
using VContainer;

namespace ALotOfDamage.World
{
    // Acende as armas dos heróis conforme a forja (ConsultarBrilhoDaArma); confere de tempos em tempos, porque a forja muda pela janela
    public class BrilhoDasArmas : MonoBehaviour
    {
        public float intervalo = 0.5f;

        ConsultarBrilhoDaArma consulta;
        PecaDeEquipamento[] pecas;
        float proximaConsulta;

        [Inject]
        public void Construir(ConsultarBrilhoDaArma consulta) => this.consulta = consulta;

        void Start() => pecas = GetComponentsInChildren<PecaDeEquipamento>(true);

        void Update()
        {
            if (consulta == null || Time.unscaledTime < proximaConsulta) return;
            proximaConsulta = Time.unscaledTime + intervalo;
            float brilho = consulta.Executar();
            foreach (var p in pecas) p.Brilho = brilho;
        }
    }
}
