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
            var botao = new BotaoDeIcone("TrocarHeroi", KitDeInterface.Canvas(), IconeDoHud.Heroi, "", 84, Trocar);
            // no canto, ao lado da placa do Poder
            KitDeInterface.Ancorar(botao.Rect, new Vector2(0, 1), new Vector2(24, -24), Vector2.one * 84);
            rotulo = botao.Rotulo;
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
            rotulo.text = visual.name;
        }
    }
}
