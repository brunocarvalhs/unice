# Direção de arte: Unice nordestino

Fase 1 do plano de estilização. Este documento é a referência para as fases seguintes (tokens de UI, botões, arte do mundo, áudio). Nada aqui altera regras: o `manifest.json` do mapa, as colisões e o `Domain/` ficam como estão.

## 1. Ponto de partida

O jogo já é nordestino na história (`story.*` em `Localization.Strings.cs`): um cordel que lembra a casa da vó no interior da Bahia, a **Tia Unice** (a mais velha da família) e a **boneca de pano** que ela guarda. O visual atual (casa vitoriana, bordô, lareira, piano, papel envelhecido) é o que destoa da história.

**Conceito:** *"A peleja dos primos na casa da vó"*. A casa é uma lembrança de infância, vista como um folheto de cordel colorido à mão. O medo vem da tensão da brincadeira (a tia chegando, a boneca rindo), não de terror gótico.

**Tom:** lúdico com um arrepio. Cor quente, luz de fim de tarde e lamparina; a ameaça vem da sombra, do som e do tempo, não de sangue ou sustos.

## 2. Referências visuais

| Elemento | Referência | Onde entra |
|---|---|---|
| Xilogravura de cordel | Traço preto grosso, hachura, fundo de papel | Títulos, molduras, ícones, banners |
| Chita | Estampa floral de cor forte | Roupas, cortinas, toalhas, fundo de botões |
| Casa de interior | Parede de cal, rodapé de cor, porta e janela de madeira, telha | Mapa |
| Ladrilho hidráulico | Piso geométrico de cor | Pisos de sala, cozinha e entrada |
| Renda de bilro e crochê | Bordas rendadas | Painéis e cartões |
| Couro e palha | Chapéu de couro, chapéu de palha, bornal | Kit de personagem (já existe em `Kit.cs`) |

## 3. Paleta (tokens)

Nomes pensados para virar variáveis USS (`--cor-*`) na Fase 2. Os contrastes foram calculados (WCAG), e o mínimo para texto é 4.5:1.

### Base
| Token | Hex | Uso |
|---|---|---|
| `noite` | `#1d1410` | Fundo escuro do jogo, HUD e menus (substitui o roxo-preto atual) |
| `papel` | `#f3e6c8` | Folhas, cartões, painéis claros (cordel) |
| `cal` | `#fbf4e2` | Texto sobre fundo escuro, realces |
| `tinta` | `#2a1a12` | Texto e traço de xilogravura sobre papel |

### Acentos
| Token | Hex | Uso | Variante p/ texto |
|---|---|---|---|
| `sol` | `#f2b632` | Destaque principal, botão ativo, relógio, tempo | (já passa) |
| `barro` | `#b5532a` | Telhas, terra, botão primário | claro `#e08a5a` / escuro `#8f3d1c` |
| `anil` | `#2f5d8c` | Azul de ladrilho e parede, botão secundário | claro `#7fb0e0` |
| `mandacaru` | `#4f7a3a` | Vitória, "pronto", plantas | claro `#9fcf7a` / escuro `#3d6229` |
| `chita` | `#c8323c` | Perigo, a Unice, tempo acabando | claro `#ef6b6f` / escuro `#a8232d` |

**Regra de uso:** o acento puro serve para fundos e preenchimentos. Para **texto**, use a variante clara sobre `noite` e a escura sobre `papel`.

### Contrastes medidos
| Texto sobre fundo | Razão |
|---|---|
| `tinta` / `papel` | 13.5 |
| `tinta` / `cal` | 15.3 |
| `cal` / `noite` | 16.5 |
| `papel` / `noite` | 14.6 |
| `sol` / `noite` | 9.9 |
| `tinta` / `sol` (texto do botão ativo) | 9.2 |
| `chita` claro / `noite` | 6.0 |
| `mandacaru` claro / `noite` | 10.1 |
| `barro` claro / `noite` | 6.9 |
| `anil` claro / `noite` | 7.9 |
| `barro` escuro / `papel` | 5.9 |
| `chita` escuro / `papel` | 5.8 |
| `mandacaru` escuro / `papel` | 5.7 |
| `anil` / `papel` | 5.5 |

Não usar: `chita`, `mandacaru` ou `barro` puros como texto (3.4 a 4.3:1).

### Mapeamento das cores atuais
| Hoje (`unice.uss`) | Passa a ser |
|---|---|
| `rgb(154, 44, 69)` bordô | `chita` escuro (texto em papel) / `chita` (fundo) |
| `rgb(239, 115, 148)` rosa | `chita` claro |
| `rgb(240, 176, 87)` vela | `sol` |
| `rgb(143, 203, 156)` musgo | `mandacaru` claro |
| `rgb(17, 12, 18)` / `rgb(26, 19, 24)` | `noite` |
| `rgb(236, 226, 203)` papel | `papel` |
| `rgb(43, 30, 26)` tinta | `tinta` |

## 4. Tipografia

Todas as fontes precisam ser OFL (já temos `OFL-LICENSES.txt`) e ter acentos do português e do espanhol.

| Papel | Hoje | Proposta |
|---|---|---|
| Títulos e botões grandes (display) | IM Fell English SC | Uma fonte de título com cara de tipo móvel/cordel. Candidatas a testar no jogo: **Rye**, **Sancreek**, **Ewert**, **Bigelow Rules**. Escolha final depois de ver no `.menu-title` a 112 px e no `.mbtn` a 54 px. |
| Texto corrido, nomes, botões pequenos | Alegreya Sans | **Mantém.** Legível a 16 px e já tem Bold. |
| Código da sala, relógio, números | Courier Prime Bold | **Mantém** como padrão. Reavaliar só se destoar do display novo. |

Critérios de aceite para a fonte display: legível a 28 px (botão pequeno), acentos (ã, ç, é, ó), maiúsculas e minúsculas, e licença OFL baixada junto de `OFL-LICENSES.txt`. Os arquivos ficam em `Resources/Fonts/`.

## 5. Identidade gráfica

- **Contorno:** traço escuro grosso (2–3 px no painel de 540 px), levemente irregular, com cara de xilogravura.
- **Preenchimento:** cor chapada, sem degradê, com textura de papel e hachura só em sombras.
- **Painéis:** papel com borda dupla; cantos rendados ou recortados em vez de cantos arredondados padrão.
- **Divisores:** faixa de bandeirinha de festa junina ou barra de ladrilho.
- **Ícones de ação:** estilo carimbo/xilogravura em `tinta` sobre `sol` ou `papel`.
- **Luz:** quente, de fim de tarde. A névoa (`UniceFog`) e a vinheta passam de roxo a **âmbar-marrom**, como luz de candeeiro.

## 6. Mundo: casa da vó

Cada cômodo mantém id, tamanho e função (o `manifest.json` não muda). Só a roupagem muda.

| Cômodo (id) | Hoje | Proposta |
|---|---|---|
| `master` | quarto principal | Quarto da tia: cama de ferro, colcha de chita, oratório |
| `bath` | banheiro | Banheiro de azulejo azul-anil |
| `kids` | quarto das crianças | Quarto dos primos: redes e beliche |
| `library` | biblioteca | Sala dos santos e retratos: estante, folhetos de cordel, cadeira de balanço |
| `hall` | corredor | Corredor de ladrilho hidráulico |
| `living` | sala de estar | Sala: sofá, rádio antigo, cadeira de balanço |
| `dining` | sala de jantar | Mesa grande de madeira com toalha de chita |
| `kitchen` | cozinha | Cozinha: fogão a lenha, pilão, panelas de barro |
| `estufa` | estufa | **Quintal:** mandacaru, horta, varal, galinheiro |
| `entrance` | entrada | Varanda: rede, banco, porta de duas folhas |
| `pantry` | despensa | Despensa: sacos, potes, queijo e carne de sol |
| `laundry` | lavanderia | Tanque e área de serviço |

### Móveis (30 sprites `furn_*`, mesmas caixas de colisão)
`bed`→cama de ferro · `wardrobe`→guarda-roupa de madeira · `dresser`→cômoda com espelho · `tub`→banheira de zinco · `sink`→pia de louça · `bunk`→beliche · `chest`→baú · `dollhouse`→casinha de brinquedo · `shelves`→estante com cordéis · `desk`→escrivaninha · `clock`→relógio de parede · `armchair`→cadeira de balanço · `closet`→armário · `commode`→cômoda · `sofa`→sofá de palhinha · `fireplace`→**fogão a lenha** · `piano`→**sanfona sobre banco** (ou rádio) · `table`→mesa de jantar · `cabinet`→guarda-louça · `sinkcab`→pia de cozinha · `stove`→fogão · `ktable`→mesa de cozinha · `ferns`→**mandacaru e vasos** · `planters`→canteiros · `stairs`→escada · `coats`→cabide de chapéus · `barrel`→**pote de barro** · `pshelves`→prateleiras da despensa · `hamper`→cesto de palha · `tank`→tanque de lavar.

### Personagens
- **Crianças:** os cinco corpos (`body0–4`) ganham roupas de chita, camisa de botão e macacão; chapéus: palha, couro de vaqueiro, boné e gorro (o gorro vira lenço de cabeça). O acessório "Boneca de pano" e o bornal já existem.
- **Tia Unice:** matriarca brava e bem-vestida: vestido longo de estampa miúda, xale, coque e a **bengala** (já tem som, `sound.cane`). Silhueta forte para se reconhecer de longe.
- **Boneca de pano:** destaque visual (fita vermelha no cabelo de lã) para a leitura rápida de quem a carrega.

## 7. Texto e narrativa

A base já está certa (cordel, vó, Bahia). O trabalho de texto é de **afinação de voz**, sem mudar as chaves de `Localization.Strings.cs`.

- **Português:** leve sotaque nordestino em chamadas curtas ("Bora!", "Oxe!", "Visse!"), sem forçar ortografia caricata e sem prejudicar a clareza das instruções.
- **Inglês e espanhol:** traduzir o sentido e manter palavras-marca (cordel, Tia Unice, sertão) com explicação curta quando for preciso.
- **Termos do jogo:**

| Hoje | Proposta (pt) |
|---|---|
| "ESCONDE-ESCONDE NUMA CASA ANTIGA" | "ESCONDE-ESCONDE NA CASA DA VÓ" |
| "Quem mexeu nas minhas coisas?!" | Manter: já é bem a tia |
| "A Unice chegou em casa!" | "A Tia Unice chegou!" |
| "PEGO!" | "PEGUEI!" ou "ACHEI!" |
| "Um robô assumiu no lugar." | "Um robô entrou na brincadeira." |
| "Risadinha" | Manter |
| "Cochicho" | Manter (já é regional) |

- **Nomes das rodadas:** a definir com você, por exemplo "Tarde na varanda", "Hora do almoço" e "Noite de São João".

## 8. Escopo de áudio (insumo da Fase 5)
Instrumentos de referência para o `ToneSynth`: zabumba, triângulo e pífano/sanfona. Ambiente: grilos, galo ao fundo, vento, cancela, rádio longe. A risadinha da boneca fica aguda e curta, com leve eco.

## 9. Decisões para fechar antes da Fase 4
1. **Fonte display final.** Testar as 4 candidatas no jogo.
2. **Grau de terror.** Manter o tom lúdico com arrepio (recomendado) ou ir para algo mais sombrio.
3. **Estilo de ilustração.** Chapado com contorno de xilogravura (recomendado, mais barato de produzir em 30 móveis e 12 pisos) ou pintado com textura.
4. **Quem produz a arte.** Os PNGs atuais têm 30 móveis, 12 pisos, 33 paredes e cerca de 150 peças de personagem; é preciso definir ilustrador ou geração assistida com revisão.
5. **Nomes das rodadas e revisão do texto em pt** por alguém do Nordeste, para evitar caricatura.

## 10. Próximos passos
- **Fase 2** (código, sem depender de arte): passar `unice.uss` para as variáveis acima e unificar os botões.
- **Fase 4** (arte): começar por um cômodo piloto (`kitchen`) para validar o estilo antes de produzir o resto.
