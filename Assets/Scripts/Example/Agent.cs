using System;
using System.Collections.Generic;
using INF1771_GameAI;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{
    private bool training = false;
    private Bot bot;
    private LocalBot localBot;
    private NeuralNetwork net;
    
    [SerializeField]
    private List<MeshRenderer> mats;

    public Bot GetBot() //What a Breach (again not really caring about code smells sorry)
    {
        return this.bot;
    }
    
    public LocalBot GetLocalBot() //What a Breach (again not really caring about code smells sorry)
    {
        return this.localBot;
    }

    private void FixedUpdate()
    {
        var newPos = training ? localBot.GetPlayerPos() : bot.GetPlayerPos();
        var direction = training ? localBot.getAi().GetPlayerDir() : bot.getAi().GetPlayerDir();
        var energy = training ? localBot.getAi().GetEnergy() : bot.getAi().GetEnergy();
        var state = training ? localBot.getAi().GetState() : bot.getAi().GetState();
        transform.position = new Vector3(newPos.x, newPos.y, 10);
        transform.rotation = Quaternion.Euler(GetRotationAngles(direction));
        if (energy > 0 && state != "dead") return;
        foreach (var mat in mats)
        {
            mat.material.SetColor("_Color",Color.red);
        }

    }

    public void SetAsFittest()
    {
        foreach (var mat in mats)
        {
            mat.material.SetColor("_Color",Color.yellow);
        }
    }

    private Vector3 GetRotationAngles(string dir)
    {
        Vector3 angle;
        switch (dir)
        {
            case "north":
                angle = new Vector3(0, 0, 0);
                break;
            case "west":
                angle = new Vector3(0, 0, 90);
                break;
            case "south":
                angle = new Vector3(0, 0, 180);
                break;
            case "east":
                angle = new Vector3(0, 0, 270);
                break;
            default:
                angle = new Vector3(0, 0, 0);
                break;
        }
        return angle;
    }

    public void Init(NeuralNetwork net,int index, bool train, Tournament tournament = null)
    {
        this.training = train;
        this.net = net;
        this.bot = !training ? new Bot(net,index): null;
        this.localBot = tournament != null && training ? new LocalBot(net, index, tournament) : null;
    }
    private void OnApplicationQuit()
    {
        bot?.Quit();
    }

}

