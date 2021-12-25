echo "Pulling changes"
git pull
echo "rebooting bot"
cd bin/Debug/net5.0/ || exit
screen -XS bot_runner quit
screen -S bot_runner dotnet PoliNetworkBot_CSharp.dll 2