# Editor de mapas arrastável — plano

Objetivo: o mapa (salas, portas, móveis, esconderijos, ponto de partida e entrada) é editado **arrastando** numa janela do
Unity; o código continua lendo os mapas como hoje. Divisão do trabalho: o Bruno organiza o mapa no editor, o Claude cuida do código.

## Decisões

| Tema | Decisão | Por quê |
|---|---|---|
| Fonte da verdade | Os arquivos `Domain/Maps/Map*.cs` continuam sendo os mapas, mas passam a ser **gerados pelo editor** | O Domain não referencia o Unity (`noEngineReferences`): nada de ScriptableObject/JsonUtility lá. C# gerado mantém os testes, o jogo e o online exatamente como estão |
| Onde fica o editor | Assembly `Unice.Editor` (`Assets/Unice/Scripts/Editor/MapEditor/`), menu `Unice > Editor de mapas` | Ferramenta só do Editor, fora do build |
| Interface | `EditorWindow` com UI Toolkit: canvas com zoom/arrasto à esquerda, painel de propriedades à direita | Mesma tecnologia das telas do jogo |
| Arte da casa | Móveis seguem a peça sozinhos (âncora relativa no manifesto). Pisos e paredes pintados têm posição própria em `Resources/Art/manifest.json`: o editor move o piso junto com a sala e deixa arrastar as paredes, gravando o manifesto **sem perder campos** | Senão a arte desalinha do mapa |
| Ordem dos móveis da casa | Nunca reordenar os 30 primeiros; novos móveis vão no fim | `manifest.json` (`furn[].i`) aponta para eles por posição |
| Validação | As mesmas regras de `MapDefinitionsTests` (salas no mundo, ids únicos, esconderijos suficientes e alcançáveis, entrada chega ao spawn) rodam ao vivo no editor e bloqueiam o salvar com erro | O mapa salvo é jogável |

## Etapas

Cada etapa termina com: compila, **todos os testes Edit Mode passam** (`unice-run-tests`), diff revisado e um commit.

### Etapa 1 — Gerador de código dos mapas (base)
- `MapCodeWriter` (Editor): `MapDefinition` → código C# do arquivo `Map<Nome>.cs` (cabeçalho explicando que é gerado pelo editor e pode ser editado à mão; números invariantes; `'\0'`/null omitidos).
- Regerar os três mapas atuais (`MapHouse`, `MapLobisomem`, `MapRua`) com ele: **os dados não mudam** (os testes provam), só a formatação. Comentários importantes da casa (ordem do manifesto) vão para o cabeçalho gerado.
- Menu `Unice > Mapas > Regerar código dos mapas` (sanidade).

### Etapa 2 — Janela do editor (ver e arrastar)
- `Unice > Editor de mapas`: escolhe o mapa; canvas desenha mundo, salas (com o piso pintado na casa), portas, móveis (com a arte na casa, silhueta nos outros), esconderijos com o lado de aproximação e o ponto onde a criança fica, spawn e entrada.
- Zoom (roda do mouse), pan (botão do meio / espaço + arrastar), grade com encaixe de 8 px (Shift desliga).
- Selecionar e **arrastar/redimensionar** salas, móveis e portas; arrastar spawn e entrada. Desfazer (Ctrl+Z) e refazer.
- Painel à direita: campos do item selecionado (id/tipo, x, y, w, h, lado do esconderijo, cor), adicionar/remover sala, móvel e porta.
- **Salvar**: valida, grava `Map<Nome>.cs` pelo `MapCodeWriter` e recompila.

### Etapa 3 — Validação ao vivo e arte da casa
- Painel de problemas que roda as regras de `MapDefinitionsTests` sobre o mapa em edição (via `Map.Load` + `Navigation`), destacando no canvas o item com problema; salvar é bloqueado com erro.
- Casa: mover/redimensionar sala atualiza o piso no `manifest.json`; paredes pintadas aparecem e podem ser arrastadas; gravação do manifesto sem perder campos (ida e volta idêntica quando nada muda).

### Etapa 4 — Documentação e fluxo
- `docs/MAP_EDITOR.md` (pt): como abrir, arrastar, salvar, o que o vermelho significa, cuidados (ordem dos móveis, arte).
- Atualizar a skill `unice-new-story` para criar mapas novos pelo editor.

## Fora do escopo agora
Pintar arte nova, editar colisão fina (as paredes de colisão continuam sendo as bordas das salas), editar localização dos nomes.
