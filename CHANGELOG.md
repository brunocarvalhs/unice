# Changelog

Todas as alterações relevantes do projeto serão documentadas neste arquivo.

## [Unreleased]

### Adicionado
- Identidade visual (`Assets/Unice/Branding/`): ícone do app com a marca "Unice" (desktop, iOS e Android adaptativo: fundo + marca dentro da área segura) e abertura do jogo: "Bruno Carvalho apresenta", a marca Unice e depois o logo da Unity.
- Criação de histórias por especificação (SDD): `docs/stories/` (processo, modelo e a spec de `doll`) e a skill `unice-new-story`, com a qual o Claude transforma uma história contada em spec, mapa novo, cartão, HQ e textos pt/en/es. Novos testes (`MapDefinitionsTests`, `StoryContentTests`) validam todo mapa e história registrados.
- História "O Lobisomem" (`lobisomem`): o quintal da tia à noite, desenhado a partir da planta do autor: a casa é um buraco que não se entra nem se enxerga através, a sacada é a única área iluminada, 14 esconderijos e HQ de introdução; `Room` ganhou cor de chão própria (`tint`); nasce como "Em breve" (arte lisa), jogável no Editor e em build de desenvolvimento.
- Seletor de histórias: "Jogar solo" e "Criar sala" passam por uma tela de cartas; cada história define o mapa (hoje só A Boneca = a casa).
- HQ de introdução: enquanto os outros se escondem, quem é a Unice lê uma HQ de 5 quadros da história da boneca (sempre na rodada 1; depois só se nunca viu). Toque para pular.
- Versão web (WebGL) publicada no GitHub Pages a cada release, com os mesmos assets, regras e salas online do app: quem está no navegador joga com quem está no celular. No navegador não há Firebase (analytics/Crashlytics) nem botão "Sair".
- Google Analytics (GA4) e Crashlytics via Firebase: eventos de partida, sala e ações do jogador; erros tratados enviados como não-fatais (catálogo em `docs/ANALYTICS.md`).
- Configuração do Claude Code para o time: plugin oficial da Unity habilitado em `.claude/settings.json` e skills do projeto em `.claude/skills/` (`unity-code`, `unice-test-design`, `unice-run-tests`).
- Testes Edit Mode da regra de eleição de host (`HostElectionTests`) e do passo do host (`MatchHostTests`).

### Removido
- Os mapas de rascunho Quintal e Praça (salas desconectadas, sem arte) e seus textos; histórias novas entram pelo processo de `docs/stories/`.

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
