echo "Pulling changes"
cd .. || exit
git pull
echo "rebooting bot"
cd ./bin/Debug/net5.0/ || exit
screen -XS bot_runner quit
screen -d -m -S bot_runner dotnet PoliNetworkBot_CSharp.dll 2
exit