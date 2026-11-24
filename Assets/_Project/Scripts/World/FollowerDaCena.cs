using ALotOfDamage.Core;
using ALotOfDamage.Visual;
using UnityEngine;
using VContainer;

namespace ALotOfDamage.World
{
    // Mostra o follower ativo (CalcularAtributos.Follower) com o modelo da família dele e troca quando o jogador escolhe outro.
    // No mapa segue o herói (SeguidorNoMapa): quem voa fica no alto, quem anda fica no chão; na batalha fica parado ao lado.
    public class FollowerDaCena : MonoBehaviour
    {
        public FollowerLowPoly modelo;
        public SeguidorNoMapa seguidor;          // só no mapa
        public SombraDeContato sombra;
        public float alturaVoando = 1.8f;        // acima do centro do herói (que fica 1 acima do chão)
        public float intervalo = 0.5f;
        [Tooltip("Na batalha: para onde o follower olha")]
        public Vector3 olharPara = Vector3.zero;

        CalcularAtributos atributos;
        float proximaConsulta;
        Vector3 ultimaPosicao, chaoNaBatalha;
        Quaternion olhando = Quaternion.identity;

        [Inject]
        public void Construir(CalcularAtributos atributos) => this.atributos = atributos;

        void Start()
        {
            ultimaPosicao = transform.position;
            chaoNaBatalha = transform.position;
            if (olharPara.sqrMagnitude > 0) olhando = Quaternion.LookRotation(new Vector3(olharPara.x, 0, olharPara.z));
        }

        void Update()
        {
            if (atributos != null && Time.unscaledTime >= proximaConsulta)
            {
                proximaConsulta = Time.unscaledTime + intervalo;
                var f = atributos.Executar(null).Follower;
                if (f != null && f.Id != modelo.IdMontado) Trocar(f);
            }

            // anda e vira para onde está indo
            var delta = transform.position - ultimaPosicao;
            ultimaPosicao = transform.position;
            delta.y = 0;
            modelo.Andando = delta.sqrMagnitude > 0.0004f;
            if (modelo.Andando) olhando = Quaternion.LookRotation(delta);
            modelo.transform.localRotation = Quaternion.Slerp(modelo.transform.localRotation, olhando, 8f * Time.deltaTime);
        }

        void Trocar(DefinicaoDeFollower f)
        {
            modelo.Montar(f);
            bool voa = FollowerLowPoly.Voa(f.Familia);
            if (seguidor)
            {
                // a raiz acompanha o centro do herói; no chão fica 1 abaixo dele
                seguidor.altura = voa ? alturaVoando : -1f;
                seguidor.amplitudeDoVoo = voa ? 0.15f : 0f;
                if (sombra) sombra.transform.localPosition = new Vector3(0, -(seguidor.altura + 1f) + 0.02f, 0);
            }
            else
            {
                transform.position = chaoNaBatalha + (voa ? Vector3.up * (alturaVoando + 0.6f) : Vector3.zero);
                if (sombra) sombra.transform.localPosition = new Vector3(0, voa ? -(alturaVoando + 0.6f) + 0.02f : 0.02f, 0);
            }
        }
    }
}
