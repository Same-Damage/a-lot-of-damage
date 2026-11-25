using System;
using System.Collections.Generic;
using ALotOfDamage.Core;
using ALotOfDamage.UI;
using ALotOfDamage.Visual;
using ALotOfDamage.World;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.Network
{
    // Cena da cidade: entra numa sala, publica onde o jogador está e desenha os outros. Sem internet, a cidade abre vazia
    public class CidadeOnline : MonoBehaviour
    {
        const int LinhasDaConversa = 5;

        public Transform jogador;
        public HeroisDaCena herois;             // de onde saem as cópias dos heróis dos outros
        public FollowerLowPoly modeloDeFollower;
        public SombraDeContato modeloDeSombra;

        IContaOnline conta;
        ICatalogo catalogo;
        IRepositorioDeProgresso repositorio;
        GuildaDoJogador guilda;
        ConversaDoJogador conversa;
        IPortaoDaCidade portao;
        EscolhaDeHeroi escolha;

        readonly SalaDaCidade sala = new SalaDaCidade();
        readonly Dictionary<JogadorNaCidade, Visitante> visitantes = new Dictionary<JogadorNaCidade, Visitante>();
        readonly List<JogadorNaCidade> sairam = new List<JogadorNaCidade>();
        RectTransform canvas;
        Canvas tela;
        Text situacao;
        readonly Text[] linhas = new Text[LinhasDaConversa];
        Camera camera_;
        Vector3 ultimaEnviada = Vector3.one * 9999;
        float ultimoEnvio, proximaPresenca;
        int pessoasMostradas = -1, conversaMostrada = -1;
        string aviso = "Entrando na cidade...";
        JogadorNaCidade meu;

        [Inject]
        public void Construir(IContaOnline conta, ICatalogo catalogo, IRepositorioDeProgresso repositorio, GuildaDoJogador guilda,
            ConversaDoJogador conversa, IPortaoDaCidade portao, EscolhaDeHeroi escolha)
        {
            this.conta = conta;
            this.catalogo = catalogo;
            this.repositorio = repositorio;
            this.guilda = guilda;
            this.conversa = conversa;
            this.portao = portao;
            this.escolha = escolha;
        }

        void Start()
        {
            // o NetworkManager se põe fora da cena ao ligar; aqui ele deve sair junto com ela
            if (NetworkManager.Singleton) SceneManager.MoveGameObjectToScene(NetworkManager.Singleton.gameObject, gameObject.scene);
            camera_ = Camera.main;
            canvas = KitDeInterface.Canvas();
            tela = canvas.GetComponent<Canvas>();
            var placa = KitDeInterface.Placa("Cidade", canvas);
            KitDeInterface.Ancorar(placa, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(520, 48));
            situacao = KitDeInterface.Texto("Situação", placa, 22, KitDeInterface.TextoClaro, TextAnchor.MiddleCenter, true);
            KitDeInterface.Esticar((RectTransform)situacao.transform);
            var voltar = new BotaoDeIcone("Voltar", canvas, IconeDoHud.Fugir, "Mapa", 72, portao.VoltarAoMapa);
            KitDeInterface.Ancorar(voltar.Rect, new Vector2(0, 1), new Vector2(124, -24), Vector2.one * 72);
            var janela = FindAnyObjectByType<JanelaDeChat>();
            var falar = new BotaoDeIcone("Conversa", canvas, IconeDoHud.Conversa, "Conversa", 72, janela.Alternar);
            KitDeInterface.Ancorar(falar.Rect, new Vector2(0, 1), new Vector2(220, -24), Vector2.one * 72);
            for (int i = 0; i < LinhasDaConversa; i++)
            {
                linhas[i] = KitDeInterface.Texto("Conversa", canvas, 21, KitDeInterface.TextoClaro, TextAnchor.MiddleLeft);
                linhas[i].supportRichText = false;
                KitDeInterface.Ancorar((RectTransform)linhas[i].transform, new Vector2(0, 0), new Vector2(32, 400 + (LinhasDaConversa - 1 - i) * 30), new Vector2(900, 28));
            }
            Entrar();
        }

        async void Entrar()
        {
            if (!conta.Conectada) { aviso = "Sem conexão: a cidade está vazia."; pessoasMostradas = -1; return; }
            try { await sala.Entrar(); }
            catch (FalhaOnline e)
            {
                if (!this) return;
                aviso = e.Message;
                pessoasMostradas = -1;
                return;
            }
            if (!this) return;
            aviso = null;
            pessoasMostradas = -1;
            // sem a conversa, a cidade continua
            try { await conversa.Entrar(CanalDeChat.Cidade, sala.Id); }
            catch (FalhaOnline e) { Debug.LogWarning($"Conversa: {e.Message}"); }
            try
            {
                if (guilda.Vista == null) await guilda.Pedir(AcaoDaGuilda.Ver);
                await conversa.Entrar(CanalDeChat.Guilda, guilda.Vista.Canal);
            }
            catch (FalhaOnline e) { Debug.LogWarning($"Guilda: {e.Message}"); }
        }

        async void OnDestroy()
        {
            foreach (var v in visitantes.Values) v.Destruir();
            visitantes.Clear();
            try
            {
                await sala.Sair();
                await conversa.Entrar(CanalDeChat.Cidade, "");
            }
            catch (Exception e) { Debug.LogWarning($"Cidade: {e.GetType().Name}"); }
        }

        PresencaNaCidade Minha()
        {
            var p = repositorio.Carregar();
            return new PresencaNaCidade
            {
                JogadorId = conta.JogadorId, Nome = conta.Apelido, Guilda = guilda.Vista != null && guilda.Vista.TemGuilda ? guilda.Vista.Nome : "",
                HeroiId = escolha.Atual(herois.Nomes), FollowerId = p.FollowerAtivoId,
            };
        }

        void Update()
        {
            Publicar();
            Acompanhar();
            Mostrar();
        }

        void Publicar()
        {
            if (!meu || !meu.IsSpawned)
            {
                meu = null;
                foreach (var j in JogadorNaCidade.Presentes) if (j.EhMeu) meu = j;
                if (!meu) return;
                ultimaEnviada = Vector3.one * 9999;
                proximaPresenca = 0;
            }
            if (Time.unscaledTime >= proximaPresenca)
            {
                proximaPresenca = Time.unscaledTime + 1f;
                meu.Escrever(Minha());
            }
            if (!RegrasDaCidade.DeveEnviar(Vector3.Distance(jogador.position, ultimaEnviada), Time.unscaledTime - ultimoEnvio)) return;
            ultimaEnviada = jogador.position;
            ultimoEnvio = Time.unscaledTime;
            meu.Escrever(jogador.position);
        }

        void Acompanhar()
        {
            foreach (var j in JogadorNaCidade.Presentes)
            {
                if (j.EhMeu) continue;
                if (!visitantes.TryGetValue(j, out var v)) visitantes[j] = v = new Visitante(this, canvas);
                v.Atualizar(j, camera_);
            }
            sairam.Clear();
            foreach (var j in visitantes.Keys) if (!j || !JogadorNaCidade.Presentes.Contains(j)) sairam.Add(j);
            foreach (var j in sairam)
            {
                visitantes[j].Destruir();
                visitantes.Remove(j);
            }
        }

        void Mostrar()
        {
            int pessoas = visitantes.Count + 1;
            if (pessoas != pessoasMostradas)
            {
                pessoasMostradas = pessoas;
                situacao.text = aviso ?? (pessoas == 1 ? "Cidade · só você por enquanto" : $"Cidade · {pessoas} pessoas");
            }
            if (conversaMostrada == conversa.Versao) return;
            conversaMostrada = conversa.Versao;
            var h = conversa.Historico;
            for (int i = 0; i < LinhasDaConversa; i++)
            {
                int k = h.Count - LinhasDaConversa + i;
                linhas[i].text = k < 0 ? "" : $"{(h[k].Canal == CanalDeChat.Guilda ? "[Guilda] " : "")}{h[k].Nome}: {h[k].Texto}";
                linhas[i].color = k >= 0 && h[k].Canal == CanalDeChat.Guilda ? KitDeInterface.Verde : KitDeInterface.TextoClaro;
            }
        }

        // Outro jogador na tela: cópia do herói, follower, sombra e nome
        class Visitante
        {
            const float Velocidade = 6f;

            readonly CidadeOnline cidade;
            readonly Transform raiz;
            readonly FollowerLowPoly follower;
            readonly Transform raizDoFollower;
            readonly Text nome;
            PersonagemVisual visual;
            string heroi, textoDoNome;
            bool posicionado;
            Vector3 posicaoDoFollower;

            public Visitante(CidadeOnline cidade, RectTransform canvas)
            {
                this.cidade = cidade;
                raiz = new GameObject("Visitante").transform;
                var sombra = Instantiate(cidade.modeloDeSombra, raiz);
                sombra.transform.localPosition = new Vector3(0, 0.02f, 0);
                raizDoFollower = new GameObject("Follower").transform;
                follower = Instantiate(cidade.modeloDeFollower, raizDoFollower);
                follower.gameObject.SetActive(true);
                nome = KitDeInterface.Texto("Nome", canvas, 20, KitDeInterface.TextoClaro, TextAnchor.LowerCenter, true);
                nome.supportRichText = false;
                var rt = KitDeInterface.Ancorar((RectTransform)nome.transform, Vector2.zero, Vector2.zero, new Vector2(400, 60));
                rt.pivot = new Vector2(0.5f, 0);
                rt.SetAsFirstSibling();
            }

            public void Atualizar(JogadorNaCidade j, Camera camera)
            {
                var p = j.Presenca();
                if (p.HeroiId != heroi) TrocarHeroi(p.HeroiId);
                if (p.FollowerId != follower.IdMontado && cidade.catalogo.Follower(p.FollowerId) is DefinicaoDeFollower f) follower.Montar(f);

                var d = j.Destino;
                if (!RegrasDaCidade.Ponto(ref d.x, ref d.z)) return;
                d.y = 0;
                var antes = raiz.position;
                bool salta = !posicionado || Vector3.Distance(antes, d) > RegrasDaCidade.DistanciaDoSalto;
                // um pouco mais rápido que o herói, para alcançar sem ficar para trás
                raiz.position = salta ? d : Vector3.MoveTowards(antes, d, Velocidade * 1.15f * Time.deltaTime);
                posicionado = true;
                var velocidade = salta || Time.deltaTime <= 0 ? Vector3.zero : (raiz.position - antes) / Time.deltaTime;
                if (visual) visual.Mover(velocidade, Velocidade, Vector2.zero);
                SeguirComFollower(salta);

                string texto = string.IsNullOrEmpty(p.Nome) ? "Sem nome" : p.Nome;
                if (!string.IsNullOrEmpty(p.Guilda)) texto = p.Guilda + "\n" + texto;
                if (texto != textoDoNome) nome.text = textoDoNome = texto;
                var ponto = camera.WorldToScreenPoint(raiz.position + Vector3.up * 2.6f);
                bool visivel = ponto.z > 0;
                if (nome.enabled != visivel) nome.enabled = visivel;
                if (visivel) ((RectTransform)nome.transform).anchoredPosition = new Vector2(ponto.x, ponto.y) / cidade.tela.scaleFactor;
            }

            void TrocarHeroi(string id)
            {
                heroi = id;
                if (visual) Destroy(visual.gameObject);
                int i = 0;
                for (int k = 0; k < cidade.herois.herois.Length; k++) if (cidade.herois.Nomes[k] == id) i = k;
                var copia = Instantiate(cidade.herois.herois[i].gameObject, raiz);
                copia.transform.localPosition = Vector3.zero;
                copia.SetActive(true);
                visual = copia.GetComponent<PersonagemVisual>();
            }

            void SeguirComFollower(bool salta)
            {
                bool voa = follower.IdMontado != null && cidade.catalogo.Follower(follower.IdMontado) is DefinicaoDeFollower f && FollowerLowPoly.Voa(f.Familia);
                var lado = posicaoDoFollower - raiz.position;
                lado.y = 0;
                if (salta || lado.sqrMagnitude < 0.01f) lado = Vector3.left;
                var alvo = raiz.position + lado.normalized * 1.6f + Vector3.up * (voa ? 2.6f : 0);
                var antes = posicaoDoFollower;
                posicaoDoFollower = salta ? alvo : Vector3.Lerp(antes, alvo, 1 - Mathf.Exp(-5 * Time.deltaTime));
                raizDoFollower.position = posicaoDoFollower;
                var delta = posicaoDoFollower - antes;
                delta.y = 0;
                follower.Andando = delta.sqrMagnitude > 0.0004f;
                if (follower.Andando) follower.transform.rotation = Quaternion.Slerp(follower.transform.rotation, Quaternion.LookRotation(delta), 8f * Time.deltaTime);
            }

            public void Destruir()
            {
                if (raiz) UnityEngine.Object.Destroy(raiz.gameObject);
                if (raizDoFollower) UnityEngine.Object.Destroy(raizDoFollower.gameObject);
                if (nome) UnityEngine.Object.Destroy(nome.gameObject);
            }
        }
    }
}
