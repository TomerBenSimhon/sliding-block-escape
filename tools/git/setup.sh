#!/bin/sh
# One-time setup after cloning: registers the Unity Smart Merge driver
# that .gitattributes refers to (merge=unityyamlmerge).
# Run from the repo root:  sh tools/git/setup.sh

git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "sh tools/git/unity-yaml-merge.sh %O %A %B %P"
git config merge.unityyamlmerge.recursive binary
git lfs install --local

echo "Git merge driver and LFS configured for this repo."
