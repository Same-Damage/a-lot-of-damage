# A Lot of Damage

**Roteiro de Game Design e Produção**

Personagens pintados · Cenário HD-2D · Câmera isométrica · Campanha infinita · Combate clicker

Versão 0.3 — 23/11/2026. Documento vivo: atualizar conforme as decisões forem tomadas. O PDF é gerado a partir de `docs/roteiro.md`.

## O que mudou desde a v0.2

- O jogo ganhou nome: **A Lot of Damage**. Repositório privado `Same-Damage/a-lot-of-damage`.
- **Direção de arte revista:** os personagens deixam de ser pixel art e passam a ser **pintados em estilo anime**, na direção do Legend Online. O cenário continua 3D com pós-processamento HD-2D.
- **Câmera do mapa** mais baixa e quase sem perspectiva (32°, campo de visão 24°), para os personagens pintados ficarem retos e grandes na tela.
- **Chefes a cada 5 estágios** (a v0.2 dizia 10), com só o chefe avançando o estágio.
- A Fase 0 e a Fase 1 de código estão prontas. Ver **Estado atual** e **Próximos passos** no fim.

## 1. Visão geral

**Pitch:** um RPG online para mobile e desktop, com personagens pintados em estilo anime dentro de cenários 3D com pós-processamento HD-2D, câmera isométrica de pouco movimento e um combate clicker que dá satisfação a cada toque. O jogador explora um mundo que muda conforme avança numa campanha infinita, encontra monstros no mapa e entra em batalha, acompanhado por um follower de suporte. No começo o click é o coração da luta; no end game a vitória vem da build, da estratégia e da progressão AFK.

### Pilares

- **Click que dá prazer:** cada toque tem feedback forte (número de dano, hit stop, tremor, som, partícula, combo).
- **Ficar forte é visível:** números crescendo, marcos claros, o personagem mudando.
- **Sempre há algo para melhorar:** várias fontes de poder fora do combate competem pelo tempo e pelos recursos do jogador.
- **Números gigantes:** dano e poder crescendo sem limite (milhões, trilhões, aa, ab...).
- **Mundo vivo e social:** lobby/taverna com jogadores e NPCs, co-op, guildas e PvP.
- **Respeito ao jogador:** nada se compra com dinheiro real, nem cosmético. Tudo se conquista jogando.
- **Leve:** roda bem em celular médio e em PC modesto.

### Referências

| Referência | O que pegamos |
|---|---|
| Legend Online (7Road) | Direção de arte dos personagens (pintados, anime, armaduras com dourado, auras), da interface (painéis escuros com moldura dourada, Poder em destaque) e do hub (cidade flutuante). Painel de pendências. Nada do que é pago. |
| Octopath Traveler II / Final Fantasy Resonance | Cenário HD-2D: profundidade de campo, bloom, iluminação dinâmica. |
| Lost Ark | Câmera isométrica de ângulo fixo com zona morta. |
| AFK Arena / AFK Journey | Campanha infinita, recompensa AFK, mapa que muda com a campanha, gacha de personagens. |
| Clair Obscur: Expedition 33 | Monstro visível no mapa, ataque no mapa e batalha numa arena (vantagem para quem acerta primeiro). |
| Cookie Clicker / Clicker Heroes / Tap Titans | Dopamina do toque, números gigantes, marcos, prestígio, click no celular. |

## 2. Loop de gameplay

### Loop principal (minutos)

- Andar pelo mapa da região → ver um monstro → atacar no mapa → batalha na arena.
- Batalha clicker → recompensa (ouro, materiais, progresso no bestiário e no estágio) → volta ao mesmo ponto do mapa.
- Investir em upgrades, forja e outras fontes de poder → voltar mais forte → avançar estágio → o mapa muda.

### Meta loop (horas/dias)

- Melhorar personagem, equipamento e follower.
- Puxar followers no gacha com moeda do jogo.
- Coletar a recompensa AFK ao voltar ao jogo.
- Taverna/lobby: missões, conversa, grupos para co-op.
- Guilda, PvP e rankings como objetivo de longo prazo.

## 3. Mundo, mapa e câmera

### Mapa evolutivo

- O mundo é dividido em regiões que mudam com a campanha: monstros, cenário, cores, luz, névoa e partículas.
- Hoje cada região é uma **ilha de terra no mar** (morros, praia e água em volta) gerada por regras determinísticas pela semente da região (o mesmo mapa se reproduz sempre, inclusive no servidor).
- Regiões atuais (trocam a cada 10 estágios e se repetem): **Campos Verdes** (slime e lobo), **Bosque Sombrio** (lobo e golem), **Ruínas Antigas** (slime e golem).
- Monstros ficam visíveis andando pelo mapa e desviam dos obstáculos (NavMesh). Não há encontro aleatório. Derrotado, o monstro volta depois de 30 s.
- Pontos de interesse (futuro): baús, coleta, NPCs com missões, portais.

### Entrada em batalha

- Atacar o monstro no mapa leva a uma arena separada; ao fim, o jogador volta ao mesmo ponto, com alguns segundos sem emboscada.
- **Ataque preventivo:** acertar antes de ser visto dá ultimate cheia e um golpe inicial forte.
- **Emboscada:** ser alcançado trava os clicks por 1,5 s e atrasa o auto-ataque.
- Atacar um monstro que já persegue dá batalha normal.
- Futuro: monstros muito mais fracos vencidos direto no mapa, para reduzir grinding repetitivo.

### Câmera

- Isométrica de ângulo fixo, com zona morta: a câmera fica parada enquanto o personagem anda no centro da tela e só acompanha perto da borda.
- **Ângulo atual:** 32° de inclinação, campo de visão 24° (quase sem perspectiva), como no lobby do Legend Online.
- Na arena, enquadramento travado.
- Controles: teclado (WASD/setas), gamepad e joystick virtual com botão de ataque no celular.

## 4. Combate clicker

A principal feature. Quatro camadas que se somam:

| Camada | Como funciona | Por que gera dopamina |
|---|---|---|
| Auto-ataque | Personagem e follower atacam sozinhos num ritmo fixo. | Progresso mesmo sem clicar; base do end game. |
| Dano por click | Cada toque causa dano; clicks seguidos sobem um combo que multiplica o dano. | Resposta imediata: número saltando, hit stop, tremor, som. |
| Pontos fracos | Janela temporária em que o click dá muito mais dano e carrega a ultimate. | Exige atenção, recompensa precisão. |
| Ultimate | Clicks e pontos fracos enchem a barra; cheia, dispara a ultimate da classe. | Clímax da batalha, efeito forte. |

### Feedback (implementado)

- Números de dano que saltam e somem (pool fixo, sem alocação por frame); crítico, ponto fraco e ultimate com cor e tamanho próprios.
- **Hit stop** de até 0,12 s, tremor do inimigo, partículas e som, com intensidade crescente do auto-ataque até a ultimate.
- Sons gerados por código até chegar o áudio de verdade.
- Futuro: vibração no celular, marcos de combo (25, 50, 100), rastro de golpe.

### Curva de importância do click

| Fase | Peso do click | O que decide a luta |
|---|---|---|
| Início | Alto (~70% do dano) | Clicar rápido e acertar pontos fracos. |
| Meio | Médio (~40%) | Click + escolhas de skills e follower. |
| Final da campanha | Baixo (~15%) | Build e sinergias; o click acelera. |
| End game | Irrelevante | Build, auto-batalha, AFK, guilda e PvP. |

O jogador nunca é punido por parar de clicar, só deixa de ganhar o bônus. Limite de 15 clicks/s contra autoclicker; no online o servidor valida.

## 5. Personagem e progressão

### Classes

| Classe | Papel | Estilo de click | Ultimate | Estado |
|---|---|---|---|---|
| Guerreiro | Corpo a corpo, resistente | Passiva **Fúria**: +5% de dano no click a cada 10 de combo, até ×1,5 | **Golpe Devastador** (×50 o click) | Implementado |
| Mago | Dano mágico à distância | Clicks carregam elementos | Chuva de meteoros | Futuro |
| Arqueiro | Dano físico à distância | Bônus alto em pontos fracos | Saraivada de flechas | Futuro |
| Ladino | Crítico e velocidade | Combos longos, crítico alto | Golpes nas sombras | Futuro |

### Follower (suporte)

- Um follower por vez, que segue o jogador no mapa e luta junto na arena. Pode ser pessoa, animal ou criatura mística. **Não existem pets.**
- **Pip, o falcão** (implementado): ataca a cada 1,5 s com 20% do auto-ataque e deixa a janela do ponto fraco 30% mais longa. Voa ao lado do jogador batendo as asas.
- Futuro: nível, estrelas por duplicata, sinergia com classes, skill ativa e passiva por follower.

### Gacha (só com moeda do jogo) — implementado

- **40 followers** em 5 raridades (14 Comuns, 10 Incomuns, 8 Raros, 5 Épicos, 3 Lendários): companheiros (humanos, elfos, anões, goblin, robôs), bestas, aves, dragões, criaturas míticas e espíritos. O Pip vem de início.
- Cada follower ataca a cada 1,5 s com uma parte do auto-ataque (20% no Comum a 75% no Lendário) e tem **um efeito**: dano extra, ponto fraco mais longo, +ouro, +AFK, +crítico ou +drop; os raros dão efeitos maiores. Só o follower ativo dá o efeito.
- **Invocar:** 100 cristais (10 por 900). Taxas públicas: Comum 50%, Incomum 30%, Raro 14%, Épico 5%, Lendário 1%. **Pity:** Épico garantido na 30ª seguida sem Épico, Lendário na 90ª sem Lendário.
- **Repetido** vira estrela (até 5, cada uma +20% no efeito e no dano); com 5 estrelas devolve 50 cristais.
- **Cristais** (nunca com dinheiro real): conquistas, 25 por chefe, 200 + 5 por Alma na Ascensão e 1 a cada 20 vitórias do AFK.

## 6. Números gigantes

| Faixa | Exemplo exibido |
|---|---|
| Até 999.999 | 845.320 |
| Milhar a trilhão | 12,4 K · 3,51 M · 870 B · 2,2 T |
| Acima de trilhão | 4,7 aa · 120 ab · 9,9 zz · 1,3 aaa (infinito) |
| Opção nas configurações | 4,7e15 (notação científica) |

- **Upgrades:** custo em progressão geométrica (ex.: Força do Golpe 10 × 1,07^nível); dano com marcos multiplicativos (×2 a cada 10 níveis, ×4 a cada 25, ×10 a cada 100). A planilha `docs/balanceamento.csv` é gerada pelas mesmas fórmulas do jogo.
- **Fontes de poder se multiplicam entre si:** upgrades × itens equipados × bestiário contra o tipo × bestiário geral.
- **Poder Total** em destaque, com contador animado em escala logarítmica e o ganho relativo (+38%, ×4) a cada melhoria.
- Tecnicamente: `double` por enquanto; acima de ~1e300, migrar para BigDouble (BreakInfinity.cs).

## 7. Fontes de poder fora do combate

| Sistema | O que faz | Consome | Bônus | Estado |
|---|---|---|---|---|
| Upgrades | Força do Golpe, Ataque Automático, Precisão | Ouro | Dano e crítico com marcos | Implementado |
| Itens | 8 slots (Arma, Elmo, Peitoral, Luvas, Botas, Cinto, Anel, Colar), 5 raridades; caem de monstros (25%) e sempre de chefes (Incomum ou melhor); inventário de 48 casas, cheio vende o drop na hora | Batalhas | Bônus pela raridade, crescendo com o estágio em que o item caiu; Arma, Luvas e Anel no click, o resto no auto-ataque | Regras implementadas, tela de inventário a seguir |
| Forja | Refina o item (+N fica no próprio item, item novo começa em +0, sucesso crítico +2) e fabrica item novo do slot escolhido com raridade sorteada | Ouro + material do tipo do item | ×1,3 a ×1,5 por nível de refino | Implementado |
| Venda e desmonte | Item guardado vira ouro ou material da forja | Item | Abre espaço e financia a forja | Implementado |
| Bestiário | Marcos em 10, 100, 1.000 derrotas por tipo | Batalhas | +25% contra o tipo, +2% geral por marco | Implementado |
| Taverna própria | 4 prédios com nível: Balcão (2% do ouro de uma vitória por segundo, por nível, também com o jogo fechado, guardado até 12 h), Cozinha (+3% de dano), Quartos (+3% de AFK) e Adega (+3% de ouro) por nível. Fica na Ascensão | Ouro (custo geométrico) | Ouro/s e bônus permanentes | Implementado |
| Expedições | 3 vagas; followers que não estão lutando saem em Patrulha (1 h), Caçada (4 h) ou Jornada (8 h). Raridade (+50% por degrau) e estrelas aumentam o que trazem. O follower fora não pode ser o ativo | Tempo + followers | Cristais, material da região e itens | Implementado |
| Coleta no mapa | 12 pontos na ilha: moita de erva-lunar, cogumelo rubro e veio de minério celeste. O ataque do mapa golpeia o ponto (4 a 8 golpes); esgotado, dá o material (mais em estágios altos) e rebrota em 40 a 70 s | Tempo no mapa | Ingredientes da alquimia | Implementado |
| Alquimia | 5 poções preparadas e bebidas na hora: Força (×2 dano), Fortuna (+100% ouro), Sorte (+100% drop), Precisão (+10% crítico), todas por 10 min (beber de novo soma o tempo), e Sono Profundo (×2 na próxima recompensa AFK) | Ingredientes da coleta + material de monstros + ouro (vitórias do estágio) | Buffs temporários grandes | Implementado |
| Relíquias | 12 relíquias únicas, montadas com fragmentos e melhoradas até o nível 10 (custo 10 × nível); cada chefe dá 3 fragmentos, +1 a cada 10 estágios. Ficam na Ascensão | Fragmentos de chefe | Click, auto-ataque, follower (multiplicam entre si), ouro, AFK, crítico, drop e ponto fraco | Implementado |
| Afinidade do follower | Presentes e conversa | Presentes | Passivas e falas | Futuro |
| Conquistas | 9 metas com degraus (vitórias, chefes, estágio, Ascensões, itens Épicos, refino, fabricação, ouro total, marcos do bestiário); cada degrau é coletado na janela | Jogar | +1% em todo o dano por degrau e cristais (10, 40, 90, 160, 250 pelo degrau) | Implementado |
| Ascensão | Libera no estágio 50; recomeça estágio, upgrades e ouro por Almas = (estágio − 45)² ÷ 5 (estágio 50: 5, 60: 45, 80: 245). Mantém itens, materiais, bestiário e o resto | Progresso atual | Cada Alma +10% em todo o dano, para sempre | Implementado |

### O dilema "o que fazer agora?"

- Recursos compartilhados (o ouro serve a vários sistemas) e timers diferentes.
- Materiais por monstro/região: a forja manda o jogador de volta a lutar contra um tipo específico.
- **Painel "Pronto para fazer"** (implementado): chefe disponível, upgrade ou refino do item equipado ao alcance, bestiário perto do marco.
- Um sistema novo liberado por capítulo, na ordem do roteiro. Sem energia nem stamina.

## 8. Campanha infinita e AFK

- **Estágios** (implementado): 10 vitórias para avançar; vida dos monstros ×1,25 por estágio; ouro proporcional à vida.
- **Chefe a cada 5 estágios** (implementado): aparece no mapa maior e avermelhado, com ×10 de vida; só derrotá-lo avança.
- Regiões trocam a cada 10 estágios.
- **Mundo e história** (implementado no Capítulo 1): um mundo de ilhas de terra num mar sem fim. Quando a Coroa Celeste se partiu, os fragmentos caíram pelas ilhas e despertaram monstros. Ilhas próximas se alcançam de barco; as perigosas demais para o mar, só pelo céu, em naves aéreas. O Rei Caído caça os fragmentos para refazer a coroa.
- **Capítulo 1 — O Porto de Partida** (implementado, estágios 1 a 30): o herói sobrevive a um naufrágio e ajuda a Capitã Marina a consertar o navio. Termina no chefe do estágio 30, o **Almirante Espectral** (nome próprio e ×3 de vida), e no gancho da nave aérea. As cenas aparecem uma vez numa caixa de diálogo embaixo da tela (nome de quem fala; avança com clique, Enter ou A), quando o maior estágio alcançado chega ao delas (`BalanceamentoDaHistoria`, `RegrasDosCapitulos`).
- **Liberação dos sistemas** (implementado): cada sistema aparece por estágio, apresentado por uma cena: Forja 3, Inventário 5, Bestiário 7, Followers 10, Conquistas 12, Relíquias 15, Taverna 18, Expedições 21, Alquimia 24, Ascensão 50. Trancado, o ícone some do menu, a tecla não abre e as pendências dele não aparecem.
- Futuro: capítulos seguintes (nave aérea, ilhas do céu); torre infinita.
- **Recompensa AFK** (implementado): enquanto o jogador está fora, o herói luta sozinho no estágio atual só com auto-ataque e follower (cada luta dura vida ÷ dano por segundo, mais 5 s entre lutas). Rende **25%** dessas lutas em ouro, material dos monstros da região e itens pela chance de drop, até **12 h**. Ao abrir o mapa, uma janela mostra o tempo, as vitórias e o que foi ganho, com o botão Coletar. A presença é marcada a cada 30 s e ao pausar ou fechar o jogo. No online, calculada no servidor pelo tempo e pelo poder.
- **Save** (implementado): progresso salvo em JSON no dispositivo, com gravação segura (temporário + troca).

## 9. Social e online (futuro)

| Recurso | Descrição |
|---|---|
| Lobby / cidade | Hub flutuante (inspirado na Cidade do Céu do Legend Online) com taverna, ferreiro, NPCs; jogadores se veem com o follower ao lado. |
| Taverna | Missões em grupo, recrutamento, NPCs com histórias. |
| Co-op | Até 3-4 jogadores na mesma batalha; chefes com pontos fracos para cada um. |
| Guildas | Boss semanal, loja, ranking, salão. |
| PvP | Arena assíncrona contra a build salva de outro jogador, por temporada. |
| Rankings | Estágio, torre, boss de guilda, PvP. |

## 10. End game

No end game o click deixa de ser decisivo. Passam a importar: build e estratégia, auto-batalha (2×/4×), recompensa AFK, guilda e PvP.

## 11. Economia e modelo

- **Sem dinheiro real:** nada é vendido, nem cosmético. O projeto não tem receita.
- **Moedas e recursos:** Ouro (upgrades, forja), materiais por monstro (Gosma, Presa, Pedra Rúnica), e no futuro Cristais (gacha), Almas (Ascensão), Fragmentos de chefe e moeda de guilda/PvP.
- **Sumidouros:** custo crescente de upgrade e de forja, reforja, lojas rotativas.

Atenção: mesmo sem receita há custos de servidor, domínio e contas de loja (Google Play US$ 25 uma vez, Apple US$ 99/ano).

## 12. Engine

Decisão tomada: **Unity 6 (6000.6.10f1) com URP**, Personal (gratuita). Na comparação da v0.2 ficou Unity 56, Godot 54 e Unreal 37 (Unreal fora pelo peso do editor em 12 GB de RAM e do app no celular). Godot segue como plano B se os termos da Unity mudarem.

## 13. Arquitetura técnica

| Parte | Tecnologia | Responsabilidade |
|---|---|---|
| Cliente | Unity 6 + URP + C# | Render, input, interface, animação. |
| Domínio | C# puro (assembly `Core`, sem UnityEngine) | Regras de dano, custo, campanha, forja, bestiário, poder, animação. Roda igual no cliente e no servidor. |
| Casos de uso | `Core/UseCases` | Uma classe por ação do jogador (comprar, forjar, registrar vitória...). |
| Composição | VContainer | Injeção de dependência; escopo do jogo entre cenas e escopo por cena. |
| Save | JSON no dispositivo | Adaptador da porta `IRepositorioDeProgresso`; o servidor será outro adaptador. |
| Backend (futuro) | Nakama (self-hosted) + PostgreSQL | Contas, save na nuvem, chat, guildas, rankings, gacha, AFK. |
| Números gigantes | `double` (BreakInfinity.cs no futuro) | Formatação K, M, B, T, aa... |

### Princípios

- **Arquitetura hexagonal:** o domínio no centro, sem Unity; Unity, assets e servidor são adaptadores das portas `IRepositorioDeProgresso`, `ICatalogo` e `ISorteio`.
- **Servidor autoritativo** (futuro): o cliente envia ações ("cliquei no alvo X"), o servidor decide o resultado.
- **TDD:** todo bug corrigido vira teste. Hoje há mais de 150 testes EditMode (regras e casos de uso) e testes PlayMode de cena (composição, ida e volta da batalha, follower).
- **CI** em preparação: testes EditMode e PlayMode em todo PR (falta configurar a licença da Unity nos secrets).
- Balanceamento em ScriptableObjects (`Data/Resources`), lidos só pelo catálogo.

## 14. Pipeline de arte

- **Personagens, followers e monstros:** arte 2D **pintada em estilo anime**, gerada no GPT, na direção do Legend Online. Nunca copiar personagens ou arte de terceiros, só o estilo.
- **Entrega:** uma folha de personagem com o desenho inteiro e as **partes separadas** (cabeça, tronco, braços, antebraços, pernas, arma, capa) em PNG/WebP com fundo transparente. As folhas-fonte ficam em `imagens_entidades/`.
- **Montagem:** um script recorta as partes e gera o `montagem.json` (pivôs, encaixes, rotação de base, para que lado o desenho olha). O `ConstrutorDeCenas` monta a hierarquia de ossos.
- **Animação:** procedural, por código, a partir de uma pose: parado (respiração), andar (passo com perna levantada, balanço, virar como papel), golpe e dano. Aves batem as asas.
- **Exibição:** billboard de frente para a câmera, sombra de contato oval, filtro bilinear com mipmaps, ASTC 6x6 no celular.
- **Monstros:** por enquanto low-poly animados por código (gosma, quadrúpede, golem), até ganharem arte pintada.
- **Cenário:** ilha de terra low-poly com praia e mar gerada por código, com textura de chão em pixel art, luz, névoa, partículas e pós-processamento por região (desfoque desligado no celular).
- **Interface:** uGUI montada por código, com moldura dourada, botões de bronze e couro gerados por código (9-slice); arte de verdade entra trocando os sprites.

## 15. Equipe

| Papel | Responsabilidades |
|---|---|
| Programação de gameplay | Combate, câmera, movimento, interface, integração com o servidor. |
| Programação de backend | Nakama, banco, autoridade de batalha, gacha, AFK, guildas, PvP. |
| Arte (via GPT + curadoria) | Personagens, followers e monstros pintados; folhas de partes. |
| Game design | Balanceamento, fórmulas, economia, campanha. |

## 16. Roadmap

| Fase | Entrega | Estado |
|---|---|---|
| 0. Base técnica | Projeto Unity + URP, câmera com zona morta, movimento, formatação de números, testes. | Concluída |
| 1. Protótipo de diversão | Encontro no mapa, combate com 4 camadas, upgrades com marcos, forja, bestiário, Poder Total, campanha com chefes e regiões, save, feedback do golpe, interface, Guerreiro e Pip, cenário HD-2D. | Código concluído; falta validar com jogadores |
| 2. Vertical slice | Arte final dos personagens e monstros, 4 classes, gacha de followers, AFK, Ascensão, capítulo 1 com história. | Próxima |
| 3. Online | Nakama, contas, save na nuvem, lobby/taverna, chat, cálculo autoritativo. | Futuro |
| 4. Alpha | Co-op, guildas, PvP assíncrono, rankings, teste fechado. | Futuro |
| 5. Beta / lançamento | Balanceamento, otimização mobile, publicação. | Futuro |

## 17. Estado atual

O que já existe na `main` (PRs #1 a #15):

- **Mapa:** ilha de terra no mar por região, monstros low-poly animados andando com NavMesh, ataque no mapa com preventivo e emboscada, câmera isométrica com zona morta, controles de teclado, gamepad e toque.
- **Batalha:** auto-ataque, click com combo e limite de 15/s, ponto fraco, ultimate, Fúria do Guerreiro, Pip atacando junto, hit stop, partículas, sons, números de dano.
- **Progressão:** upgrades com marcos, forja com sucesso crítico, bestiário, Poder Total com ganho relativo, campanha com estágios, chefes e três regiões, save local.
- **Interface:** HUD do mapa, painel "Pronto para fazer", janelas de Upgrades, Forja e Bestiário, HUD de batalha, tudo com tema ornamentado.
- **Personagens:** Guerreiro ruivo e Pip, o falcão, pintados e animados por partes.
- **Qualidade:** arquitetura hexagonal com VContainer, 150+ testes EditMode e testes PlayMode; CI de testes aberto (PR #14), aguardando a licença da Unity nos secrets.

## 18. Próximos passos

### Agora (fechar a Fase 1)

- **Mesclar o CI (PR #14):** configurar `UNITY_LICENSE`, `UNITY_EMAIL` e `UNITY_PASSWORD` nos secrets e exigir os checks na `main`.
- **Decidir a animação do humanoide:** as partes rígidas parecem robóticas. Proposta: usar a imagem inteira com movimento procedural (respiração, balanço, avanço no golpe) e um **rastro de golpe** brilhante; animações especiais (ultimate) por quadros gerados com IA de vídeo, se valer a memória.
- **Build para teste:** gerar um build de Windows/Android e fazer o **teste com 5 pessoas por 20 minutos** (o click é gostoso? dá vontade de continuar?).
- **Arte dos monstros:** slime, lobo e golem pintados no mesmo estilo, trocando os modelos low-poly.

### Fase 2 (vertical slice)

1. **Recompensa AFK** (até 12 h). Decidir quanto do ganho ativo vai para o offline (sugestão: 50%).
2. **Ascensão.** Decidir a partir de que estágio libera (sugestão: 20, depois de passar pelas três regiões).
3. **Mago, Arqueiro e Ladino**, com escolha de classe.
4. **Gacha de followers** com cristais ganhos jogando, pity e taxas públicas.
5. **Sistemas fora do combate**, um por capítulo: taverna, expedições, coleta, alquimia, relíquias, conquistas.
6. **Capítulo 1 com história** (feito), com o mapa já como ilha de terra no mar.

### Fase 3 em diante

Nakama, contas, save na nuvem (novo adaptador da mesma porta), lobby/taverna com jogadores, guildas, PvP assíncrono e rankings.

## 19. Riscos e decisões em aberto

| Risco | Mitigação |
|---|---|
| Custo de servidor sem receita | Começar com 1 VPS barata; PvP assíncrono; poucos jogadores por instância. |
| Escopo grande para equipe pequena | Validar a diversão antes; PvP ao vivo, torre e co-op só depois. |
| Animação por partes parecer robótica | Trocar por imagem inteira + movimento procedural + efeitos (decisão pendente). |
| Arte gerada por IA inconsistente | Uma folha por personagem, pedir sempre "o mesmo personagem", recorte automático. |
| Click cansativo | Auto-ataque sempre presente, limite de clicks/s, sem punição por parar. |
| HD-2D pesado no celular | Presets de qualidade, desfoque desligado no celular, cenário em poucos draw calls. |
| Excesso de sistemas | Um sistema novo por capítulo, com recompensa imediata. |
| Números gigantes perderem sentido | Ganho relativo sempre visível; marcos a cada nova grandeza. |

### Decisões em aberto

- Animação do humanoide: partes ou imagem inteira com movimento procedural.
- Número de regiões e capítulos antes dos estágios infinitos.
- PvP ao vivo: entra ou não.
- Plataformas de PC: Steam, itch.io ou download próprio.
