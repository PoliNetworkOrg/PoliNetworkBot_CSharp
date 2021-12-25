echo "Pulling changes"
cd /home/ubuntu/bot/PoliNetworkBot_CSharp/
git pull
echo "rebooting bot"
cd /home/ubuntu/bot/PoliNetworkBot_CSharp/PoliNetworkBot_CSharp/bin/Debug/net5.0/
read REPLY
screen -XS bot_runner quit
read REPLY
screen -d -m -S bot_runner dotnet PoliNetworkBot_CSharp.dll 2
read REPLY