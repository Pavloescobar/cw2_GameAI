using System;
using UnityEngine;

/// <summary>
/// used for Inspector to store directions of each room before instantiating it.
/// </summary>
[Serializable]
public class RoomPrefabDirections
{
    public GameObject m_RoomPrefab;
    public Direction[] m_PrefabDirections;
}
