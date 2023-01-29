#!/bin/bash

cd ./data/polinetworkWebsiteData || exit
eval "$(ssh-agent -s)"
ssh-add /git/ssh-key
git fetch org
git pull --force
git add . --ignore-errors
git commit -m "[Automatic Commit] Updated Group List --author="'"'"polinetwork3@gmail.com<polinetwork3@gmail.com>"'"'
git push -u origin main --all -f
hub pull-request -m "[AutoCommit] Groups Update" -b PoliNetworkOrg:main -h PoliNetworkDev:main -l bot -f