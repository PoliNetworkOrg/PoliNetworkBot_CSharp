#!/bin/bash

cd ./data || exit
eval "$(ssh-agent -s)"
ssh-add /git/ssh-key
git clone git@github.com:PoliNetworkDev/polinetworkWebsiteData.git
