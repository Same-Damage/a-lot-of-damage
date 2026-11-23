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
                    new DefinicaoDeItem { Id = "espada", Alvo = AlvoDoItem.Click, BonusPorNivel = 1, CustoBaseEmOuro = 10, MaterialId = "presa", MaterialPorNivel = 1, ChanceDeSucessoCritico = 0.1 },
                },
                Monstros = new List<DefinicaoDeMonstro>
                {
                    new DefinicaoDeMonstro { Id = "lobo", MaterialId = "presa", MaterialPorVitoria = 2, Forma = FormaDoMonstro.Quadrupede, Cor = "#9999A6" },
                },
            };
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
        public void Forjar_usa_o_sorteio_injetado()
        {
            repositorio.Progresso.ReceberOuro(10);
            repositorio.Progresso.AdicionarMaterial("presa", 1);
            var r = new ForjarItem(repositorio, catalogo, new SorteioFixo { Valor = 0.05 }).Executar("espada");
            Assert.IsTrue(r.SucessoCritico);
            Assert.AreEqual(2, repositorio.Progresso.NivelDoItem("espada"));
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Vitoria_da_ouro_material_e_conta_no_bestiario()
        {
            var r = new RegistrarVitoria(repositorio, catalogo).Executar("lobo", false);
            Assert.AreEqual(50, repositorio.Progresso.Ouro, 1e-9);
            Assert.AreEqual(2, repositorio.Progresso.Material("presa"));
            Assert.AreEqual(1, r.Derrotas);
            Assert.AreEqual(2, r.MaterialGanho);
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Vitoria_sem_tipo_so_da_ouro()
        {
            var r = new RegistrarVitoria(repositorio, catalogo).Executar(null, false);
            Assert.IsNull(r.Monstro);
            Assert.AreEqual(50, repositorio.Progresso.Ouro, 1e-9);
        }

        [Test]
        public void Atributos_multiplicam_upgrade_forja_e_bestiario()
        {
            repositorio.Progresso.ReceberOuro(10);
            repositorio.Progresso.AdicionarMaterial("presa", 1);
            new ForjarItem(repositorio, catalogo, new SorteioFixo { Valor = 0.99 }).Executar("espada"); // ×2 no click

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
            var vitoria = new RegistrarVitoria(repositorio, catalogo);
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
        public void Repositorio_em_memoria_devolve_o_mesmo_progresso()
        {
            var r = new RepositorioDeProgressoEmMemoria();
            r.Carregar().ReceberOuro(5);
            Assert.AreEqual(5, r.Carregar().Ouro, 1e-9);
        }
    }
}
