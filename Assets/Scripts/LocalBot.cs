using System;
using System.Collections;
using System.Collections.Generic;
using INF1771_GameAI;
using INF1771_GameAI.Map;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;




public class LocalBot
{
    public int hitsDone { get; set; }
    public int hitsTaken { get; set; }
    
    private NeuralNetwork net;
    private string name = "LocalBot";
    GameAI gameAi = new GameAI();
    
    public event Action GameEnded;

    private Tournament tournament;

    public Position GetPlayerPos()
    {
        return gameAi.GetPlayerPosition();
    }

    public GameAI getAi()
    {
        return gameAi;
    }

    public NeuralNetwork getNet()
    {
        return net;
    }

    public LocalBot(NeuralNetwork net,int count, Tournament t)
    {
        name += ""+count;
        this.net = net;
        this.tournament = t;

        hitsDone = 0;
        hitsTaken = 0;
        
        tournament.InscribeBot(this);
    }

    public void Tick()
    {
        string status = tournament.GetGameStatus();
        
        switch (status)
        {
            case "Game":
            {
                DoDecision();
                var surplusEnergy = gameAi.GetEnergy() > 100 ? gameAi.GetEnergy() - 100 : 0;
                var kd = hitsTaken >0 ? (hitsDone/hitsTaken): 0;
                net.AddFitness((kd+surplusEnergy));
                break;
            }
            case "GameOver":
                //Debug.ClearDeveloperConsole();
                //GameEnded?.Invoke();
                break;
        }
    }

    public void TriggerEndEvent()
    {
        tournament.ClearLists();
        GameEnded?.Invoke();
    }
    private void DoDecision()
    {
        switch (gameAi.GetDecision(net))
        {
            case "virar_direita":
                tournament.sendTurn(this.gameAi,1);
                break;
            case "virar_esquerda":
                tournament.sendTurn(this.gameAi,-1);
                break;
            case "andar":
                tournament.sendForward(this.gameAi);
                break;
            case "atacar":
                tournament.sendShoot(this);
                break;
            case "pegar_ouro":
                tournament.sendGetItem(this.gameAi);
                break;
            case "pegar_anel":
                tournament.sendGetItem(this.gameAi);
                break;
            case "pegar_powerup":
                tournament.sendGetItem(this.gameAi);
                break;
            case "andar_re":
                tournament.sendBackward(this.gameAi);
                break;
        }
        
    }
    
    
}

