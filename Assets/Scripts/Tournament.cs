using System;
using System.Collections.Generic;
using INF1771_GameAI;
using INF1771_GameAI.Map;
using UnityEngine;

public class Tournament : MonoBehaviour
{
    private string gameStatus = "Ready";
    private List<LocalBot> botList = new List<LocalBot>();
    private List<Bullet> bulletList = new();
    private List<Bullet> bulletListToClean = new List<Bullet>();
    private Map map;

// Update is called once per frame

private void FixedUpdate()
    {
        if (gameStatus == "Game")
        {
            foreach (var bot in botList)
            {
                bot.Tick();
            }
            UpdateBullets();
            CheckGameOver();
            CleanBullets();
        }
    }

    private void UpdateBullets()
    {
        foreach (var bullet in bulletList)
        {
            bullet.UpdatePos(map);
            
            var hit = botList.FindAll(p => bullet.CheckHit(p.GetPlayerPos()));
            
            if (hit.Count == 1)
            {
                bulletListToClean.Add(bullet);
                
                var ai = hit[0].getAi();
                var currEnergy = ai.GetEnergy();
                var currPos = ai.GetPlayerPosition();
                var currDir = ai.GetPlayerDir();
                var currState = ai.GetState();

                if (currState != "alive") continue; //Only register hit if the target is alive
                
                hit[0].hitsTaken++;
                bullet.owner.hitsDone++;
                ai.SetStatus(currPos.x,currPos.y, currDir,"alive", 0, currEnergy-10);
                    
                currEnergy = ai.GetEnergy();
                if (currEnergy > 0) continue; //If Energy past the hit still positive don't register kill
                    
                //Hit killed
                ai.SetStatus(currPos.x,currPos.y, currDir,"dead", 0, currEnergy);
                
                ai = bullet.owner.getAi();
                currEnergy = ai.GetEnergy();
                currPos = ai.GetPlayerPosition();
                currDir = ai.GetPlayerDir();
                ai.SetStatus(currPos.x,currPos.y, currDir,"alive", 0, currEnergy+1000);



            }
            else if(hit.Count > 1)
            {
                Debug.Log("2 corpos ocuparam o mesmo espaço bicho...");
            }
            
        }
    }

    private void CleanBullets()
    {
        foreach (var bullet in bulletListToClean)
        {
            bulletList.Remove(bullet);
            Destroy(bullet.gameObject);
        }
        bulletListToClean.Clear();
    }


    public void SetMap(Map tileMap)
    {
        this.map = tileMap;
    }
    
    public void StartTournament()
    {
        gameStatus = "Game";
    }

    private void CheckGameOver()
    {
        int count = 0;
        foreach (var bot in botList)
        {
            var ai = bot.getAi();
            var currEnergy = ai.GetEnergy();

            if (currEnergy > 0 ) continue; //if Still Alive don't register death
            var currPos = ai.GetPlayerPosition();
            var currDir = ai.GetPlayerDir();
            
            bot.getAi().SetStatus(currPos.x, currPos.y, currDir, "dead", 0, currEnergy);
            count++;
        }

        if (count < botList.Count - 1) return;
        
        gameStatus = "GameOver";
        
        foreach (var bullet in bulletList)
        {
            OnBulletCollision(bullet);
        }
        
        botList[0].TriggerEndEvent();
        
        
        //Debug.Log("GameEnded");

    }

    public void ClearLists()
    {
        bulletList.Clear();
        botList.Clear();
    }

    public string GetGameStatus()
    {
        return gameStatus;
    }

    public void InscribeBot(LocalBot bot)
    {
        botList.Add(bot);
        bot.getAi().SetStatus(UnityEngine.Random.Range(0, map.xLen), UnityEngine.Random.Range(0, map.yLen), "north", "alive",
            0, 100);
    }

    public void sendTurn(GameAI bot,int turn)
    {
        if (bot.GetState() == "dead"|| bot.GetEnergy() <= 0) return;
        bot.GetObservationsClean();
        var currEnergy = bot.GetEnergy();
        var currPos = bot.GetPlayerPosition();
        var currDir = bot.GetPlayerDir();
        var cardinalDirectiondir = GetCardinalDirection(currDir,turn);
        
        bot.SetStatus(currPos.x,currPos.y, cardinalDirectiondir,"alive", 0, currEnergy-1);
        
        var currObs = ConstructObservations(bot.GetAllAdjacentPositions());
        bot.GetObservations(currObs);
    }

    private string GetCardinalDirection(string currdir,int turn)
    {
        var cmd = currdir switch
        {
            "north" when turn == 1 => "east",
            "north" when turn == -1 => "west",
            "east" when turn == 1 => "south",
            "east" when turn == -1 => "north",
            "south" when turn == 1 => "west",
            "south" when turn == -1 => "east",
            "west" when turn == 1 => "north",
            "west" when turn == -1 => "south",
            _ => ""
        };
        return cmd;
    }

    public void sendForward(GameAI bot)
    {
        if (bot.GetState() == "dead"|| bot.GetEnergy() <= 0) return;
        bot.GetObservationsClean();
        
        var currPos = bot.GetPlayerPosition();
        var nextPosition = bot.NextPosition();
        var currEnergy = bot.GetEnergy();
        var dir = bot.GetPlayerDir();

        if (CheckInBounds(nextPosition))
        {
            if (CheckPlayerAlreadyThere(nextPosition))
            {
                bot.SetStatus(nextPosition.x, nextPosition.y, dir, "alive", 0, currEnergy - 1);
            }
            else if(CheckPoco(nextPosition))
            {
                //CurrentEnergy + (-1000)
                bot.SetStatus(nextPosition.x, nextPosition.y, dir, "dead", 0, currEnergy + (int)TileType.Poco);
            }
            else
            {
                var newPos = CheckTp(nextPosition); //If tp, teletransports to the new position, else, goes to the current currentPosition;
                bot.SetStatus(newPos.x, newPos.y, dir, "alive", 0, currEnergy - 1);
            }
        }
        else
        {
            bot.SetStatus(currPos.x, currPos.y, dir, "alive", 0, currEnergy - 1);
        }
        
        

        var currObs = ConstructObservations(bot.GetAllAdjacentPositions());
        bot.GetObservations(currObs);
        
    }
    public void sendBackward(GameAI bot)
    {
        if (bot.GetState() == "dead"|| bot.GetEnergy() <= 0) return;
        bot.GetObservationsClean();
        var currPos = bot.GetPlayerPosition();
        var nextPosition = bot.BackwardsPosition();
        var currEnergy = bot.GetEnergy();
        var dir = bot.GetPlayerDir();
        
        if (CheckInBounds(nextPosition))
        {
            if (CheckPlayerAlreadyThere(nextPosition))
            {
                bot.SetStatus(nextPosition.x, nextPosition.y, dir, "alive", 0, currEnergy - 1);
            }
            else if(CheckPoco(nextPosition))
            {
                //CurrentEnergy + (-1000)
                bot.SetStatus(nextPosition.x, nextPosition.y, dir, "dead", 0, currEnergy + (int)TileType.Poco);
            }
            else
            {
                var newPos = CheckTp(nextPosition); //If tp, teletransports to the new position, else, goes to the current currentPosition;
                bot.SetStatus(newPos.x, newPos.y, dir, "alive", 0, currEnergy - 1); 
            }
        }
        else
        {
            bot.SetStatus(currPos.x, currPos.y, dir, "alive", 0, currEnergy - 1);
        }
        var currObs = ConstructObservations(bot.GetAllAdjacentPositions());
        bot.GetObservations(currObs);
    }

    private bool CheckPoco(Position nextPosition)
    {
        return (map.tileMap[nextPosition.x, nextPosition.y].type == TileType.Poco);
    }
    
    private bool CheckInBounds(Position pos)
    {
        return (pos.x > 0 &&
                pos.x < map.xLen &&
                pos.y > 0 &&
                pos.y< map.yLen);
    }

    private bool CheckPlayerAlreadyThere(Position nextPos)
    {
        var index = botList.FindIndex(p => p.GetPlayerPos() == nextPos);
        return (index != -1);
    }

    private Position CheckTp(Position nextPos)
    {
        return map.tileMap[nextPos.x, nextPos.y].type == TileType.Teleport ?
            new Position(map.tileMap[nextPos.x, nextPos.y].TpBrother.xRef, map.tileMap[nextPos.x, nextPos.y].TpBrother.yRef) :
            nextPos;
    }


    public List<string> ConstructObservations(List<Position> currObservationPos)
    {
        List<String> currObservations = new List<string>();
        foreach (var pos in currObservationPos)
        {
            if (CheckInBounds(pos))
            {
                var temp = TileTypeToString(map.tileMap[pos.x, pos.y].type);
                if(temp != "")
                    currObservations.Add(temp);
                else
                {
                    temp = CheckIfEnemy(pos);
                    if (temp != "")
                    {
                        currObservations.Add(temp);
                    }
                } 
            }
            else
            {
                currObservations.Add("blocked");
            }
            
        }
        return currObservations;
    }
    
    private string TileTypeToString(TileType o)
    {
        var cmd = o switch
        {
            TileType.Default => "",
            TileType.Poco => "breeze",
            TileType.Obstacle => "breeze",
            TileType.Teleport => "flash",
            TileType.Potion10 => "redLight",
            TileType.GoldCoin => "blueLight",
            TileType.GoldRing => "blueLight",
            TileType.Potion20 => "redLight",
            TileType.Potion50 => "redLight",
            _ => ""
        };
        return cmd;
    }

    private string CheckIfEnemy(Position position)
    {
        var temp = botList.FindIndex(p => p.GetPlayerPos() == position);
        return temp != -1 ? "steps" : "";
    }


    public void sendGetItem(GameAI bot)
    {
        if (bot.GetState() == "dead"|| bot.GetEnergy() <= 0) return;
        bot.GetObservationsClean();
        var currPos = bot.GetPlayerPosition();
        var currEnergy = bot.GetEnergy();
        var dir = bot.GetPlayerDir();
        var prize = CheckItem(currPos);
        
        bot.SetStatus(currPos.x, currPos.y, dir, "alive", 0, currEnergy + prize - 5);
        
        var currObs = ConstructObservations(bot.GetAllAdjacentPositions());
        bot.GetObservations(currObs);
    }

    private int CheckItem(Position pos)
    {
        Tile currTile = map.tileMap[pos.x, pos.y];
        TileType currTileType =currTile.type;

        int qtd = 0;
        if ((int) currTileType < 10 || !currTile.CanGetStuff()) return qtd;
        qtd = (int) currTileType;
        currTile.OnLocked += UnlockItemHandler;

        return qtd;
    }

    private List<Tile> tilesToBeUnlocked;
    private void UnlockItemHandler(Tile tile)
    {
        tilesToBeUnlocked.Add(tile);
        Invoke("UnlockItens",3f);
        tile.OnLocked -= UnlockItemHandler;
    }

    private void UnlockItens()
    {
        foreach (Tile tile in tilesToBeUnlocked)
        {
            tile.Unlock();
        }
        tilesToBeUnlocked.Clear();
    }
    
    
    
    [SerializeField]
    private Bullet bulletPrefab;

    public void sendShoot(LocalBot localBot)
    {
        GameAI bot = localBot.getAi();
        if (bot.GetState() == "dead"|| bot.GetEnergy() <= 0) return;
        bot.GetObservationsClean();
        var currPos = bot.GetPlayerPosition();
        var currEnergy = bot.GetEnergy();
        var currDir = bot.GetPlayerDir();

        Bullet bullet = Instantiate(bulletPrefab, new Vector3(currPos.x, currPos.y, 0),bulletPrefab.transform.rotation,transform);
        bullet.OnCollision += OnBulletCollision;
        bulletList.Add(bullet);
        bullet.Init(currDir, localBot);
        
        
        bot.SetStatus(currPos.x, currPos.y, currDir, "alive", 0, currEnergy - 10);
        
        var currObs = ConstructObservations(bot.GetAllAdjacentPositions());
        bot.GetObservations(currObs);
    }
    
    
    private void OnBulletCollision(Bullet bullet)
    {
        bulletListToClean.Add(bullet);
    }
}

