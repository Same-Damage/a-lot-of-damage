using ALotOfDamage.UI;
using NUnit.Framework;
using UnityEngine;

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
        public void Orbe_e_redondo_com_canto_transparente()
        {
            var t = TemaVisual.TexturaDoOrbe;
            Assert.AreEqual(0f, t.GetPixel(0, 0).a, 0.01f);
            Assert.Greater(t.GetPixel(t.width / 2, t.height / 2).a, 0.9f);
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
        public void Slot_tem_fundo_mais_escuro_que_a_moldura()
        {
            var t = TemaVisual.TexturaDoSlot;
            Assert.Less(Brilho(t.GetPixel(t.width / 2, t.height / 2)), Brilho(t.GetPixel(t.width / 2, 2)));
        }

        [Test]
        public void Suporte_do_orbe_e_vazado_no_meio()
        {
            var t = TemaVisual.TexturaDoSuporte;
            Assert.AreEqual(0f, t.GetPixel(t.width / 2, t.height / 2).a, 0.01f);
        }

        [Test]
        public void Ruido_e_deterministico()
        {
            Assert.AreEqual(TemaVisual.Ruido(10, 20), TemaVisual.Ruido(10, 20));
            Assert.That(TemaVisual.Ruido(3, 7), Is.InRange(0f, 1f));
        }

        [TestCase(0.0, 0f)]
        [TestCase(0.5, 0.5f)]
        [TestCase(2.0, 1f)]
        [TestCase(-1.0, 0f)]
        [TestCase(double.NaN, 0f)]
        public void Fracao_da_barra_fica_entre_zero_e_um(double valor, float esperado)
        {
            Assert.AreEqual(esperado, TemaVisual.Fracao(valor), 0.0001f);
        }
    }
}
