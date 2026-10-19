# História: O Lobisomem (`lobisomem`)

## 1. Brief
- **Quem conta / de onde vem:** o autor, uma lembrança do ano passado.
- **Lugar e época:** uma casa do interior, no quintal, de noite, no escuro.
- **Pessoas:** o tio (conta histórias de terror), as primas e o autor, a tia Unice (grita de medo, corre para casa e tranca a porta).
- **O que aconteceu:** o tio contava a história do lobisomem no meio da noite. De repente, um uivo de lobo na escuridão. A tia Unice gritou "lobisomem!", correu para casa e trancou todo mundo do lado de fora. A turma saiu correndo, o "lobisomem" vindo atrás, e cada um se escondeu.
- **Tom:** arrepio de brincadeira, risada nervosa; nada de violência.
- **Como vira esconde-esconde:** a turma está trancada fora de casa, no escuro, e se esconde pelo quintal e pela casa. Quem procura (a "Unice") faz o papel do lobisomem da história: a tia Unice é o nome da caçadora do jogo, então a que gritou de medo "vira" o lobisomem na brincadeira. *(Suposição minha: a história não diz quem faz o lobisomem.)*

## 2. Lugar → mapa (`lobisomem`)
Mundo 1720 × 1180, 7 áreas em 3 faixas, paredes de 24 px. Chão `#2f4a3a` (terra/mato à noite).

| Id da sala | Nome (pt) | x | y | w | h | O que tem |
|---|---|---|---|---|---|---|
| `sala` | Sala da Casa | 40 | 40 | 520 | 340 | sofá, poltrona, rede |
| `cozinha` | Cozinha da Casa | 584 | 40 | 480 | 340 | fogão (decoração), mesa, armário da pia |
| `quarto` | Quarto de Hóspedes | 1088 | 40 | 592 | 340 | cama, guarda-roupa, baú |
| `varanda` | Varanda | 40 | 404 | 1640 | 120 | barril, vasos, samambaias (liga a casa ao quintal) |
| `terreiro` | Terreiro | 40 | 548 | 520 | 592 | lenha, tanque, casinha do cachorro, poço e varal (decoração) |
| `pomar` | Pomar | 584 | 548 | 480 | 592 | duas árvores grandes, monte de palha, carroça |
| `curral` | Curral | 1088 | 548 | 592 | 592 | galinheiro, cocho, galpão, lenha, tanque |

- **Portas (verticais, entre faixas):** x = 440, 824, 1384 em y = 380 (casa → varanda) e em y = 524 (varanda → quintal).
- **Portas (horizontais, laterais):** (560, 210) e (1064, 210) na casa; (560, 850) e (1064, 850) no quintal.
- **Entrada da Unice (`entry`):** 1620, 1090 — o fundo do curral, de onde vem o uivo.
- **Spawn dos escondidos:** 300, 740 — o terreiro, onde a turma ficou trancada do lado de fora, longe da entrada.

## 3. Esconderijos (23)
| Tipo | Sala | Lado | Frase (pt) |
|---|---|---|---|
| `sofa` | sala | n | atrás do sofá |
| `armchair` | sala | s | atrás da poltrona |
| `hammock` | sala | s | atrás da rede |
| `ktable` | cozinha | s | debaixo da mesa da cozinha |
| `sinkcab` | cozinha | s | no armário da pia |
| `bed` | quarto | s | debaixo da cama |
| `wardrobe` | quarto | e | no guarda-roupa |
| `chest` | quarto | s | no baú |
| `barrel` | varanda | e | dentro do barril |
| `planters` | varanda | n | atrás dos vasos de plantas |
| `ferns` | varanda | e | atrás das samambaias |
| `woodpile` | terreiro | e | atrás da pilha de lenha |
| `tank` | terreiro | e | atrás do tanque de água |
| `doghouse` | terreiro | w | dentro da casinha do cachorro |
| `bigtree` (×2) | pomar | n | atrás da árvore grande |
| `haystack` | pomar | w | dentro do monte de palha |
| `cart` | pomar | n | debaixo da carroça |
| `coop` | curral | s | no galinheiro |
| `trough` | curral | n | atrás do cocho |
| `shed` | curral | s | dentro do galpão |
| `woodpile` | curral | e | atrás da pilha de lenha |
| `tank` | curral | w | atrás do tanque de água |

Decorativos: `stove` (cozinha), `well` (terreiro), `clothesline` (terreiro).

## 4. Cordel
Não há (só a história `doll` tem a tela do cordel).

## 5. HQ de introdução (5 quadros)
| # | Cenário | Quem aparece | Legenda (pt) |
|---|---|---|---|
| 1 | chão do mapa (noite) | quatro primos em roda, o "tio" (primo de capa) e um barril | Noite no interior: o tio contava histórias de lobisomem no quintal. |
| 2 | chão do mapa | primos assustados e a lua (`dot`) | De repente, no meio da escuridão, um uivo de lobo! |
| 3 | `floor_living` | a Unice correndo para casa e dois primos | A tia Unice gritou "lobisomem!" e correu pra casa. |
| 4 | chão do mapa | primos batendo na porta (guarda-roupa) | Ela trancou a porta e deixou a gente lá fora! |
| 5 | chão do mapa | a Unice, a lua e primos escondidos atrás de barril e samambaias | Foi todo mundo correr e se esconder. Agora o lobisomem é você! |

## 6. Textos
- `map.lobisomem.name`: O Quintal da Noite / The Night Yard / El Patio de la Noche
- `stories.lobisomem.name`: O Lobisomem / The Werewolf / El Hombre Lobo
- `stories.lobisomem.blurb`: No quintal, um uivo na escuridão… e a porta trancada! Esconde, que o lobisomem vem!
- en e es de salas, esconderijos e legendas: em `Localization.Strings.cs`.

## 7. Tarefas
- [x] `Domain/Maps/MapLobisomem.cs` e registro em `Maps.InOrder`
- [x] Entrada em `Stories.All` (`released: false`)
- [x] Textos pt/en/es
- [x] Testes Edit Mode verdes
- [x] `CHANGELOG.md` e tabela de histórias

## 8. Critérios de aceite
- [x] Testes verdes (`MapDefinitionsTests`, `LocalizationTests`, `StoryContentTests`)
- [ ] Rodada solo jogável no mapa (checagem manual)
- [ ] HQ aparece para a Unice na rodada 1 e some com um toque
