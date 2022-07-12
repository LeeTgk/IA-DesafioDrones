using System;
using System.Collections;
using System.Collections.Generic;
using INF1771_GameAI.Map;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    private string direction;
    public LocalBot owner { get; private set; }
    public void Init(string dir, LocalBot owner)
    {
        this.direction = dir;
        this.owner = owner;
    }
    
    public event Action<Bullet> OnCollision;
    
    public bool CheckHit(Position playerPos)
    {
        var position = transform.position;
        var hit = (playerPos.x == (int) position.x && playerPos.y == (int) position.x);
        if (hit)
            OnCollision?.Invoke(this);
        return hit;
    }

    public void UpdatePos(Map map)
    {
        //Debug.Log("Alo");
        Position updated = getDirectionPos();
        if (CheckColision(map, updated)) return;
        transform.position = new Vector2(updated.x, updated.y);
        transform.rotation = Quaternion.Euler(GetRotationAngles());
    }
    
    private Vector3 GetRotationAngles()
    {
        Vector3 angle = direction switch
        {
            "north" => new Vector3(0, 0, 0),
            "west" => new Vector3(0, 0, 90),
            "south" => new Vector3(0, 0, 180),
            "east" => new Vector3(0, 0, 270),
            _ => new Vector3(0, 0, 0)
        };
        return angle;
    }

    private bool CheckColision(Map map, Position nextPos)
    {
        if (CheckInBounds(map, nextPos))
        {
            var type = map.tileMap[nextPos.x, nextPos.y].type;
            if (type == TileType.Obstacle)
            {
                OnCollision?.Invoke(this);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            OnCollision?.Invoke(this);
            return true;
        }
    }

    private bool CheckInBounds(Map map, Position nextPos)
    {
        return (nextPos.x > 0 &&
                nextPos.x < map.xLen &&
                nextPos.y > 0 &&
                nextPos.y< map.yLen);
    }

    private Position getDirectionPos()
    {
        var pos = transform.position;
        Position ret = direction switch
        {
            "north" => new Position((int)pos.x, (int)pos.y - 1),
            "east" => new Position((int)pos.x + 1, (int)pos.y),
            "south" => new Position((int)pos.x, (int)pos.y + 1),
            "west" => new Position((int)pos.x - 1,(int) pos.y),
            _ => null
        };

        return ret;
    }
}
