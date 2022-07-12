using INF1771_GameClient.dto;
using INF1771_GameClient.Socket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using UnityEngine;
using UnityEngine.UIElements;
using Color = System.Drawing.Color;
using Position = INF1771_GameAI.Map.Position;
using Timer = System.Timers.Timer;

namespace INF1771_GameAI
{
    public class Bot
    {
        private string configpath = "CustomConfigs/config.txt";

        private string name = "Beyblade";
        private string host = "atari.icad.puc-rio.br";

        HandleClient client = new HandleClient();
        Dictionary<long, PlayerInfo> playerList = new Dictionary<long, PlayerInfo>();
        List<ShotInfo> shotList = new List<ShotInfo>();
        List<ScoreBoard> scoreList = new List<ScoreBoard>();
        private NeuralNetwork net;
        private string userStatus = "";
        public int hitsDone { get; private set; }
        public int hitsTaken { get; private set; }
        

        GameAI gameAi = new GameAI();

        private Timer timer1 = new Timer();

        long time = 0;

        String gameStatus = "";
        String sscoreList = "";

        List<String> msg = new List<String>();
        double msgSeconds = 0;

        public event Action GameEnded;

        public Position GetPlayerPos()
        {
            return gameAi.GetPlayerPosition();
        }
        
        public GameAI getAi()
        {
            return gameAi;
        }

        public Bot(NeuralNetwork net,int count)
        {
            this.name += "";
            this.net = net;

            hitsDone = 0;
            hitsTaken = 0;
            
            timer1.Enabled = true;
            timer1.Elapsed += new ElapsedEventHandler(timer1_Tick);
            timer1.Interval = 100;

            ReadCustomConfigs();
            
            HandleClient.CommandEvent += ReceiveCommand;
            HandleClient.ChangeStatusEvent += SocketStatusChange;

            client.connect(host);
            timer1.Start();

        }

        private void ReadCustomConfigs()
        {
            using (TextReader reader = File.OpenText(configpath))
            {
                this.name = reader.ReadLine();
                this.host = reader.ReadLine();
                timer1.Interval = int.Parse(reader.ReadLine());
            }
        }

        private Color convertFromString(String c)
        {
            var p = c.Split(new char[] { ',', ']' });

            int A = Convert.ToInt32(p[0].Substring(p[0].IndexOf('=') + 1));
            int R = Convert.ToInt32(p[1].Substring(p[1].IndexOf('=') + 1));
            int G = Convert.ToInt32(p[2].Substring(p[2].IndexOf('=') + 1));
            int B = Convert.ToInt32(p[3].Substring(p[3].IndexOf('=') + 1));

            return Color.FromArgb(A, R, G, B);
        }

        public void ReceiveCommand(object sender, EventArgs args)
        {
            CommandEventArgs cmdArgs = (CommandEventArgs)args;
            if (cmdArgs.cmd != null)
                if (cmdArgs.cmd.Length > 0)
                    try
                    {
                        switch (cmdArgs.cmd[0])
                        {

                            case "o":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (cmdArgs.cmd[1].Trim() == "")
                                        gameAi.GetObservationsClean();

                                    else
                                    {
                                        List<String> o = new List<String>();

                                        if (cmdArgs.cmd[1].IndexOf(",") > -1)
                                        {
                                            String[] os = cmdArgs.cmd[1].Split(',');
                                            for (int i = 0; i < os.Length; i++)
                                                o.Add(os[i]);
                                        }
                                        else
                                            o.Add(cmdArgs.cmd[1]);

                                        gameAi.GetObservations(o);
                                    }
                                }
				else
                                	gameAi.GetObservationsClean();

                                break;
                            case "s":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    gameAi.SetStatus(int.Parse(cmdArgs.cmd[1]),
                                                        int.Parse(cmdArgs.cmd[2]),
                                                        cmdArgs.cmd[3],
                                                        cmdArgs.cmd[4],
                                                        long.Parse(cmdArgs.cmd[5]),
                                                        int.Parse(cmdArgs.cmd[6]));
                                }
                                break;

                            case "player":
                                lock (playerList)
                                {
                                    if (cmdArgs.cmd.Length == 8)
                                        if (!playerList.ContainsKey(long.Parse(cmdArgs.cmd[1])))
                                            playerList.Add(long.Parse(cmdArgs.cmd[1]), new PlayerInfo(
                                                long.Parse(cmdArgs.cmd[1]),
                                                cmdArgs.cmd[2],
                                                int.Parse(cmdArgs.cmd[3]),
                                                int.Parse(cmdArgs.cmd[4]),
                                                (PlayerInfo.Direction)int.Parse(cmdArgs.cmd[5]),
                                                (PlayerInfo.State)int.Parse(cmdArgs.cmd[6]),
                                               convertFromString(cmdArgs.cmd[7])));
                                        else
                                        {
                                            playerList[long.Parse(cmdArgs.cmd[1])] = new PlayerInfo(
                                                long.Parse(cmdArgs.cmd[1]),
                                                cmdArgs.cmd[2],
                                                int.Parse(cmdArgs.cmd[3]),
                                                int.Parse(cmdArgs.cmd[4]),
                                                (PlayerInfo.Direction)int.Parse(cmdArgs.cmd[5]),
                                                (PlayerInfo.State)int.Parse(cmdArgs.cmd[6]),
                                               convertFromString(cmdArgs.cmd[7]));

                                        }
                                }

                                break;

                            case "g":
                                if (cmdArgs.cmd.Length == 3)
                                {
                                    if (gameStatus != cmdArgs.cmd[1])
                                        playerList.Clear();

                                    if (gameStatus != cmdArgs.cmd[1])
                                        Debug.Log("New Game Status: " + cmdArgs.cmd[1]);

                                    gameStatus = cmdArgs.cmd[1];
                                    time = long.Parse(cmdArgs.cmd[2]);
                                }
                                break;
                            case "u":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    for (int i = 1; i < cmdArgs.cmd.Length; i++)
                                    {
                                        String[] a = cmdArgs.cmd[i].Split('#');

                                        if (a.Length == 4)
                                            scoreList.Add(new ScoreBoard(
                                                a[0],
                                                (a[1] == "connected"),
                                                int.Parse(a[2]),
                                                int.Parse(a[3]), System.Drawing.Color.Black));
                                        else if (a.Length == 5)
                                            scoreList.Add(new ScoreBoard(
                                                a[0],
                                                (a[1] == "connected"),
                                                int.Parse(a[2]),
                                                int.Parse(a[3]), convertFromString(a[4])));
                                    }
                                    sscoreList = "";
                                    foreach (ScoreBoard sb in scoreList)
                                    {
                                        sscoreList += sb.name + "\n";
                                        sscoreList += (sb.connected ? "connected" : "offline") + "\n";
                                        sscoreList += sb.energy + "\n";
                                        sscoreList += sb.score + "\n";
                                        sscoreList += "---\n";
                                    }
                                    scoreList.Clear();
                                }
                                break;
                            case "notification":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (msg.Count == 0)
                                        msgSeconds = 0;
                                    msg.Add(cmdArgs.cmd[1]);
                                }

                                break;
                            case "hello":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (msg.Count == 0)
                                        msgSeconds = 0;

                                    msg.Add(cmdArgs.cmd[1] + " has entered the game!");
                                }

                                break;

                            case "goodbye":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (msg.Count == 0)
                                        msgSeconds = 0;

                                    msg.Add(cmdArgs.cmd[1] + " has left the game!");
                                }

                                break;


                            case "changename":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (msg.Count == 0)
                                        msgSeconds = 0;

                                    msg.Add(cmdArgs.cmd[1] + " is now known as " + cmdArgs.cmd[2] + ".");
                                }

                                break;
                            case "h":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    List<String> o = new List<String>();
                                    o.Add("hit");
                                    gameAi.GetObservations(o);
                                    msg.Add("you hit " + cmdArgs.cmd[1]);
                                    hitsDone++;
                                }
                                break;
                            case "d":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    List<String> o = new List<String>();
                                    o.Add("damage");
                                    gameAi.GetObservations(o);
                                    msg.Add(cmdArgs.cmd[1] + " hit you");
                                    hitsTaken++;
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
        }

        private void sendMsg(string msg)
        {
            if (msg.Trim().Length > 0)
                client.sendSay(msg);
        }

        private string GetTime()
        {

            TimeSpan t = TimeSpan.FromSeconds(time);

            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                            t.Hours,
                            t.Minutes,
                            t.Seconds);

            return answer;
        }

        private void DoDecision()
        {
            switch (gameAi.GetDecision(net))
                {
                    case "virar_direita":
                        client.sendTurnRight();
                        break;
                    case "virar_esquerda":
                        client.sendTurnLeft();
                        break;
                    case "andar":
                        client.sendForward();
                        break;
                    case "atacar":
                        client.sendShoot();
                        break;
                    case "pegar_ouro":
                        client.sendGetItem();
                        break;
                    case "pegar_anel":
                        client.sendGetItem();
                        break;
                    case "pegar_powerup":
                        client.sendGetItem();
                        break;
                    case "andar_re":
                        client.sendBackward();
                        break;
                }
                
                client.sendRequestUserStatus();
                client.sendRequestObservation();
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            msgSeconds += timer1.Interval;

            client.sendRequestGameStatus();
            if (gameStatus == "Game")
            {
                DoDecision();
                // var surplusEnergy = gameAi.GetEnergy() > 100 ? gameAi.GetEnergy() - 100 : 0;
                // net.AddFitness(((hitsDone/hitsTaken)+surplusEnergy));
            }
            else if (msgSeconds >= 5000)
            {
                Debug.Log(gameStatus);
                Debug.Log(GetTime());
                Debug.Log("-----------------");
                Debug.Log(sscoreList);

                client.sendRequestScoreboard();
                
            }

            if (msgSeconds >= 5000)
            {
                if (msg.Count > 0)
                {
                    foreach (String s in msg)
                        Debug.Log(s);
                    msg.Clear();
                }
                msgSeconds = 0;
            }
        }

        public void Quit()
        {
            client.sendGoodbye();
        }
        
        public event Action AgentDied;
        private void CheckAlive()
        {
            client.sendRequestUserStatus();
            if (userStatus == "Dead")
                AgentDied?.Invoke();
        }

        private void SocketStatusChange(object sender, EventArgs e)
        {
            if (client.connected)
            {
                Debug.Log("Connected");
                client.sendName(name);
                client.sendRequestGameStatus();
                client.sendRequestUserStatus();
                client.sendRequestObservation();

            }
            else
                Debug.Log("Disconnected");
        }
        
    }
}
