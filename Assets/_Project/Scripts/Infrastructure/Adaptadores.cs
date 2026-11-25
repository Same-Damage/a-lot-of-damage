using System;
using System.Collections.Generic;
using System.IO;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Progresso só em memória, até existir save local ou o servidor
    public class RepositorioDeProgressoEmMemoria : IRepositorioDeProgresso
    {
        readonly Progresso progresso = new Progresso();

        public Progresso Carregar() => progresso;

        public void Salvar(Progresso p) { }
    }

    // Progresso salvo em JSON num arquivo do dispositivo. Grava num temporário e troca, para não corromper o save.
    public class RepositorioDeProgressoEmArquivo : IRepositorioDeProgresso
    {
        readonly string caminho;
        Progresso progresso;

        public RepositorioDeProgressoEmArquivo(string caminho) => this.caminho = caminho;

        public Progresso Carregar()
        {
            if (progresso != null) return progresso;
            DadosDoProgresso dados = null;
            try
            {
                if (File.Exists(caminho)) dados = JsonUtility.FromJson<DadosDoProgresso>(File.ReadAllText(caminho));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save ilegível, começando do zero: {e.Message}");
            }
            return progresso = Progresso.Importar(dados);
        }

        public void Salvar(Progresso p)
        {
            progresso = p;
            Directory.CreateDirectory(Path.GetDirectoryName(caminho));
            string temporario = caminho + ".tmp";
            File.WriteAllText(temporario, JsonUtility.ToJson(p.Exportar()));
            if (File.Exists(caminho)) File.Delete(caminho);
            File.Move(temporario, caminho);
        }
    }

    // Catálogo lido dos ScriptableObjects de Data/Resources
    public class CatalogoDeBalanceamento : ICatalogo
    {
        public IReadOnlyList<DefinicaoDeUpgrade> Upgrades { get; }
        public IReadOnlyList<DefinicaoDeItem> Itens { get; }
        public IReadOnlyList<DefinicaoDeMonstro> Monstros { get; }
        public ParametrosDoBestiario Bestiario { get; }
        public IReadOnlyList<DefinicaoDeClasse> Classes { get; }
        public IReadOnlyList<DefinicaoDeFollower> Followers { get; }
        public ParametrosDoGacha Gacha { get; }
        public ParametrosDaCampanha Campanha { get; }
        public ParametrosDosItens ParametrosDosItens { get; }
        public IReadOnlyList<DefinicaoDeConquista> Conquistas { get; }
        public IReadOnlyList<DefinicaoDeReliquia> Reliquias { get; }
        public IReadOnlyList<DefinicaoDePredio> Predios { get; }
        public double LimiteDaTavernaEmHoras { get; }
        public IReadOnlyList<DefinicaoDeMissao> Missoes { get; }
        public int VagasDeExpedicao { get; }
        public IReadOnlyList<DefinicaoDeRecurso> Recursos { get; }
        public IReadOnlyList<DefinicaoDePocao> Pocoes { get; }
        public IReadOnlyList<LiberacaoDeSistema> Liberacoes { get; }
        public IReadOnlyList<DefinicaoDeCapitulo> Capitulos { get; }
        public ParametrosDasReliquias ParametrosDasReliquias { get; }
        public ParametrosDasConquistas ParametrosDasConquistas { get; }
        public IReadOnlyList<DefinicaoDeRegiao> Regioes { get; }
        public ParametrosDaGuilda Guilda { get; }
        public ParametrosDaArena Arena { get; }

        public CatalogoDeBalanceamento()
        {
            var bestiario = BalanceamentoDoBestiario.Carregar();
            Upgrades = BalanceamentoDeUpgrades.Carregar().upgrades;
            var forja = BalanceamentoDaForja.Carregar();
            Itens = forja.itens;
            ParametrosDosItens = forja.parametros;
            Monstros = bestiario.monstros;
            Bestiario = bestiario.parametros;
            var personagens = BalanceamentoDePersonagens.Carregar();
            Classes = personagens.classes;
            Followers = personagens.followers;
            Gacha = personagens.gacha;
            var campanha = BalanceamentoDaCampanha.Carregar();
            Campanha = campanha.parametros;
            Regioes = campanha.regioes;
            Recursos = campanha.recursos;
            Pocoes = campanha.pocoes;
            var historia = BalanceamentoDaHistoria.Carregar();
            Liberacoes = historia.liberacoes;
            Capitulos = historia.capitulos;
            var conquistas = BalanceamentoDasConquistas.Carregar();
            Conquistas = conquistas.conquistas;
            ParametrosDasConquistas = conquistas.parametros;
            var reliquias = BalanceamentoDasReliquias.Carregar();
            Reliquias = reliquias.reliquias;
            ParametrosDasReliquias = reliquias.parametros;
            var online = BalanceamentoDaGuilda.Carregar();
            Guilda = online.parametros;
            Arena = online.arena;
            var taverna = BalanceamentoDaTaverna.Carregar();
            Predios = taverna.predios;
            LimiteDaTavernaEmHoras = taverna.limiteEmHoras;
            Missoes = taverna.missoes;
            VagasDeExpedicao = taverna.vagasDeExpedicao;
        }
    }

    public class RelogioDoSistema : IRelogio
    {
        public DateTime AgoraUtc => DateTime.UtcNow;
    }

    public class SorteioDaUnity : ISorteio
    {
        public double Proximo() => UnityEngine.Random.value;
    }
}
