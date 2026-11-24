using System;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Inventário: à esquerda o herói (vitrine 3D), os 8 slots em volta e o Poder; à direita o grid dos itens guardados.
    // Clicar num item mostra os detalhes; os botões de baixo equipam, vendem, desmontam ou organizam.
    public class JanelaDeInventario : JanelaDoJogo
    {
        const float Celula = 82, Espaco = 6;
        const int Colunas = 8;
        const float LarguraDaEsquerda = 720;

        protected override string Titulo => "Inventário";
        protected override Key Tecla => Key.I;
        protected override int Ordem => 3;
        protected override int Linhas => 0;
        protected override float AlturaDaLinha => 0;
        protected override Vector2 Tamanho => new Vector2(1520, 880);
        protected override void MontarLinha(int i, RectTransform linha) { }
        protected override void AtualizarLinha(int i) { }

        static readonly SlotDeItem[] SlotsDaEsquerda = { SlotDeItem.Arma, SlotDeItem.Elmo, SlotDeItem.Peitoral, SlotDeItem.Luvas };
        static readonly SlotDeItem[] SlotsDaDireita = { SlotDeItem.Botas, SlotDeItem.Cinto, SlotDeItem.Anel, SlotDeItem.Colar };
        static readonly string[] NomesDosSlots = { "Arma", "Elmo", "Peitoral", "Luvas", "Botas", "Cinto", "Anel", "Colar" };

        static readonly Color[] CoresDasRaridades = CoresDoDominio();

        static Color[] CoresDoDominio()
        {
            var cores = new Color[RegrasDosItens.NomesDasRaridades.Length];
            for (int i = 0; i < cores.Length; i++) ColorUtility.TryParseHtmlString(RegrasDosItens.CorDaRaridade((Raridade)i), out cores[i]);
            return cores;
        }

        CalcularPoder calcularPoder;
        ConsultarItem consultarItem;
        EquiparItem equiparItem;
        DesequiparItem desequiparItem;
        VenderItem venderItem;
        DesmontarItem desmontarItem;
        OrganizarInventario organizarInventario;

        Casa[] casas;
        Casa[] slots;
        Text poder, detalhes, ocupacao, rotuloDoEquipar;
        Button equipar, vender, desmontar;
        RawImage vitrine;
        Texture texturaDaVitrine;
        int selecionado;   // id do item selecionado (0 = nenhum)

        // A vitrine 3D do herói é desenhada fora da interface (World) e só entregue aqui como textura
        public Texture TexturaDaVitrine
        {
            set
            {
                texturaDaVitrine = value;
                if (!vitrine) return;
                vitrine.texture = value;
                vitrine.color = value ? Color.white : Color.clear;
            }
        }

        // Arrastar a vitrine gira o herói (graus de giro pedidos)
        public event Action<float> AoGirarVitrine;

        public bool EstaAberta => Aberta;

        [Inject]
        public void Construir(CalcularPoder calcularPoder, ConsultarItem consultarItem, EquiparItem equiparItem, DesequiparItem desequiparItem,
            VenderItem venderItem, DesmontarItem desmontarItem, OrganizarInventario organizarInventario)
        {
            this.calcularPoder = calcularPoder;
            this.consultarItem = consultarItem;
            this.equiparItem = equiparItem;
            this.desequiparItem = desequiparItem;
            this.venderItem = venderItem;
            this.desmontarItem = desmontarItem;
            this.organizarInventario = organizarInventario;
        }

        // Uma casa do grid ou um slot: moldura na cor da raridade, ícone e o +N
        class Casa
        {
            public Image Moldura, Fundo, Icone;
            public Text Nivel, Rotulo;
            public int ItemId = -1;
            public string TipoId;
            public Raridade Raridade;
            public int NivelMostrado = -1;
            public bool Selecionada;
        }

        protected override void MontarConteudo(RectTransform painel)
        {
            // Fundo opaco por dentro da moldura: o mapa não aparece atrás dos itens
            var miolo = KitDeInterface.Painel("Miolo", painel, new Color(0.06f, 0.045f, 0.035f, 1), false);
            var rtMiolo = (RectTransform)miolo.transform;
            KitDeInterface.Esticar(rtMiolo);
            rtMiolo.offsetMin = new Vector2(16, 16);
            rtMiolo.offsetMax = new Vector2(-16, -16);
            miolo.raycastTarget = false;
            rtMiolo.SetAsFirstSibling();   // atrás do título e do botão de fechar
            if (TemaVisual.ArtePintada("Painel")) miolo.enabled = false;

            // Esquerda: vitrine, slots e poder
            var fundoDaVitrine = KitDeInterface.Painel("Vitrine", painel, Color.clear, false);
            KitDeInterface.Ancorar((RectTransform)fundoDaVitrine.transform, new Vector2(0, 1), new Vector2(170, -90), new Vector2(380, 560));
            vitrine = KitDeInterface.Novo("Herói", fundoDaVitrine.transform).gameObject.AddComponent<RawImage>();
            KitDeInterface.Esticar(vitrine.rectTransform);
            vitrine.texture = texturaDaVitrine;
            vitrine.color = texturaDaVitrine ? Color.white : Color.clear;
            vitrine.gameObject.AddComponent<ArrastoDaVitrine>().AoArrastar = dx => AoGirarVitrine?.Invoke(-dx * 0.5f);

            slots = new Casa[NomesDosSlots.Length];
            for (int i = 0; i < 4; i++)
            {
                MontarSlot(painel, SlotsDaEsquerda[i], new Vector2(40, -100 - i * 136));
                MontarSlot(painel, SlotsDaDireita[i], new Vector2(LarguraDaEsquerda - 40 - 110, -100 - i * 136));
            }

            var faixaDoPoder = TemaVisual.Arte("FaixaDoPoder");
            if (faixaDoPoder)
            {
                var img = KitDeInterface.Novo("Faixa do Poder", painel).gameObject.AddComponent<Image>();
                img.sprite = faixaDoPoder;
                img.preserveAspect = true;
                img.raycastTarget = false;
                KitDeInterface.Ancorar(img.rectTransform, new Vector2(0, 1), new Vector2(60, -650), new Vector2(LarguraDaEsquerda - 120, 130));
            }
            poder = KitDeInterface.Texto("Poder", painel, 46, KitDeInterface.Dourado, TextAnchor.MiddleCenter, true);
            KitDeInterface.Ancorar((RectTransform)poder.transform, new Vector2(0, 1), new Vector2(0, -680), new Vector2(LarguraDaEsquerda, 70));

            // Direita: grid, ocupação, botões e detalhes
            float x0 = LarguraDaEsquerda + 40;
            int limite = catalogo.ParametrosDosItens.LimiteDoInventario;
            casas = new Casa[limite];
            for (int i = 0; i < limite; i++)
            {
                int coluna = i % Colunas, linha = i / Colunas;
                int indice = i;
                casas[i] = NovaCasa(painel, new Vector2(x0 + coluna * (Celula + Espaco), -90 - linha * (Celula + Espaco)), Celula, () => SelecionarCasa(indice));
            }
            float yDepoisDoGrid = -90 - ((limite + Colunas - 1) / Colunas) * (Celula + Espaco) - 10;

            ocupacao = KitDeInterface.Texto("Ocupação", painel, 22, KitDeInterface.TextoClaro, TextAnchor.MiddleRight);
            KitDeInterface.Ancorar((RectTransform)ocupacao.transform, new Vector2(0, 1), new Vector2(x0, yDepoisDoGrid), new Vector2(Colunas * (Celula + Espaco) - Espaco, 30));

            float yBotoes = yDepoisDoGrid - 40;
            equipar = BotaoEm(painel, "Equipar", x0, yBotoes, Equipar);
            rotuloDoEquipar = equipar.GetComponentInChildren<Text>();
            vender = BotaoEm(painel, "Vender", x0 + 176, yBotoes, Vender);
            desmontar = BotaoEm(painel, "Desmontar", x0 + 352, yBotoes, Desmontar);
            BotaoEm(painel, "Organizar", x0 + 528, yBotoes, Organizar);

            detalhes = KitDeInterface.Texto("Detalhes", painel, 22, KitDeInterface.TextoClaro, TextAnchor.UpperLeft);
            detalhes.verticalOverflow = VerticalWrapMode.Truncate;
            KitDeInterface.Ancorar((RectTransform)detalhes.transform, new Vector2(0, 1), new Vector2(x0, yBotoes - 70), new Vector2(Colunas * (Celula + Espaco), 160));
        }

        void MontarSlot(RectTransform painel, SlotDeItem slot, Vector2 posicao)
        {
            var casa = NovaCasa(painel, posicao, 110, () => SelecionarSlot(slot));
            casa.Rotulo = KitDeInterface.Texto("Slot", casa.Moldura.transform, 18, KitDeInterface.DouradoEscuro, TextAnchor.LowerCenter);
            KitDeInterface.Ancorar((RectTransform)casa.Rotulo.transform, new Vector2(0.5f, 0), new Vector2(0, -26), new Vector2(110, 24));
            casa.Rotulo.text = NomesDosSlots[(int)slot];
            slots[(int)slot] = casa;
        }

        static Casa NovaCasa(RectTransform painel, Vector2 posicao, float tamanho, Action aoClicar)
        {
            var moldura = KitDeInterface.Painel("Casa", painel, CoresDasRaridades[0], false);
            KitDeInterface.Ancorar((RectTransform)moldura.transform, new Vector2(0, 1), posicao, new Vector2(tamanho, tamanho));
            var fundo = KitDeInterface.Painel("Fundo", moldura.transform, new Color(0.08f, 0.06f, 0.05f, 1), false);
            var rtFundo = (RectTransform)fundo.transform;
            KitDeInterface.Esticar(rtFundo);
            rtFundo.offsetMin = new Vector2(3, 3);
            rtFundo.offsetMax = new Vector2(-3, -3);
            fundo.raycastTarget = false;

            var icone = KitDeInterface.Novo("Ícone", moldura.transform).gameObject.AddComponent<Image>();
            KitDeInterface.Esticar(icone.rectTransform);
            icone.rectTransform.offsetMin = new Vector2(8, 8);
            icone.rectTransform.offsetMax = new Vector2(-8, -8);
            icone.preserveAspect = true;
            icone.raycastTarget = false;

            var nivel = KitDeInterface.Texto("Nível", moldura.transform, 20, KitDeInterface.TextoClaro, TextAnchor.LowerRight, true);
            KitDeInterface.Esticar((RectTransform)nivel.transform);
            ((RectTransform)nivel.transform).offsetMax = new Vector2(-6, 0);

            var botao = moldura.gameObject.AddComponent<Button>();
            botao.transition = Selectable.Transition.None;
            botao.onClick.AddListener(() => aoClicar());
            return new Casa { Moldura = moldura, Fundo = fundo, Icone = icone, Nivel = nivel };
        }

        static Button BotaoEm(RectTransform painel, string rotulo, float x, float y, Action aoClicar)
        {
            var b = KitDeInterface.Botao(rotulo, painel, rotulo, aoClicar);
            KitDeInterface.Ancorar((RectTransform)b.transform, new Vector2(0, 1), new Vector2(x, y), new Vector2(164, 56));
            return b;
        }

        void SelecionarCasa(int indice)
        {
            var inventario = Progresso.Inventario;
            selecionado = indice < inventario.Count ? inventario[indice].Id : 0;
            AtualizarConteudo();
        }

        void SelecionarSlot(SlotDeItem slot)
        {
            selecionado = Progresso.Equipado(slot)?.Id ?? 0;
            AtualizarConteudo();
        }

        void Equipar()
        {
            var d = consultarItem.Executar(selecionado);
            if (d.Nome == null) return;
            bool ok = d.Equipado ? desequiparItem.Executar(d.Slot) : equiparItem.Executar(selecionado);
            if (!ok && d.Equipado) faixa.Mostrar("Inventário cheio", 1.2f);
            AtualizarConteudo();
        }

        void Vender()
        {
            double ouro = venderItem.Executar(selecionado);
            if (ouro <= 0) return;
            faixa.Mostrar($"+{FormatadorDeNumeros.Formatar(ouro)} ouro", 1.2f);
            selecionado = 0;
            AtualizarConteudo();
        }

        void Desmontar()
        {
            var d = consultarItem.Executar(selecionado);
            int material = desmontarItem.Executar(selecionado);
            if (material <= 0) return;
            faixa.Mostrar($"+{material} {d.Material}", 1.2f);
            selecionado = 0;
            AtualizarConteudo();
        }

        void Organizar()
        {
            organizarInventario.Executar();
            AtualizarConteudo();
        }

        protected override void AtualizarConteudo()
        {
            var p = Progresso;
            var inventario = p.Inventario;
            for (int i = 0; i < casas.Length; i++)
                Mostrar(casas[i], i < inventario.Count ? inventario[i] : null);
            for (int i = 0; i < slots.Length; i++)
                Mostrar(slots[i], p.Equipado((SlotDeItem)i));

            ocupacao.text = $"{inventario.Count}/{casas.Length}";
            poder.text = $"Poder: {FormatadorDeNumeros.Formatar(calcularPoder.Executar())}";

            var d = consultarItem.Executar(selecionado);
            if (d.Nome == null)
            {
                selecionado = 0;
                detalhes.text = "Clique num item para ver os detalhes.";
                equipar.interactable = vender.interactable = desmontar.interactable = false;
                rotuloDoEquipar.text = "Equipar";
                return;
            }
            string alvo = d.Alvo == AlvoDoItem.Click ? "dano do click" : "dano do auto-ataque";
            detalhes.text = $"<color=#{ColorUtility.ToHtmlStringRGB(CoresDasRaridades[(int)d.Raridade])}>{d.Nome} {d.NomeDaRaridade} +{d.Nivel}</color>" +
                            $"\n×{FormatadorDeNumeros.Formatar(d.Multiplicador)} no {alvo}   ({NomesDosSlots[(int)d.Slot]})" +
                            (d.Equipado ? "\nEquipado" : $"\nVende por {FormatadorDeNumeros.Formatar(d.PrecoDeVenda)} ouro · desmonta em {d.MaterialAoDesmontar} {d.Material}");
            equipar.interactable = true;
            rotuloDoEquipar.text = d.Equipado ? "Desequipar" : "Equipar";
            vender.interactable = desmontar.interactable = !d.Equipado;
        }

        // Só reescreve o que mudou (a tela atualiza várias vezes por segundo)
        void Mostrar(Casa casa, ItemDoJogador item)
        {
            int id = item?.Id ?? 0;
            bool selecionada = id != 0 && id == selecionado;
            int nivel = item?.Nivel ?? -1;
            if (casa.ItemId == id && casa.NivelMostrado == nivel && casa.Selecionada == selecionada
                && (item == null || (casa.TipoId == item.TipoId && casa.Raridade == item.Raridade))) return;

            casa.ItemId = id;
            casa.NivelMostrado = nivel;
            casa.Selecionada = selecionada;
            casa.TipoId = item?.TipoId;
            casa.Raridade = item?.Raridade ?? Raridade.Comum;

            var cor = item == null ? new Color(0.25f, 0.2f, 0.15f) : CoresDasRaridades[(int)item.Raridade];
            // moldura pintada por raridade (Resources/Interface/Moldura<Raridade>.png, ou MolduraVazia); sem ela, a cor chapada
            var arte = TemaVisual.Arte(item == null ? (casa.Rotulo ? "MolduraDoSlot" : "MolduraVazia") : "Moldura" + item.Raridade);
            casa.Moldura.sprite = arte;
            casa.Fundo.enabled = !arte;
            casa.Moldura.color = arte ? (selecionada ? new Color(1.4f, 1.4f, 1.4f) : Color.white) : (selecionada ? Color.white : cor);
            casa.Icone.enabled = item != null;
            if (item != null) casa.Icone.sprite = IconesDosItens.Do(item.TipoId, item.Slot, cor);
            casa.Nivel.text = item != null && item.Nivel > 0 ? $"+{item.Nivel}" : "";
            if (casa.Rotulo) casa.Rotulo.enabled = item == null;
        }
    }

    // Arrastar com mouse ou dedo; devolve o deslocamento horizontal em pixels
    public class ArrastoDaVitrine : MonoBehaviour, IDragHandler
    {
        public Action<float> AoArrastar;

        public void OnDrag(PointerEventData e) => AoArrastar?.Invoke(e.delta.x);
    }
}
