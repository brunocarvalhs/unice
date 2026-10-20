# Histórias (SDD): da história contada ao mapa jogável

A ideia: **as histórias que você conta viram jogos de esconde-esconde.** Cada **história** do Unice é um mapa novo mais uma narrativa
(o lugar vira o mapa, as pessoas viram os escondidos, quem procura vira "a Unice"). O fluxo é *spec-driven*: primeiro se escreve a especificação
da história (um arquivo `docs/stories/<id>.md`), depois o Claude implementa a partir dela e prova com testes que o mapa funciona.
O Claude segue este fluxo sozinho com a skill **`unice-new-story`** (`.claude/skills/unice-new-story/`): basta contar a história.

```
 você conta a história  →  1. Spec (docs/stories/<id>.md)  →  2. Design do mapa  →  3. Implementação  →  4. Verificação
                              brief, cenas, HQ, vídeo       salas, portas, esconderijos   código + textos + testes     testes Edit Mode verdes
```

## Como pedir
Conte a história do jeito que lembrar, em qualquer tamanho. Exemplos:
- "Nova história: o quintal da minha vó em Jequié. Tinha um pé de umbu, um poço e a casa de farinha. A gente se escondia da tia Nena."
- "Cria um mapa da feira de domingo: barracas de frutas, o carro de som, a igreja..."

O Claude só pergunta o que não dá para deduzir (ex.: nome da pessoa que vira "a Unice" nessa história). Não precisa saber nada de código.

## Etapa 1 — Spec da história (`docs/stories/<id>.md`)
O Claude cria o arquivo a partir de [`_template.md`](_template.md). Ele contém:
1. **Brief:** o lugar, a época, as pessoas, o que acontecia (a sua história, organizada).
2. **Lugar → mapa:** as salas/áreas (6 a 12), como se ligam, o que cada uma tem.
3. **Esconderijos:** de 10 a 20 objetos/cantos onde alguém se esconde (mínimo **8**: é o máximo de jogadores).
4. **HQ de introdução:** 5 quadros, cada um com legenda curta e quem aparece, mostrados à Unice enquanto os outros se escondem.
5. **Textos** em pt, en e es.
6. **Tarefas** (checklist de implementação) e **critérios de aceite**.

## Etapa 2 — Design do mapa
Regras de geometria (o jogo trata o mundo como retângulos em "house pixels"):
- Mundo `1720 × 1180` (igual à casa, para a rodada ter duração comparável).
- Salas são retângulos; **duas salas vizinhas ficam separadas por exatamente 24 px** (a parede). A passagem é uma *porta* centrada nessa fresta.
- `MapDoor` em fresta horizontal (anda-se na vertical): `x` = centro da porta, `y` = borda de baixo da sala de cima. Em fresta vertical: `x` = borda direita da sala da esquerda, `y` = centro.
- Esconderijo = móvel com lado de aproximação (`n`/`s`/`e`/`w`); a pessoa fica 22 px fora desse lado, e esse ponto precisa ser chão livre.
- A Unice entra pela `entry` (porta de entrada da história) e as crianças começam em `spawn`; ambos em chão livre e ligados por caminho.
- Mais detalhes e exemplo comentado: `.claude/skills/unice-new-story/resources/map-design-guide.md`.

## Etapa 3 — Implementação (o que o Claude altera)
| Peça | Onde |
|---|---|
| Dados do mapa | `Domain/Maps/Map<Nome>.cs` (novo) + 1 linha em `Maps.InOrder` (`Domain/Maps/MapDefinition.cs`) |
| História (cartão, HQ) | 1 entrada em `Stories.All` (`Domain/Maps/Stories.cs`) |
| Textos | `Core/Localization/Localization.Strings.cs`: `map.<mapa>.name`, `room.<sala>.name/.in`, `spot.<tipo>`, `stories.<id>.name/.blurb`, `comic.<id>.1…5` — sempre pt, en e es |
| Testes | nada a escrever à mão: `MapDefinitionsTests`, `LocalizationTests` e `StoryContentTests` já varrem todo mapa/história registrado |
| Docs | `CHANGELOG.md`, este diretório (`<id>.md`) |

Arte: mapas novos usam formas lisas coloridas (chão pela cor do terreno, móveis como silhuetas) — a arte pintada é um passo à parte,
feito pelo time de arte. A HQ usa os sprites que já existem (catálogo em `.claude/skills/unice-new-story/resources/art-catalog.md`).

## Etapa 4 — Verificação (definição de pronto)
- Testes Edit Mode 100% verdes (a skill `unice-run-tests` mostra como rodar).
- `MapDefinitionsTests` provam: salas dentro do mundo e sem repetir id; ≥ 8 esconderijos; entrada e spawn em chão livre e ligados; todo esconderijo alcançável; toda sala alcançável.
- `StoryContentTests` provam: nome da história, legendas da HQ e figuras existem nos 3 idiomas / em `Resources/Art`.
- Checagem manual (o Claude lista no resumo, não finge que testou): jogar uma rodada solo no mapa, ver a HQ como Unice, olhar o cartão no seletor.

## Vídeo de abertura (opcional)
Quando a partida começa (rodada 1), todos os jogadores veem o vídeo da história, ao mesmo tempo, e só depois a rodada começa: o tempo de esconder
só corre quando o vídeo acaba, e ninguém se mexe enquanto ele toca (nem os bots). Não dá para pular. Depois, quem é a Unice vê a HQ enquanto espera.
Para uma história ter vídeo: coloque o arquivo em `Assets/StreamingAssets/Videos/` (use `.mp4` em H.264 + AAC, **até 30 segundos**) e passe o nome e a
duração em `Stories.cs` (`introVideo: "doll.mp4", introSeconds: 29.5f`, um pouco mais que o vídeo). Se o arquivo não existir ou não tocar, o jogo mostra a HQ
nesse tempo e segue normalmente. O vídeo da Boneca é feito em Remotion (`unice-trailer/`, composição `HistoriaBoneca`).

## Histórias existentes
| Id | Mapa | Estado | Spec |
|---|---|---|---|
| `doll` | `house` | lançada, com HQ e vídeo | [doll.md](doll.md) |
| `lobisomem` | `lobisomem` | pronta (a partir da planta do autor), "Em breve" (arte lisa) | [lobisomem.md](lobisomem.md) |
| `rua` | `rua` | pronta, "Em breve" (arte lisa) | [rua.md](rua.md) |
