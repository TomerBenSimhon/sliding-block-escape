#!/bin/sh
# Git merge driver for Unity YAML files (scenes, prefabs, assets).
#
# Runs Unity's UnityYAMLMerge (Smart Merge) as a premerge, then hands any
# real conflicts to `git merge-file` so they show up as normal conflict markers.
#
# Registered in .git/config by tools/git/setup.sh:
#   driver = sh tools/git/unity-yaml-merge.sh %O %A %B %P
#
# Args (filled in by git):
#   $1 %O  common ancestor (base)
#   $2 %A  our version; the merge result must be written here
#   $3 %B  their version
#   $4 %P  the file's path in the repo (only used for its extension)
#
# Exit 0 = merged cleanly, non-zero = conflict left for you to resolve.

base=$1
ours=$2
theirs=$3
path=$4
ext=${path##*.}

top=$(git rev-parse --show-toplevel)

# Locate UnityYAMLMerge: env var, then git config, then the editor this project uses.
tool=$UNITY_YAML_MERGE
[ -z "$tool" ] && tool=$(git config --get unity.yamlmerge)
if [ -z "$tool" ]; then
    version=$(sed -n 's/^m_EditorVersion: //p' "$top/ProjectSettings/ProjectVersion.txt" | tr -d '\r')
    tool="C:/Program Files/Unity/Hub/Editor/$version/Editor/Data/Tools/UnityYAMLMerge.exe"
fi

text_merge() {
    git merge-file -L ours -L base -L theirs "$ours" "$base" "$theirs"
    exit $?
}

if [ ! -x "$tool" ]; then
    echo "unity-yaml-merge: UnityYAMLMerge not found at '$tool', using a plain text merge" >&2
    text_merge
fi

# Git's temp files have no extension, and UnityYAMLMerge picks its parser by
# extension, so copy them to properly named files first.
tmp=$(mktemp -d)
trap 'rm -rf "$tmp"' EXIT
cp "$base" "$tmp/base.$ext"
cp "$theirs" "$tmp/theirs.$ext"
cp "$ours" "$tmp/ours.$ext"
cp "$ours" "$tmp/result.$ext"

# -p  premerge: merge everything that doesn't conflict
# -h  headless: never open error dialogs (they block git)
# --fallback none: don't launch a GUI merge tool on conflicts
# The last two args receive premerged base/ours, used for the text merge below.
"$tool" merge -p -h --fallback none \
    "$tmp/base.$ext" "$tmp/theirs.$ext" "$tmp/ours.$ext" "$tmp/result.$ext" \
    "$tmp/pbase.$ext" "$tmp/pours.$ext" > "$tmp/log.txt" 2>&1
status=$?

case $status in
    0)  # Clean semantic merge.
        cp "$tmp/result.$ext" "$ours"
        exit 0 ;;
    2)  # Real conflicts. Premerged files differ only where both sides changed
        # the same property, so the text merge marks just those lines.
        sed -n '/^Left\|^Right/p' "$tmp/log.txt" | sed 's/^/unity-yaml-merge: /' >&2
        cp "$tmp/pours.$ext" "$ours"
        git merge-file -L ours -L base -L theirs "$ours" "$tmp/pbase.$ext" "$tmp/result.$ext"
        exit 1 ;;
    *)  # Tool couldn't parse the files; fall back to git's normal merge.
        echo "unity-yaml-merge: Smart Merge failed (exit $status), using a plain text merge" >&2
        text_merge ;;
esac
