# História: <Nome> (`<id>`)

> Modelo de spec de história. O Claude copia este arquivo para `docs/stories/<id>.md` e o preenche a partir do que você contou.
> `<id>`: minúsculas, sem acento, uma palavra (`doll`, `feira`, `engenho`). O mapa tem id próprio (`<mapa>`, ex. `feira`).

## 1. Brief
- **Quem conta / de onde vem:** …
- **Lugar e época:** …
- **Pessoas:** … (quem vira "a Unice" nesta história? quem são os primos/amigos?)
- **O que aconteceu:** … (3 a 6 frases, na ordem)
- **Tom:** nostálgico / engraçado / de arrepio … (a Unice é uma caçadora de brincadeira, nunca violenta)

## 2. Lugar → mapa (`<mapa>`)
Mundo 1720 × 1180. Salas separadas por 24 px de parede. Cada linha é uma sala.

| Id da sala | Nome (pt) | x | y | w | h | O que tem |
|---|---|---|---|---|---|---|
| … | … | … | … | … | … | … |

- **Portas:** … (lista de `MapDoor`, com qual sala liga a qual)
- **Entrada da Unice (`entry`):** x, y — e por que ali (a porta da frente, o portão…)
- **Spawn dos escondidos (`spawn`):** x, y
- **Cor do chão (`groundTint`):** `#rrggbb` — …

## 3. Esconderijos (mín. 8, ideal 12–18)
| Tipo (`spot.<tipo>`) | Sala | x | y | w | h | Lado | Frase (pt) |
|---|---|---|---|---|---|---|---|
| … | … | … | … | … | … | n/s/e/w | "atrás do pé de umbu" |

Peças só decorativas (sem lado): …

## 4. Vídeo de abertura (opcional)
Até 30 s, mostrado a todos quando a partida começa (`introVideo`/`introSeconds` em `Stories.cs`; ver o README desta pasta). Sem vídeo, a HQ ocupa o tempo.

## 5. Textos
- `map.<mapa>.name`: …
- `stories.<id>.name`: …  /  `stories.<id>.blurb` (1 frase para o cartão): …
- en e es de tudo acima: …

## 6. Tarefas
- [ ] `Domain/Maps/Map<Nome>.cs` e registro em `Maps.InOrder`
- [ ] Entrada em `Stories.All`
- [ ] Textos pt/en/es em `Localization.Strings.cs`
- [ ] Testes Edit Mode verdes
- [ ] `CHANGELOG.md` e tabela de histórias em `docs/stories/README.md`

## 7. Critérios de aceite
- [ ] Testes verdes (`MapDefinitionsTests`, `LocalizationTests`, `StoryContentTests`)
- [ ] Rodada solo jogável no mapa (checagem manual)
