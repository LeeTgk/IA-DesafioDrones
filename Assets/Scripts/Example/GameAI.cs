using INF1771_GameAI.Map;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace INF1771_GameAI
{
    public class GameAI
    {
        Position player = new Position();
        String state = "ready";
        String dir = "north";
        long score = 0;
        int energy = 0;
        float[] lastEvent = new float[8];


        public void SetStatus(int x, int y, String dir, String state, long score, int energy)
        {
            player.x = x;
            player.y = y;
            this.dir = dir.ToLower();

            this.state = state;
            this.score = score;
            this.energy = energy;
        }

        public string GetState()
        {
            return state; 
        }

        public List<Position> GetObservableAdjacentPositions()
        {
            List<Position> ret = new List<Position>();

            ret.Add(new Position(player.x - 1, player.y));
            ret.Add(new Position(player.x + 1, player.y));
            ret.Add(new Position(player.x, player.y - 1));
            ret.Add(new Position(player.x, player.y + 1));

            return ret;
        }

        public List<Position> GetAllAdjacentPositions()
        {
            List<Position> ret = new List<Position>();

            ret.Add(new Position(player.x - 1, player.y - 1));
            ret.Add(new Position(player.x, player.y - 1));
            ret.Add(new Position(player.x + 1, player.y - 1));

            ret.Add(new Position(player.x - 1, player.y));
            ret.Add(new Position(player.x + 1, player.y));

            ret.Add(new Position(player.x - 1, player.y + 1));
            ret.Add(new Position(player.x, player.y + 1));
            ret.Add(new Position(player.x + 1, player.y + 1));

            return ret;
        }

        public Position NextPosition()
        {
            Position ret = null;
            switch (dir)
            {
                case "north":
                    ret = new Position(player.x, player.y - 1);
                    break;
                case "east":
                    ret = new Position(player.x + 1, player.y);
                    break;
                case "south":
                    ret = new Position(player.x, player.y + 1);
                    break;
                case "west":
                    ret = new Position(player.x - 1, player.y);
                    break;
            }

            return ret;
        }

        public Position BackwardsPosition()
        {
            Position ret = null;
            switch (dir)
            {
                case "north":
                    ret = new Position(player.x, player.y + 1);
                    break;
                case "east":
                    ret = new Position(player.x - 1, player.y);
                    break;
                case "south":
                    ret = new Position(player.x, player.y - 1);
                    break;
                case "west":
                    ret = new Position(player.x + 1, player.y);
                    break;
            }

            return ret;
        }

        public string GetPlayerDir()
        {
            return dir;
        }
        
        public Position GetPlayerPosition()
        {
            return player;
        }

        public void SetPlayerPosition(int x, int y)
        {
            player.x = x;
            player.y = y;

        }

        public void GetObservations(List<String> o)
        {
            String cmd = "";
            foreach (String s in o)
            {
                switch (s)
                {
                    case "blocked":
                        //Wall
                        lastEvent[0] = 1;
                        break;
                    case "steps":
                        //Inimigo em X
                        lastEvent[1] = 1;
                        break;
                    case "breeze":
                        //Obstáculo
                        lastEvent[2] = 1;
                        break;
                    case "flash":
                        //Inimigo Teletransporte
                        lastEvent[3] = 1;
                        break;
                    case "blueLight":
                        //PowerUps
                        lastEvent[4] = 1;
                        break;
                    case "redLight":
                        //Tesouros
                        lastEvent[5] = 1;
                        break;
                    case "greenLight":
                        lastEvent[6] = 1;
                        //Poison
                        break;
                    case "weakLight":
                        //Something - Not Certain
                        lastEvent[7] = 1;
                        break;
                }
            }

        }

        public float GetScore()
        {
            Debug.Log("Scooore "+ score);
            return score;
        }

        public int GetEnergy()
        {
            return energy;
        }
        
        public void GetObservationsClean()
        {
            //Reset Observed Params
            for (int i = 0; i < lastEvent.Length; i++)
            {
                lastEvent[i] = 0;
            }
            
        }

        public string GetDecision(NeuralNetwork net)
        { 
            var tempObservable = GetObservableAdjacentPositions();
            List<float> inputs = new List<float>();
            inputs.Add(energy); 
            inputs.Add(score);
            for (int i = 0; i < lastEvent.Length; i++) 
            { 
                inputs.Add(lastEvent[i]); 
            }

            inputs.Add(player.x);
            inputs.Add(player.y);
            for (int i = 0; i < tempObservable.Count; i++)
            {
                var tempPos = tempObservable[i];
                inputs.Add(tempPos.x);
                inputs.Add(tempPos.y);
            }

            float[] inputsArr = inputs.ToArray();

            float[] outputs = net.FeedForward(inputsArr);

            int n = (int)Mathf.Abs(outputs[0]*10f);
            
            //Debug.Log("Output of Net: "+ n);
            
            switch (n)
            {
                case 0:
                    return "virar_direita";
                case 1:
                    return "virar_esquerda";
                case 2:
                    return "andar";
                case 3:
                    return "atacar";
                case 4:
                    return "pegar_ouro";
                case 5:
                    return "pegar_anel";
                case 6:
                    return "pegar_powerup";
                case 7:
                    return "andar_re";
            }

            return "";
        }


    }
}
