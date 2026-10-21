# Aparência dos personagens editável no Unity — plano

Objetivo: adicionar um cabelo, chapéu, acessório, roupa ou cor novo sem editar código — só soltar o arquivo de arte em
`Resources/Art` e adicionar uma linha num asset, no Inspector. `Kit.cs` (Domain) continua puro C# e testável; o asset
só alimenta os mesmos campos que ele já tinha.

## O que descobri no código de hoje

- `Kit.Skins/Hairs/HairColors/Outfits/OutfitColors/Hats/Accessories` são `string[]` fixos; `Kit.OptionCount[slot]` diz
  quantas opções cada campo tem (máximo **8**, porque o visual vai para a rede compactado em 3 bits por campo).
- `Kit.Hairs`, `Outfits`, `Hats`, `Accessories` (os nomes) **não são lidos por nada** — só documentação ("only for
  reference"). O jogo usa o **índice numérico** direto para montar o nome do arquivo (`kid_hair{N}_{view}.png`) e para
  buscar o rótulo traduzido (`kit.hair.{N}`) em `Localization.Strings.cs`.
- `KidSprites.Build` (que desenha o boneco) tem **seis casos especiais** amarrados a um índice fixo: roupa 4 = capa
  (desenha uma camada extra), acessório 1 = bornal, acessório 2 = óculos, acessório 3 = boneca de pano, chapéu 2 =
  gorro (com pompom extra, tingido da cor da roupa), chapéu 5 = orelhinhas (com camada extra, tingida da cor do
  cabelo). Toda outra opção de roupa/chapéu/acessório já funciona só pela convenção de nome de arquivo, sem caso
  especial.
- O visual escolhido é salvo no aparelho (`ProfileStore`, `PlayerPrefs`) como um número compactado. **Reordenar ou
  remover** uma opção existente muda silenciosamente a aparência de quem já jogou; **adicionar no fim** da lista é
  sempre seguro.

## Decisão

Um `ScriptableObject` (`KitCatalog`, em `Assets/Unice/Resources/Kit/KitCatalog.asset`) com uma lista por campo
(pele, cabelo, cor do cabelo, roupa, cor da roupa, chapéu, acessório). Cada opção de **cor** tem `id` + hex; cada
opção de **estilo** (cabelo/roupa/chapéu/acessório) tem `id` + rótulo em pt/en/es + um "comportamento especial"
opcional (nenhum, capa, bornal, óculos, boneca de pano, gorro, orelhinhas) — os seis casos de hoje, escolhidos por
`id` em vez de número cru, para reordenar no Inspector não trocar qual opção é "a capa" por engano.

No boot, o Core carrega o asset e aplica os valores nos campos (agora mutáveis) de `Kit.cs`, exatamente como o
`PanelSettings` já é carregado hoje para a UI. `KidSprites.Build` troca as seis comparações `== 4`/`== 1`/… por uma
consulta ao comportamento especial da opção escolhida. `Localization.KitOptionName` passa a usar o rótulo do próprio
asset quando existir, com o texto de hoje (`kit.*` em `Localization.Strings.cs`) como reserva — para as opções
antigas continuarem funcionando sem precisar migrar nada à força.

## Depois disso, para adicionar uma opção nova

1. Desenhe a arte seguindo a convenção (`kid_hair4_s.png`/`kid_hair4_b.png` para um 5º corte de cabelo, por exemplo)
   e solte em `Resources/Art`.
2. Abra `Assets/Unice/Resources/Kit/KitCatalog.asset`, adicione uma linha **no fim** da lista certa: id, rótulo em
   pt/en/es (e hex, se for cor).
3. Pronto — sem tocar em `.cs`. Um comportamento especial novo (uma camada extra, por exemplo) ainda precisa de
   código; isso é uma funcionalidade nova de desenho, não dá para generalizar sem especificar o que desenhar.

## Etapa única (mais documentação)

1. **`KitCatalog` + aplicação em `Kit`/`KidSprites`/`Localization`**: o asset, a migração dos dados de hoje para ele
   (sem mudar nenhum valor — mesmos ids, hex e rótulos), os testes provando que nada mudou no visual existente, e um
   aviso (`OnValidate`) se um campo passar de 8 opções.
2. **Documentação** (`docs/KIT_EDITOR.md`, feita à mão depois, sem agente): o passo a passo acima e o aviso sobre
   reordenar/remover.
