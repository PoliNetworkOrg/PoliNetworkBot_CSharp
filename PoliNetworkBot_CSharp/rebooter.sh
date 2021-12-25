echo "Pulling changes"
cd cd /home/ubuntu/bot/PoliNetworkBot_CSharp/
git pull
echo "rebooting bot"
cd cd /home/ubuntu/bot/PoliNetworkBot_CSharp/PoliNetworkBot_CSharp/bin/Debug/net5.0/
screen -XS bot_runner quit
screen -d -m -S bot_runner dotnet PoliNetworkBot_CSharp.dll 2