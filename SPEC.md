# Spec: Unice (projeto)

## Objective
Jogo mobile multiplayer de "esconde-esconde" (host-autoritativo): até 8 jogadores (humanos e bots) entram numa sala; um deles é sorteado "Unice" (a caçadora) e os outros se escondem, com uma boneca amaldiçoada que dá risadinhas e entrega quem a carrega. Fluxo: Home → (escolha da história) → Lobby → Hide → Hunt → Result → Final, repetindo por `totalRounds` rodadas. Port 1:1 da versão web — mesmas regras, mesmo ritmo (`Domain/Match/Game.cs`).

### Regras principais
Valores em `Domain/Match/Rules.cs` (o host pode mudar tempo de esconder, de caça e intervalo da risadinha na sala de espera).
- **Rodada:** fase de esconder (30 s, a Unice espera do lado de fora de olhos fechados) e fase de caça (120 s, a Unice entra pela porta da frente).
- **Boneca:** sorteada para um escondido no início da rodada. Na fase de caça, enquanto quem a carrega está escondido, ela dá uma risadinha a cada 10 s e a Unice ouve onde foi. Quem a carrega pode passá-la para outro escondido a até 60 px (espera de 5 s entre passes).
- **Rir de propósito (R):** qualquer escondido pode rir, **com ou sem a boneca** — mas nem sempre: só durante a rodada, nunca a Unice nem quem já foi pego, e com 5 s de espera entre uma risada e outra. A Unice ouve a risada igual à risadinha da boneca (entrega a posição); se quem riu está com a boneca, o contador da próxima risadinha recomeça.
- **Captura:** a Unice pega quem ela encosta (30 px) ou abre o esconderijo onde a pessoa está. Pegou quem tinha a boneca → a Unice vence a rodada (+3). Pegou outra pessoa → ela sai da rodada (Unice +1) e a Unice fica 2,5 s parada.
- **Tempo esgotado:** o time vence — cada escondido +2, quem terminou com a boneca +1.
- **Próxima Unice:** quem foi pego com a boneca; se o tempo acabou, quem foi Unice menos vezes. Com 2+ pessoas na sala, só pessoas são sorteadas (bots não).

### Histórias (mapas)
A ideia do projeto: **cada história que a pessoa conta vira um jogo de esconde-esconde** (o lugar vira o mapa, as pessoas viram os escondidos, quem procura vira "a Unice"). As regras da partida não mudam; muda o cenário, os nomes e os textos.
Cada **história** (`Domain/Maps/Stories.cs`) é um mapa mais uma narrativa; o jogador escolhe a história antes de jogar. A escolha define `Game.mapId` (o host decide; os convidados recebem no snapshot).
- **Menu:** "Jogar solo" e "Criar sala" abrem o seletor de histórias (`StoryPicker`, uma carta por história); entrar numa sala por código não passa por ele. O botão "CRÉDITOS" abre a tela de créditos (`CreditsScreen`): a história do jogo, a homenagem à tia Unice (com a foto dela, `Art/tia_moldura`) e quem fez o quê, em texto que rola sozinho (segurar acelera). Os textos são `credits.*` em `Localization.Strings`. A sala de espera mantém o seletor ‹ › de mapa para o host trocar.
- **Histórias:** `doll` ("A Boneca", casa da vó — lançada), as próximas histórias entram pelo processo SDD abaixo. Histórias com `released: false` aparecem como "Em breve" no seletor e só são jogáveis no Editor ou em build de desenvolvimento (`Debug.isDebugBuild`).
- **Vídeo de abertura** (`Story.introVideo`/`introSeconds`, `Game.InIntro`): na rodada 1, todos os jogadores veem o vídeo da história ao mesmo tempo (até 30 s) antes de a rodada começar; o host soma `introSeconds` ao `timeLeft` da primeira fase de esconder (nada novo no snapshot: `InIntro` = fase Hide com `timeLeft > hideTime`), e durante esse tempo só esse relógio corre — ninguém se mexe, bots incluídos. Sem o arquivo, nada ocupa a espera: a partida simplesmente começa. Os escondidos jogam sem interrupção. A HQ de introdução que existia no lugar do vídeo foi removida, sem substituto por enquanto.
- **Criar uma história nova (SDD):** a pessoa conta a história e o Claude, com a skill `unice-new-story` (`.claude/skills/unice-new-story/`), segue o processo de `docs/stories/README.md`: (1) escreve a spec `docs/stories/<id>.md` (modelo em `_template.md`), (2) desenha o mapa (regras de geometria em `resources/map-design-guide.md`), (3) implementa `Domain/Maps/Map<Nome>.cs`, 1 linha em `Maps.InOrder`, 1 entrada em `Stories.All` (cartão) e os textos pt/en/es, (4) roda os testes. `MapDefinitionsTests`, `LocalizationTests` e `StoryContentTests` varrem todo mapa/história registrado, então a história nova é validada sem escrever teste. Mapas novos usam arte lisa (cores) até a arte pintada existir; por isso nascem `released: false`, a não ser que a pessoa peça para lançar.

Sucesso = paridade de regras com a versão web, build Android estável via CI, e baixa latência percebida no sync de posição/estado entre host e peers.

## Experiência do jogador (intuitividade)
Quem abre o jogo pela primeira vez, sem ler regra nenhuma, deve conseguir jogar uma rodada inteira. Princípios (valem para toda tela e mudança nova):
1. **Um objetivo por tela.** Cada tela tem uma ação principal óbvia (botão grande, com cor/realce); as secundárias ficam menores (`mbtn-sm`, `mbtn-back`). Nada de menu com mais de ~5 escolhas no mesmo nível.
2. **O jogo ensina jogando.** O objetivo do papel aparece no cartão do papel (`RoleCard`, recolhe em 10 s); a dica de ação aparece só quando a ação existe (`ActionHints`: "E esconder embaixo da cama"); a história explica o porquê sem pedir leitura de regras. "Como jogar" é consulta, nunca obrigação.
3. **Sempre dizer o que está acontecendo e o que fazer agora.** Cada fase tem um aviso claro (banner, contagem, legenda de sons ouvidos mas não vistos); quem espera (a Unice no esconder) sabe quanto falta mesmo de olhos fechados (a tela preta tem sua própria contagem); na rodada 1, antes disso, todo mundo vê junto o vídeo de abertura da história, quando ela tem um.
4. **Controles previsíveis.** Toque: joystick à esquerda, botões de ação à direita, só os que servem naquele momento. Teclado/mouse: dicas de tecla na tela. Troca automática pelo último dispositivo usado (`InputMode`).
5. **Fácil de voltar e de errar.** Todo sub-menu tem "Voltar"; o botão voltar do sistema volta uma página (`HomeMenu.Back`); sair/pausar nunca perde a partida sem aviso; erro de rede aparece na página onde a pessoa está (ex.: "Criar sala" volta para a página online).
6. **Feedback imediato.** Todo toque/ação tem resposta visual ou sonora em até 100 ms (botões com `:hover/:active`, ondas de som, vibração visual); nada de espera sem indicação (estado "conectando…").
7. **Legível em qualquer tela.** Alvos de toque ≥ 64 px do painel de 540 px (≈ 48 dp), texto ≥ 13 px (no celular e no tablet o painel tem 540 px de altura; no computador ele cresce com a janela, de 540 a 900 px entre janelas de 540 e 1080 px de altura, para os botões não parecerem de tablet em tela grande: `Gameplay/PanelScale.cs`), respeitar a área segura (notch), três idiomas (pt/en/es) sem texto cortado.
8. **Sem beco sem saída para quem joga sozinho.** Solo com bots é sempre possível; online é opcional.

Critérios de aceite de usabilidade (verificação manual, em celular real e no desktop):
- Do ícone do app até estar escondido numa rodada solo: **≤ 4 toques** (Jogar solo → história → Pronto/Começar → mover).
- A primeira rodada é jogável sem abrir "Como jogar"; o primeiro papel (Unice ou escondido) é entendido pelo cartão do papel e pelas dicas.
- Trocar de história, voltar e trocar de idioma funcionam em qualquer página sem reiniciar.
- Nenhum texto sobreposto ou cortado em 16:9, 20:9 e 4:3, nos 3 idiomas (`Core/Dev/LayoutShots` gera as capturas).

## Tech Stack
- Unity **6000.6.3f1**, C#, Universal Render Pipeline 17.6.0
- Netcode for GameObjects 2.13.3 + Unity Services Multiplayer 2.3.3 (transporte/relay)
- Firebase 13.17: Analytics (GA4) e Crashlytics integrados via `Core/Analytics/Metrics.cs` (catálogo de eventos em `docs/ANALYTICS.md`); App Check instalado, não usado no código
- Google Play Games plugin for Unity 2.3.0 (`Assets/GooglePlayGames`): login automático no Android, com login anônimo de reserva
- Unity Input System 1.20.0
- Plataformas alvo: Android (prioridade, publicado no Google Play), iOS, Windows, macOS e Linux, mais WebGL. Ligadas em `.github/build-targets.yml`.
  - Play Games e as barras do sistema são só do Android (`#if UNITY_ANDROID`); fora dele o login é anônimo.
  - Crashlytics só existe no Android e no iOS; no desktop o Firebase faz só Analytics (`Core/Analytics/Metrics.cs`).
  - Entrada por toque ou teclado/mouse, conforme o último dispositivo usado (`Core/Ui/InputMode.cs`).
  - Windows, macOS e Linux são gerados no runner Linux (Mono), sem assinatura; o iOS sai como projeto Xcode, assinado e publicado à mão.

## Commands
Não há CLI local de build/test (projeto Unity puro, sem Node/Make). Fluxos reais:

- **Build debug (CI, PR para develop/main/master):** `.github/workflows/debug.yml` → `game-ci/unity-builder@v4`, `targetPlatform: Android`, `customParameters: -quit -batchmode -logFile ...`
- **Testes no CI:** `.github/workflows/tests.yml` (chamado por `debug.yml` e `release.yml` antes do build)
- **Release (Google Play):** `.github/workflows/release.yml`
- **Versionamento (tags):** `.github/workflows/version.yml`
- **Local:** abrir o projeto no Unity Editor `6000.6.3f1`, abrir a cena principal, Play ou `File > Build Settings` para Android.
- **Testes:** `Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml` (ou Test Runner no Editor). No CI, rodam no `tests.yml` com cobertura — ver Testing Strategy.

## Project Structure
```
Assets/Unice/Scripts/Domain/  → regras puras, sem UnityEngine (assembly Unice.Domain, testado em Edit Mode)
  Match/    → Game (regras e partida; Game.Snapshot = estado enviado host→convidados), MatchHost (passo do host,
              comandos dos outros jogadores, assumir como host), Character, Rules, Kit,
              GameEvent, RoundResult, DollState, Phase, PlayerAction, Achievements (conquistas/ranking Play Games)
    Bots/   → HiderBot, UniceBot (IA dos bots), BotBrain (memória do bot), BotGoal, BotPath (andar pelo caminho)
  Maps/     → Map (geometria, colisão, linha de visão), Navigation (busca de caminho), MapParts, definições dos mapas, Stories (histórias = mapa + vídeo de abertura)
  Online/   → porta de rede (INetBackend), Presence/PeerInfo, PresenceCodec, HostElection
  Common/   → Point, Geometry, GameRandom, TextFormat
  Tests/    → testes Edit Mode do domínio (MapDefinitionsTests valida o layout de todo mapa)
Assets/Unice/Scripts/Gameplay/ → regras da partida do lado do jogador, sem UnityEngine (assembly Unice.Gameplay, testado em Edit Mode)
                  Perspective (o que o jogador vê e sabe), PlayerController (movimento, barulho e ações E/Q/R),
                  EventFeed (eventos → banners/sons/efeitos), HudModels (textos e medidores dos cartões do HUD), HideComic (quando o vídeo de abertura aparece),
                  SoundMath (alcance, volume, pan e descrição de um som), Ports (IStage, IMatchContext: o que a UI,
                  o áudio e a analítica implementam; BannerStyle, RippleKind, StageColor)
  Tests/        → FakeStage (grava o que as regras pediram) e os testes do módulo
Assets/Unice/Scripts/Audio/    → som sem UnityEngine (assembly Unice.Audio, testado em Edit Mode)
                  ToneSynth (gera as amostras), SfxCatalog (os sons), SfxMix (ganho e voz), Soundscape (sons ouvidos no
                  mundo, passos, relógio, contagem), Heartbeat; ISoundOutput é a porta para alto-falante, seta e legenda
Assets/Unice/Scripts/Core/    → adapters Unity (assembly Unice.Core)
  App/          → App (monta as peças e roda o loop), ProfileStore, SystemBars
  Gameplay/     → PlayerControl (lê teclado e joystick), AppStage (liga IStage/IMatchContext/ISoundOutput à UI, ao mundo, ao som e à analítica)
  Online/       → RoomSession (criar/entrar/sair, lobby), RoomSync (eleição de host, snapshots, presença)
  World/        → WorldView, MapScenery, CharacterView, KidSprites, RippleEffects, FogView, CameraRig, Art, ArtManifest
  Audio/        → Sfx (toca nos alto-falantes), ClipSynth (amostras → AudioClip)
  Localization/ → módulo próprio (assembly Unice.Localization): Localization (lógica) e Localization.Strings (tabela pt/en/es);
                  Tests/ confere chaves, idiomas, placeholders e textos de cada mapa
  Analytics/    → Metrics (Google Analytics + Crashlytics via Firebase)
  Ui/           → Ui (troca de telas), UiKit/UiPart (base), SettingToggles, KeyboardLift, CharacterPortrait
    Screens/    → HomeMenu, StoryPicker (escolha da história), CreditsScreen (créditos e homenagem), KidCustomizer, KitOptionPicker, LobbyScreen, MatchOptions, ResultScreen, PauseMenu
    InGame/     → Hud, MatchCard, RoleCard, PlayerListPanel, MiniMap, Messages, HideComicView (o vídeo de abertura na tela da Unice), WorldLabels, SoundCues,
                  ScreenEffects, TouchControls, Joystick, ActionIcons, ActionHints
  Tests/        → testes Edit Mode do Core
Assets/Unice/Scripts/Net/     → implementações Unity Services: NgoBackend (Netcode), NetPlayer,
                                PlayerSignIn (login único: Google Play Games ou anônimo — docs/PLAY_GAMES.md),
                                PlayGamesProgress (envio de conquistas/ranking e os IDs do Play Console)
Assets/Unice/Scripts/Editor/  → utilitários de editor (incl. Net/Editor)
Assets/Unice/Resources/       → UI (USS/uxml), Art, Fonts, config de rede
docs/stories/                 → spec de cada história (README = processo SDD, _template.md, <id>.md)
.claude/skills/               → skills do Claude neste projeto (unity-code, unice-test-design, unice-run-tests, unice-new-story)
ProjectSettings/, Packages/   → configuração Unity e dependências
.github/workflows/            → CI/CD (debug, pulls, release, version) — doc em .github/workflows/README.md
```

## Code Style
Exemplo real do estilo adotado (`Domain/Match/Rules.cs`):
```csharp
/// <summary>The tuning numbers of the game: times in seconds, distances and speeds in house pixels. Same as the web version.</summary>
public static class Rules
{
    /// <summary>The doll giggles this often while its holder is hidden.</summary>
    public const float GiggleInterval = 10;
    /// <summary>The Unice catches whoever it gets this close to.</summary>
    public const float CatchRange = 30;
}
```
Convenções (o objetivo é que qualquer engenheiro entenda o código sem ajuda):
- **Nomes descritivos em inglês** para classes, métodos, campos, parâmetros e variáveis (`timeLeft`, `hidingSpot`, `PassTarget`). Nada de abreviações (`tl`, `ph`, `r`, `e`); exceções aceitas: `x`/`y`, `dt`, `i`/`j` em laços, `id`, `me` (o personagem deste aparelho) e `ax, ay, bx, by` para pontos A e B.
- **Uma responsabilidade por classe**, em pastas por assunto (ver Project Structure). Quando uma classe começa a fazer duas coisas, ela é dividida — ex.: `Hud` só monta `MatchCard`, `RoleCard`, `PlayerListPanel`.
- **Comentários `/// <summary>`** em toda classe e em todo membro cujo propósito não seja óbvio pelo nome; explicam o *porquê* e as unidades (segundos, house pixels).
- **Números mágicos viram constantes nomeadas** (`const float StunTime = 2.5f`), com comentário quando a unidade ou o motivo não é óbvio.
- **Strings de estado viram enums** (`Phase`, `GameEventType`, `HiderMode`, `BannerStyle`, `RippleKind`, `UiScreen`).
- `sealed class` para tipos que não serão herdados; `partial` só quando a divisão em arquivos não pode virar classes (ex.: `Game` e `Game.Snapshot`).
- Sem regions, sem interfaces com uma implementação só (exceto a porta `INetBackend`, que existe para o jogo compilar sem o pacote de rede).
- **Nunca renomear** os campos de `ArtManifest.cs` (o `JsonUtility` casa pelo nome com `manifest.json`), as chaves de `PlayerPrefs` (`unice.name`, `unice.kit`, `unice.lang`, `unice.sound`) nem as chaves de `Localization.Strings`.
- Mudou o formato de bytes do snapshot ou da presença? Suba `Game.SnapshotVersion` / `PresenceCodec.Version`.

## Testing Strategy
- Testes Edit Mode (NUnit, Unity Test Framework) em `Domain/Tests` (regras, IA dos bots, snapshot, eleição de host, mapas, histórias), `Gameplay/Tests` (eventos, ações, HUD, vídeo de abertura, som), `Audio/Tests` (síntese, mixagem, soundscape, batimento), `Core/Localization/Tests` (textos) e `Core/Tests` (perfil, conteúdo das histórias).
- Lógica nova de jogo vai para `Gameplay` ou `Audio` (sem Unity, atrás de `IStage`/`ISoundOutput`); o `Core` fica só com o que toca a Unity. É assim que ela ganha teste.
- Rodar: `Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml` (ou o Test Runner no Editor).
- Os testes do `Game` são de caracterização: fixam o comportamento atual (paridade com a versão web). Se um falhar depois de uma mudança em `Game.cs`, a regra mudou — confirme se era a intenção.
- Cobertura: o `tests.yml` falha se a cobertura de linhas do `Unice.Domain` ficar abaixo de 85% (o `Unice.Core` entra no relatório, mas não no gate). Medido localmente: `Unice.Domain` 95%, `Unice.Gameplay` 99%, `Unice.Audio` 100%, `Unice.Localization` 100%, `Unice.Core` <1% (UI Toolkit, mundo e `App`).
- UI, rede e áudio não têm teste automatizado: verificar manualmente (Home→história→Lobby→Hide→Hunt→Result→Final; sala online com dois aparelhos). Conferir também o vídeo de abertura (ser a Unice na rodada 1; tocar para pular) e os critérios de usabilidade acima.

## Boundaries
- **Sempre fazer:** manter paridade com a versão web ao tocar em `Game.cs`/`Rules`/`Bots`; validar build Android (CI) antes de merge; seguir as convenções de Code Style (nomes descritivos, uma responsabilidade por classe, comentários); rodar os testes Edit Mode; atualizar `docs/ANALYTICS.md` ao mudar eventos e `docs/ARCHITECTURE_AUDIT.md`/Project Structure ao mudar a estrutura.
- **Sempre fazer (segurança):** manter as ações do GitHub fixadas por SHA; nunca enviar para a faixa de produção pelo CI; conferir as permissões do AAB depois de mudar pacotes (`docs/SECURITY.md`).
- **Perguntar antes:** mudar versão do Unity ou de pacotes em `Packages/manifest.json`; alterar `ProjectSettings/` (ex.: `GvhProjectSettings.xml`, `ProjectSettings.asset`) fora do escopo pedido; mudar workflows de CI/CD; adicionar nova plataforma de build; mudar o backend de rede (`NgoBackend`/Unity Services Multiplayer).
- **Nunca fazer:** commitar segredos/chaves do Firebase ou assinatura Android; editar `Library/`, `Temp/`, pastas geradas pelo Unity; remover testes sem aprovação; quebrar o "straight port" das regras sem alinhar com a versão web primeiro; mudar o formato do snapshot/presença sem subir a versão (`Game.SnapshotVersion` / `PresenceCodec.Version`).

## Success Criteria
- Build Android gerado com sucesso pelos workflows `debug.yml`/`release.yml`.
- Fluxo completo de partida (Home→história→Lobby→Hide→Hunt→Result→Final) funcional em host e peers via `NgoBackend`.
- Analytics e Crashlytics enviando dados de builds em aparelho, sem bloquear o jogo se o Firebase falhar.
- Testes Edit Mode passando e cobertura do `Unice.Domain` ≥ 85%.
- Nenhuma regressão de paridade de regras em relação à versão web.

- Os critérios de usabilidade de "Experiência do jogador" cumpridos em aparelho real.

## Open Questions
1. Há plano para iOS, ou Android continua sendo a única plataforma alvo?
2. O Firebase App Check (instalado) deve ser ativado?
3. Vale investir num `INetBackend` falso para testar a troca de host em Edit Mode (hoje só manual)? Ver `docs/ARCHITECTURE_AUDIT.md`.
4. Quando uma história nova ganhar arte pintada, ela ganha também cordel próprio (hoje só `doll` tem a tela do cordel)?
