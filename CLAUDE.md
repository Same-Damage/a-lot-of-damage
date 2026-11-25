# A Lot of Damage

RPG clicker online para mobile (Android/iOS) e desktop, feito em Unity. Visual com personagens pintados em estilo anime (direção de arte do Legend Online) dentro de cenários 3D com pós-processamento HD-2D (como Octopath Traveler), câmera isométrica que se move pouco (como Lost Ark), campanha infinita com o mapa mudando conforme se avança (como AFK Journey) e encontros vistos no mapa que levam a uma arena de batalha (como Clair Obscur: Expedition 33). O centro do jogo é o combate clicker com números gigantes (como Cookie Clicker e Clicker Heroes). O documento de design completo está em `docs/roteiro.md` (fonte, editar este) e `docs/roteiro.pdf` (gerado dele).

## Repositório

```
Assets/
  _Project/       tudo que é nosso: scripts, cenas, prefabs, arte, dados
  (outros)        pastas do template e de pacotes; não coloque código nosso nelas
Packages/         manifest.json e packages-lock.json (dependências da Unity)
ProjectSettings/  configurações do projeto (commitadas)
docs/             roteiro e documentos de design
CLAUDE.md         este arquivo, único para o projeto inteiro
```

Repositório privado `Same-Damage/a-lot-of-damage`, branch principal `main`. Binários (imagens, áudio, modelos, fontes) vão pelo Git LFS, conforme o `.gitattributes`.

## Regras gerais

### TDD
- Escreva o teste antes da funcionalidade. O teste falha, o código o faz passar, depois refatore.
- Todo bug corrigido ganha um teste que reproduz o problema, para ele não voltar.
- Lógica (fórmulas de dano, custo, vida, marcos, AFK, gacha, combo, formatação de números, regras de encontro) fica no domínio (`Core`), em C# puro, sem `MonoBehaviour` e sem `UnityEngine` (ver Arquitetura hexagonal). O `MonoBehaviour` só lê input, chama a lógica e desenha o resultado.
- Tempo, aleatoriedade e input entram na lógica por parâmetro ou interface, para o teste controlar (nada de `Time.time` ou `Random` direto dentro da regra).
- Testes de lógica são **EditMode** (rápidos, sem cena). Fluxos que precisam de cena, física ou troca de cena são **PlayMode**.
- Os testes ficam em `ALotOfDamage.Testes.EditMode` e `ALotOfDamage.Testes.PlayMode`; assembly nova entra nas referências delas.

### Código
- Nunca duplique código. Se algo se repete, extraia para o módulo dono ou para `Core`.
- Pastas em inglês (`Core`, `Combat`, `World`, `UI`, `Tests`).
- O resto em português: nomes de arquivos, classes, métodos, variáveis, cenas, prefabs, textos da interface e nomes dos testes. Termos da Unity, do C# e de bibliotecas continuam como são (`MonoBehaviour`, `Awake`, `Update`, `ScriptableObject`).
- Namespace raiz `ALotOfDamage`, seguido da pasta (`ALotOfDamage.Combat`).
- Um módulo usa outro só pelas classes públicas dele; detalhes internos ficam `internal` ou `private`.
- Comentários só quando explicam algo que o código não mostra, em frases simples.
- Dados de balanceamento (classes, monstros, upgrades, followers) ficam em `ScriptableObject` em `Assets/_Project/Data`, não espalhados como números soltos no código.
- Segredos (chaves do servidor, tokens) nunca vão no código nem em assets commitados.

### Unity
- Todo arquivo dentro de `Assets/` tem um `.meta`: commite os dois juntos e nunca edite `.meta` à mão.
- Para mover ou renomear assets, use o editor da Unity (ou mova o `.meta` junto), senão as referências quebram.
- Não edite cenas (`.unity`) e prefabs à mão. Para montar cenas de teste, use ou amplie o `ConstrutorDeCenas` (menu **A Lot of Damage**).
- Renomear um script exige renomear a classe junto (a Unity exige o mesmo nome no arquivo e na classe) e mover o `.meta` (`git mv`), para as cenas não perderem o componente.
- Versão do editor: **6000.6.10f1**, com URP. Trocar de versão é decisão do projeto, não de um PR qualquer.
- Input pelo Input System novo (`UnityEngine.InputSystem`). Todo controle tem versão de teclado/mouse, gamepad e toque.
- Pense no celular: nada de alocação por frame no `Update`, poucos draw calls, texturas comprimidas, efeitos com preset de qualidade.

### Economia de tokens
- Durante o trabalho, rode só os testes do assembly alterado (filtro `-testFilter`). A suíte inteira roda só no final.
- Não rode build de player a cada passo, só antes do PR.
- Não leia nem busque em `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `Build/`, arquivos `.unity`, `.prefab`, `.asset` grandes, nem em arte e áudio.
- Leia `docs/roteiro.md` só quando a mudança envolver design do jogo (regras, sistemas, balanceamento).

### Problemas encontrados de passagem
- Se aparecer algo instável ou suspeito que não tem a ver com a mudança em andamento (um teste que falha e depois passa, um aviso estranho, um bug de outra cena), não corrija nesse PR: anote em `pontos-instaveis-para-verificar-depois.md`, na raiz, com onde apareceu, o que se viu, a suspeita e o próximo passo.
- Se o problema tiver a ver com a mudança em andamento, ele é parte dela: corrija no mesmo PR e não anote.
- Quando um item de `pontos-instaveis-para-verificar-depois.md` for resolvido, apague o item do arquivo na mesma correção.

### Sessões
- Pedido sem relação com o trabalho da sessão atual: sugira abrir uma sessão nova para ele em vez de fazer na mesma.

### Git
- Commits convencionais, curtos e em português: `feat: adiciona ponto fraco no combate`, `fix: corrige retorno ao mapa após fuga`, `test: cobre formatação de números`.
- Nunca adicione co-autor nos commits nem assinatura nas descrições de PR.
- Um PR por funcionalidade ou pedido, sem misturar contextos. Abra o PR ao terminar.
- Ajustes pedidos em sequência (tamanho, cor, posição, valores de balanceamento...) ficam só no código local. Commit e PR só depois que quem pediu aprovar o resultado, num commit só, e não um commit por mensagem.
- Ao terminar um conjunto de mudanças relevante (uma funcionalidade, uma correção ou um passo completo do pedido), sugira criar um commit. Pequenos ajustes não pedem essa sugestão.
- A descrição do PR é escrita para quem vai revisar o código, não como resposta para quem pediu. Escreva como um engenheiro sênior explicando a mudança de forma simples: o problema, o que mudou e por quê, como foi testado e o que precisa para rodar. Nada de falar com o leitor ("você"), recados da conversa ou instruções pessoais.
- Nada que comprometa a segurança entra em commit, mensagem, título ou descrição de PR: segredos, tokens, chaves, senhas, e-mails e ids de contas reais, endereços internos ou o passo a passo de uma falha ainda não corrigida. Em exemplos, use valores fictícios (`'...'`, `alguem@exemplo.com`).
- Cada PR parte da `main` atualizada e aponta para a `main`, nunca para a branch de outro PR. Se depender de outro PR, espere ele entrar na `main` e atualize a sua branch.

## Inspirações

O que cada jogo de referência significa para o A Lot of Damage. Quando um pedido citar um desses jogos, é disto que ele está falando. Pegamos a mecânica ou o estilo, nunca personagens, arte, música ou nomes.

| Referência | O que pegamos | O que significa no jogo |
|---|---|---|
| **Octopath Traveler II** e **Final Fantasy Resonance** | **Cenário HD-2D** | Personagens 2D dentro de cenários 3D, com profundidade de campo (bordas desfocadas), bloom e iluminação dinâmica. Pegamos o cenário e o pós-processamento; os personagens aqui são pintados (ver Legend Online), não pixel art. |
| **Lost Ark** | **Câmera isométrica** com **zona morta** | Vista de cima na diagonal, ângulo fixo, sem o jogador girar a câmera. A câmera fica parada enquanto o personagem anda dentro de uma área invisível no centro da tela (dead zone / camera window) e só acompanha quando ele chega perto da borda. Em lutas, a câmera trava no enquadramento da arena. |
| **AFK Arena** e **AFK Journey** | **Campanha infinita**, **recompensa AFK**, **mapa que evolui** e **gacha de personagens** | Estágios que nunca acabam, com dificuldade crescente e chefes periódicos. O personagem "continua lutando" offline e o jogador coleta ouro, XP e materiais ao voltar. No AFK Journey o mundo é explorável e muda conforme a campanha avança (abre caminhos, troca monstros, reconstrói vilas). O gacha aqui é só de followers e só com moeda do jogo. |
| **Clair Obscur: Expedition 33** | **Encontro visível no mapa** que leva a uma **arena** | Os monstros andam pelo mapa à vista; o jogador ataca o monstro no mapa e a luta acontece numa arena separada, voltando ao mesmo ponto depois. Acertar primeiro dá vantagem (ataque preventivo); ser pego dá vantagem ao monstro (emboscada). |
| **Cookie Clicker** e **Clicker Heroes** | **Combate clicker**, **números gigantes** e **prestígio** | Cada click/toque causa dano e dá feedback satisfatório. Dano, ouro e custos crescem de forma exponencial até notações como aa, ab... Upgrades com marcos multiplicativos e automação gradual do click. Prestígio (aqui, Ascensão): recomeçar em troca de um multiplicador permanente. |
| **Tap Titans** | **Click no mobile** | O mesmo combate clicker pensado para toque na tela do celular. |
| **Legend Online** (神曲, 7Road / Oasis Games) | **Direção de arte dos personagens, da interface e do hub**, **feedback de combate** e **painel de pendências** | Cidade-hub flutuante em isométrico de mármore e ouro com cristais azuis (a "Cidade do Céu"), que inspira o nosso lobby/taverna. Personagens pintados em estilo anime, de proporção heroica, armaduras com detalhes dourados e auras coloridas por elemento/raridade. Interface com painéis escuros e moldura dourada, ficha com boneco e slots em volta e o número de **Poder** em destaque. No combate: números de dano grandes e estilizados, nome da habilidade em faixa e contador de combo. Lista lateral de coisas prontas ("desafios grátis 10/10", filas, coletas) como dopamina fora do combate. Não pegamos nada pago (VIP, recarga, pacotes) nem o excesso de ícones. Referências em `Downloads\inspiracoes ALOT\Legend Online` (uso interno, fora do repositório). |
| **Diablo** e **Path of Exile** | Família de **ARPG isométrico** | Referência geral de ângulo de câmera, leitura de tela e loot, junto com Lost Ark. |

## Regras do jogo

Decisões já tomadas. Mudar qualquer uma delas é decisão de design, não de implementação: pergunte antes.

### Pilares
- O click tem que dar prazer: todo toque tem feedback forte (número de dano, hit stop, tremor, som, partícula, combo).
- Ficar forte é visível: números crescendo, marcos claros, o personagem mudando.
- Sempre há algo para melhorar: várias fontes de poder competem pelo tempo e pelos recursos do jogador.
- Leve: roda bem em celular médio e em PC modesto.
- **Nada se compra com dinheiro real**, nem cosmético. O jogo não tem receita. Não crie loja paga, anúncio nem moeda premium comprável.

### Combate clicker
- Quatro camadas que se somam: auto-ataque, dano por click com combo, pontos fracos temporários e barra de ultimate.
- O peso do click cai ao longo do jogo: alto no começo (~70% do dano), médio no meio, baixo no fim da campanha e irrelevante no end game, onde decidem build, auto-batalha, AFK e guilda/PvP.
- O jogador nunca é punido por parar de clicar, só deixa de ganhar o bônus.
- Limite de clicks por segundo (hoje 15/s) contra autoclicker; no online, o servidor valida.

### Números gigantes
- Dano, vida, ouro e poder crescem de forma exponencial e sem teto.
- Exibição sempre pelo `FormatadorDeNumeros`: completo até 999.999, depois K, M, B, T, depois aa, ab... zz, aaa..., com opção de notação científica.
- Custo de upgrade em progressão geométrica; dano com marcos multiplicativos (×2 a cada 10 níveis, ×4 a cada 25, ×10 a cada 100).
- Fontes de poder **se multiplicam entre si**, não somam.
- `double` por enquanto; quando passar de ~1e300, migrar para BigDouble (BreakInfinity.cs).
- A interface mostra também o ganho relativo (+38%, ×4), não só o valor absoluto.

### Mundo e encontros
- O mapa muda conforme a campanha avança. Monstros são visíveis andando no mapa; não há encontro aleatório.
- Atacar o monstro antes de ser visto = **ataque preventivo** (ultimate cheia e golpe inicial). Ser alcançado = **emboscada** (clicks travados e auto-ataque atrasado no início). Atacar um monstro que já persegue = batalha normal.
- A batalha acontece numa arena separada; ao fim, o jogador volta ao mesmo ponto do mapa, com alguns segundos sem emboscada.
- Câmera isométrica de ângulo fixo com zona morta (dead zone); na arena, enquadramento travado.

### Progressão
- Quatro classes iniciais: Guerreiro, Mago, Arqueiro e Ladino.
- **Um follower de suporte** por vez, que segue o jogador e luta junto. Pode ser pessoa, animal ou criatura mística/lendária. **Não existem pets.**
- Gacha só de followers, só com cristais ganhos jogando (conquistas, chefes, Ascensão, AFK), com pity e taxas públicas (`RegrasDoGacha`, `ParametrosDoGacha` e os 40 followers em `BalanceamentoDePersonagens`). Duplicata vira estrela (até 5, +20% cada); no máximo, devolve cristais. Cada follower tem um efeito (`EfeitoDoFollower`) que só vale enquanto ele é o ativo.
- Fontes de poder fora do combate: forja, bestiário, taverna própria, expedições, coleta no mapa, alquimia, relíquias, afinidade do follower, conquistas e Ascensão (prestígio). Um sistema novo é liberado por capítulo, na ordem do roteiro.
- Ascensão libera no estágio 50 e reinicia estágio, upgrades e ouro em troca de Almas = (estágio − 45)² ÷ 5, cada uma +10% em todo o dano, para sempre (`RegrasDaAscensao`, números em `ParametrosDaCampanha`); mantém equipamentos, materiais, relíquias, followers, bestiário, taverna e conquistas.
- **Itens**: 8 slots (Arma, Elmo, Peitoral, Luvas, Botas, Cinto, Anel, Colar) e 5 raridades (Comum a Lendário). Caem de monstros (com chance) e sempre de chefes (raridade mínima), e a forja também fabrica item do slot escolhido. O refino (+N) fica no próprio item; item novo começa em +0. Item guardado pode ser vendido (ouro) ou desmontado (material). Inventário com limite de casas; cheio, o drop é vendido na hora. Só item equipado dá bônus. Regras em `RegrasDosItens`, números em `BalanceamentoDaForja` (tipos de item e `ParametrosDosItens`).
- **Relíquias**: 12 relíquias únicas (`BalanceamentoDasReliquias`), montadas com fragmentos de chefe e melhoradas até o nível 10 (`RegrasDasReliquias`); as do mesmo alvo de dano se multiplicam, e as de ouro, AFK, crítico, drop e ponto fraco somam com o follower. Ficam na Ascensão.
- **Taverna própria**: 4 prédios (`BalanceamentoDaTaverna`) melhorados com ouro (`RegrasDaTaverna`); o Balcão gera ouro por segundo que acompanha o ouro do estágio e fica guardado até 12 h (inclusive offline), e os outros dão +dano, +AFK e +ouro. Melhorar entrega antes o que estava guardado. Fica na Ascensão.
- **Expedições**: 3 vagas e missões com horas (`DefinicaoDeMissao`, no `BalanceamentoDaTaverna`); só followers que o jogador tem, fora o ativo e os que já estão fora (`RegrasDasExpedicoes`). Voltam com cristais, material da região e itens, multiplicados pela raridade e pelas estrelas.
- **Coleta no mapa**: pontos de coleta (`DefinicaoDeRecurso`, no `BalanceamentoDaCampanha`) golpeados pelo ataque do mapa quando não há monstro perto (`PontoDeColeta`, mini-clicker); esgotado, dá o material (`RegrasDaColeta`, caso de uso `ColetarRecurso`) e rebrota. São os ingredientes da alquimia.
- **Alquimia**: poções (`DefinicaoDePocao`, no `BalanceamentoDaCampanha`) feitas com ingredientes da coleta, material de monstros e ouro proporcional ao estágio; bebidas na hora, valem por minutos (a do AFK, até a próxima coleta) (`RegrasDaAlquimia`). O `CalcularAtributos` só aplica poções quando recebe o `IRelogio` (o VContainer injeta; nos testes é opcional).
- **Conquistas**: metas de todo o jogo com degraus (`BalanceamentoDasConquistas`); cada degrau atingido é coletado na janela e dá cristais (moeda do gacha) e +1% em todo o dano, somando (`RegrasDasConquistas`). Os contadores ficam no `Progresso` e não zeram na Ascensão.
- **História e liberação**: mundo de ilhas de terra num mar (barcos e naves aéreas), Coroa Celeste partida, Rei Caído. Capítulo 1 "O Porto de Partida" (estágios 1–30, chefe final Almirante Espectral ×3). Cada sistema libera num estágio (`LiberacaoDeSistema`, caso de uso `ConsultarLiberacao`), apresentado por uma cena (`ProximaCena`, `MarcarCenaVista`, `CaixaDeDialogo`); dados em `BalanceamentoDaHistoria`. Janela nova declara o `Sistema` dela na `JanelaDoJogo`.
- Recompensa AFK com limite de 12 h acumuladas: o herói luta sozinho no estágio atual (só auto-ataque e follower) e rende 25% dessas lutas em ouro, material da região e itens (`RegrasDoAfk`, números em `ParametrosDaCampanha`). O tempo fora conta da última presença (`Progresso.UltimaPresencaUtc`, marcada pela `JanelaDaRecompensaAfk` a cada 30 s e ao pausar ou fechar); a hora entra pela porta `IRelogio`. Sem energia/stamina: o limite é só tempo e recurso.

### Online
- Backend: **Unity Gaming Services** (faixa gratuita), no lugar do Nakama planejado antes. Etapa 1 pronta no código: conta (convidado, com usuário e senha opcionais), save na nuvem e ranking; como configurar e publicar em `docs/online.md`. O servidor é o módulo C# do Cloud Code em `Assets/_Project/Server~` (a Unity ignora a pasta), que compila os mesmos arquivos do `Core`. O jogo usa sempre o save local; `SincronizarProgresso` + `RegrasDaNuvem` comparam com a nuvem e, se os dois lados mudaram, o jogador escolhe. Sem projeto vinculado ou nos testes, entra o `OnlineDesligado`.
- **Guilda**: até 20 membros, entrada por código, líder expulsa e troca o código. Um chefe por semana (vira na segunda, UTC) com fases; cada membro tem 3 lutas por dia de 60 s na arena (`LutaDaGuilda`). Só contam cliques, combo, pontos fracos e ultimates (dano relativo, sem Poder), com teto no servidor (`RegrasDaGuilda`, números em `BalanceamentoDaGuilda`). Cada fase derrubada dá cristais e fragmentos a quem lutou na semana; o servidor guarda o total entregue e o `Progresso` soma só a diferença. No servidor (`ServicoDaGuilda`), cada membro grava a própria parte do dano.
- **Arena (PvP assíncrono)**: 3 adversários perto na classificação, 5 lutas por dia; o servidor decide a luta em melhor de 5 com o Poder dos saves (`RegrasDaArena`, `ServicoDaArena`). Só quem ataca ganha pontos e perder não tira nada. Temporada de um mês; a recompensa é pela liga em que terminou. Números em `BalanceamentoDaGuilda.arena`.
- **Cidade**: cena própria (`ModeloDaCidade`, montada pelo `ConstrutorDeCenas`), liberada no estágio 5, com salas de até 20 pessoas (Multiplayer Sessions + Netcode, Distributed Authority) na assembly `ALotOfDamage.Rede` (`Scripts/Network`). `JogadorNaCidade` só carrega dados; a `CidadeOnline` desenha os outros. O que vem da rede passa por `RegrasDaCidade`. Não apague o `Online/JogadorNaCidade.prefab`: o id dele na rede mudaria.
- **Conversa**: texto por Vivox na cidade e na guilda (`ConversaDoJogador`, `RegrasDoChat`), com limite, filtro, bloqueio neste aparelho e denúncia (guardada no Cloud Save, item `denuncias-<mês>`). Texto e nome de outra pessoa nunca usam rich text. Antes de abrir ao público: regras de uso, moderação e política de privacidade.
- Ao mudar balanceamento, exporte o catálogo do servidor (menu **A Lot of Damage**) e publique o módulo. Tecla do jogo se lê por `KitDeInterface.Teclado`, que fica nulo enquanto se digita.
- Próximas etapas: NPCs e missões na cidade e co-op ao vivo.
- O servidor é a fonte da verdade: dano, gacha, AFK, ranking e recompensas são calculados ou validados nele. O cliente envia ações (ex.: "cliquei no alvo X"), não resultados.
- Por isso a lógica de regras fica em C# puro desde já: o mesmo código pode rodar no cliente e no servidor.

## Projeto Unity

### Stack
- Unity 6000.6.10f1 + URP + C#
- Input System, VContainer (injeção de dependência), uGUI (interface montada por código pelo `KitDeInterface` e controles de toque).
- Unity Test Framework (NUnit), EditMode e PlayMode

### Comandos

Unity pela linha de comando (feche o editor antes, ele não deixa duas instâncias no mesmo projeto):

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.6.10f1\Editor\Unity.exe"
$proj  = "C:\Users\vitor\Downloads\Profile\ALotOfDamage"

# testes EditMode (use -testFilter para rodar só uma parte)
& $unity -batchmode -projectPath $proj -runTests -testPlatform EditMode -testResults "$env:TEMP\editmode.xml" -logFile -

# testes PlayMode
& $unity -batchmode -projectPath $proj -runTests -testPlatform PlayMode -testResults "$env:TEMP\playmode.xml" -logFile -

# recria as cenas de protótipo, o asset de balanceamento (se faltar) e a planilha docs/balanceamento.csv
& $unity -batchmode -quit -projectPath $proj -executeMethod ALotOfDamage.Ferramentas.ExportadorDeBalanceamento.CriarTudo -logFile -
```

**CI (a fazer):** criar um GitHub Actions com o GameCI (`game-ci/unity-test-runner`) que rode EditMode e PlayMode em todo PR e bloqueie o merge se falhar, com a licença da Unity nos secrets do repositório. Assim ninguém precisa rodar a suíte na máquina a cada mudança. Até lá, os testes continuam sendo rodados localmente com os comandos acima.

A planilha `docs/balanceamento.csv` é gerada pelas mesmas fórmulas do jogo (`RegrasDeUpgrade`). Ao mudar fórmula ou valor de balanceamento, gere de novo e commite junto.

Com o editor aberto, use **Window → General → Test Runner**.

### Estrutura

```
Assets/_Project/
  Scripts/
    Core/            domínio puro, sem UnityEngine (assembly ALotOfDamage.Core)
      Ports/         interfaces que o domínio precisa (repositório, catálogo, sorteio)
      UseCases/      casos de uso: uma classe por ação do jogador
    Infrastructure/  adaptadores da Unity e composição VContainer (assembly ALotOfDamage.Infraestrutura)
    Combat/          combate clicker (MonoBehaviours)
    World/           mapa: movimento, câmera, monstros, ataque no mapa
    UI/              controles de toque e interface
    Network/         cidade online (assembly ALotOfDamage.Rede)
    Visual/          bonecos de recorte animados (assembly ALotOfDamage.Visual)
    Editor/          ferramentas do editor (ConstrutorDeCenas), assembly ALotOfDamage.Ferramentas
  Tests/
    EditMode/        domínio e casos de uso, com fakes das portas
    PlayMode/        fluxos que precisam de cena (composição, mapa ↔ batalha)
  Scenes/            Mapa (cena inicial), Batalha e Cidade
  Data/              ScriptableObjects de balanceamento
  Art/               Sprites, Environment, VFX
  Audio/
```

Pastas novas seguem esse padrão; não crie código fora de `Assets/_Project`.

### Arquitetura hexagonal

O cliente segue a arquitetura hexagonal (portas e adaptadores). O domínio fica no centro e não conhece a Unity; a Unity, os assets e (no futuro) o servidor são adaptadores em volta dele. É isso que permite rodar as mesmas regras no servidor.

| Camada | Onde | O que tem |
|---|---|---|
| Domínio | `Core/` | Regras puras (`RegrasDeUpgrade`, `RegrasDaForja`, `RegrasDosItens`, `RegrasDoBestiario`, `RegrasDoClick`, `RegrasDoEncontro`, `RegrasDePoder`, `RegrasDePersonagem`, `RegrasDaCampanha`, `RegrasDeFeedback`, `RegrasDeAnimacao`, `RegrasDoCenario`, `FormatadorDeNumeros`...), definições (`DefinicaoDeUpgrade`...) e a entidade `Progresso` |
| Portas | `Core/Ports/` | `IRepositorioDeProgresso`, `ICatalogo`, `ISorteio`, `IRelogio` |
| Casos de uso | `Core/UseCases/` | `ComprarUpgrade`, `RefinarItem`, `FabricarItem`, `EquiparItem`, `DesequiparItem`, `VenderItem`, `DesmontarItem`, `RegistrarVitoria` (com drop de item), `CalcularAtributos`, `CalcularPoder`, `CalcularInimigo`, `ConsultarCampanha`, `ListarPendencias`: carregam o progresso, aplicam a regra e salvam |
| Adaptadores de saída | `Infrastructure/` | `RepositorioDeProgressoEmArquivo` (save em JSON), `RepositorioDeProgressoEmMemoria`, `CatalogoDeBalanceamento` (lê os ScriptableObjects), `SorteioDaUnity`, `FluxoDeEncontro` (troca de cena) |
| Adaptadores de entrada | `Combat/`, `World/`, `UI/` | `MonoBehaviour`s: leem input, chamam casos de uso, desenham |
| Composição | `Infrastructure/EscopoDoJogo`, `EscopoDaCena` | VContainer monta tudo |

Regras:
- O sentido das dependências é sempre para dentro. `Core` não referencia nenhuma assembly e tem `noEngineReferences: true`: o compilador recusa `UnityEngine` ali.
- `Rede` → `Core`, `UI`, `Visual` e `World`. `Infraestrutura` → `Core`. `Visual` (bonecos animados) → só `Core`. `Combat` e `World` → `Core`, `Infraestrutura`, `UI` e `Visual`. `UI` → só `Core`. `Core` nunca conhece quem está fora.
- Toda dependência externa (salvar, assets, aleatoriedade, tempo, rede) entra no domínio por uma porta, para o teste trocar por um fake.
- Uma ação nova do jogador vira um caso de uso em `Core/UseCases`, testado em EditMode com fakes das portas. O `MonoBehaviour` só chama o caso de uso; nada de regra de negócio nele.
- Estado do jogo mora na entidade `Progresso`, acessada pelo `IRepositorioDeProgresso`. Nada de classe estática com estado global nem de singletons (`.Instance`) para serviços.
- Injeção com **VContainer**:
  - `EscopoDoJogo` é a raiz. Ele é criado antes da primeira cena, fica vivo entre as cenas e registra portas, adaptadores e casos de uso como singletons.
  - Cada cena tem um `EscopoDaCena` (o `ConstrutorDeCenas` coloca), que injeta os `MonoBehaviour`s do jogo da cena.
  - `MonoBehaviour` recebe dependências por um método `[Inject] public void Construir(...)` e só as usa a partir do `Start`, nunca no `Awake`.
  - Serviço novo é registrado no `EscopoDoJogo`.
- Valores de balanceamento ficam em assets de `Data/Resources` com o nome da classe (`BalanceamentoDeUpgrades`, `BalanceamentoDaForja`, `BalanceamentoDoBestiario`, `BalanceamentoDePersonagens`, `BalanceamentoDaCampanha`, `BalanceamentoDasConquistas`, `BalanceamentoDasReliquias`, `BalanceamentoDaTaverna`, `BalanceamentoDaGuilda`), herdando de `Balanceamento<T>`. O jogo os lê só pelo `ICatalogo`.
- O online entra por portas próprias (`IContaOnline`, `INuvemDeProgresso`, `IRanking`), com adaptadores em `Infrastructure/Online`; as regras que o servidor confere estão no `Core`.
- Nível de upgrade começa em 1; item da forja começa em +0.
- Save: o `EscopoDoJogo` usa `RepositorioDeProgressoEmArquivo` (`progresso.json` em `Application.persistentDataPath`; no editor, `progresso-editor.json`). Rodando com `-runTests`, usa o repositório em memória para os testes não mexerem no save. Campo novo no `Progresso` entra também em `DadosDoProgresso`, com valor seguro no `Importar` para saves antigos.
- Campo novo numa definição de balanceamento (`DefinicaoDeMonstro`...) não pega o valor do `Padrao()` em assets que já existem: ajuste o asset ou apague-o para o `ConstrutorDeCenas` recriar.

### Interface
- Toda a interface do jogo é uGUI montada por código com o `KitDeInterface` (painel escuro com moldura dourada, texto com sombra, botão, barra), na direção de arte do Legend Online. Nada de `OnGUI`.
- Molduras, botões e fundos de barra são sprites fatiados (9-slice) gerados por código no `TemaVisual` (moldura dourada com cravos, couro, bronze); a fonte é serifada do sistema, com a padrão como reserva. Arte de verdade entra trocando esses sprites.
- HUD limpo: só o essencial fica à vista, em placas translúcidas (`KitDeInterface.Placa`) e botões redondos (`BotaoDeIcone`, ícones em `IconesDoHud`); a moldura pintada fica para as janelas. Coisa nova entra como ícone do `MenuDoMapa` (3 principais à vista, o resto atrás de Menu), com selo de pendência, e não como painel fixo. Tecla de atalho aparece só no título da janela, e os controles de toque só em aparelho com toque.
- Janelas do mapa herdam de `JanelaDoJogo` (ícone no `MenuDoMapa`, tecla de atalho, uma aberta por vez, Esc fecha); a `Faixa` de destaque é uma por canvas; números de dano usam o pool de `NumerosFlutuantes`.
- Texto só é reescrito quando o valor muda, e nada é criado ou destruído por frame (pense no celular).
- Feedback de golpe (hit stop, tremor, partículas, som) segue a intensidade de `RegrasDeFeedback`; os sons são gerados por código até chegar o áudio de verdade.

### Arte
- O mapa é uma ilha de terra no mar (área plana, morros, praia de areia descendo até a água e o plano do mar no `NivelDoMar`, com a cor `CorDoMar` da região), montada pelo `CenarioDaRegiao` a partir do `VisualDaRegiao` de cada região (no `BalanceamentoDaCampanha`): cores, luz, névoa, quantidade de cada objeto e partículas. Onde cada objeto nasce e o relevo da borda vêm de `RegrasDoCenario` (determinístico pela semente da região). Árvores, pedras, cristais, ruínas e grama são low-poly gerados por código (`ModelosDoCenario`); os monstros também, até chegar a arte pintada: `MonstroLowPoly` monta gosma, quadrúpede ou golem pela `Forma` da `DefinicaoDeMonstro` e anima com `RegrasDeAnimacao.Monstro`, numa malha só por material; trocar por modelos de verdade é trocar esse gerador.
- **Personagens, followers e monstros**: arte 2D **pintada em estilo anime**, na direção do Legend Online (proporção heroica, armaduras com detalhes dourados, auras e brilhos como sinal de poder). Nunca copiar personagens ou arte do Legend Online: só o estilo.
- **Personagens 3D (direção nova, em transição):** imagem master no GPT (T-pose, sem acessórios soltos como arco ou capa larga, que viram objetos à parte) → modelo 3D por image-to-3D → reduzido no Blender para ~30 mil triângulos → rig e animações no Mixamo (FBX for Unity). Os arquivos ficam em `Art/Characters/<nome>/` (`<nome>.fbx`, `<nome>@Parada|Andar|Correr|Golpe|Dano|Morte.fbx`, `<nome>_Cor.png`) e o `ConstrutorDePersonagens3D` configura o humanoide, cria o Animator (árvore de locomoção por `RegrasDeAnimacao.Locomocao` + gatilhos de golpe e dano) e aplica o shader `PersonagemPintado` (luz em faixas, brilho de borda e contorno). O `PersonagemAnimado3D` gira o modelo para onde anda (`RegrasDeAnimacao.Rumo`). Um herói pode ter cabeça própria: `<nome>_Malha.asset` (submalhas corpo e cabeça, no esqueleto do FBX) e `<nome>_Cabeca_Cor.png`, com material separado (o Ladino, o Mago e o Lanceiro usam, sem contorno no rosto). Trecho e deslocamento de um clipe são ajustados em `Recortes` no `ConstrutorDePersonagens3D`; roupa solta de tecido fino usa `_RecuoDoContorno` no material. Mapa e combate falam com qualquer personagem pela `PersonagemVisual`. Todo personagem humanoide segue esse formato (hoje o Cavaleiro, a Arqueira, o Ladino, o Mago e o Lanceiro; `Correr` é opcional); os heróis ficam todos na cena (`HeroisDaCena`, lista `Herois` do `ConstrutorDeCenas`) e o jogador alterna no mapa pelo botão ou pela tecla H (`TrocaDeHeroi`); a escolha é só visual e fica salva em `Progresso.HeroiId`. Os followers (inclusive o Pip) são modelos low-poly gerados por código (`FollowerLowPoly`, uma forma por `FamiliaDoFollower` com as cores da definição e uma joia na cor da raridade a partir de Raro); o `FollowerDaCena` monta o follower ativo e troca quando o jogador escolhe outro (quem voa fica no alto, quem anda fica no chão). O boneco de recorte abaixo segue só na cena de comparação.
- **Equipamento visual dos personagens 3D**, em três camadas (decisão de design):
  - **Armas e acessórios rígidos são peças separadas** (espada, arco, escudo, aljava, elmo, ombreiras...): cada uma é um modelo 3D próprio, sem rig, em `Art/Equipment/<peça>/`, presa a um osso humanoide (`Animator.GetBoneTransform`: mãos, cabeça, peito, costas) com ajuste de posição e rotação por peça. A mesma peça serve em qualquer herói. Quem leva o quê fica na tabela `Equipamento` do `ConstrutorDePersonagens3D`, medida na pose `Parada` no referencial do corpo (peças modeladas em metros para um humano de 1,7 m, em pé no eixo y, reduzidas para ~6 mil triângulos).
  - **Roupas e armaduras de corpo são conjuntos, como skins**: cada conjunto é um modelo humanoide inteiro do herói (mesmo fluxo GPT → 3D → Mixamo), que reaproveita todas as animações pelo avatar humanoide. Trocar de conjunto é trocar o modelo; as peças da camada anterior continuam encaixando. Não fazemos roupa modular peça a peça (corpo-base + pesos transferidos) enquanto não houver artista 3D.
  - **Poder aparece por efeito**, sem modelo novo: nível da forja e raridade mudam o brilho de borda do `PersonagemPintado`, a aura, as partículas e o rastro da arma no golpe. Hoje: a arma da mão mostra o item do slot Arma (`ConsultarVisualDoHeroi`): some sem item e, com item, ganha a cor da raridade (`RegrasDosItens.CorDaRaridade`, a mesma das molduras do inventário) no brilho de borda e na emissão do `PersonagemPintado`, com força de `RegrasDeAparencia.BrilhoDoItem` (raridade + refino). Quem aplica é a `BrilhoDasArmas` em cada `PecaDeEquipamento` marcada como arma; acessórios (aljava) ficam sempre.
- Bonecos de recorte são exibidos como **billboard virado de frente para a câmera** (gira em torno dos pés), com a câmera do mapa baixa e quase sem perspectiva (inclinação 32°, campo de visão 24°), como no lobby do Legend Online, com **sombra de contato** oval no chão (`SombraDeContato`), como os personagens no lobby do Legend Online. Filtro **Bilinear**, **mipmaps ligados** (a arte aparece bem menor que o original) e compressão de qualidade alta (ASTC 6x6 no celular). Altura de referência: 512 px por personagem.
- **Animação por recorte com ossos**, feita por código (nada de Spine pago): cada personagem é entregue em partes separadas (cabeça, tronco, braços, antebraços, pernas, arma, capa), e o `montagem.json` da pasta dele diz o pivô de cada peça, onde ela encaixa no pai, a rotação de base da peça (`RotacaoDoSprite`, para peças desenhadas na diagonal) e para que lado o desenho olha (`OlhaParaDireita`; as poses são pensadas para a direita e espelhadas quando preciso). Aves (o Pip) usam a `AveDeRecorte`, que bate as asas com `RegrasDeAnimacao.Voo`. O `ConstrutorDeCenas.CriarBoneco` monta a hierarquia de ossos e o `BonecoDeRecorte` (assembly `Visual`) gira os ossos com as poses de `RegrasDeAnimacao`. Uma pose desenhada só por personagem; parado, andar, golpe e dano saem dela.
- **Cenário**: 3D low-poly com texturas pintadas, mais o pós-processamento HD-2D (depth of field, bloom, vinheta) com preset baixo para celular.
- Produção: a arte é gerada no GPT. As partes vêm em PNG com fundo transparente e são importadas em `Assets/_Project/Art/Sprites/<personagem>/`.
- Arquivos-fonte pesados (`.psd`, `.blend`) preferencialmente fora do repositório; dentro dele, só o que a Unity usa.
