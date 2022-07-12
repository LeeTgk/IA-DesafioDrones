using System;

public enum TileType
{
    Poco = -1000,
    GoldCoin = 1000,
    GoldRing = 500,
    Potion10 = 10,
    Potion20 = 20,
    Potion50 = 50,
    Teleport = 2,
    Obstacle = 1,
    Default = 0
}
public class Tile
{
    public TileType type;
    public int xRef, yRef;

    public Tile TpBrother;

    public bool locked;
    
    public Tile(TileType type, int x, int y)
    {
        xRef = x;
        yRef = y;
        this.type = type;
        locked = false;
    }

    public void SetTpBrother(Tile target)
    {
        this.TpBrother = target;
    }

    public event Action<Tile> OnLocked;
    
    public bool CanGetStuff()
    {
        if (type is not (TileType.Potion10 or
            TileType.Potion20 or
            TileType.Potion50 or
            TileType.GoldCoin or
            TileType.GoldRing) || locked) return false;
        
        locked = true;
        OnLocked?.Invoke(this);
        return true;
    }

    public void Unlock()
    {
        locked = false;
    }

}
