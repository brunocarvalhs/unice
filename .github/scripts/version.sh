#!/bin/bash

# Versionamento automático (adaptado do cestou): lê a última tag vX.Y.Z, decide o bump
# pelos Conventional Commits desde ela e registra os commits no CHANGELOG.md.
# Não mexe no ProjectSettings: o release.yml (game-ci, versioning: Semantic) tira
# versão e versionCode da tag. Imprime a nova versão (sem "v") no stdout, ou nada
# se não houver o que versionar.

set -euo pipefail

cd "$(dirname "$0")/../.."

CHANGELOG_FILE="CHANGELOG.md"

# 1️⃣ Última tag (se houver)
latest_tag=$(git describe --tags --abbrev=0 --match 'v[0-9]*' 2>/dev/null || echo "")

# Merges ficam de fora: "Merge pull request ..." não é Conventional Commit e forçaria patch sempre.
if [ -z "$latest_tag" ]; then
  >&2 echo "Nenhuma tag anterior encontrada. Lendo todos os commits."
  current_version="0.0.0"
  commit_subjects_raw=$(git log --no-merges --pretty=format:"%s")
else
  >&2 echo "Última tag: $latest_tag. Lendo commits desde então."
  current_version="${latest_tag#v}"
  commit_subjects_raw=$(git log --no-merges "${latest_tag}"..HEAD --pretty=format:"%s")
fi

# 2️⃣ Filtra commits que já estão no CHANGELOG
commit_subjects_array=()
while IFS= read -r subject; do
  if [ -n "$subject" ] && ! grep -qF "$subject" "$CHANGELOG_FILE" 2>/dev/null; then
    commit_subjects_array+=("$subject")
  fi
done <<< "$commit_subjects_raw"

if [ ${#commit_subjects_array[@]} -eq 0 ]; then
    >&2 echo "Nenhum commit novo para adicionar ao changelog. Saindo."
    exit 0
fi

# 2.5️⃣ Tipos que, sozinhos, não justificam versão nova (preset Angular):
# docs, ci, chore, style, test, refactor, build. "tipo!:" é BREAKING e não entra aqui.
only_non_bumping_types=true
for subject in "${commit_subjects_array[@]}"; do
    if ! echo "$subject" | grep -qiE "^(docs?|ci|chore|style|test|refactor|build)(\([^)]*\))?:"; then
        only_non_bumping_types=false
        break
    fi
done

if [ "$only_non_bumping_types" = true ]; then
    >&2 echo "Todos os commits novos são doc/ci/chore/style/test/refactor/build - nenhum bump de versão necessário. Saindo."
    exit 0
fi

# 3️⃣ Tipo de incremento
version_bump="patch"
for subject in "${commit_subjects_array[@]}"; do
    if echo "$subject" | grep -qiE "BREAKING CHANGE|^[a-z]+(\([^)]*\))?!:"; then
        version_bump="major"
        break
    elif echo "$subject" | grep -qi "^feat"; then
        version_bump="minor"
    fi
done

>&2 echo "Tipo de incremento da versão: $version_bump"

# 4️⃣ Incrementa
IFS=. read -r major minor patch <<< "$current_version"
case "$version_bump" in
  "major") major=$((major + 1)); minor=0; patch=0 ;;
  "minor") minor=$((minor + 1)); patch=0 ;;
  "patch") patch=$((patch + 1)) ;;
esac
new_version="$major.$minor.$patch"

>&2 echo "Incrementando versão de $current_version para $new_version"

# 5️⃣ CHANGELOG.md: a nova versão entra logo abaixo de "## [Unreleased]", com os commits,
# e o que já estava escrito à mão em Unreleased passa a pertencer a ela.
today=$(date +%Y-%m-%d)
changelog_body=$(printf "%s\n" "${commit_subjects_array[@]}" | sed 's/^/* /')
entry=$(printf "## [%s] - %s\n\n### Commits\n%s\n" "$new_version" "$today" "$changelog_body")

if [ ! -f "$CHANGELOG_FILE" ]; then
    printf "# Changelog\n\n## [Unreleased]\n" > "$CHANGELOG_FILE"
fi

if grep -q '^## \[Unreleased\]' "$CHANGELOG_FILE"; then
    ENTRY="$entry" awk '{ print } /^## \[Unreleased\]/ && !done { print ""; print ENVIRON["ENTRY"]; done = 1 }' \
        "$CHANGELOG_FILE" > "$CHANGELOG_FILE.tmp"
else
    ENTRY="$entry" awk 'NR == 1 { print; print ""; print ENVIRON["ENTRY"]; next } { print }' \
        "$CHANGELOG_FILE" > "$CHANGELOG_FILE.tmp"
fi
mv "$CHANGELOG_FILE.tmp" "$CHANGELOG_FILE"

>&2 echo "Versionamento completo."

# 6️⃣ Nova versão para o workflow
echo "$new_version"
