# História: A Rua da Vó (`rua`)

## 1. Brief
- **Quem conta / de onde vem:** o autor, memória de infância.
- **Lugar e época:** a praça e a rua da cidade, na frente da casa da vó, de dia.
- **Pessoas:** os primos (muita gente), a tia Unice.
- **O que aconteceu:** os primos se juntavam para brincar de esconde-esconde. Uma pessoa contava e os outros se escondiam. A tia Unice gritava, do nada, onde uma pessoa qualquer estava escondida. Todo mundo corria muito, para lá e para cá, pela praça e pela rua, na frente da casa da vó.
- **Tom:** bagunça alegre de infância; nada de susto.
- **Como vira esconde-esconde:** é o próprio esconde-esconde da história. A turma começa na calçada da vó e se espalha pela rua e pela praça. A Unice é a tia (a que "grita onde está alguém"). *(Suposição: no jogo a Unice procura andando, como em todas as histórias; o grito aleatório da tia aparece na HQ e no tom, não como regra nova.)*

## 2. Lugar → mapa (`rua`)
Feito a partir de uma imagem de satélite da praça da cidade do autor (print em `docs/stories/rua-satelite.png`). Duas ruas correm lado a lado (Rua do Mocambo e Rua Monsenhor José Magalhães) em volta de uma ilha cheia de árvores, se juntam numa ponta e se abrem, na outra, no largo calçado da igreja. **Era proibido entrar nas casas**: todas as casas e a igreja são parede (fora do mapa; a igreja é o buraco `MapRua.Church`, 1582, 300, 98 × 600, no fim do largo). O print foi girado de lado (o topo vira o oeste) e esticado para caber no mundo de 1720 × 1180. Áreas se sobrepõem em 60 px ou mais, sem portas. Chão `#6b6a63`.

| Área | Onde | Chão |
|---|---|---|
| `praca` | a ilha de árvores entre as ruas (180, 300, 1060 × 420) | verde-musgo `#5a6b3d` |
| `largo` | o largo calçado em frente à igreja (1240, 210, 320 × 780) | claro `#8a867a` |
| `contorno` | onde as ruas se juntam (100, 200, 200 × 630) | asfalto |
| `mocambo` | Rua do Mocambo (100, 200, 1240 × 170) | asfalto |
| `monsenhor` | Rua Monsenhor José Magalhães (100, 650, 1240 × 180) | asfalto |

- **Entrada da Unice (`entry`):** 1480, 920 — o canto do largo, em frente à igreja.
- **Início da turma (`spawn`):** 800, 300 — a Rua do Mocambo, na frente da casa da vó. *(Suposição: a casa da vó fica na Rua do Mocambo; a história só diz "na frente da casa da vó".)*

## 3. Esconderijos (19)
| Tipo | Onde | Lado | Frase (pt) |
|---|---|---|---|
| `gate` | Rua do Mocambo, portão da vó | s | atrás do portão da vó |
| `car` (×2) | Mocambo / Monsenhor | s / n | debaixo do carro |
| `lamppost` (×4) | Mocambo / ilha | s / e | atrás do poste |
| `bigtree` (×7) | ilha de árvores | vários | atrás da árvore grande |
| `bench` (×2) | ilha | s / n | atrás do banco |
| `popcorn` | largo | s | atrás do carrinho de pipoca |
| `cross` | meio do largo, em frente à igreja | w | atrás da cruz |
| `phonebooth` | largo | w | atrás do orelhão |
| `bin` | largo | n | atrás da lixeira |

*(O autor contou: igreja no fim da praça, cruz no meio, orelhão e luzes de poste ligadas entre si. Suposição: li "ponte ... interligado com luzes" como poste com fios de luz (poste de luz), não como ponte; a iluminação é só desenho de postes, o jogo não tem luz dinâmica. Outras suposições: portão, banco, carrinho de pipoca e lixeira não aparecem no print nem na história; são o que se espera de uma praça de interior. Os carros e as árvores vêm do print.)*

## 4. Cordel
Não há.

## 5. HQ de introdução (5 quadros)
> Removida do jogo (sem substituto por enquanto); fica aqui como registro de design e referência para um futuro vídeo.

| # | Cenário | Quem aparece | Legenda (pt) |
|---|---|---|---|
| 1 | chão do mapa | cinco primos | Na frente da casa da vó, a criançada se juntava pra brincar. |
| 2 | chão do mapa | um primo de olhos fechados (guarda-roupa de "parede") e os outros correndo | Um contava de olhos fechados e todo mundo corria pra se esconder. |
| 3 | chão do mapa | primos atrás de barril e samambaias | Na praça e na rua, era gente correndo pra lá e pra cá! |
| 4 | chão do mapa | a Unice apontando, primos escondidos | E a tia Unice, do nada, gritava onde alguém estava escondido! |
| 5 | chão do mapa | a Unice e primos | Agora a tia Unice é você: ache a criançada! |

## 6. Textos
- `map.rua.name`: A Rua da Vó / Grandma's Street / La Calle de la Abuela
- `stories.rua.name`: A Rua da Vó / Grandma's Street / La Calle de la Abuela
- Salas, esconderijos e legendas em pt/en/es: em `Localization.Strings.cs`.

## 7. Tarefas
- [x] `Domain/Maps/MapRua.cs` e registro em `Maps.InOrder`
- [x] Entrada em `Stories.All` (`released: false`)
- [x] Textos pt/en/es
- [x] `CHANGELOG.md` e tabela de histórias
- [x] Testes Edit Mode rodados no Unity (546/546)

## 8. Critérios de aceite
- [ ] Testes verdes (`MapDefinitionsTests`, `LocalizationTests`, `StoryContentTests`, `StoriesTests`)
- [ ] Rodada solo jogável (checagem manual): dá para correr pela rua e pela praça; as casas não deixam entrar
