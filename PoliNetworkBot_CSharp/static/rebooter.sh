echo "killing bot"
screen -XS bot_runner quit
echo "Pulling changes"
cd /home/ubuntu/bot/PoliNetworkBot_CSharp/
git pull --allow-unrelated-histories -f -X theirs --no-edit
echo "rebooting bot"
cd /home/ubuntu/bot/PoliNetworkBot_CSharp/
sleep  1
screen -d -m -S bot_runner dotnet PoliNetworkBot_CSharp.dll 2
echo "screen started"
sleep  10
