# Online: conta, save na nuvem e ranking

O online usa os Unity Gaming Services (UGS): Authentication, Cloud Code, Cloud Save e Leaderboards. Sem um projeto da Unity vinculado, o jogo funciona só offline e a janela **Conta** avisa isso.

## Como funciona

- O jogo sempre usa o save local. O `SincronizarProgresso` (Core) compara com a nuvem ao abrir e envia o que mudou a cada 5 minutos e ao pausar.
- O módulo C# `Nuvem` (`Assets/_Project/Server~`) compila os mesmos arquivos do `Core`, confere o progresso com `RegrasDaNuvem` e grava nos dados protegidos do Cloud Save. O cliente não grava no save.
- Se os dois lados mudaram, o jogador escolhe. O lado que perdeu fica em `<save>-backup-nuvem.json`.
- A guilda fica em itens privados do Cloud Save (`guilda-<id>` e `codigo-<código>`), que só o servidor lê e grava. A regra está em `RegrasDaGuilda` e o fluxo em `ServicoDaGuilda`.
- O ranking é pontuado pelo servidor a cada envio, por maior estágio e por Poder.

## Configurar (uma vez)

1. No [painel da Unity](https://cloud.unity.com), crie o projeto e aceite os termos. Não é preciso cadastrar pagamento para a faixa gratuita.
2. No editor: **Edit > Project Settings > Services**, vincule o projeto. O id vai para o `ProjectSettings` e pode ser commitado.
3. No painel, crie o ambiente `desenvolvimento`. O editor e as builds de desenvolvimento usam esse; as builds finais usam `production`.
4. Em **Authentication**, ligue o provedor **Username & Password** (o convidado já vem ligado).
5. Publique o módulo e os rankings em cada ambiente, pela janela **Services > Deployment** do editor: `Nuvem.ccmr`, `estagio.lb`, `poder.lb` e `arena.lb`, em `Assets/_Project/Online`. O editor precisa do SDK do .NET 10.
6. Antes de abrir ao público, publique a política de acesso `Assets/_Project/Server~/acesso.json`, para só o servidor pontuar no ranking. Com a [CLI do UGS](https://services.docs.unity.com/guides/ugs-cli/latest/general/overview/): `ugs access upsert-project-policy acesso.json`.

7. No painel, ative **Multiplayer Services** (salas da cidade) e **Vivox Voice & Text** (conversa). Não é preciso cadastrar pagamento para a faixa gratuita.

Chaves de conta de serviço e tokens da CLI ficam fora do Git.

## Conferir depois de publicar

1. No editor, abra o mapa: a janela **Conta** deve dizer "Conectado".
2. Vença uma luta, clique em **Sincronizar** e veja o dado `progresso` do jogador no painel (Cloud Save, acesso protegido).
3. Abra o **Ranking** e confira as duas abas.
4. Confirme que uma pontuação enviada direto pelo cliente é recusada (passo 6 acima).
5. Na arena, com dois jogadores sincronizados, um deve aparecer para o outro.
6. Na cidade, com o editor e um build de desenvolvimento abertos em contas diferentes, um deve ver o outro andar, e as mensagens devem chegar.
7. Crie usuário e senha e entre na conta em outro aparelho ou com outro save.

## Ao mudar o balanceamento ou o formato do save

- Use **A Lot of Damage > Exportar catálogo do servidor** e publique o módulo. Um teste EditMode falha se o catálogo estiver velho.
- Se um jogo antigo não puder sobrescrever o save novo, suba `RegrasDaNuvem.VersaoDoSave`.

## Testes do servidor

```powershell
dotnet test "Assets/_Project/Server~/Nuvem.Testes"
```

## Limites conhecidos

- Na guilda, o resultado da luta vem do cliente; o servidor corta o que 15 cliques por segundo não permitem, mas um resultado no teto pode ser forjado.
- A validação barra só o impossível (vitórias, estágios e ouro para o tempo que passou). Batalha, gacha e drop ainda são decididos no cliente, e o primeiro envio de uma conta aceita o progresso que o aparelho tiver.
- Na conversa da guilda, o canal usa o id da guilda, que só membros recebem; quem foi expulso ainda o conhece. A arena usa o Poder do save aceito pelo servidor.
- A liga entregue vem dos pontos guardados no servidor; o ranking `arena` zera no dia 1 (agendamento no `arena.lb`).
- Convidado que desinstala o jogo sem criar usuário e senha perde a conta. Não há recuperação de senha, porque o jogo não guarda e-mail.
- Apagar a conta zera os pontos e apaga o save; a linha zerada do ranking não aparece no jogo, mas removê-la do servidor exige o painel.
- Antes de publicar para o público, é preciso uma política de privacidade.
