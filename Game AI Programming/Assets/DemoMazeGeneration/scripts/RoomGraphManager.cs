using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGraphManager : MonoBehaviour {
    // Set from inspector
    public GameObject waypointSet;
    public GameObject m_EnemyPrefab , m_PowerPrefab, m_HealthPrefab;
    public GameObject m_player;
    
    public List<AStarAgent> m_Agents = new List<AStarAgent>();
    public List<Transform> m_AgentSpawnPoints = new List<Transform>();
    public List<Transform> m_PowerSpawnPoints = new List<Transform>();
    public List<Transform> m_HealthSpawnPoints = new List<Transform>();
    public string m_PlayerTag = "Player";

    //__________________________agents generation
    public int m_MaxConcurrentAgentsCount = 3;
    public float m_MinAgentSpeed = 0.01f, m_MaxAgentSpeed = .05f;
    /// <summary>
    /// time in seconds of generation of new agents if not reaching max count yet.
    /// </summary>
    public int m_AgentSpawningFrequency = 15;//15 seconds

    //______________________items genertion___________
    /// <summary>
    /// time in seconds of generation of new power/health item
    /// </summary>
    public int m_PowerHealthSpawningFrequency = 15;//15 seconds
    public int m_MaxConcurrentPowerItemsCount = 1, m_MaxConcurrentHealthItemsCount = 1;
    public List<GameObject> m_PowerItemsSet = new List<GameObject>();
    public List<GameObject> m_HealthItemsSet = new List<GameObject>();

    //_____________player state
    private bool m_isPlayerInsideRoom = false;

    //use this for newly created agents
    private Vector3? m_lastKnowPlayerLocation = null;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //periodic update of agent count while player inside room
    IEnumerator AutoGenerateEnemiesifNeeded()
    {
        while (m_isPlayerInsideRoom)
        {
            if (m_Agents.Count < m_MaxConcurrentAgentsCount
                && m_EnemyPrefab != null
                && m_AgentSpawnPoints.Count > 0)
            {

                //choose random spawn
                int randIndex = UnityEngine. Random.Range(0, m_AgentSpawnPoints.Count);
                Transform agentLocation = m_AgentSpawnPoints[randIndex];

                //create new enemy
                GameObject AgentNew = Instantiate(m_EnemyPrefab);
                AgentNew.transform.position = agentLocation.position;
                AgentNew.transform.rotation = Quaternion.identity;
                //copy required settings
                AStarAgent AStarComponent = AgentNew.GetComponent<AStarAgent>();
                if (AStarComponent != null)
                {
                    AStarComponent.waypointSet = this.waypointSet;
                    AStarComponent.speed = UnityEngine.Random.Range(m_MinAgentSpeed, m_MaxAgentSpeed);
                    AStarComponent.InitGraph();
                    m_Agents.Add(AStarComponent);
                   
                    AStarComponent.EnableAgentMovementAlgorithm(true);
                    if(m_lastKnowPlayerLocation!=null)
                        AStarComponent.SetPlayerLocation(m_lastKnowPlayerLocation.Value);   
                }

                EnemyActionManager enemyActionComponent = AgentNew.GetComponent<EnemyActionManager>();
                if (enemyActionComponent != null)
                {
                    //store player reference in enemy
                    enemyActionComponent.m_Player = this.m_player;
                    enemyActionComponent.m_ownerRoomManager = this;
                }
            }
            yield return new WaitForSeconds(m_AgentSpawningFrequency);
        }
        
    }

    internal void RemoveAgent(EnemyActionManager enemyActionManager)
    {
        AStarAgent agent = enemyActionManager.gameObject.GetComponent<AStarAgent>();
        if (agent != null)
            m_Agents.Remove(agent);
    }

    internal void RemovePowerUnit(GameObject powerItem)
    {
        m_PowerItemsSet.Remove(powerItem);
      
    }

    internal void RemoveHealthUnit(GameObject healthItem)
    {
        m_HealthItemsSet.Remove(healthItem);

    }
    /// <summary>
    /// auto generate power and health items periodically
    /// </summary>
    /// <returns></returns>
    IEnumerator AutoGenerateItemsifNeeded()
    {
        while (m_isPlayerInsideRoom)
        {
            //check power items
            if (m_PowerItemsSet.Count < m_MaxConcurrentPowerItemsCount
                && m_PowerPrefab != null
                && m_PowerSpawnPoints.Count > 0)
            {

                //choose random spawn
                int randIndex = UnityEngine. Random.Range(0, m_PowerSpawnPoints.Count);
                Transform powerLocation = m_PowerSpawnPoints[randIndex];

                //create new enemy
                GameObject PowerNew = Instantiate(m_PowerPrefab);
                PowerNew.transform.position = powerLocation.position;
                m_PowerItemsSet.Add(PowerNew);
               
            }

            //check health items
            if (m_HealthItemsSet.Count < m_MaxConcurrentHealthItemsCount
                && m_HealthPrefab != null
                && m_HealthSpawnPoints.Count > 0)
            {

                //choose random spawn
                int randIndex = UnityEngine.Random.Range(0, m_HealthSpawnPoints.Count);
                Transform HealthLocation = m_HealthSpawnPoints[randIndex];

                //create new enemy
                GameObject HealthNew = Instantiate(m_HealthPrefab);
                HealthNew.transform.position = HealthLocation.position;
                m_HealthItemsSet.Add(HealthNew);

            }
            yield return new WaitForSeconds(m_PowerHealthSpawningFrequency);
        }

    }
    /// <summary>
    /// updates the goal of each agent when player moves
    /// </summary>
    public void NotifyPlayerLocationChange(Vector3 PlayerNewLocation)
    {
        if(m_isPlayerInsideRoom)
        { 
            foreach (AStarAgent agent in m_Agents)
            {
                agent.SetPlayerLocation(PlayerNewLocation);
                agent.EnableAgentMovementAlgorithm(true);
                m_lastKnowPlayerLocation = PlayerNewLocation;
            }

            StartCoroutine(AutoGenerateEnemiesifNeeded());//thread for creating Enemies
            StartCoroutine(AutoGenerateItemsifNeeded());//thread for collectable items
        }
    }

    public void NotifyPlayerExitingRoom()
    {
        foreach (AStarAgent agent in m_Agents)
        {
            agent.EnableAgentMovementAlgorithm(false);
            
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == m_PlayerTag)
        {
            //player entered the room
            m_isPlayerInsideRoom = true;

            //notify Agents of player new position
            NotifyPlayerLocationChange(other.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == m_PlayerTag)
        {
            //player exit the room
            m_isPlayerInsideRoom = false;

            //stop algorithm
            NotifyPlayerExitingRoom();
        }
    }
}
