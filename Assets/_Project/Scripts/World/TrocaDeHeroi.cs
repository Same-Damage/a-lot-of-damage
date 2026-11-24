using ALotOfDamage.Core;
using ALotOfDamage.UI;
using ALotOfDamage.Visual;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.World
{
    // Botão (e tecla H / Select no controle) para alternar o herói do mapa; a escolha fica salva no progresso
    public class TrocaDeHeroi : MonoBehaviour
    {
        public HeroisDaCena herois;
        public MovimentoDoJogador movimento;
        public AtaqueNoMapa ataque;

        EscolhaDeHeroi escolha;
        Text rotulo;

        [Inject]
        public void Construir(EscolhaDeHeroi escolha) => this.escolha = escolha;

        void Start()
        {
            var botao = KitDeInterface.Botao("TrocarHeroi", KitDeInterface.Canvas(), "", Trocar, 22);
            // logo abaixo do painel do HUD
            KitDeInterface.Ancorar((RectTransform)botao.transform, new Vector2(0, 1), new Vector2(20, -284), new Vector2(300, 56));
            rotulo = botao.GetComponentInChildren<Text>();
            Aplicar(escolha.Atual(herois.Nomes));
        }

        void Update()
        {
            var k = Keyboard.current;
            bool pediu = (k != null && k.hKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame);
            if (pediu) Trocar();
        }

        void Trocar() => Aplicar(escolha.Trocar(herois.Nomes));

        void Aplicar(string nome)
        {
            var visual = herois.Mostrar(nome);
            movimento.boneco = visual;
            ataque.boneco = visual;
            rotulo.text = $"Herói: {visual.name} (H)";
        }
    }
}
