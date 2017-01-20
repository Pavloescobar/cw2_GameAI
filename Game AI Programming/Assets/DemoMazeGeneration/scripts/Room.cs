using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    //note: some codes are based on https://unity3d.com/learn/tutorials/topics/scripting/basic-2d-dungeon-generation
   
    /// <summary>
    /// the outgoing directions of the corridors
    /// </summary>
    public Direction[] m_enteringCorridors; // The direction of the corridor that is entering this room.

    private List<Direction> m_unclosedDirections = new List<Direction>();
    //public List<Direction> m_doneDirections = new List<Direction>();

    public WayPoint[] m_RoomWaypoints;

    public List<Transform> m_playerSpawnPoints = new List<Transform>();
    //public Transform[] m_healthSpawnPoints;
    //public Transform[] m_EnenmieSpawnPoints;
    //public Transform[] m_powerAbilitiesSpawnPoints;
    //public const string m_playerPositionTag = "PlayerSpawn";
    // Use this for initialization
    void Start()
    {
        m_unclosedDirections.AddRange(m_enteringCorridors);
        //foreach (Transform child in this.transform)
        //{
        //    if (child.gameObject.tag == m_playerPositionTag)
        //    {
        //        m_playerSpawnPoints.Add(child);
        //    }
        //}

    }

    public List<Direction> GetRemainingOpenDirections(Direction outgoingDirection)
    {
        List<Direction> remainingDirections = new List<Direction>();
        foreach (Direction dir in m_enteringCorridors)
        {
            if ( dir == outgoingDirection)
                continue;
            else
                remainingDirections.Add(dir);
        }
        return remainingDirections;
    }

    public void MarkDirectionAsDone(Direction outgoingDirection)
    {
        m_unclosedDirections.Remove(outgoingDirection);
    }

    public Direction[] GetUnclosedDirections()
    {
        return m_unclosedDirections.ToArray();
    }
}
