# Segurança, privacidade e Google Play — Unice

Revisão feita em 2026-10-02 antes do primeiro teste no Google Play. Mostra o que o app faz para proteger dados, o que foi
corrigido e o que precisa ser preenchido no Play Console.

**Atualização em 2026-10-20:** o chat de voz (Vivox) foi removido por completo — código, pacote, permissões e UI — em vez de
só desligado. Motivo: a Lei 15.211/2025 (ECA Digital, "Lei Felca") exige verificação de idade e consentimento parental para
chat de voz entre usuários menores de idade, que o jogo não implementa. O app não pede mais permissão de microfone.

## Resumo técnico

| Tema | Estado |
|---|---|
| Criptografia na rede | Multiplayer (Unity Relay / autoridade distribuída) usa **DTLS** por padrão no Android; Firebase usa HTTPS; tráfego sem criptografia (HTTP) bloqueado (targetSdk 37) |
| Dados guardados no aparelho | Só apelido, visual e idioma (`PlayerPrefs`) — nada sensível, sem necessidade de criptografia própria |
| Build | IL2CPP + R8/minify no release, só ARM64, não depurável, targetSdk 37 |
| Segredos | Keystore, senhas e conta de serviço do Play só nos *secrets* do GitHub; `.apk`/`.aab` fora do git |
| Dados de outros jogadores | Validados: remetente, tamanho máximo (presença 512 B, snapshot 8 KB), versão do formato, NaN, nomes limpos de caracteres invisíveis; o host rejeita teleporte e ações fora de alcance |
| Limite conhecido | Não há servidor dedicado: um host modificado consegue trapacear na partida. Aceitável para teste; resolver exigiria um servidor autoritativo |

## O que foi corrigido nesta revisão

- **Release ia direto para produção.** `release.yml` agora envia para a faixa **interna** como **rascunho**; a promoção é feita
  à mão no Console.
- **Release gerava APK em vez de AAB.** A entrada `androidAppBundle` não existe no `game-ci/unity-builder@v4`; trocada por
  `androidExportType: androidAppBundle`. O `versionCode` vem da versão semântica das tags (`v1.2.3` → `1002003`).
- **Ações do GitHub fixadas por SHA** em todos os workflows (elas recebem a keystore, a licença Unity e a conta do Play).
- **ID de publicidade removido.** O jogo não tem anúncios: `Assets/Plugins/Android/AndroidManifest.xml` (Custom Main Manifest)
  remove `AD_ID`, `ACCESS_ADSERVICES_AD_ID` e `ACCESS_ADSERVICES_ATTRIBUTION` e desliga a coleta do ID de publicidade do
  Firebase Analytics (`google_analytics_adid_collection_enabled=false`).
- **Pacotes sem uso removidos:** Unity IAP (trazia a permissão `BILLING`) e Firebase App Check (não era inicializado e levava o
  provedor de *debug* para o build).
- **Chat de voz (Vivox) removido por completo** (ver atualização de 2026-10-20 acima): pacote, código (`IVoiceChat`,
  `VivoxVoice`), botões de microfone/sussurro/silenciar e os campos de voz no protocolo de presença.

## Permissões do app (build de release)

| Permissão | Por quê |
|---|---|
| `INTERNET`, `ACCESS_NETWORK_STATE`, `ACCESS_WIFI_STATE`, `WAKE_LOCK` | Salas online, Firebase |
| `BIND_GET_INSTALL_REFERRER_SERVICE` | Firebase Analytics (origem da instalação) |

Depois de cada mudança em pacotes, confira o APK/AAB gerado:
`aapt2 dump badging app.apk | grep uses-permission` (o `aapt2` está em `PlaybackEngines/AndroidPlayer/SDK/build-tools`).

## Keystore e secrets do GitHub

O app é assinado com uma keystore que **nunca entra no repositório**. Com o Play App Signing, ela é a **chave de upload**:
o Google guarda a chave que assina o app de verdade, e a chave de upload pode ser trocada pelo Console se for perdida ou
vazada.

- **A pipeline é a dona da assinatura.** A keystore (em base64), as senhas e o alias vivem só nos secrets do GitHub;
  o `release.yml` assina o AAB com eles. Nenhuma máquina de desenvolvimento precisa da keystore.
- **Keystore atual (gerada em 2026-10-02):** alias `unice`, RSA 4096, PKCS12, válida até 2054. Certificado da chave de
  upload: SHA-1 `27:C2:90:59:F9:71:5A:94:4E:D9:1F:5E:9A:A4:90:0D:71:06:4A:93`.
- **Cópia temporária:** a keystore foi gerada em `~/.android-keystores/unice/release.keystore` (Mac do Bruno) com a senha
  no Keychain ("Unice release keystore"). Ela só existe para reconfigurar os secrets caso o primeiro build de release no
  CI falhe por causa deles; **apague as duas depois do primeiro build de release bem-sucedido.** Perder a chave depois
  disso não é grave: com o Play App Signing a chave de upload pode ser trocada pelo Console.
- **Primeiro envio:** rode o workflow Release; sem o secret `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON` o envio ao Play é pulado e
  o AAB assinado fica no artefato `Unice-release-aab` — baixe-o e envie pelo Console. Depois, configure o secret e o CI
  passa a enviar sozinho para a faixa interna.
- Qualquer keystore de release serve (um `release.keystore` gerado com `keytool`). O ideal é **uma keystore só para o
  Unice**: se ela vazar ou se perder, só este app é afetado. Usar a mesma de outro app funciona, mas liga os dois.
- Guarde a keystore e as senhas num cofre de senhas, com backup — o CI só tem a cópia em base64 nos secrets.
- Build local assinado (ex.: o primeiro envio à mão): *Player Settings → Publishing Settings → Custom Keystore*. O caminho
  da keystore vai para o `ProjectSettings.asset` — aponte para um lugar fora do repositório e **não faça commit** dessa
  mudança (as senhas o Unity não salva).

Secrets necessários em *GitHub → Settings → Secrets and variables → Actions* (hoje **nenhum está configurado**, por isso
os builds do CI falham com "Missing Unity License"):

| Secret | O que é |
|---|---|
| `UNITY_LICENSE` (+ `UNITY_EMAIL`, `UNITY_PASSWORD`; `UNITY_SERIAL` só para licença Pro) | Licença do Unity para o CI |
| `ANDROID_KEYSTORE_BASE64` | A keystore em base64: `base64 -i release.keystore \| gh secret set ANDROID_KEYSTORE_BASE64` |
| `ANDROID_KEYSTORE_NAME` | Nome do arquivo, ex.: `release.keystore` |
| `ANDROID_KEYSTORE_PASS`, `ANDROID_KEYALIAS_NAME`, `ANDROID_KEYALIAS_PASS` | Senha da keystore, alias e senha do alias (`gh secret set NOME` pede o valor sem mostrar) |
| `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON` | JSON da conta de serviço com acesso ao app no Play Console |
| `GOOGLE_PLAY_PACKAGE_NAME` | `com.brunocarvalhs.unice` |

## Checklist do Play Console (feito por você)

1. **Primeiro envio à mão.** A API do Play não aceita o primeiro AAB de um app novo: envie o primeiro pelo Console (faixa de
   teste interno). Depois disso o `release.yml` assume.
2. **Play App Signing:** aceite. A chave do GitHub vira a *chave de upload*; o Google guarda a chave de assinatura.
3. **Restringir a chave da API do Firebase** (Google Cloud Console → APIs e serviços → Credenciais): restrição "Apps Android"
   com o pacote `com.brunocarvalhs.unice` + SHA-1 da **chave de assinatura do Play** (Console → Integridade do app) e da chave
   de upload. A chave fica no repositório por design do Firebase — ela identifica o projeto, não é senha.
4. **Política de privacidade (URL obrigatória):** o app coleta dados de uso e falhas.
   URL: `https://brunocarvalhs.github.io/unice/privacy.html` (`docs/privacy.html`, publicada pelo mesmo deploy do
   jogo web — veja `.github/workflows/web.yml`). Landing page de divulgação: `docs/landing.html`
   (`https://brunocarvalhs.github.io/unice/landing.html`).
5. **Público-alvo:** o jogo não tem mais chat de voz nem anúncios; o único tema sensível é o suspense leve de
   esconde-esconde. Responda o questionário IARC no Console para confirmar a faixa etária — sem voz entre jogadores,
   as regras mais estritas de Famílias provavelmente não se aplicam, mas quem preenche é você.
6. **ID de publicidade:** responder **"não usa"** (removido no manifesto).
7. **Segurança dos dados (Data safety)** — respostas sugeridas:

| Tipo de dado | Coletado | Compartilhado | Finalidade | Observação |
|---|---|---|---|---|
| Interações no app (eventos de jogo) | Sim | Não | Análise | Firebase Analytics (`docs/ANALYTICS.md`) |
| Logs de falhas, diagnósticos | Sim | Não | Análise, desempenho | Firebase Crashlytics |
| IDs do dispositivo ou outros (ID de instalação Firebase, ID anônimo Unity) | Sim | Não | Análise, funcionalidade do app | Sem ID de publicidade |
| Nome (apelido escolhido no jogo) | Sim | Não | Funcionalidade do app | Visível só para quem está na mesma sala |

   - Dados **criptografados em trânsito: sim**.
   - Coleta **obrigatória** para analytics/falhas (não há opção de recusar no app hoje).
   - Exclusão de dados: o app não tem conta; descreva na política de privacidade como pedir exclusão (os dados do Firebase
     podem ser apagados pelo ID de instalação).

## Próximos passos (não bloqueiam o teste)

- **Consentimento (LGPD/GDPR):** para lançar na UE, mostrar aviso de consentimento e usar o Consent Mode do Firebase.
- **Símbolos IL2CPP no Crashlytics:** enviar os símbolos no release para crashes nativos legíveis.
- **Trapaça do host:** só um servidor autoritativo resolve; avaliar se o jogo crescer.
