# Editor de mapas

`Unice > Editor de mapas`: arrasta salas, móveis, portas, o ponto de início e a entrada no mapa, em vez de editar
coordenadas na mão em `Domain/Maps/Map<Nome>.cs`. Veja o plano completo em [`MAP_EDITOR_PLAN.md`](MAP_EDITOR_PLAN.md).

## Abrir

Menu `Unice > Editor de mapas`. Escolha o mapa no menu suspenso no topo (`casa`, `lobisomem`, `rua`).

## Navegar e editar

- **Zoom:** roda do mouse, centrado no cursor.
- **Mover a vista (pan):** botão do meio, ou espaço + arrastar.
- **Selecionar:** clique numa sala, móvel, porta, no início (bolinha azul "início") ou na entrada (bolinha laranja "entrada").
- **Mover:** arraste o item selecionado pelo meio.
- **Redimensionar** (só salas e móveis): arraste pela borda ou pelo canto.
- **Encaixe de 8 px:** ligado por padrão; segure **Shift** para mover livre, sem encaixe.
- **Desfazer / refazer:** Ctrl+Z / Ctrl+Shift+Z (ou os botões "Desfazer"/"Refazer" no topo).
- **Painel à direita:** mostra os campos do item selecionado (id, posição, tamanho, lado do esconderijo, cor). Editar ali
  atualiza o desenho; arrastar no mapa atualiza os campos.
- **Adicionar:** botões "+ Sala", "+ Móvel", "+ Porta vertical/horizontal" — o novo item aparece no centro da tela.
- **Remover:** selecione e clique "Remover" no painel (ou pelo botão de excluir). Remover um dos **30 primeiros móveis da
  casa** mostra um aviso mais forte: a arte pintada deles é ligada pela posição no array (`Resources/Art/manifest.json`),
  então removê-los desalinha a arte dos móveis seguintes.

## Problemas e salvar

O painel **"Problemas"** roda, ao vivo, as mesmas regras dos testes automáticos (salas dentro do mundo, esconderijos
suficientes para 8 jogadores, a entrada chega ao início, todo esconderijo e toda sala são alcançáveis...). Clicar num
problema seleciona o item no mapa.

**Salvar é bloqueado** se houver qualquer problema na lista — nada é gravado até o mapa ficar jogável.

Salvar grava `Domain/Maps/Map<Nome>.cs` de novo (mesmo formato de sempre, só os números mudam). No mapa da casa, também
atualiza a posição do piso pintado da sala em `Resources/Art/manifest.json`, para a arte acompanhar a sala.

A validação (e o salvar) fica **desligada durante o Play**: o `Map` que o editor usaria é o mesmo que a partida ao vivo
usa, então mexer nele no meio de uma partida bagunçaria o jogo. Espere sair do Play para editar.

## O que o editor não faz (por enquanto)

- **Paredes pintadas da casa:** aparecem só como referência da arte do piso; não são arrastáveis. São 33 pedaços sem um
  vínculo direto com uma sala ou porta, então movê-las com segurança pediria um passo de regeneração que ainda não existe.
- **Um mapa novo** (de uma história nova) precisa primeiro ser registrado em código (`Maps.InOrder` e um `Entry` em
  `MapSourceFiles.cs` — veja a skill `unice-new-story`) antes de aparecer no menu suspenso do editor. Depois disso, dá
  para arrastar a geometria em vez de digitar as coordenadas.
- **Arte nova** (um móvel ou piso sem arte pintada ainda) não é desenhada — o editor mostra um retângulo liso até a arte
  existir em `Resources/Art`.
