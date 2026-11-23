# A Lot of Damage

RPG clicker online para mobile (Android/iOS) e desktop, feito em Unity. Visual com personagens pintados em estilo anime (direção de arte do Legend Online) dentro de cenários 3D com pós-processamento HD-2D (como Octopath Traveler), câmera isométrica que se move pouco (como Lost Ark), campanha infinita com o mapa mudando conforme se avança (como AFK Journey) e encontros vistos no mapa que levam a uma arena de batalha (como Clair Obscur: Expedition 33). O centro do jogo é o combate clicker com números gigantes (como Cookie Clicker e Clicker Heroes). O documento de design completo está em `docs/roteiro.pdf`.

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
- Leia `docs/roteiro.pdf` só quando a mudança envolver design do jogo (regras, sistemas, balanceamento).

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
- Gacha só de followers, só com moeda ganha jogando, com pity e taxas públicas. Duplicata vira estrela/fragmento.
- Fontes de poder fora do combate: forja, bestiário, taverna própria, expedições, coleta no mapa, alquimia, relíquias, afinidade do follower, conquistas e Ascensão (prestígio). Um sistema novo é liberado por capítulo, na ordem do roteiro.
- Ascensão reinicia nível e estágios em troca de Almas (multiplicador permanente); mantém equipamentos, relíquias, followers, bestiário, taverna e conquistas.
- Recompensa AFK com limite de 12 h acumuladas. Sem energia/stamina: o limite é só tempo e recurso.

### Online (futuro)
- Backend planejado: Nakama (self-hosted) + PostgreSQL. Lobby/taverna com jogadores e NPCs, co-op, guildas, PvP assíncrono e rankings.
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
    Visual/          bonecos de recorte animados (assembly ALotOfDamage.Visual)
    Editor/          ferramentas do editor (ConstrutorDeCenas), assembly ALotOfDamage.Ferramentas
  Tests/
    EditMode/        domínio e casos de uso, com fakes das portas
    PlayMode/        fluxos que precisam de cena (composição, mapa ↔ batalha)
  Scenes/            Mapa (cena inicial) e Batalha
  Data/              ScriptableObjects de balanceamento
  Art/               Sprites, Environment, VFX
  Audio/
```

Pastas novas seguem esse padrão; não crie código fora de `Assets/_Project`.

### Arquitetura hexagonal

O cliente segue a arquitetura hexagonal (portas e adaptadores). O domínio fica no centro e não conhece a Unity; a Unity, os assets e (no futuro) o servidor são adaptadores em volta dele. É isso que permite rodar as mesmas regras no servidor.

| Camada | Onde | O que tem |
|---|---|---|
| Domínio | `Core/` | Regras puras (`RegrasDeUpgrade`, `RegrasDaForja`, `RegrasDoBestiario`, `RegrasDoClick`, `RegrasDoEncontro`, `RegrasDePoder`, `RegrasDePersonagem`, `RegrasDaCampanha`, `RegrasDeFeedback`, `RegrasDeAnimacao`, `FormatadorDeNumeros`...), definições (`DefinicaoDeUpgrade`...) e a entidade `Progresso` |
| Portas | `Core/Ports/` | `IRepositorioDeProgresso`, `ICatalogo`, `ISorteio` |
| Casos de uso | `Core/UseCases/` | `ComprarUpgrade`, `ForjarItem`, `RegistrarVitoria`, `CalcularAtributos`, `CalcularPoder`, `CalcularInimigo`, `ConsultarCampanha`, `ListarPendencias`: carregam o progresso, aplicam a regra e salvam |
| Adaptadores de saída | `Infrastructure/` | `RepositorioDeProgressoEmArquivo` (save em JSON), `RepositorioDeProgressoEmMemoria`, `CatalogoDeBalanceamento` (lê os ScriptableObjects), `SorteioDaUnity`, `FluxoDeEncontro` (troca de cena) |
| Adaptadores de entrada | `Combat/`, `World/`, `UI/` | `MonoBehaviour`s: leem input, chamam casos de uso, desenham |
| Composição | `Infrastructure/EscopoDoJogo`, `EscopoDaCena` | VContainer monta tudo |

Regras:
- O sentido das dependências é sempre para dentro. `Core` não referencia nenhuma assembly e tem `noEngineReferences: true`: o compilador recusa `UnityEngine` ali.
- `Infraestrutura` → `Core`. `Visual` (bonecos animados) → só `Core`. `Combat` e `World` → `Core`, `Infraestrutura`, `UI` e `Visual`. `UI` → só `Core`. `Core` nunca conhece quem está fora.
- Toda dependência externa (salvar, assets, aleatoriedade, tempo, rede) entra no domínio por uma porta, para o teste trocar por um fake.
- Uma ação nova do jogador vira um caso de uso em `Core/UseCases`, testado em EditMode com fakes das portas. O `MonoBehaviour` só chama o caso de uso; nada de regra de negócio nele.
- Estado do jogo mora na entidade `Progresso`, acessada pelo `IRepositorioDeProgresso`. Nada de classe estática com estado global nem de singletons (`.Instance`) para serviços.
- Injeção com **VContainer**:
  - `EscopoDoJogo` é a raiz. Ele é criado antes da primeira cena, fica vivo entre as cenas e registra portas, adaptadores e casos de uso como singletons.
  - Cada cena tem um `EscopoDaCena` (o `ConstrutorDeCenas` coloca), que injeta os `MonoBehaviour`s do jogo da cena.
  - `MonoBehaviour` recebe dependências por um método `[Inject] public void Construir(...)` e só as usa a partir do `Start`, nunca no `Awake`.
  - Serviço novo é registrado no `EscopoDoJogo`.
- Valores de balanceamento ficam em assets de `Data/Resources` com o nome da classe (`BalanceamentoDeUpgrades`, `BalanceamentoDaForja`, `BalanceamentoDoBestiario`, `BalanceamentoDePersonagens`, `BalanceamentoDaCampanha`), herdando de `Balanceamento<T>`. O jogo os lê só pelo `ICatalogo`.
- Quando chegar o servidor (Nakama), entram adaptadores novos para as mesmas portas (ex.: `RepositorioDeProgressoNoServidor`) e os casos de uso viram as ações que o cliente envia. O domínio não muda.
- Nível de upgrade começa em 1; item da forja começa em +0.
- Save: o `EscopoDoJogo` usa `RepositorioDeProgressoEmArquivo` (`progresso.json` em `Application.persistentDataPath`; no editor, `progresso-editor.json`). Rodando com `-runTests`, usa o repositório em memória para os testes não mexerem no save. Campo novo no `Progresso` entra também em `DadosDoProgresso`, com valor seguro no `Importar` para saves antigos.
- Campo novo numa definição de balanceamento (`DefinicaoDeMonstro`...) não pega o valor do `Padrao()` em assets que já existem: ajuste o asset ou apague-o para o `ConstrutorDeCenas` recriar.

### Interface
- Toda a interface do jogo é uGUI montada por código com o `KitDeInterface` (painel escuro com moldura dourada, texto com sombra, botão, barra), na direção de arte do Legend Online. Nada de `OnGUI`.
- Molduras, botões e fundos de barra são sprites fatiados (9-slice) gerados por código no `TemaVisual` (moldura dourada com cravos, couro, bronze); a fonte é serifada do sistema, com a padrão como reserva. Arte de verdade entra trocando esses sprites.
- Janelas do mapa herdam de `JanelaDoJogo` (botão na coluna da direita, tecla de atalho, uma aberta por vez); a `Faixa` de destaque é uma por canvas; números de dano usam o pool de `NumerosFlutuantes`.
- Texto só é reescrito quando o valor muda, e nada é criado ou destruído por frame (pense no celular).
- Feedback de golpe (hit stop, tremor, partículas, som) segue a intensidade de `RegrasDeFeedback`; os sons são gerados por código até chegar o áudio de verdade.

### Arte
- **Personagens, followers e monstros**: arte 2D **pintada em estilo anime**, na direção do Legend Online (proporção heroica, armaduras com detalhes dourados, auras e brilhos como sinal de poder). Nunca copiar personagens ou arte do Legend Online: só o estilo.
- Exibidos como **billboard virado de frente para a câmera** (gira em torno dos pés), com a câmera do mapa baixa e quase sem perspectiva (inclinação 32°, campo de visão 24°), como no lobby do Legend Online, com **sombra de contato** oval no chão (`SombraDeContato`), como os personagens no lobby do Legend Online. Filtro **Bilinear**, **mipmaps ligados** (a arte aparece bem menor que o original) e compressão de qualidade alta (ASTC 6x6 no celular). Altura de referência: 512 px por personagem.
- **Animação por recorte com ossos**, feita por código (nada de Spine pago): cada personagem é entregue em partes separadas (cabeça, tronco, braços, antebraços, pernas, arma, capa), e o `montagem.json` da pasta dele diz o pivô de cada peça, onde ela encaixa no pai, a rotação de base da peça (`RotacaoDoSprite`, para peças desenhadas na diagonal) e para que lado o desenho olha (`OlhaParaDireita`; as poses são pensadas para a direita e espelhadas quando preciso). Aves (o Pip) usam a `AveDeRecorte`, que bate as asas com `RegrasDeAnimacao.Voo`. O `ConstrutorDeCenas.CriarBoneco` monta a hierarquia de ossos e o `BonecoDeRecorte` (assembly `Visual`) gira os ossos com as poses de `RegrasDeAnimacao`. Uma pose desenhada só por personagem; parado, andar, golpe e dano saem dela.
- **Cenário**: 3D low-poly com texturas pintadas, mais o pós-processamento HD-2D (depth of field, bloom, vinheta) com preset baixo para celular.
- Produção: a arte é gerada no GPT. As partes vêm em PNG com fundo transparente e são importadas em `Assets/_Project/Art/Sprites/<personagem>/`.
- Arquivos-fonte pesados (`.psd`, `.blend`) preferencialmente fora do repositório; dentro dele, só o que a Unity usa.
