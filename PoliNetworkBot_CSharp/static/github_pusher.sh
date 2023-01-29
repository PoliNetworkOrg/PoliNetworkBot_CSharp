#!/bin/bash

cd ./data/polinetworkWebsiteData || exit
eval "$(ssh-agent -s)"
ssh-add /git/ssh-key
git fetch org
git config pull.rebase false
git pull --force
git add . --ignore-errors
git config --global user.email "polinetwork3@gmail.com"
git config --global user.name "PoliNetworkDev"
git commit -m "[Automatic Commit] Updated Group List "
git push -u origin main -f
hub pull-request -m "[AutoCommit] Groups Update" -b PoliNetworkOrg:main -h PoliNetworkDev:main -l bot -f