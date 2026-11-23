using ALotOfDamage.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ALotOfDamage.Infrastructure
{
    // Raiz da composição: serviços que vivem o jogo inteiro (progresso, catálogo, encontro, casos de uso).
    // Criada antes da primeira cena e mantida entre as trocas de cena.
    public class EscopoDoJogo : LifetimeScope
    {
        public static EscopoDoJogo Instancia { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void CriarAoIniciar()
        {
            if (Instancia) return;
            var go = new GameObject(nameof(EscopoDoJogo));
            DontDestroyOnLoad(go);
            Instancia = go.AddComponent<EscopoDoJogo>();
        }

        // No editor o save fica separado do da build
        public static string CaminhoDoSave => System.IO.Path.Combine(Application.persistentDataPath,
            Application.isEditor ? "progresso-editor.json" : "progresso.json");

        static bool RodandoTestes => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-runTests") >= 0;

        protected override void Configure(IContainerBuilder builder)
        {
            // Testes automáticos não podem mexer no save de quem está desenvolvendo
            if (RodandoTestes)
                builder.Register<IRepositorioDeProgresso, RepositorioDeProgressoEmMemoria>(Lifetime.Singleton);
            else
                builder.RegisterInstance<IRepositorioDeProgresso>(new RepositorioDeProgressoEmArquivo(CaminhoDoSave));
            builder.Register<ICatalogo, CatalogoDeBalanceamento>(Lifetime.Singleton);
            builder.Register<ISorteio, SorteioDaUnity>(Lifetime.Singleton);
            builder.Register<FluxoDeEncontro>(Lifetime.Singleton);

            builder.Register<ComprarUpgrade>(Lifetime.Singleton);
            builder.Register<ForjarItem>(Lifetime.Singleton);
            builder.Register<RegistrarVitoria>(Lifetime.Singleton);
            builder.Register<CalcularAtributos>(Lifetime.Singleton);
            builder.Register<CalcularPoder>(Lifetime.Singleton);
            builder.Register<CalcularInimigo>(Lifetime.Singleton);
            builder.Register<ConsultarCampanha>(Lifetime.Singleton);
            builder.Register<ListarPendencias>(Lifetime.Singleton);
        }
    }
}
