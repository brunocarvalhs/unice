# História: A Boneca (`doll`)

> Spec retroativa da primeira história, escrita no formato de [`_template.md`](_template.md) como exemplo de referência.

## 1. Brief
- **Quem conta / de onde vem:** o autor do jogo, uma lembrança de criança.
- **Lugar e época:** a casa da vó, no interior da Bahia, nas férias.
- **Pessoas:** a tia Unice (a mais velha, guarda a boneca), o primo Márcio (apronta), os primos, a vó (chama para almoçar).
- **O que aconteceu:** a boneca da tia Unice era um tesouro intocável. Márcio desafiou os primos a pegá-la sem a tia ver. A boneca passou de mão em mão, a tia deu por falta e saiu feito trovoada. Primos se esconderam pela casa toda, rindo, até a vó gritar "Menino, almoçar!".
- **Tom:** nostálgico e brincalhão.

## 2. Lugar → mapa (`house`)
12 salas (`Domain/Maps/MapHouse.cs`): master, bath, kids, library (andar de cima, em fileira), hall (corredor), living, dining, kitchen, estufa, entrance, pantry, laundry.
Entrada da Unice: porta da frente (`824, 1085`), sala `entrance`. Spawn dos escondidos: `620, 474` (corredor).
Arte pintada própria (`Resources/Art/manifest.json`); os índices de `furniture` não podem ser reordenados.

## 3. Esconderijos
28 peças com lado de esconderijo (cama, guarda-roupa, baú, banheira, mesa…), frases em `spot.<tipo>`.

## 4. Cordel
5 estrofes (`story.1…5.t/.b`): "Peço licença", "A boneca da tia Unice", "A peleja", "Cada um no seu canto", "Menino, almoçar!". (O cordel animado em tela cheia foi removido; as estrofes seguem como texto de referência.)

## 5. HQ de introdução (5 quadros)
Definida em `Stories.All` (`doll`), legendas em `comic.doll.1…5`:
1. Cozinha: primos correndo ao redor da mesa — "Na casa da vó tinha mais menino que um quartel."
2. Quarto da tia: a Unice vigia a boneca, Márcio espia — "No quarto da tia Unice morava uma boneca: o tesouro dela."
3. Sala: a boneca passa de mão em mão — "Márcio desafiou: quem pega a boneca sem a tia ver?"
4. Jantar: cada um no seu canto — "Quando a tia deu por falta… foi primo pra todo canto!"
5. A Unice de olhos fechados — "Agora é com você, tia: conte até zero e ache a boneca!"

## 6. Textos
`stories.doll.name` = "A Boneca" / "The Doll" / "La Muñeca"; blurb: "Na casa da vó, a boneca da tia Unice sumiu. Esconde, que ela vem!".

## 7. Tarefas
- [x] Mapa, história, textos, HQ e testes.

## 8. Critérios de aceite
- [x] Testes verdes.
- [ ] Verificação manual da HQ em aparelho (como Unice na rodada 1; tocar para pular).
