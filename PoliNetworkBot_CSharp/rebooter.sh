screen -S rebooter
echo "Pulling changes"
cd .. || exit
git pull
echo "rebooting bot"
cd PoliNetworkBot_CSharp/bin/Debug/net5.0/ || exit
screen -XS bot_runner quit
screen -S bot_runner dotnet PoliNetworkBot_CSharp.dll 2
screen -XS rebooter quit