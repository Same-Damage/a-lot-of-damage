using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class ProgressoTestes
    {
        static readonly DefinicaoDeUpgrade Forca = new DefinicaoDeUpgrade
        {
            Id = "teste_forca", CustoBase = 10, CrescimentoDoCusto = 1.07, ValorBase = 3,
        };

        Progresso progresso;

        [SetUp]
        public void Limpar() => progresso = new Progresso();

        [Test]
        public void Upgrade_comeca_no_nivel_1()
        {
            Assert.AreEqual(1, progresso.Nivel(Forca.Id));
            Assert.AreEqual(3, progresso.Valor(Forca), 1e-9);
        }

        [Test]
        public void Compra_guarda_o_nivel_e_desconta_o_ouro()
        {
            progresso.ReceberOuro(100);
            progresso.Comprar(Forca, 1);
            Assert.AreEqual(2, progresso.Nivel(Forca.Id));
            Assert.AreEqual(90, progresso.Ouro, 1e-9);
        }

        [Test]
        public void Compra_sem_ouro_nao_muda_nada()
        {
            progresso.ReceberOuro(5);
            var r = progresso.Comprar(Forca, 1);
            Assert.IsFalse(r.Comprou);
            Assert.AreEqual(1, progresso.Nivel(Forca.Id));
            Assert.AreEqual(5, progresso.Ouro, 1e-9);
        }

        static readonly DefinicaoDeItem Espada = new DefinicaoDeItem
        {
            Id = "teste_espada", Slot = SlotDeItem.Arma, Alvo = AlvoDoItem.Click, BonusPorNivel = 1,
            CustoBaseEmOuro = 10, CrescimentoDoCusto = 2, MaterialId = "presa", MaterialPorNivel = 1,
        };

        static readonly ParametrosDosItens Itens = new ParametrosDosItens
        {
            BonusDaRaridade = new[] { 1.0, 2, 3, 4, 5 }, CrescimentoPorEstagio = 1, LimiteDoInventario = 3,
        };

        static readonly ParametrosDoBestiario Bestiario = new ParametrosDoBestiario
        {
            PrimeiroMarco = 10, FatorEntreMarcos = 10, BonusContraTipoPorMarco = 0.5, BonusGeralPorMarco = 0.1,
        };

        ItemDoJogador Receber(SlotDeItem slot = SlotDeItem.Arma, string tipo = "teste_espada") =>
            progresso.ReceberItem(tipo, slot, Raridade.Comum, 1, Itens.LimiteDoInventario);

        [Test]
        public void Vitoria_sem_monstro_do_catalogo_conta_so_a_vitoria()
        {
            Assert.IsFalse(progresso.RegistrarVitoria(null, Bestiario));
            Assert.AreEqual(1, progresso.Vitorias);
        }

        [Test]
        public void Vitoria_conta_no_bestiario_e_da_material()
        {
            var monstro = new DefinicaoDeMonstro { Id = "lobo", MaterialId = "presa", MaterialPorVitoria = 2 };
            progresso.RegistrarVitoria(monstro, Bestiario);
            Assert.AreEqual(1, progresso.Derrotas("lobo"));
            Assert.AreEqual(2, progresso.Material("presa"));
        }

        [Test]
        public void Item_recebido_vai_para_o_inventario_com_id_unico()
        {
            var a = Receber();
            var b = Receber();
            Assert.AreEqual(2, progresso.Inventario.Count);
            Assert.AreNotEqual(a.Id, b.Id);
            Assert.AreEqual(0, a.Nivel, "item novo começa em +0");
        }

        [Test]
        public void Inventario_cheio_nao_recebe_item()
        {
            for (int i = 0; i < 3; i++) Assert.IsNotNull(Receber());
            Assert.IsNull(Receber());
            Assert.AreEqual(3, progresso.Inventario.Count);
        }

        [Test]
        public void Equipar_tira_do_inventario_e_devolve_o_antigo()
        {
            var a = Receber();
            var b = Receber();
            Assert.IsTrue(progresso.Equipar(a.Id));
            Assert.AreSame(a, progresso.Equipado(SlotDeItem.Arma));
            Assert.AreEqual(1, progresso.Inventario.Count);

            Assert.IsTrue(progresso.Equipar(b.Id));
            Assert.AreSame(b, progresso.Equipado(SlotDeItem.Arma));
            CollectionAssert.Contains(progresso.Inventario, a);
        }

        [Test]
        public void Desequipar_devolve_ao_inventario_se_houver_espaco()
        {
            var a = Receber();
            progresso.Equipar(a.Id);
            Receber(); Receber(); Receber();
            Assert.IsFalse(progresso.Desequipar(SlotDeItem.Arma, Itens.LimiteDoInventario), "inventário cheio");
            progresso.RemoverDoInventario(progresso.Inventario[0].Id);
            Assert.IsTrue(progresso.Desequipar(SlotDeItem.Arma, Itens.LimiteDoInventario));
            Assert.IsNull(progresso.Equipado(SlotDeItem.Arma));
        }

        [Test]
        public void Organizar_poe_raridade_maior_primeiro_depois_slot_e_refino()
        {
            var comumAnel = progresso.ReceberItem("anel", SlotDeItem.Anel, Raridade.Comum, 1, 10);
            var raraArma = progresso.ReceberItem("espada", SlotDeItem.Arma, Raridade.Raro, 1, 10);
            var comumArma = progresso.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            var comumArmaRefinada = progresso.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            comumArmaRefinada.Nivel = 2;

            progresso.OrganizarInventario();
            CollectionAssert.AreEqual(new[] { raraArma, comumArmaRefinada, comumArma, comumAnel }, progresso.Inventario);
        }

        [Test]
        public void Item_equipado_nao_pode_ser_removido_do_inventario()
        {
            var a = Receber();
            progresso.Equipar(a.Id);
            Assert.IsNull(progresso.RemoverDoInventario(a.Id));
        }

        [Test]
        public void Refinar_sobe_o_nivel_do_proprio_item_e_gasta_ouro_e_material()
        {
            var a = Receber();
            progresso.ReceberOuro(15);
            progresso.AdicionarMaterial("presa", 3);
            var r = progresso.Refinar(a.Id, Espada, 0.99);
            Assert.IsTrue(r.Forjou);
            Assert.AreEqual(1, a.Nivel);
            Assert.AreEqual(5, progresso.Ouro, 1e-9);
            Assert.AreEqual(2, progresso.Material("presa"));
        }

        [Test]
        public void Gastar_so_tira_o_que_tem()
        {
            progresso.ReceberOuro(10);
            progresso.AdicionarMaterial("presa", 2);
            Assert.IsFalse(progresso.Gastar(11, "presa", 1));
            Assert.IsFalse(progresso.Gastar(5, "presa", 3));
            Assert.IsTrue(progresso.Gastar(5, "presa", 2));
            Assert.AreEqual(5, progresso.Ouro, 1e-9);
            Assert.AreEqual(0, progresso.Material("presa"));
        }

        [Test]
        public void Fontes_de_poder_se_multiplicam()
        {
            progresso.Equipar(Receber().Id); // comum: ×2
            var lobo = new DefinicaoDeMonstro { Id = "lobo" };
            for (int i = 0; i < 10; i++) progresso.RegistrarVitoria(lobo, Bestiario); // ×1,5 contra lobo, ×1,1 geral

            var tipos = new[] { Espada };
            Assert.AreEqual(2 * 1.5 * 1.1, progresso.MultiplicadorDeDano(AlvoDoItem.Click, "lobo", tipos, Itens, Bestiario), 1e-9);
            Assert.AreEqual(2 * 1.1, progresso.MultiplicadorDeDano(AlvoDoItem.Click, "slime", tipos, Itens, Bestiario), 1e-9);
            Assert.AreEqual(1.1, progresso.MultiplicadorDeDano(AlvoDoItem.Automatico, "slime", tipos, Itens, Bestiario), 1e-9);
        }

        [Test]
        public void Item_no_inventario_nao_conta_para_o_dano()
        {
            Receber();
            Assert.AreEqual(1, progresso.MultiplicadorDeDano(AlvoDoItem.Click, null, new[] { Espada }, Itens, Bestiario), 1e-9);
        }

        [Test]
        public void Itens_e_equipados_vao_e_voltam_do_save()
        {
            var a = Receber();
            var b = Receber(SlotDeItem.Anel, "anel");
            progresso.Equipar(a.Id);
            a.Nivel = 4;

            var copia = Progresso.Importar(progresso.Exportar());
            Assert.AreEqual(4, copia.Equipado(SlotDeItem.Arma).Nivel);
            Assert.AreEqual(b.Id, copia.Inventario[0].Id);
            var c = copia.ReceberItem("anel", SlotDeItem.Anel, Raridade.Comum, 1, 10);
            Assert.Greater(c.Id, b.Id, "ids continuam únicos depois de carregar");
        }

        [Test]
        public void Save_antigo_com_forja_por_nivel_vira_itens_equipados()
        {
            var dados = new DadosDoProgresso();
            dados.NiveisDosItens.Add(new ParDeValor { Id = "espada", Valor = 3 });
            dados.NiveisDosItens.Add(new ParDeValor { Id = "manopla", Valor = 1 });
            dados.NiveisDosItens.Add(new ParDeValor { Id = "anel", Valor = 0 });

            var p = Progresso.Importar(dados);
            Assert.AreEqual("espada", p.Equipado(SlotDeItem.Arma).TipoId);
            Assert.AreEqual(3, p.Equipado(SlotDeItem.Arma).Nivel);
            Assert.AreEqual(1, p.Equipado(SlotDeItem.Luvas).Nivel);
            Assert.IsNull(p.Equipado(SlotDeItem.Anel), "item nunca forjado não vira item");
            Assert.AreEqual(Raridade.Comum, p.Equipado(SlotDeItem.Arma).Raridade);
        }

        [Test]
        public void Exportar_e_importar_mantem_tudo()
        {
            progresso.ReceberOuro(1234.5);
            progresso.Comprar(Forca, 1);
            progresso.AdicionarMaterial("presa", 7);
            progresso.RegistrarVitoria(new DefinicaoDeMonstro { Id = "lobo" }, Bestiario);
            progresso.AvancarCampanha(new ParametrosDaCampanha { VitoriasParaAvancar = 1 }, false);

            var copia = Progresso.Importar(progresso.Exportar());
            Assert.AreEqual(progresso.Ouro, copia.Ouro, 1e-9);
            Assert.AreEqual(2, copia.Nivel(Forca.Id));
            Assert.AreEqual(7, copia.Material("presa"));
            Assert.AreEqual(1, copia.Derrotas("lobo"));
            Assert.AreEqual(1, copia.Vitorias);
            Assert.AreEqual(2, copia.Estagio);
        }

        [Test]
        public void Importar_dados_invalidos_volta_a_valores_seguros()
        {
            var p = Progresso.Importar(new DadosDoProgresso { Ouro = -5, Estagio = 0, Niveis = null });
            Assert.AreEqual(0, p.Ouro, 1e-9);
            Assert.AreEqual(1, p.Estagio);
            Assert.AreEqual(Progresso.ClassePadrao, p.ClasseId);
            Assert.AreEqual(0, Progresso.Importar(null).Ouro, 1e-9);
        }

        [Test]
        public void Balanceamento_padrao_tem_os_tres_upgrades_do_prototipo()
        {
            var b = BalanceamentoDeUpgrades.Padrao();
            Assert.IsNotNull(b.Find(u => u.Id == IdsDosUpgrades.Forca));
            Assert.IsNotNull(b.Find(u => u.Id == IdsDosUpgrades.Automatico));
            Assert.IsNotNull(b.Find(u => u.Id == IdsDosUpgrades.Precisao));
        }

        [Test]
        public void Almas_e_ascensoes_vao_e_voltam_do_save()
        {
            progresso.Ascender(7);
            var copia = Progresso.Importar(progresso.Exportar());
            Assert.AreEqual(7, copia.Almas);
            Assert.AreEqual(1, copia.Ascensoes);
        }

        [Test]
        public void Heroi_escolhido_vai_e_volta_do_save()
        {
            var p = new Progresso();
            p.TrocarHeroi("Arqueira");
            Assert.AreEqual("Arqueira", Progresso.Importar(p.Exportar()).HeroiId);
            Assert.AreEqual("", Progresso.Importar(new DadosDoProgresso()).HeroiId);
        }
    }
}
