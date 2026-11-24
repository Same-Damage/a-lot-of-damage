# Arte pintada da interface

Coloque aqui os PNGs gerados (fundo transparente de verdade). O jogo usa a arte que existir e, para o que faltar, continua com o desenho feito por código. Nenhum código precisa mudar.

## Resources/Interface/

| Arquivo | Onde aparece | Como é usado |
|---|---|---|
| `Painel.png` | Fundo de todas as janelas | Esticado em 9-slice: os 12% de cada borda (cantos ornamentados) ficam fixos |
| `Botao.png`, `BotaoHover.png`, `BotaoPressionado.png` | Botões | 9-slice, como o painel |
| `FundoDaBarra.png` | Fundo das barras | 9-slice |
| `MolduraVazia.png` | Casa do inventário sem item | Inteira, quadrada |
| `MolduraDoSlot.png` | Slot de equipamento vazio (em volta do herói) | Inteira, quadrada |
| `MolduraComum.png`, `MolduraIncomum.png`, `MolduraRaro.png`, `MolduraEpico.png`, `MolduraLendario.png` | Casa e slot com item, pela raridade | Inteira, quadrada; o ícone vai por cima |
| `FaixaDoPoder.png` | Atrás do número de Poder no inventário | Inteira, mantendo a proporção |

## Resources/Icones/

Um ícone por tipo de item, com o id do tipo: `espada.png`, `elmo.png`, `peitoral.png`, `manopla.png`, `botas.png`, `cinto.png`, `anel.png`, `colar.png`. Quadrado (512 × 512), objeto centralizado.

Não precisa mudar nada na importação: o jogo aceita os arquivos como textura padrão ou como sprite.
