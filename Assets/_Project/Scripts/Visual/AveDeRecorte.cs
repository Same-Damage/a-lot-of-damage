using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Ave pintada em partes (corpo, asas, cabeça, cauda), batendo as asas com RegrasDeAnimacao.Voo
    public class AveDeRecorte : MonoBehaviour
    {
        public Transform corpo, asaFrente, asaTras, cabeca, cauda;
        public bool desenhoOlhaParaDireita = true;
        public bool olhandoParaDireita = true;
        public float velocidade = 1f;

        Vector3 posicaoDoCorpo;
        float fase;

        public bool OlhandoParaDireita
        {
            get => olhandoParaDireita;
            set { olhandoParaDireita = value; AplicarDirecao(); }
        }

        void Start()
        {
            if (corpo) posicaoDoCorpo = corpo.localPosition;
            fase = Random.value * RegrasDeAnimacao.CicloDaBatida;
            AplicarDirecao();
        }

        void AplicarDirecao()
        {
            if (!corpo) return;
            var s = corpo.localScale;
            s.x = Mathf.Abs(s.x) * RegrasDeAnimacao.EscalaHorizontal(desenhoOlhaParaDireita, olhandoParaDireita);
            corpo.localScale = s;
        }

        void Update()
        {
            var p = RegrasDeAnimacao.Voo(Time.unscaledTime * velocidade + fase);
            Girar(asaFrente, p.AsaFrente);
            Girar(asaTras, p.AsaTras);
            Girar(cabeca, p.Cabeca);
            Girar(cauda, p.Cauda);
            if (corpo) corpo.localPosition = posicaoDoCorpo + new Vector3(0, p.Elevacao, 0);
        }

        static void Girar(Transform osso, float angulo)
        {
            if (osso) osso.localRotation = Quaternion.Euler(0, 0, angulo);
        }
    }
}
