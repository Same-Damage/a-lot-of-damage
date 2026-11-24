using ALotOfDamage.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace ALotOfDamage.Testes.EditMode
{
    public class TemaVisualTestes
    {
        static float Brilho(Color c) => (c.r + c.g + c.b) / 3f;

        [Test]
        public void Painel_tem_moldura_dourada_e_fundo_escuro()
        {
            var t = TemaVisual.TexturaDoPainel;
            Color borda = t.GetPixel(t.width / 2, 2);
            Color centro = t.GetPixel(t.width / 2, t.height / 2);

            Assert.Greater(borda.r, borda.b + 0.2f, "moldura deve puxar para o dourado");
            Assert.Less(Brilho(centro), 0.2f, "fundo do painel deve ser escuro");
            Assert.Greater(centro.a, 0.8f);
        }

        [Test]
        public void Painel_tem_cravo_dourado_no_canto()
        {
            var t = TemaVisual.TexturaDoPainel;
            int c = TemaVisual.CentroDoCravo;
            Color cravo = t.GetPixel(c, c);
            Assert.Greater(cravo.r, cravo.b + 0.25f);
            Assert.Greater(Brilho(cravo), 0.45f);
        }

        [Test]
        public void Miolo_do_painel_tem_textura_de_couro()
        {
            var t = TemaVisual.TexturaDoPainel;
            int m = t.width / 2;
            bool variou = false;
            for (int i = 1; i < 12 && !variou; i++)
                variou = t.GetPixel(m + i, m) != t.GetPixel(m, m);
            Assert.IsTrue(variou, "o miolo não deve ser uma cor chapada");
        }

        [Test]
        public void Botao_pressionado_e_mais_escuro_que_o_normal()
        {
            var normal = TemaVisual.TexturaDoBotao;
            var pressionado = TemaVisual.TexturaDoBotaoPressionado;
            int x = normal.width / 2, y = normal.height / 2;
            Assert.Less(Brilho(pressionado.GetPixel(x, y)), Brilho(normal.GetPixel(x, y)));
        }

        [Test]
        public void Sprites_sao_fatiados_para_os_cantos_nao_esticarem()
        {
            Assert.Greater(TemaVisual.Painel.border.x, TemaVisual.CentroDoCravo);
            Assert.Greater(TemaVisual.Botao.border.x, 0);
            Assert.AreSame(TemaVisual.Painel, TemaVisual.Painel, "o sprite é criado uma vez só");
        }

        [Test]
        public void Placa_do_hud_deixa_o_cenario_aparecer()
        {
            var t = TemaVisual.TexturaDaPlaca;
            Assert.That(t.GetPixel(t.width / 2, t.height / 2).a, Is.InRange(0.4f, 0.75f));
            Assert.Less(t.GetPixel(0, 0).a, 0.05f, "cantos redondos");
            Assert.Greater(TemaVisual.Placa.border.x, 0);
        }

        [Test]
        public void Disco_do_hud_e_redondo_com_aro_dourado()
        {
            var t = TemaVisual.TexturaDoDisco;
            Assert.Less(t.GetPixel(2, 2).a, 0.05f);
            Color aro = t.GetPixel(t.width / 2, 3);
            Assert.Greater(aro.r, aro.b + 0.2f);
        }

        [Test]
        public void Todo_icone_do_hud_tem_desenho()
        {
            foreach (IconeDoHud i in System.Enum.GetValues(typeof(IconeDoHud)))
            {
                var t = IconesDoHud.Do(i).texture;
                float cheio = 0;
                foreach (var p in t.GetPixels32(0)) cheio += p.a / 255f;
                Assert.That(cheio / (t.width * t.height), Is.InRange(0.08f, 0.8f), i.ToString());
            }
        }

        [Test]
        public void Selo_do_botao_de_icone_so_aparece_com_pendencia()
        {
            var raiz = new GameObject("Raiz", typeof(RectTransform));
            try
            {
                var b = new BotaoDeIcone("Forja", raiz.transform, IconeDoHud.Forja, "Forja", 72, () => { });
                Assert.IsFalse(b.ComSelo);
                b.Selo(2);
                Assert.IsTrue(b.ComSelo);
                b.Selo(0);
                Assert.IsFalse(b.ComSelo);
            }
            finally
            {
                Object.DestroyImmediate(raiz);
            }
        }

        [Test]
        public void Ruido_e_deterministico()
        {
            Assert.AreEqual(TemaVisual.Ruido(10, 20), TemaVisual.Ruido(10, 20));
            Assert.That(TemaVisual.Ruido(3, 7), Is.InRange(0f, 1f));
        }

        [Test]
        public void Kit_monta_painel_e_botao_com_os_sprites_ornamentados()
        {
            var raiz = new GameObject("Raiz", typeof(RectTransform));
            try
            {
                var painel = KitDeInterface.Painel("Painel", raiz.transform);
                Assert.AreSame(TemaVisual.Painel, painel.sprite);
                Assert.AreEqual(Image.Type.Sliced, painel.type);

                var botao = KitDeInterface.Botao("Botao", raiz.transform, "Ok", () => { });
                Assert.AreSame(TemaVisual.Botao, ((Image)botao.targetGraphic).sprite);
                Assert.AreEqual(Selectable.Transition.SpriteSwap, botao.transition);
                Assert.AreSame(TemaVisual.BotaoPressionado, botao.spriteState.pressedSprite);
            }
            finally
            {
                Object.DestroyImmediate(raiz);
            }
        }
    }
}
