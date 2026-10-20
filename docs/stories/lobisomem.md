# História: O Lobisomem (`lobisomem`)

## 1. Brief
- **Quem conta / de onde vem:** o autor, uma lembrança do ano passado.
- **Lugar e época:** o quintal da casa da tia, no interior da Bahia, de noite, no escuro. Terra árida; coqueiros e árvores do interior.
- **Pessoas:** o tio (conta histórias de terror), as primas e o autor, a tia Unice (grita de medo, corre para casa e tranca a porta).
- **O que aconteceu:** a turma ouvia o tio contar a história do lobisomem, na sacada. A única luz era a da sacada; o resto era escuridão. De repente, um uivo de lobo. A tia Unice gritou "lobisomem!", correu para dentro de casa e trancou todo mundo do lado de fora. A turma saiu correndo e cada um se escondeu pelo quintal.
- **A casa por dentro:** ninguém tinha acesso (a tia trancou). Por isso a casa **não faz parte do mapa**: é um buraco que não se pode entrar nem enxergar através.
- **Tom:** arrepio de brincadeira, risada nervosa; nada de violência.
- **Como vira esconde-esconde:** a turma começa na sacada (iluminada) e se esconde pelo quintal escuro. Quem procura (a "Unice") faz o papel do lobisomem. *(Suposição: a história não diz quem faz o lobisomem; usei a Unice, que é a caçadora do jogo.)*

## 2. Lugar → mapa (`lobisomem`)
Planta desenhada pelo autor, reduzida para o mundo de 1720 × 1180. O quintal é **aberto**: áreas que se sobrepõem em 60 px ou mais, sem paredes nem portas. Chão `#2a2018` (terra seca à noite).

| Cor na planta | O que é | No mapa |
|---|---|---|
| lilás com X + roxo | a casa por dentro (sem acesso) | buraco `MapLobisomem.House` (557, 322, 648 × 230): nada o cobre |
| rosa | sacada (única luz) | sala `sacada` (557, 552, 648 × 96), chão quente `#b97a35`; é onde a turma começa (`spawn` 880, 600) |
| marrom | casa de ferramentas do tio | esconderijo `toolhouse` (40, 40, 323 × 168) |
| laranja | galinheiro, ao lado da casa de ferramentas | esconderijo `coop` (40, 208, 248 × 242) |
| amarelo | jardim da tia, cheio de flores | sala `jardim` (40, 733, 390 × 407), chão verde-escuro `#223a26`, 3 canteiros `flowerbed` |
| verdes | coqueiros, árvore grande, moita | `coconut` ×2, `bigtree`, `bush` |
| azul | caixa d'água | esconderijo `watertank` (1443, 83, 220 × 237), por trás |
| vermelho | carro trancado | esconderijo `car` (1415, 687, 98 × 172), por baixo |

Áreas (6): `sacada`, `fundos` (40, 40, 1640 × 280), `oeste` (40, 200, 495 × 580), `leste` (1227, 200, 453 × 740), `terreiro` (330, 588, 1350 × 552), `jardim`.
- **Entrada da Unice (`entry`):** 320, 1090 — o fundo escuro do jardim, de onde vem o uivo; longe da sacada.
- **Luz:** só a sacada tem cor quente (`Room.tint`); o resto é escuro. O jogo ainda não tem iluminação dinâmica por mapa; a sacada "acesa" é só a cor do chão.

## 3. Esconderijos (14)
| Tipo | Onde | Lado | Frase (pt) |
|---|---|---|---|
| `toolhouse` | fundos | e | atrás da casa de ferramentas |
| `coop` | oeste | e | atrás do galinheiro |
| `coconut` (×2) | fundos / terreiro | s / n | atrás do coqueiro |
| `bigtree` | oeste | e | atrás da árvore grande |
| `bush` | terreiro | n | atrás da moita |
| `watertank` | leste | s | atrás da caixa d'água |
| `car` | leste | w | debaixo do carro |
| `flowerbed` (×3) | jardim | s / e / n | atrás do canteiro de flores |
| `woodpile` | terreiro | n | atrás da pilha de lenha |
| `barrel` | terreiro | w | dentro do barril |
| `doghouse` | terreiro | w | dentro da casinha do cachorro |

Decorativo: `clothesline` (varal). *(Suposições: lenha, barril, casinha do cachorro e varal não estavam na planta; acrescentei para o mapa ter esconderijos suficientes no meio.)*

## 4. Cordel
Não há (só a história `doll` tem a tela do cordel).

## 5. HQ de introdução (5 quadros)
> Removida do jogo (sem substituto por enquanto); fica aqui como registro de design e referência para um futuro vídeo.

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
- Salas, esconderijos e legendas em pt/en/es: em `Localization.Strings.cs`.

## 7. Tarefas
- [x] `Domain/Maps/MapLobisomem.cs` (a partir da planta) e registro em `Maps.InOrder`
- [x] Entrada em `Stories.All` (`released: false`)
- [x] Textos pt/en/es
- [x] `Room.tint` (cor de chão por sala) para a sacada iluminada
- [x] Testes do buraco da casa em `MapTests`
- [ ] Testes Edit Mode rodados no Unity (o Editor estava aberto; ver o resumo)
- [x] `CHANGELOG.md` e tabela de histórias

## 8. Critérios de aceite
- [ ] Testes verdes (`MapDefinitionsTests`, `LocalizationTests`, `StoryContentTests`, `MapTests`)
- [ ] Rodada solo jogável no mapa (checagem manual): a casa não deixa entrar nem enxergar; a sacada é a única área quente
