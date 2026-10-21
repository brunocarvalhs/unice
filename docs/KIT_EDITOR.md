# Aparência dos personagens editável no Unity

As opções de visual (pele, cabelo, cor do cabelo, roupa, cor da roupa, chapéu, acessório) vêm de um asset:
`Assets/Unice/Resources/Kit/KitCatalog.asset`. Veja o plano completo em [`KIT_EDITOR_PLAN.md`](KIT_EDITOR_PLAN.md).

## Adicionar uma cor nova (pele, cabelo ou roupa)

1. Selecione `Assets/Unice/Resources/Kit/KitCatalog.asset` no Project e olhe o Inspector.
2. Abra a lista certa (`Skins`, `Hair Colors` ou `Outfit Colors`).
3. Clique **+** no fim da lista, dê um `Id` (um nome curto, ex. "rose") e o `Hex` (ex. `#f2a4c0`).
4. Salve a cena/asset (Ctrl+S). Pronto — sem tocar em código.

## Adicionar um cabelo, roupa, chapéu ou acessório novo

1. Desenhe a arte seguindo o padrão de nome já usado pelos outros (veja os arquivos existentes em
   `Assets/Unice/Resources/Art` para o seu tipo — por exemplo, um 5º corte de cabelo é `kid_hair4_s.png` e
   `kid_hair4_b.png`, já que hoje há 4 cortes, índices 0 a 3). Solte os arquivos em `Resources/Art`.
2. No `KitCatalog.asset`, abra a lista certa (`Hairs`, `Outfits`, `Hats` ou `Accessories`).
3. Clique **+** no fim da lista: `Id` (um nome interno, não aparece no jogo), e os rótulos `Label Pt`, `Label En`,
   `Label Es` (o que o jogador vê). Deixe `Special` como `None`.
4. Salve. A opção aparece na roda `‹ ›` da tela "Minha criança" com o rótulo certo, sem mexer em nenhum `.cs`.

## Regras importantes

- **No máximo 8 opções por lista.** O visual escolhido vai para a rede compactado em 3 bits por campo; o Inspector
  avisa se passar disso, e o jogo recusa carregar o catálogo se isso acontecer.
- **Só adicione no fim da lista.** O visual de cada jogador fica salvo no aparelho dele como um número (a posição na
  lista). Reordenar ou apagar uma opção existente troca a aparência de quem já jogou, silenciosamente.
- **"Special" é só para os seis comportamentos que já existem** (capa, bornal, óculos, boneca de pano, gorro,
  orelhinhas — e o boné, que ganha a cor da roupa) — cada um desenha uma camada extra ou um tingimento diferente no
  código (`KidSprites.cs`). Uma opção nova com um comportamento de desenho **novo de verdade** (não um desses seis)
  ainda precisa de código; isso é uma funcionalidade nova, não dá para generalizar num asset.

## Se o catálogo "sumir" ou ficar com erro

O jogo cai para os valores de hoje (`Kit.ApplyDefaults`) se o asset não carregar, e mostra um erro no Console — ele
não trava. Se precisar recriar o catálogo do zero com os valores originais, use
`Unice > Kit > Recriar catálogo padrão` (isso **apaga** qualquer opção que você tenha adicionado no Inspector).
