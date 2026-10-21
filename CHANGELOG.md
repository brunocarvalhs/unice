# Changelog

Todas as alterações relevantes do projeto serão documentadas neste arquivo.

## [Unreleased]

## [0.2.0] - 2026-10-21

### Commits
* Fix duplicate panelSettings field and duplicate test after the develop merge
* Remove voice chat (Vivox) entirely for Lei 15.211/2025 (ECA Digital) compliance
* feat: create flow
* Story cards: dim unreleased stories everywhere, not just in real builds — lobisomem and rua now look visibly "off" in the Editor/dev builds too, where they stay clickable for testing
* Remove the intro comic (HQ): the 5-panel page the Unice read while the others hid is gone, no replacement — a story with no opening video now just shows the plain black "eyes closed" screen for the whole hiding phase
* Dev tool (like LayoutShots): KitPreviewShots opens "Minha criança" and screenshots a handful of kits, one per special drawing behavior, to check the catalog migration visually without a human
* docs/KIT_EDITOR.md: how to add a color, hairstyle, outfit, hat or accessory from the Inspector (no code), and the rules that keep it safe (8-per-slot cap, add at the end only)
* Character kit (skin, hair, outfit, hat, accessory and their colors) moves from hardcoded Kit.cs literals to a KitCatalogAsset ScriptableObject, applied at boot — a new option (with its art dropped into Resources/Art and a row added to the asset in the Inspector) needs no code, no Localization.Strings.cs edit
* Map editor, stage 4: docs/MAP_EDITOR.md (how to drag, validate and save) and the unice-new-story skill points to it once a new map is registered
* Map editor, stage 3: live validation (MapValidation, shared with MapDefinitionsTests' rules) blocks Salvar on a broken map; the house's rooms stay synced to their painted floor in manifest.json on save, and the canvas shows the real floor art instead of flat rectangles
* Map editor, stage 2: a draggable EditorWindow (Unice > Editor de mapas) — canvas with zoom/pan/select/drag/resize and 8px grid-snap, undo/redo, a properties panel, add/delete, and a save button that writes the map back through MapSourceFiles
* Map editor, stage 1: MapCodeWriter turns a MapDefinition back into its Map<Name>.cs file; the three maps are regenerated (no data changes) via the new Unice > Mapas > Regerar código dos mapas menu item
* Characters without the pixel look: mipmaps and trilinear filtering on the kid, Unice and doll sprites (their 156x204 textures were shrunk on screen with jagged edges)
* Opening video for everyone in the first match (the match waits for it, skippable by tap, Enter, Space or gamepad A; the host's skip ends it for all); no comic over the video; later rounds and matches have none
* Intro video: source set to Url, sound through an AudioSource (Direct output played silently), longer prepare timeout and logs of the path, size, audio tracks and failures
* Match options (one Stepper template used per option) and the kid customizer in UXML, editable in the UI Builder
* Player, bots and story cards in UXML (one file each, repeated by the code); the Editor opens the home menu in the UI Builder when the project opens
* Story video for the doll (Videos/doll.mp4) brought from main, so the intro plays on this branch instead of the comic fallback
* Menu pages, waiting room, result, pause, HUD and touch buttons laid out in UXML (editable in the UI Builder); the code binds names, texts and clicks
* Home menu main page laid out in UXML (editable in the UI Builder); the code binds names, texts and clicks
* Desktop layout fixes: pause sheet capped in width, four result cards per row, waiting-room note above the poster ground, room for long labels in My child
* Gamepad buttons (A, X, Y) in the action hints instead of keyboard keys, and the controls rule mentions the gamepad
* Buttons sized by the screen's physical height (min phone, max 2x; TVs at the max), 4:3 menu without overlaps, gamepad for play and menus
* Layout by device: touch/pointer and wide/normal/boxy classes on the root; house plan and players docked under the match card with keyboard and mouse
* Credits page with the aunt's photo replaces the animated story; new app icon and styled scrollbars
* CI: turn off the runner's existing swap before creating ours (fallocate failed with 'Text file busy')

## [0.1.0] - 2026-10-20

### Commits
* Ignore the Remotion project folder (unice-trailer/: node_modules, renders and the ElevenLabs key file)
* UI size follows the window on a computer: the panel grows from 540 to 900 px tall (controls up to 40 % smaller on a big screen), phones and tablets keep 540
* Characters use the normal sprite material again: no pixelation shader
* CI: swap and a second try when Unity is killed (exit 137) on a platform build, with the kernel's out-of-memory log
* Credits screen in place of the story: the game's story, a tribute to aunt Unice with her photo, and the credits, as text that scrolls by itself
* Remove the cordel story screen from the menu: the story is told by the opening video in the match
* The game no longer opens on the story the first time: the cordel is only shown from the "A HISTÓRIA" button
* Music only in the main menu (not the waiting room); match sounds unchanged
* No music during the match: the menu theme is the only music
* Menu music: forró track from Resources/Audio (the synthesized theme stays as fallback); soundtrack for the match; silence during the opening video
* New app icon: cordel sun, cacti and the UNICE lettering
* Opening video for every player: the first round waits for the story video (the match is frozen while it plays), the Unice sees the comic afterwards
* Intro video for the Unice while the others hide, with the comic as fallback when the file is missing
* Tests for the bots' decisions (domain, gameplay and localization at 100% of lines); remove unused USS classes, strings and code
* Cordel on the result and final screens (map only while the match is live); giggle meter beside the clock, doll holder no longer shown
* Cordel style for the pause sheet and the match HUD: paper cards, ink borders, Rye lettering, touch controls
* Cordel theme through the waiting room: shared backdrop, paper cards, no moving house map until the match starts
* Metas for the cordel backdrop script and art
* Menu: animated full-bleed cordel backdrop, no house map behind the home screen, new subtitle
* Menu as a cordel poster: full-screen sand background with cacti, birds and stars, paper cards, Rye lettering, story picker that fits narrow screens
* Skill unice-new-story: pictures as input, holes for places you cannot enter, Unity-free map check
* Story "A Rua da Vó": the town square map from the author's satellite print, story card and intro comic
* Splash: author name as Brunocarvalhs
* Branding: app icon, Android adaptive icon and splash with the author and Unity logo
* Lobisomem map redone from the author's floor plan: house hole, lit balcony, 14 hiding spots
* Story "O Lobisomem": the night yard map, story card and intro comic
* Stories: story picker, intro comic for the Unice and the SDD to turn stories into maps
* Audio module: synthesis, mixing, soundscape and heartbeat without Unity
* Gameplay: the HUD cards' numbers and words as tested models
* Gameplay module: the match rules out of Core, behind IStage
* Split the text table into its own module, Unice.Localization
* HUD: fold the goal away after 10 s, drop the room line, stronger doors
* HUD: a lighter in-game layout
* Map: make the doorways stand out
* Docs: art direction for the northeastern look
* Dev: layout screenshots in touch and keyboard/mouse modes
* Joystick: ignore a release when no finger is down
* Story: fix the tint property name in the backdrop and vignette styles
* Platforms: iOS, Windows, macOS and Linux next to Android
* Input: follow the last device used instead of the platform
* Menu: sound and language beside quit, as icons
* Add the Unity meta file for StoryScreen
* Story: replace the scrolling text panel with a full-screen animation
* feat(art): visual nordestino (cordel/chita) e a história em cordel no menu
* ci: version by Conventional Commits with CHANGELOG entry (script from cestou)

### Adicionado
- Identidade visual (`Assets/Unice/Branding/`): ícone do app com a marca "Unice" (desktop, iOS e Android adaptativo: fundo + marca dentro da área segura) e abertura do jogo: "Bruno Carvalho apresenta", a marca Unice e depois o logo da Unity.
- Criação de histórias por especificação (SDD): `docs/stories/` (processo, modelo e a spec de `doll`) e a skill `unice-new-story`, com a qual o Claude transforma uma história contada em spec, mapa novo, cartão, HQ e textos pt/en/es. Novos testes (`MapDefinitionsTests`, `StoryContentTests`) validam todo mapa e história registrados.
- História "A Rua da Vó" (`rua`): a praça da cidade do autor (a partir de um print de satélite): duas ruas em volta de uma ilha de árvores e um largo, casas e igreja proibidas, cruz, orelhão e postes; 19 esconderijos e HQ de 5 quadros ("Em breve", só no Editor e em builds de desenvolvimento).
- História "O Lobisomem" (`lobisomem`): o quintal da tia à noite, desenhado a partir da planta do autor: a casa é um buraco que não se entra nem se enxerga através, a sacada é a única área iluminada, 14 esconderijos e HQ de introdução; `Room` ganhou cor de chão própria (`tint`); nasce como "Em breve" (arte lisa), jogável no Editor e em build de desenvolvimento.
- Seletor de histórias: "Jogar solo" e "Criar sala" passam por uma tela de cartas; cada história define o mapa (hoje só A Boneca = a casa).
- HQ de introdução: enquanto os outros se escondem, quem é a Unice lê uma HQ de 5 quadros da história da boneca (sempre na rodada 1; depois só se nunca viu). Toque para pular.
- Versão web (WebGL) publicada no GitHub Pages a cada release, com os mesmos assets, regras e salas online do app: quem está no navegador joga com quem está no celular. No navegador não há Firebase (analytics/Crashlytics) nem botão "Sair".
- Google Analytics (GA4) e Crashlytics via Firebase: eventos de partida, sala e ações do jogador; erros tratados enviados como não-fatais (catálogo em `docs/ANALYTICS.md`).
- Configuração do Claude Code para o time: plugin oficial da Unity habilitado em `.claude/settings.json` e skills do projeto em `.claude/skills/` (`unity-code`, `unice-test-design`, `unice-run-tests`).
- Testes Edit Mode da regra de eleição de host (`HostElectionTests`) e do passo do host (`MatchHostTests`).

### Removido
- Os mapas de rascunho Quintal e Praça (salas desconectadas, sem arte) e seus textos; histórias novas entram pelo processo de `docs/stories/`.
- A HQ de introdução (os 5 quadros desenhados que a Unice lia enquanto os outros se escondiam): sem substituto por enquanto — uma história sem vídeo de abertura agora não mostra nada, só a tela preta de olhos fechados. O vídeo de abertura continua como estava.
- **Chat de voz (Vivox) removido por completo** (pacote, código `IVoiceChat`/`VivoxVoice`, botões de microfone/sussurro/silenciar, campos de voz no protocolo de presença e as permissões de microfone/Bluetooth do Android): a Lei 15.211/2025 (ECA Digital, "Lei Felca") exige verificação de idade e consentimento parental para chat de voz entre usuários, que o jogo ainda não implementa. **Formato de presença na versão 3** (bump do `PresenceCodec.Version`): aparelhos com builds anteriores não jogam na mesma sala que este.

### Alterado
- Código reorganizado para manutenção: nomes descritivos em inglês, comentários em todas as classes, uma responsabilidade por classe e pastas por assunto (ver `SPEC.md` → Code Style e Project Structure, e `docs/ARCHITECTURE_AUDIT.md`).
- A IA dos bots, a busca de caminho e a parte do host saíram de `Game`/`Map` para classes próprias (`Bots/`, `Navigation`, `MatchHost`).
- **Formato do snapshot de rede na versão 5:** aparelhos com builds anteriores não jogam na mesma sala que este.

### Segurança e Google Play
- Release vai para a faixa de **teste interno como rascunho** (antes ia direto para produção) e gera AAB de fato (`androidExportType`).
- Ações do GitHub fixadas por SHA em todos os workflows.
- Removidos o ID de publicidade (manifesto próprio), o Unity IAP e o Firebase App Check, que não eram usados.
- Botão "Silenciar"/"Ouvir" para outros jogadores no chat de voz. **Formato da presença na versão 2** (builds anteriores não jogam com este).
- Checklist de privacidade e do Play Console em `docs/SECURITY.md`.

### Corrigido
- Campo do código da sala: agora aceita só letras maiúsculas e números enquanto se digita, e sobe acima do teclado do celular para o texto ficar visível.

### CI/CD e documentação
- Adicionado fluxo de versionamento com tags automáticas.
- Adicionado pipeline de build de APK de debug para Android.
- Adicionado pipeline de release para Google Play.
- Adicionado workflow de validação para pull requests.
- Documentação inicial do projeto e da estrutura de CI/CD.
- Documentação atualizada para a nova estrutura: `SPEC.md` (regras do jogo, convenções de código, testes), `README.md`, `docs/ARCHITECTURE_AUDIT.md` e o novo `docs/ANALYTICS.md`.

## [1.0.0] - 2026-10-01

- Setup inicial dos workflows Unity para Android no GitHub Actions.
- Ajuste da pipeline de versionamento de tags.
- Criação da documentação base do projeto.
