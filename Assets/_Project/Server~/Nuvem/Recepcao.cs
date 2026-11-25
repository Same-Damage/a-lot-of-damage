using System;
using System.IO;
using System.Text.Json;
using ALotOfDamage.Core;

namespace ALotOfDamage.Nuvem
{
    // Parte do servidor sem a API da Unity: JSON, catálogo, Poder e a decisão de salvar (RegrasDaNuvem)
    public class Recepcao
    {
        // o limite do Cloud Save é 5 MiB; um progresso normal tem poucos KB
        public const int TamanhoMaximo = 512 * 1024;

        static readonly JsonSerializerOptions Opcoes = new JsonSerializerOptions { IncludeFields = true };
        static readonly Lazy<CatalogoEmDados> Catalogo = new Lazy<CatalogoEmDados>(LerCatalogo);

        public static ParametrosDaArena Arena => Catalogo.Value.arena;
        public static ParametrosDaGuilda Guilda => Catalogo.Value.guilda;

        public static T Ler<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json, Opcoes); }
            catch (JsonException) { return null; }
        }

        public static string Escrever<T>(T valor) => JsonSerializer.Serialize(valor, Opcoes);

        static CatalogoEmDados LerCatalogo()
        {
            using (var s = typeof(Recepcao).Assembly.GetManifestResourceStream("catalogo.json"))
            using (var r = new StreamReader(s))
                return JsonSerializer.Deserialize<CatalogoEmDados>(r.ReadToEnd(), Opcoes);
        }

        public static RespostaDoEnvio Receber(SaveNaNuvem atual, PedidoDeEnvio pedido, DateTime agoraUtc)
        {
            atual = atual ?? new SaveNaNuvem();
            if (pedido == null || pedido.Progresso == null || pedido.Progresso.Length > TamanhoMaximo)
                return new RespostaDoEnvio { Resultado = ResultadoDoEnvio.Recusado, Motivo = "pedido inválido", Nuvem = atual };
            if (pedido.Dispositivo != null && pedido.Dispositivo.Length > 60) pedido.Dispositivo = pedido.Dispositivo.Substring(0, 60);

            var novo = Ler<DadosDoProgresso>(pedido.Progresso);
            double poder = 0;
            // os dados vêm do cliente: se nem o jogo consegue ler, não são um progresso
            try { if (RegrasDaNuvem.ProblemaDosDados(novo) == null) poder = CalcularPoder.De(novo, Catalogo.Value); }
            catch (Exception e) when (e is NullReferenceException || e is ArgumentException) { novo = null; }
            if (double.IsNaN(poder) || double.IsInfinity(poder)) poder = 0;
            return RegrasDaNuvem.Receber(new ParametrosDaNuvem(), Catalogo.Value.campanha, atual, Ler<DadosDoProgresso>(atual.Progresso), pedido, novo, poder, agoraUtc);
        }
    }
}
