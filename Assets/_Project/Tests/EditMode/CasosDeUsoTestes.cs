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
            public ParametrosDoGacha Gacha { get; set; } = new ParametrosDoGacha
            {
                CustoPorInvocacao = 100, CustoDeDez = 900, Taxas = new double[] { 50, 30, 14, 5, 1 }, PityEpico = 30, PityLendario = 90,
                BonusPorEstrela = 0.2, MaximoDeEstrelas = 5, CristaisPorEstrelaExtra = 50,
                CristaisPorChefe = 25, CristaisDaAscensao = 200, CristaisPorAlma = 5, VitoriasAfkPorCristal = 20,
            };
            public IReadOnlyList<DefinicaoDeReliquia> Reliquias { get; set; } = new List<DefinicaoDeReliquia>
            {
                new DefinicaoDeReliquia { Id = "coroa", Nome = "Coroa", Alvo = AlvoDaReliquia.Click, BonusPorNivel = 1, CustoParaMontar = 3 },
                new DefinicaoDeReliquia { Id = "bolsa", Nome = "Bolsa", Alvo = AlvoDaReliquia.Ouro, BonusPorNivel = 0.5, CustoParaMontar = 3 },
            };
            public ParametrosDasReliquias ParametrosDasReliquias { get; set; } = new ParametrosDasReliquias
            {
                FragmentosPorChefe = 3, EstagiosPorFragmentoExtra = 10, NivelMaximo = 3, CustoBaseDeMelhoria = 2,
            };
            public IReadOnlyList<DefinicaoDePredio> Predios { get; set; } = new List<DefinicaoDePredio>
            {
                new DefinicaoDePredio { Id = "balcao", Nome = "Balcão", Efeito = EfeitoDoPredio.OuroPorSegundo, BonusPorNivel = 0.02, CustoBase = 100, CrescimentoDoCusto = 2 },
                new DefinicaoDePredio { Id = "cozinha", Nome = "Cozinha", Efeito = EfeitoDoPredio.Dano, BonusPorNivel = 0.1, CustoBase = 100, CrescimentoDoCusto = 2 },
                new DefinicaoDePredio { Id = "adega", Nome = "Adega", Efeito = EfeitoDoPredio.Ouro, BonusPorNivel = 0.1, CustoBase = 100, CrescimentoDoCusto = 2 },
            };
            public double LimiteDaTavernaEmHoras { get; set; } = 12;
            public IReadOnlyList<DefinicaoDeMissao> Missoes { get; set; } = new List<DefinicaoDeMissao>
            {
                new DefinicaoDeMissao { Id = "curta", Nome = "Patrulha", Horas = 1, Cristais = 5, Lutas = 10, ChanceDeItem = 0 },
                new DefinicaoDeMissao { Id = "longa", Nome = "Caçada", Horas = 8, Cristais = 60, Lutas = 90, ChanceDeItem = 1 },
            };
            public int VagasDeExpedicao { get; set; } = 2;
            public IReadOnlyList<DefinicaoDeRecurso> Recursos { get; set; } = new List<DefinicaoDeRecurso>
            {
                new DefinicaoDeRecurso { Id = "erva", Nome = "Moita de erva-lunar", MaterialId = "erva_lunar", NomeDoMaterial = "Erva-lunar", QuantidadeBase = 2, Golpes = 4 },
            };
            public IReadOnlyList<DefinicaoDeConquista> Conquistas { get; set; } = new List<DefinicaoDeConquista>
            {
                new DefinicaoDeConquista { Id = "vitorias", Nome = "Guerreiro incansável", Descricao = "Vença {0} batalhas", Meta = MetaDaConquista.Vitorias, Degraus = new double[] { 1, 3 } },
                new DefinicaoDeConquista { Id = "chefes", Nome = "Caçador de chefes", Descricao = "Derrote {0} chefes", Meta = MetaDaConquista.ChefesDerrotados, Degraus = new double[] { 1 } },
            };
            public ParametrosDasConquistas ParametrosDasConquistas { get; set; } = new ParametrosDasConquistas { BonusPorConquista = 0.01, CristaisBase = 10 };
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

        class RelogioFalso : IRelogio
        {
            public System.DateTime Agora = new System.DateTime(2026, 1, 1, 12, 0, 0, System.DateTimeKind.Utc);
            public System.DateTime AgoraUtc => Agora;
        }

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
        public void Sem_arma_equipada_o_heroi_fica_de_mao_vazia()
        {
            var v = new ConsultarVisualDoHeroi(repositorio, catalogo).Executar();
            Assert.IsFalse(v.TemArma);
            Assert.AreEqual(0, v.Brilho, 1e-6);
        }

        [Test]
        public void Arma_equipada_mostra_raridade_e_brilho_com_refino()
        {
            var p = repositorio.Progresso;
            var espada = p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Epico, 1, 10);
            p.Equipar(espada.Id);
            espada.Nivel = 3;

            var v = new ConsultarVisualDoHeroi(repositorio, catalogo).Executar();
            Assert.IsTrue(v.TemArma);
            Assert.AreEqual(Raridade.Epico, v.RaridadeDaArma);
            Assert.AreEqual(RegrasDeAparencia.BrilhoDoItem(Raridade.Epico, 3), v.Brilho, 1e-6);
        }

        [Test]
        public void Item_guardado_ou_de_outro_slot_nao_vira_arma_na_mao()
        {
            var p = repositorio.Progresso;
            var anel = p.ReceberItem("anel", SlotDeItem.Anel, Raridade.Lendario, 1, 10);
            p.Equipar(anel.Id);
            p.ReceberItem("espada", SlotDeItem.Arma, Raridade.Lendario, 1, 10);
            Assert.IsFalse(new ConsultarVisualDoHeroi(repositorio, catalogo).Executar().TemArma);
        }

        // AFK: dano automático 5/s (upgrade Automático) contra 100 de vida no estágio 1 = 20 s de luta + 5 s entre lutas.
        // 2 h = 288 lutas; 25% = 72 vitórias de 50 de ouro
        (RelogioFalso relogio, ConsultarRecompensaAfk consultar, ColetarRecompensaAfk coletar) Afk(ISorteio sorteio = null)
        {
            var relogio = new RelogioFalso();
            return (relogio, new ConsultarRecompensaAfk(repositorio, catalogo, relogio, new CalcularAtributos(repositorio, catalogo)),
                new ColetarRecompensaAfk(repositorio, catalogo, relogio, new CalcularAtributos(repositorio, catalogo), sorteio ?? SemDrop));
        }

        [Test]
        public void Primeiro_acesso_nao_tem_recompensa_e_marca_a_presenca()
        {
            var (relogio, consultar, _) = Afk();
            Assert.IsFalse(consultar.Executar().TemRecompensa);
            new RegistrarPresenca(repositorio, catalogo, relogio).Executar();
            relogio.Agora = relogio.Agora.AddHours(2);
            Assert.IsTrue(consultar.Executar().TemRecompensa);
        }

        [Test]
        public void Duas_horas_fora_rendem_um_quarto_das_lutas_em_ouro()
        {
            catalogo.ParametrosDosItens.ChanceDeDrop = 0;   // só o ouro das lutas, sem itens vendidos
            var (relogio, consultar, coletar) = Afk();
            new RegistrarPresenca(repositorio, catalogo, relogio).Executar();
            relogio.Agora = relogio.Agora.AddHours(2);

            var r = consultar.Executar();
            Assert.AreEqual(7200, r.Segundos, 1e-6);
            Assert.AreEqual(72, r.Vitorias);
            Assert.AreEqual(72 * 50, r.Ouro, 1e-6);

            coletar.Executar();
            Assert.AreEqual(72 * 50, repositorio.Progresso.Ouro, 1e-6);
        }

        [Test]
        public void Recompensa_para_de_crescer_em_12_horas()
        {
            var (relogio, consultar, _) = Afk();
            new RegistrarPresenca(repositorio, catalogo, relogio).Executar();
            relogio.Agora = relogio.Agora.AddHours(40);
            Assert.AreEqual(12 * 3600, consultar.Executar().Segundos, 1e-6);
        }

        [Test]
        public void Coletar_zera_o_tempo_fora()
        {
            var (relogio, consultar, coletar) = Afk();
            new RegistrarPresenca(repositorio, catalogo, relogio).Executar();
            relogio.Agora = relogio.Agora.AddHours(2);
            coletar.Executar();
            Assert.IsFalse(consultar.Executar().TemRecompensa);
            Assert.Greater(repositorio.Salvamentos, 0);
        }

        [Test]
        public void Pouco_tempo_fora_nao_vira_recompensa()
        {
            var (relogio, consultar, _) = Afk();
            new RegistrarPresenca(repositorio, catalogo, relogio).Executar();
            relogio.Agora = relogio.Agora.AddSeconds(30);
            Assert.IsFalse(consultar.Executar().TemRecompensa);
        }

        [Test]
        public void Afk_da_material_dos_monstros_da_regiao()
        {
            ((List<DefinicaoDeRegiao>)catalogo.Regioes)[0].Monstros.Add("lobo");
            var (relogio, consultar, coletar) = Afk();
            new RegistrarPresenca(repositorio, catalogo, relogio).Executar();
            relogio.Agora = relogio.Agora.AddHours(2);
            coletar.Executar();
            Assert.AreEqual(72 * 2, repositorio.Progresso.Material("presa"), "72 vitórias contra lobos, 2 presas cada");
        }

        [Test]
        public void Afk_da_itens_pela_chance_de_drop_e_vende_o_que_nao_cabe()
        {
            var (relogio, consultar, coletar) = Afk(new SorteioFixo { Valor = 0.0 });
            new RegistrarPresenca(repositorio, catalogo, relogio).Executar();
            relogio.Agora = relogio.Agora.AddHours(2);
            var r = coletar.Executar();
            // 72 vitórias × 30% = 21 itens; o inventário de teste tem 2 casas: 19 viram ouro (10 cada, comuns)
            Assert.AreEqual(21, r.Itens);
            Assert.AreEqual(2, repositorio.Progresso.Inventario.Count);
            Assert.AreEqual(72 * 50 + 19 * 10, repositorio.Progresso.Ouro, 1e-6);
        }

        // Ascensão: a campanha de teste libera no estágio 3 (base 1, divisor 2): estágio 3 dá (3 − 1)² ÷ 2 = 2 Almas
        void ChegarAoEstagio(int estagio)
        {
            var c = catalogo.Campanha;
            c.EstagioMinimoDaAscensao = 3; c.EstagioBaseDasAlmas = 1; c.DivisorDasAlmas = 2; c.BonusPorAlma = 0.1; c.ChefeACada = 0;
            var vitoria = new RegistrarVitoria(repositorio, catalogo, SemDrop);
            while (repositorio.Progresso.Estagio < estagio) vitoria.Executar("lobo", false);
        }

        [Test]
        public void Ascensao_indisponivel_antes_do_estagio_minimo()
        {
            ChegarAoEstagio(2);
            var a = new ConsultarAscensao(repositorio, catalogo).Executar();
            Assert.IsFalse(a.Pode);
            Assert.AreEqual(3, a.EstagioMinimo);
            Assert.IsFalse(new Ascender(repositorio, catalogo).Executar());
            Assert.AreEqual(2, repositorio.Progresso.Estagio);
        }

        [Test]
        public void Ascender_zera_estagio_upgrades_e_ouro_e_da_almas()
        {
            ChegarAoEstagio(3);
            var p = repositorio.Progresso;
            p.ReceberOuro(1000);
            new ComprarUpgrade(repositorio, catalogo).Executar(IdsDosUpgrades.Forca, 2);
            p.AdicionarMaterial("presa", 7);
            var espada = EspadaEquipada();

            var antes = new ConsultarAscensao(repositorio, catalogo).Executar();
            Assert.IsTrue(antes.Pode);
            Assert.AreEqual(2, antes.AlmasGanhas);
            Assert.AreEqual(1.2, antes.MultiplicadorDepois, 1e-9);

            Assert.IsTrue(new Ascender(repositorio, catalogo).Executar());
            Assert.AreEqual(1, p.Estagio);
            Assert.AreEqual(0, p.VitoriasNoEstagio);
            Assert.AreEqual(0, p.Ouro, 1e-9);
            Assert.AreEqual(1, p.Nivel(IdsDosUpgrades.Forca), "upgrades voltam ao nível 1");
            Assert.AreEqual(2, p.Almas);
            Assert.AreEqual(1, p.Ascensoes);
            Assert.GreaterOrEqual(p.Material("presa"), 7, "materiais ficam");
            Assert.AreSame(espada, p.Equipado(SlotDeItem.Arma), "equipamento fica");
            Assert.Greater(p.Derrotas("lobo"), 0, "bestiário fica");
        }

        [Test]
        public void Almas_multiplicam_o_dano_do_click_e_do_auto_ataque()
        {
            ChegarAoEstagio(3);
            new Ascender(repositorio, catalogo).Executar();
            var a = new CalcularAtributos(repositorio, catalogo).Executar(null);
            double bestiario = RegrasDoBestiario.MultiplicadorGeral(catalogo.Bestiario, new[] { repositorio.Progresso.Derrotas("lobo") });
            Assert.AreEqual(3 * 1.2 * bestiario * RegrasDoBestiario.MultiplicadorContraTipo(catalogo.Bestiario, 0), a.DanoDoClick, 1e-9);
            Assert.AreEqual(5 * 1.2 * bestiario * RegrasDoBestiario.MultiplicadorContraTipo(catalogo.Bestiario, 0), a.DanoAutomatico, 1e-9);
        }

        [Test]
        public void Ascensao_disponivel_vira_pendencia()
        {
            ChegarAoEstagio(3);
            var lista = new List<Pendencia>();
            new ListarPendencias(repositorio, catalogo).Executar(lista);
            CollectionAssert.Contains(lista.ConvertAll(x => x.Tipo), TipoDePendencia.Ascensao);
        }

        SituacaoDaConquista Conquista(string id) =>
            new ListarConquistas(repositorio, catalogo).Executar().Find(c => c.Id == id);

        [Test]
        public void Conquista_mostra_progresso_e_fica_pronta_ao_passar_do_degrau()
        {
            var c = Conquista("vitorias");
            Assert.AreEqual(0, c.Valor, 1e-9);
            Assert.AreEqual(1, c.ProximaMeta, 1e-9);
            Assert.AreEqual(0, c.Prontas);
            Assert.AreEqual("Vença 1 batalhas", c.Texto);

            new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", false);
            Assert.AreEqual(1, Conquista("vitorias").Prontas);
        }

        [Test]
        public void Coletar_da_cristais_e_um_por_cento_de_dano_por_degrau()
        {
            var vitoria = new RegistrarVitoria(repositorio, catalogo, SemDrop);
            for (int i = 0; i < 3; i++) vitoria.Executar("lobo", false);
            double antes = new CalcularAtributos(repositorio, catalogo).Executar(null).DanoAutomatico;

            Assert.AreEqual(10 + 40, new ColetarConquista(repositorio, catalogo).Executar("vitorias"));
            Assert.AreEqual(50, repositorio.Progresso.Cristais);
            var c = Conquista("vitorias");
            Assert.AreEqual(0, c.Prontas);
            Assert.AreEqual(2, c.Coletadas);
            Assert.IsTrue(c.Completa);
            Assert.AreEqual(antes * 1.02, new CalcularAtributos(repositorio, catalogo).Executar(null).DanoAutomatico, 1e-9);
        }

        [Test]
        public void Coletar_sem_nada_pronto_nao_da_nada()
        {
            Assert.AreEqual(0, new ColetarConquista(repositorio, catalogo).Executar("vitorias"));
            Assert.AreEqual(0, new ColetarConquista(repositorio, catalogo).Executar("nao_existe"));
            Assert.AreEqual(0, repositorio.Progresso.Cristais);
        }

        [Test]
        public void Chefe_derrotado_conta_para_a_conquista_de_chefes()
        {
            new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", true);
            Assert.AreEqual(1, Conquista("chefes").Prontas);
        }

        [Test]
        public void Conquista_pronta_vira_pendencia()
        {
            new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", false);
            var lista = new List<Pendencia>();
            new ListarPendencias(repositorio, catalogo).Executar(lista);
            CollectionAssert.Contains(lista.ConvertAll(x => x.Tipo), TipoDePendencia.Conquista);
        }

        // Gacha: um follower comum (o Pip, que o jogador já tem), um comum novo e um épico
        void ElencoDoGacha()
        {
            catalogo.Followers = new List<DefinicaoDeFollower>
            {
                new DefinicaoDeFollower { Id = Progresso.FollowerPadrao, Nome = "Pip", Raridade = Raridade.Comum, FracaoDoAutoAtaque = 0.2, IntervaloDeAtaque = 1.5f, Efeito = EfeitoDoFollower.PontoFraco, ValorDoEfeito = 0.3 },
                new DefinicaoDeFollower { Id = "lontra", Nome = "Lontra", Raridade = Raridade.Comum, FracaoDoAutoAtaque = 0.2, IntervaloDeAtaque = 1.5f, Efeito = EfeitoDoFollower.Ouro, ValorDoEfeito = 0.5 },
                new DefinicaoDeFollower { Id = "dragao", Nome = "Dragão", Raridade = Raridade.Epico, FracaoDoAutoAtaque = 0.4, IntervaloDeAtaque = 1.5f, Efeito = EfeitoDoFollower.Critico, ValorDoEfeito = 0.1 },
            };
        }

        [Test]
        public void Jogador_comeca_com_o_pip_ativo_com_uma_estrela()
        {
            Assert.AreEqual(1, repositorio.Progresso.Estrelas(Progresso.FollowerPadrao));
            Assert.AreEqual(0, repositorio.Progresso.Estrelas("lontra"));
        }

        [Test]
        public void Invocar_gasta_cristais_e_entrega_o_follower_sorteado()
        {
            ElencoDoGacha();
            repositorio.Progresso.ReceberCristais(100);
            // raridade comum (0,0), segundo comum da lista (0,6 dá a lontra)
            var r = new Invocar(repositorio, catalogo, new SorteioEmSequencia(0.0, 0.6)).Executar(1);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual("lontra", r[0].FollowerId);
            Assert.IsTrue(r[0].Novo);
            Assert.AreEqual(0, repositorio.Progresso.Cristais);
            Assert.AreEqual(1, repositorio.Progresso.Estrelas("lontra"));
        }

        [Test]
        public void Sem_cristais_nao_invoca()
        {
            ElencoDoGacha();
            repositorio.Progresso.ReceberCristais(99);
            Assert.IsEmpty(new Invocar(repositorio, catalogo, SemDrop).Executar(1));
            Assert.AreEqual(99, repositorio.Progresso.Cristais);
        }

        [Test]
        public void Dez_invocacoes_custam_novecentos()
        {
            ElencoDoGacha();
            repositorio.Progresso.ReceberCristais(900);
            var r = new Invocar(repositorio, catalogo, new SorteioFixo { Valor = 0.0 }).Executar(10);
            Assert.AreEqual(10, r.Count);
            // gastou os 900; só sobra o que voltou dos repetidos com estrelas no máximo
            int devolvidos = 0;
            foreach (var x in r) devolvidos += x.CristaisDevolvidos;
            Assert.AreEqual(devolvidos, repositorio.Progresso.Cristais);
        }

        [Test]
        public void Repetido_vira_estrela_e_no_maximo_vira_cristais()
        {
            ElencoDoGacha();
            repositorio.Progresso.ReceberCristais(600);
            var invocar = new Invocar(repositorio, catalogo, new SorteioFixo { Valor = 0.0 });   // sempre o Pip
            for (int i = 0; i < 4; i++) Assert.IsFalse(invocar.Executar(1)[0].Novo);
            Assert.AreEqual(5, repositorio.Progresso.Estrelas(Progresso.FollowerPadrao));
            var extra = invocar.Executar(1)[0];
            Assert.AreEqual(50, extra.CristaisDevolvidos);
            Assert.AreEqual(5, repositorio.Progresso.Estrelas(Progresso.FollowerPadrao));
        }

        [Test]
        public void Pity_garante_epico_na_trigesima()
        {
            ElencoDoGacha();
            repositorio.Progresso.ReceberCristais(30 * 100);
            var invocar = new Invocar(repositorio, catalogo, new SorteioFixo { Valor = 0.0 });
            for (int i = 0; i < 29; i++) Assert.AreEqual(Raridade.Comum, invocar.Executar(1)[0].Raridade);
            var trigesima = invocar.Executar(1)[0];
            Assert.AreEqual(Raridade.Epico, trigesima.Raridade);
            Assert.AreEqual("dragao", trigesima.FollowerId);
        }

        [Test]
        public void So_pode_escolher_follower_que_tem()
        {
            ElencoDoGacha();
            var escolher = new EscolherFollower(repositorio, catalogo);
            Assert.IsFalse(escolher.Executar("dragao"));
            repositorio.Progresso.ReceberFollower("dragao", 5);
            Assert.IsTrue(escolher.Executar("dragao"));
            Assert.AreEqual("dragao", repositorio.Progresso.FollowerAtivoId);
        }

        [Test]
        public void Efeito_do_follower_ativo_entra_nos_atributos()
        {
            ElencoDoGacha();
            repositorio.Progresso.ReceberFollower("dragao", 5);
            repositorio.Progresso.ReceberFollower("dragao", 5);   // 2 estrelas
            new EscolherFollower(repositorio, catalogo).Executar("dragao");
            var a = new CalcularAtributos(repositorio, catalogo).Executar(null);
            Assert.AreEqual(0.05 + 0.1 * 1.2, a.ChanceCritica, 1e-9);
            Assert.AreEqual(5 * 0.4 * 1.2, a.DanoDoFollower, 1e-9);
            Assert.AreEqual(0, a.BonusNoPontoFraco, 1e-9);
        }

        [Test]
        public void Follower_de_ouro_aumenta_o_ouro_da_vitoria()
        {
            ElencoDoGacha();
            repositorio.Progresso.ReceberFollower("lontra", 5);
            new EscolherFollower(repositorio, catalogo).Executar("lontra");
            var r = new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", false);
            Assert.AreEqual(50 * 1.5, r.Ouro, 1e-9);
        }

        [Test]
        public void Chefe_da_cristais()
        {
            new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", true);
            Assert.AreEqual(25, repositorio.Progresso.Cristais);
        }

        [Test]
        public void Ascensao_da_cristais_pela_base_e_pelas_almas()
        {
            ChegarAoEstagio(3);   // 2 Almas
            new Ascender(repositorio, catalogo).Executar();
            Assert.AreEqual(200 + 2 * 5, repositorio.Progresso.Cristais);
        }

        [Test]
        public void Afk_da_um_cristal_a_cada_vinte_vitorias()
        {
            catalogo.ParametrosDosItens.ChanceDeDrop = 0;
            var (relogio, consultar, coletar) = Afk();
            new RegistrarPresenca(repositorio, catalogo, relogio).Executar();
            relogio.Agora = relogio.Agora.AddHours(2);
            Assert.AreEqual(3, consultar.Executar().Cristais, "72 vitórias ÷ 20");
            coletar.Executar();
            Assert.AreEqual(3, repositorio.Progresso.Cristais);
        }

        [Test]
        public void Followers_possuidos_e_pity_vao_e_voltam_do_save()
        {
            var p = repositorio.Progresso;
            p.ReceberFollower("dragao", 5);
            p.RegistrarPity(7, 40);
            var copia = Progresso.Importar(p.Exportar());
            Assert.AreEqual(1, copia.Estrelas("dragao"));
            Assert.AreEqual(1, copia.Estrelas(Progresso.FollowerPadrao));
            Assert.AreEqual(7, copia.InvocacoesSemEpico);
            Assert.AreEqual(40, copia.InvocacoesSemLendario);
        }

        [Test]
        public void Chefe_da_fragmentos_de_reliquia()
        {
            new RegistrarVitoria(repositorio, catalogo, SemDrop).Executar("lobo", true);
            Assert.AreEqual(3, repositorio.Progresso.Fragmentos);
        }

        [Test]
        public void Montar_e_melhorar_reliquia_gasta_fragmentos()
        {
            var p = repositorio.Progresso;
            p.ReceberFragmentos(5);
            var melhorar = new MelhorarReliquia(repositorio, catalogo);
            Assert.IsTrue(melhorar.Executar("coroa"));
            Assert.AreEqual(1, p.NivelDaReliquia("coroa"));
            Assert.AreEqual(2, p.Fragmentos);
            Assert.IsTrue(melhorar.Executar("coroa"));   // nível 1 → 2 custa 2
            Assert.AreEqual(0, p.Fragmentos);
            Assert.IsFalse(melhorar.Executar("coroa"), "sem fragmentos");
            Assert.IsFalse(melhorar.Executar("nao_existe"));
        }

        [Test]
        public void Reliquia_no_nivel_maximo_nao_melhora()
        {
            var p = repositorio.Progresso;
            p.ReceberFragmentos(100);
            var melhorar = new MelhorarReliquia(repositorio, catalogo);
            for (int i = 0; i < 3; i++) Assert.IsTrue(melhorar.Executar("coroa"));
            Assert.IsFalse(melhorar.Executar("coroa"));
            Assert.AreEqual(3, p.NivelDaReliquia("coroa"));
        }

        [Test]
        public void Reliquias_entram_no_dano_e_no_ouro()
        {
            var p = repositorio.Progresso;
            double antes = new CalcularAtributos(repositorio, catalogo).Executar(null).DanoDoClick;
            p.ReceberFragmentos(100);
            new MelhorarReliquia(repositorio, catalogo).Executar("coroa");   // click ×2
            new MelhorarReliquia(repositorio, catalogo).Executar("bolsa");   // ouro +50%
            var a = new CalcularAtributos(repositorio, catalogo).Executar(null);
            Assert.AreEqual(antes * 2, a.DanoDoClick, 1e-9);
            Assert.AreEqual(0.5, a.BonusDeOuro, 1e-9);
        }

        [Test]
        public void Reliquias_e_fragmentos_ficam_na_ascensao_e_no_save()
        {
            ChegarAoEstagio(3);
            var p = repositorio.Progresso;
            p.ReceberFragmentos(10);
            new MelhorarReliquia(repositorio, catalogo).Executar("coroa");
            new Ascender(repositorio, catalogo).Executar();
            Assert.AreEqual(1, p.NivelDaReliquia("coroa"));
            var copia = Progresso.Importar(p.Exportar());
            Assert.AreEqual(1, copia.NivelDaReliquia("coroa"));
            Assert.AreEqual(p.Fragmentos, copia.Fragmentos);
        }

        [Test]
        public void Melhorar_predio_gasta_ouro_e_sobe_o_nivel()
        {
            var p = repositorio.Progresso;
            p.ReceberOuro(150);
            var relogio = new RelogioFalso();
            Assert.IsTrue(new MelhorarPredio(repositorio, catalogo, relogio).Executar("balcao"));
            Assert.AreEqual(1, p.NivelDoPredio("balcao"));
            Assert.AreEqual(50, p.Ouro, 1e-9);
            Assert.IsFalse(new MelhorarPredio(repositorio, catalogo, relogio).Executar("balcao"), "nível 2 custa 200");
        }

        [Test]
        public void Taverna_junta_ouro_com_o_tempo_e_coletar_entrega()
        {
            var p = repositorio.Progresso;
            var relogio = new RelogioFalso();
            p.ReceberOuro(100);
            new MelhorarPredio(repositorio, catalogo, relogio).Executar("balcao");   // 2% de 50 de ouro por vitória = 1 ouro/s
            relogio.Agora = relogio.Agora.AddSeconds(600);

            var t = new ConsultarTaverna(repositorio, catalogo, relogio).Executar();
            Assert.AreEqual(1, t.OuroPorSegundo, 1e-9);
            Assert.AreEqual(600, t.Guardado, 1e-9);

            Assert.AreEqual(600, new ColetarTaverna(repositorio, catalogo, relogio).Executar(), 1e-9);
            Assert.AreEqual(600, p.Ouro, 1e-9);
            Assert.AreEqual(0, new ConsultarTaverna(repositorio, catalogo, relogio).Executar().Guardado, 1e-9);
        }

        [Test]
        public void Melhorar_o_balcao_entrega_o_que_ja_estava_guardado()
        {
            var p = repositorio.Progresso;
            var relogio = new RelogioFalso();
            p.ReceberOuro(100);
            new MelhorarPredio(repositorio, catalogo, relogio).Executar("balcao");
            relogio.Agora = relogio.Agora.AddSeconds(300);
            p.ReceberOuro(200);
            new MelhorarPredio(repositorio, catalogo, relogio).Executar("balcao");
            Assert.AreEqual(300, p.Ouro, 1e-9, "os 300 guardados no ritmo antigo entram antes de mudar o ritmo");
        }

        [Test]
        public void Cozinha_e_adega_dao_dano_e_ouro()
        {
            var p = repositorio.Progresso;
            var relogio = new RelogioFalso();
            double antes = new CalcularAtributos(repositorio, catalogo).Executar(null).DanoAutomatico;
            p.ReceberOuro(200);
            new MelhorarPredio(repositorio, catalogo, relogio).Executar("cozinha");
            new MelhorarPredio(repositorio, catalogo, relogio).Executar("adega");
            var a = new CalcularAtributos(repositorio, catalogo).Executar(null);
            Assert.AreEqual(antes * 1.1, a.DanoAutomatico, 1e-9);
            Assert.AreEqual(0.1, a.BonusDeOuro, 1e-9);
        }

        [Test]
        public void Taverna_fica_na_ascensao_e_no_save()
        {
            ChegarAoEstagio(3);
            var p = repositorio.Progresso;
            p.ReceberOuro(100);
            new MelhorarPredio(repositorio, catalogo, new RelogioFalso()).Executar("cozinha");
            new Ascender(repositorio, catalogo).Executar();
            Assert.AreEqual(1, p.NivelDoPredio("cozinha"));
            var copia = Progresso.Importar(p.Exportar());
            Assert.AreEqual(1, copia.NivelDoPredio("cozinha"));
            Assert.AreEqual(p.UltimaColetaDaTavernaUtc, copia.UltimaColetaDaTavernaUtc);
        }

        (RelogioFalso relogio, IniciarExpedicao iniciar, ColetarExpedicao coletar, ListarExpedicoes listar) Expedicoes()
        {
            ElencoDoGacha();
            ((List<DefinicaoDeRegiao>)catalogo.Regioes)[0].Monstros.Add("lobo");
            var relogio = new RelogioFalso();
            return (relogio, new IniciarExpedicao(repositorio, catalogo, relogio), new ColetarExpedicao(repositorio, catalogo, relogio, SemDrop),
                new ListarExpedicoes(repositorio, catalogo, relogio));
        }

        [Test]
        public void Expedicao_so_aceita_follower_que_tem_e_que_nao_esta_lutando()
        {
            var (_, iniciar, _, _) = Expedicoes();
            Assert.IsFalse(iniciar.Executar("curta", Progresso.FollowerPadrao), "o Pip é o follower ativo");
            Assert.IsFalse(iniciar.Executar("curta", "lontra"), "ainda não tem a lontra");
            repositorio.Progresso.ReceberFollower("lontra", 5);
            Assert.IsTrue(iniciar.Executar("curta", "lontra"));
            Assert.IsFalse(iniciar.Executar("longa", "lontra"), "a lontra já está fora");
        }

        [Test]
        public void Vagas_de_expedicao_sao_limitadas()
        {
            var (_, iniciar, _, _) = Expedicoes();
            var p = repositorio.Progresso;
            p.ReceberFollower("lontra", 5);
            p.ReceberFollower("dragao", 5);
            p.ReceberFollower(Progresso.FollowerPadrao, 5);
            new EscolherFollower(repositorio, catalogo).Executar("dragao");
            Assert.IsTrue(iniciar.Executar("curta", "lontra"));
            Assert.IsTrue(iniciar.Executar("curta", Progresso.FollowerPadrao));
            new EscolherFollower(repositorio, catalogo).Executar("lontra");
            Assert.AreEqual("dragao", p.FollowerAtivoId, "follower em expedição não pode ser escolhido");
        }

        [Test]
        public void Coletar_antes_do_tempo_nao_da_nada()
        {
            var (relogio, iniciar, coletar, listar) = Expedicoes();
            repositorio.Progresso.ReceberFollower("lontra", 5);
            iniciar.Executar("curta", "lontra");
            relogio.Agora = relogio.Agora.AddMinutes(30);
            Assert.IsFalse(listar.Executar()[0].Pronta);
            Assert.AreEqual(1800, listar.Executar()[0].SegundosRestantes, 1e-6);
            Assert.IsNull(coletar.Executar(0));
        }

        [Test]
        public void Expedicao_pronta_da_cristais_material_e_item_e_libera_a_vaga()
        {
            var (relogio, iniciar, coletar, listar) = Expedicoes();
            var p = repositorio.Progresso;
            p.ReceberFollower("dragao", 5);   // Épico: fator 1 + 3 × 0,5 = 2,5
            iniciar.Executar("longa", "dragao");
            relogio.Agora = relogio.Agora.AddHours(8);
            Assert.IsTrue(listar.Executar()[0].Pronta);

            var r = coletar.Executar(0);
            Assert.IsNotNull(r);
            Assert.AreEqual(150, r.Value.Cristais, "60 × 2,5");
            Assert.AreEqual(150, p.Cristais);
            Assert.AreEqual((int)(90 * 2.5) * 2, p.Material("presa"), "225 lutas contra lobos, 2 presas cada");
            Assert.AreEqual(1, p.Inventario.Count, "chance de item 100%");
            Assert.IsEmpty(listar.Executar());
            Assert.IsTrue(new EscolherFollower(repositorio, catalogo).Executar("dragao"), "voltou e pode lutar");
        }

        [Test]
        public void Expedicoes_vao_e_voltam_do_save()
        {
            var (_, iniciar, _, _) = Expedicoes();
            repositorio.Progresso.ReceberFollower("lontra", 5);
            iniciar.Executar("curta", "lontra");
            var copia = Progresso.Importar(repositorio.Progresso.Exportar());
            Assert.AreEqual(1, copia.Expedicoes.Count);
            Assert.AreEqual("lontra", copia.Expedicoes[0].FollowerId);
        }

        [Test]
        public void Coletar_recurso_da_o_material_dele()
        {
            Assert.AreEqual(2, new ColetarRecurso(repositorio, catalogo).Executar("erva"));
            Assert.AreEqual(2, repositorio.Progresso.Material("erva_lunar"));
            Assert.AreEqual(1, repositorio.Progresso.RecursosColetados);
            Assert.AreEqual(1, repositorio.Salvamentos);
            Assert.AreEqual(0, new ColetarRecurso(repositorio, catalogo).Executar("nao_existe"));
        }

        [Test]
        public void Nome_do_material_da_coleta_aparece_pelo_catalogo()
        {
            Assert.AreEqual("Erva-lunar", catalogo.NomeDoMaterial("erva_lunar"));
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
