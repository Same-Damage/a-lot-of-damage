using System.Collections.Generic;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class CasosDeUsoTestes
    {
        class CatalogoFalso : ICatalogo
        {
            public IReadOnlyList<DefinicaoDeUpgrade> Upgrades { get; set; } = new List<DefinicaoDeUpgrade>();
            public IReadOnlyList<DefinicaoDeItem> Itens { get; set; } = new List<DefinicaoDeItem>();
            public IReadOnlyList<DefinicaoDeMonstro> Monstros { get; set; } = new List<DefinicaoDeMonstro>();
            public ParametrosDoBestiario Bestiario { get; set; } = new ParametrosDoBestiario();
            public IReadOnlyList<DefinicaoDeClasse> Classes { get; set; } = new List<DefinicaoDeClasse>();
            public IReadOnlyList<DefinicaoDeFollower> Followers { get; set; } = new List<DefinicaoDeFollower>();
            public ParametrosDaCampanha Campanha { get; set; } = new ParametrosDaCampanha
            {
                VidaBase = 100, CrescimentoPorEstagio = 2, OuroPorVida = 0.5, VitoriasParaAvancar = 2, ChefeACada = 5, MultiplicadorDeVidaDoChefe = 10,
            };
            public ParametrosDosItens ParametrosDosItens { get; set; } = new ParametrosDosItens
            {
                ChanceDeDrop = 0.3, PesosDaRaridade = new double[] { 1, 0, 0, 0, 0 }, RaridadeMinimaDoChefe = Raridade.Raro,
                BonusDaRaridade = new[] { 1.0, 2, 3, 4, 5 }, CrescimentoPorEstagio = 1, CrescimentoDoPreco = 1,
                LimiteDoInventario = 2, CustoDeFabricarEmOuro = 20, MaterialParaFabricar = 3,
                PrecoDeVenda = new double[] { 10, 20, 30, 40, 50 }, MaterialAoDesmontar = new[] { 1, 2, 3, 4, 5 },
            };
            public IReadOnlyList<DefinicaoDeRegiao> Regioes { get; set; } = new List<DefinicaoDeRegiao>
            {
                new DefinicaoDeRegiao { Nome = "Campos" },
            };
        }

        class RepositorioQueConta : IRepositorioDeProgresso
        {
            public readonly Progresso Progresso = new Progresso();
            public int Salvamentos;
            public Progresso Carregar() => Progresso;
            public void Salvar(Progresso p) => Salvamentos++;
        }

        class SorteioFixo : ISorteio
        {
            public double Valor;
            public double Proximo() => Valor;
        }

        // Devolve os valores na ordem e depois repete o último
        class SorteioEmSequencia : ISorteio
        {
            readonly double[] valores;
            int i;
            public SorteioEmSequencia(params double[] valores) => this.valores = valores;
            public double Proximo() => valores[System.Math.Min(i++, valores.Length - 1)];
        }

        static readonly ISorteio SemDrop = new SorteioFixo { Valor = 0.99 };

        RepositorioQueConta repositorio;
        CatalogoFalso catalogo;

        [SetUp]
        public void Montar()
        {
            repositorio = new RepositorioQueConta();
            catalogo = new CatalogoFalso
            {
                Upgrades = new List<DefinicaoDeUpgrade>
                {
                    new DefinicaoDeUpgrade { Id = IdsDosUpgrades.Forca, CustoBase = 10, CrescimentoDoCusto = 2, ValorBase = 3 },
                    new DefinicaoDeUpgrade { Id = IdsDosUpgrades.Automatico, CustoBase = 10, CrescimentoDoCusto = 2, ValorBase = 5 },
                    new DefinicaoDeUpgrade { Id = IdsDosUpgrades.Precisao, Tipo = TipoDeUpgrade.Linear, ValorBase = 0.05, ValorPorNivel = 0.01 },
                },
                Itens = new List<DefinicaoDeItem>
                {
                    new DefinicaoDeItem { Id = "espada", Nome = "Espada", Slot = SlotDeItem.Arma, Alvo = AlvoDoItem.Click, BonusPorNivel = 1, CustoBaseEmOuro = 10, MaterialId = "presa", MaterialPorNivel = 1, ChanceDeSucessoCritico = 0.1 },
                    new DefinicaoDeItem { Id = "anel", Nome = "Anel", Slot = SlotDeItem.Anel, Alvo = AlvoDoItem.Automatico, BonusPorNivel = 1, CustoBaseEmOuro = 10, MaterialId = "gosma", MaterialPorNivel = 1 },
                },
                Monstros = new List<DefinicaoDeMonstro>
                {
                    new DefinicaoDeMonstro { Id = "lobo", MaterialId = "presa", MaterialPorVitoria = 2, Forma = FormaDoMonstro.Quadrupede, Cor = "#9999A6" },
                },
            };
        }

        ItemDoJogador EspadaEquipada()
        {
            var item = repositorio.Progresso.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            repositorio.Progresso.Equipar(item.Id);
            return item;
        }

        [Test]
        public void Comprar_upgrade_salva_quando_compra()
        {
            repositorio.Progresso.ReceberOuro(10);
            var r = new ComprarUpgrade(repositorio, catalogo).Executar(IdsDosUpgrades.Forca, 1);
            Assert.IsTrue(r.Comprou);
            Assert.AreEqual(2, repositorio.Progresso.Nivel(IdsDosUpgrades.Forca));
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Comprar_upgrade_sem_ouro_nao_salva()
        {
            var r = new ComprarUpgrade(repositorio, catalogo).Executar(IdsDosUpgrades.Forca, 1);
            Assert.IsFalse(r.Comprou);
            Assert.AreEqual(0, repositorio.Salvamentos);
        }

        [Test]
        public void Upgrade_inexistente_nao_compra()
        {
            repositorio.Progresso.ReceberOuro(1000);
            Assert.IsFalse(new ComprarUpgrade(repositorio, catalogo).Executar("nao_existe", 1).Comprou);
        }

        [Test]
        public void Refinar_usa_o_sorteio_injetado()
        {
            var espada = EspadaEquipada();
            repositorio.Progresso.ReceberOuro(10);
            repositorio.Progresso.AdicionarMaterial("presa", 1);
            var r = new RefinarItem(repositorio, catalogo, new SorteioFixo { Valor = 0.05 }).Executar(espada.Id);
            Assert.IsTrue(r.SucessoCritico);
            Assert.AreEqual(2, espada.Nivel);
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Refinar_item_inexistente_nao_faz_nada()
        {
            repositorio.Progresso.ReceberOuro(1000);
            Assert.IsFalse(new RefinarItem(repositorio, catalogo, SemDrop).Executar(42).Forjou);
        }

        [Test]
        public void Vitoria_abaixo_da_chance_dropa_item_no_inventario()
        {
            // cai item (0,1), tipo: o segundo da lista (0,6 dá o anel), raridade comum
            var r = new RegistrarVitoria(repositorio, catalogo, new SorteioEmSequencia(0.1, 0.6, 0.0)).Executar("lobo", false);
            Assert.IsNotNull(r.ItemGanho);
            Assert.AreEqual("anel", r.ItemGanho.TipoId);
            Assert.AreEqual(SlotDeItem.Anel, r.ItemGanho.Slot);
            Assert.AreEqual(Raridade.Comum, r.ItemGanho.Raridade);
            Assert.AreEqual(1, r.ItemGanho.Estagio);
            CollectionAssert.Contains(repositorio.Progresso.Inventario, r.ItemGanho);
        }

        [Test]
        public void Vitoria_acima_da_chance_nao_dropa()
        {
            var r = new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", false);
            Assert.IsNull(r.ItemGanho);
            Assert.IsEmpty(repositorio.Progresso.Inventario);
        }

        [Test]
        public void Chefe_sempre_dropa_com_raridade_minima()
        {
            var r = new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", true);
            Assert.IsNotNull(r.ItemGanho);
            Assert.AreEqual(Raridade.Raro, r.ItemGanho.Raridade);
        }

        [Test]
        public void Drop_com_inventario_cheio_e_vendido_na_hora()
        {
            var p = repositorio.Progresso;
            p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            double antes = p.Ouro;
            var r = new RegistrarVitoria(repositorio, catalogo, new SorteioEmSequencia(0.1, 0.0, 0.0)).Executar(null, false);
            Assert.IsNotNull(r.ItemGanho);
            Assert.IsTrue(r.ItemVendidoPorFaltaDeEspaco);
            Assert.AreEqual(2, p.Inventario.Count);
            Assert.AreEqual(antes + r.Ouro + 10, p.Ouro, 1e-9);
        }

        [Test]
        public void Fabricar_cria_item_do_tipo_escolhido_e_gasta_ouro_e_material()
        {
            var p = repositorio.Progresso;
            p.ReceberOuro(25);
            p.AdicionarMaterial("gosma", 3);
            var r = new FabricarItem(repositorio, catalogo, new SorteioFixo { Valor = 0.0 }).Executar("anel");
            Assert.IsTrue(r.Fabricou);
            Assert.AreEqual("anel", r.Item.TipoId);
            Assert.AreEqual(5, p.Ouro, 1e-9);
            Assert.AreEqual(0, p.Material("gosma"));
            CollectionAssert.Contains(p.Inventario, r.Item);
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Fabricar_sem_recurso_ou_sem_espaco_nao_faz_nada()
        {
            var p = repositorio.Progresso;
            p.ReceberOuro(25);
            Assert.IsFalse(new FabricarItem(repositorio, catalogo, SemDrop).Executar("anel").Fabricou, "sem material");

            p.AdicionarMaterial("gosma", 3);
            p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            Assert.IsFalse(new FabricarItem(repositorio, catalogo, SemDrop).Executar("anel").Fabricou, "inventário cheio");
            Assert.AreEqual(25, p.Ouro, 1e-9);
            Assert.AreEqual(3, p.Material("gosma"));
            Assert.AreEqual(0, repositorio.Salvamentos);
        }

        [Test]
        public void Vender_tira_do_inventario_e_da_ouro()
        {
            var item = repositorio.Progresso.ReceberItem("espada", SlotDeItem.Arma, Raridade.Incomum, 1, 10);
            Assert.AreEqual(20, new VenderItem(repositorio, catalogo).Executar(item.Id), 1e-9);
            Assert.AreEqual(20, repositorio.Progresso.Ouro, 1e-9);
            Assert.IsEmpty(repositorio.Progresso.Inventario);
        }

        [Test]
        public void Item_equipado_nao_e_vendido_nem_desmontado()
        {
            var espada = EspadaEquipada();
            Assert.AreEqual(0, new VenderItem(repositorio, catalogo).Executar(espada.Id), 1e-9);
            Assert.AreEqual(0, new DesmontarItem(repositorio, catalogo).Executar(espada.Id));
            Assert.AreSame(espada, repositorio.Progresso.Equipado(SlotDeItem.Arma));
        }

        [Test]
        public void Desmontar_devolve_o_material_do_tipo()
        {
            var item = repositorio.Progresso.ReceberItem("espada", SlotDeItem.Arma, Raridade.Raro, 1, 10);
            item.Nivel = 2;
            Assert.AreEqual(5, new DesmontarItem(repositorio, catalogo).Executar(item.Id));
            Assert.AreEqual(5, repositorio.Progresso.Material("presa"));
            Assert.IsEmpty(repositorio.Progresso.Inventario);
        }

        [Test]
        public void Organizar_inventario_salva_a_nova_ordem()
        {
            var p = repositorio.Progresso;
            var comum = p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            var lendario = p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Lendario, 1, 10);
            new OrganizarInventario(repositorio, catalogo).Executar();
            Assert.AreSame(lendario, p.Inventario[0]);
            Assert.AreSame(comum, p.Inventario[1]);
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Detalhes_do_item_trazem_nome_bonus_venda_e_desmonte()
        {
            var item = repositorio.Progresso.ReceberItem("espada", SlotDeItem.Arma, Raridade.Incomum, 1, 10);
            var d = new ConsultarItem(repositorio, catalogo).Executar(item.Id);
            Assert.AreEqual("Espada", d.Nome);
            Assert.AreEqual("Incomum", d.NomeDaRaridade);
            Assert.AreEqual(AlvoDoItem.Click, d.Alvo);
            Assert.AreEqual(3, d.Multiplicador, 1e-9);
            Assert.AreEqual(20, d.PrecoDeVenda, 1e-9);
            Assert.AreEqual(2, d.MaterialAoDesmontar);
            Assert.IsFalse(d.Equipado);
            Assert.IsNull(new ConsultarItem(repositorio, catalogo).Executar(999).Nome, "item inexistente vem vazio");
        }

        [Test]
        public void Equipar_e_desequipar_pelos_casos_de_uso()
        {
            var item = repositorio.Progresso.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10);
            Assert.IsTrue(new EquiparItem(repositorio, catalogo).Executar(item.Id));
            Assert.AreSame(item, repositorio.Progresso.Equipado(SlotDeItem.Arma));
            Assert.IsTrue(new DesequiparItem(repositorio, catalogo).Executar(SlotDeItem.Arma));
            Assert.IsNull(repositorio.Progresso.Equipado(SlotDeItem.Arma));
            Assert.AreEqual(2, repositorio.Salvamentos);
        }

        [Test]
        public void Vitoria_da_ouro_material_e_conta_no_bestiario()
        {
            var r = new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", false);
            Assert.AreEqual(50, repositorio.Progresso.Ouro, 1e-9);
            Assert.AreEqual(2, repositorio.Progresso.Material("presa"));
            Assert.AreEqual(1, r.Derrotas);
            Assert.AreEqual(2, r.MaterialGanho);
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Vitoria_sem_tipo_so_da_ouro()
        {
            var r = new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar(null, false);
            Assert.IsNull(r.Monstro);
            Assert.AreEqual(50, repositorio.Progresso.Ouro, 1e-9);
        }

        [Test]
        public void Atributos_multiplicam_upgrade_forja_e_bestiario()
        {
            EspadaEquipada(); // comum: ×2 no click

            var a = new CalcularAtributos(repositorio, catalogo).Executar("lobo");
            Assert.AreEqual(3 * 2, a.DanoDoClick, 1e-9);
            Assert.AreEqual(5, a.DanoAutomatico, 1e-9);
            Assert.AreEqual(0.05, a.ChanceCritica, 1e-9);
        }

        [Test]
        public void Atributos_trazem_a_classe_e_o_follower_do_jogador()
        {
            catalogo.Classes = new List<DefinicaoDeClasse> { new DefinicaoDeClasse { Id = Progresso.ClassePadrao } };
            catalogo.Followers = new List<DefinicaoDeFollower> { new DefinicaoDeFollower { Id = Progresso.FollowerPadrao, FracaoDoAutoAtaque = 0.2 } };

            var a = new CalcularAtributos(repositorio, catalogo).Executar(null);
            Assert.AreEqual(Progresso.ClassePadrao, a.Classe.Id);
            Assert.AreEqual(Progresso.FollowerPadrao, a.Follower.Id);
            Assert.AreEqual(5 * 0.2, a.DanoDoFollower, 1e-9);
        }

        [Test]
        public void Sem_follower_no_catalogo_o_follower_nao_causa_dano()
        {
            var a = new CalcularAtributos(repositorio, catalogo).Executar(null);
            Assert.IsNull(a.Follower);
            Assert.AreEqual(0, a.DanoDoFollower, 1e-9);
        }

        [Test]
        public void Poder_total_sobe_ao_comprar_upgrade()
        {
            var poder = new CalcularPoder(new CalcularAtributos(repositorio, catalogo));
            // click 3 + auto 5, crítico 5% ×5 → fator 1,2
            Assert.AreEqual(8 * 1.2, poder.Executar(), 1e-9);

            repositorio.Progresso.ReceberOuro(10);
            new ComprarUpgrade(repositorio, catalogo).Executar(IdsDosUpgrades.Forca, 1);
            Assert.AreEqual(11 * 1.2, poder.Executar(), 1e-9);
        }

        [Test]
        public void Vitorias_avancam_a_campanha_e_o_inimigo_fica_mais_forte()
        {
            var vitoria = new RegistrarVitoria(repositorio, catalogo, SemDrop);
            var inimigo = new CalcularInimigo(repositorio, catalogo);
            Assert.AreEqual(100, inimigo.Executar("lobo", false).Vida, 1e-9);

            Assert.IsFalse(vitoria.Executar("lobo", false).AvancouEstagio);
            var r = vitoria.Executar("lobo", false);
            Assert.IsTrue(r.AvancouEstagio);
            Assert.AreEqual(2, r.Estagio);
            Assert.AreEqual(200, inimigo.Executar("lobo", false).Vida, 1e-9);
        }

        [Test]
        public void Chefe_tem_mais_vida_e_da_mais_ouro()
        {
            var chefe = new CalcularInimigo(repositorio, catalogo).Executar("lobo", true);
            Assert.AreEqual(1000, chefe.Vida, 1e-9);
            Assert.AreEqual(500, chefe.Ouro, 1e-9);
            Assert.IsTrue(chefe.Chefe);
        }

        [Test]
        public void Inimigo_da_batalha_traz_a_forma_e_a_cor_do_monstro()
        {
            var lobo = new CalcularInimigo(repositorio, catalogo).Executar("lobo", false);
            Assert.AreEqual(FormaDoMonstro.Quadrupede, lobo.Forma);
            Assert.AreEqual("#9999A6", lobo.Cor);
        }

        [Test]
        public void Consultar_campanha_mostra_estagio_e_regiao()
        {
            var c = new ConsultarCampanha(repositorio, catalogo).Executar();
            Assert.AreEqual(1, c.Estagio);
            Assert.AreEqual(2, c.VitoriasParaAvancar);
            Assert.AreEqual("Campos", c.Regiao.Nome);
            Assert.IsFalse(c.EstagioDeChefe);
        }

        [Test]
        public void Sem_ouro_nao_ha_pendencias()
        {
            var lista = new List<Pendencia>();
            new ListarPendencias(repositorio, catalogo).Executar(lista);
            Assert.IsEmpty(lista);
        }

        [Test]
        public void Pendencias_mostram_upgrade_forja_e_bestiario_prontos()
        {
            EspadaEquipada();
            repositorio.Progresso.ReceberOuro(10);
            repositorio.Progresso.AdicionarMaterial("presa", 1);
            var lobo = catalogo.Monstros[0];
            for (int i = 0; i < 8; i++) repositorio.Progresso.RegistrarVitoria(lobo, catalogo.Bestiario);

            var lista = new List<Pendencia>();
            new ListarPendencias(repositorio, catalogo).Executar(lista);
            var tipos = lista.ConvertAll(x => x.Tipo);
            CollectionAssert.Contains(tipos, TipoDePendencia.Upgrade);
            CollectionAssert.Contains(tipos, TipoDePendencia.Forja);
            CollectionAssert.Contains(tipos, TipoDePendencia.Bestiario);
            CollectionAssert.DoesNotContain(tipos, TipoDePendencia.Chefe);
        }

        [Test]
        public void Estagio_de_chefe_vira_pendencia()
        {
            catalogo.Campanha.ChefeACada = 1;
            var lista = new List<Pendencia>();
            new ListarPendencias(repositorio, catalogo).Executar(lista);
            Assert.AreEqual(TipoDePendencia.Chefe, lista[0].Tipo);
        }

        [Test]
        public void Brilho_da_arma_segue_o_item_de_click_equipado_mais_refinado()
        {
            var brilho = new ConsultarBrilhoDaArma(repositorio, catalogo);
            Assert.AreEqual(0, brilho.Executar(), 1e-6);

            EspadaEquipada().Nivel = 3;
            Assert.AreEqual(RegrasDeAparencia.BrilhoDaArma(3), brilho.Executar(), 1e-6);
        }

        [Test]
        public void Item_de_auto_ataque_ou_guardado_nao_acende_a_arma()
        {
            var p = repositorio.Progresso;
            var anel = p.ReceberItem("anel", SlotDeItem.Anel, Raridade.Comum, 1, 10);
            p.Equipar(anel.Id);
            anel.Nivel = 5;
            p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Comum, 1, 10).Nivel = 5;
            Assert.AreEqual(0, new ConsultarBrilhoDaArma(repositorio, catalogo).Executar(), 1e-6);
        }

        [Test]
        public void Repositorio_em_memoria_devolve_o_mesmo_progresso()
        {
            var r = new RepositorioDeProgressoEmMemoria();
            r.Carregar().ReceberOuro(5);
            Assert.AreEqual(5, r.Carregar().Ouro, 1e-9);
        }

        static readonly string[] Herois = { "Cavaleiro", "Arqueira" };

        [Test]
        public void Sem_heroi_escolhido_usa_o_primeiro()
        {
            Assert.AreEqual("Cavaleiro", new EscolhaDeHeroi(repositorio, catalogo).Atual(Herois));
        }

        [Test]
        public void Trocar_heroi_passa_para_o_proximo_volta_ao_primeiro_e_salva()
        {
            var escolha = new EscolhaDeHeroi(repositorio, catalogo);
            Assert.AreEqual("Arqueira", escolha.Trocar(Herois));
            Assert.AreEqual("Arqueira", escolha.Atual(Herois));
            Assert.AreEqual(1, repositorio.Salvamentos);
            Assert.AreEqual("Cavaleiro", escolha.Trocar(Herois));
        }

        [Test]
        public void Heroi_salvo_que_saiu_do_jogo_volta_para_o_primeiro()
        {
            repositorio.Progresso.TrocarHeroi("Mago");
            Assert.AreEqual("Cavaleiro", new EscolhaDeHeroi(repositorio, catalogo).Atual(Herois));
        }
    }
}
