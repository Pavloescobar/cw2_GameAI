using UnityEngine;
using System.Text;
using System.Collections.Generic;

public abstract class Pathfinder
{
    public Graph navGraph;
    public abstract List<int> findPath(int a, int b);
}

[RequireComponent(typeof(EnemyActionManager))]
public abstract class PathAgent : MonoBehaviour
{

    // Set from inspector
    public GameObject waypointSet;

    // Waypoints
    protected WaypointGraph m_waypointsGraph;
    protected int? current;

    protected List<int> path;
    protected Pathfinder pathfinder;

    public float speed;
    protected static float NEARBY = 0.5f;
    protected static System.Random rnd = new System.Random();

    public abstract Pathfinder createPathfinder();

    private Vector3? m_GoalLocation;
    private int? m_GoalNodeIndex = null;//index of closest waypoint to current player position
    /// <summary>
    /// set it to true to enable the work of AI algorithm of movement and player tracking
    /// </summary>
    private bool m_EnableAgentMovement = false;

    private EnemyActionManager m_ActionManager;
    void Start()
    {
        InitGraph();
        m_ActionManager = this.GetComponent<EnemyActionManager>();
    }

    public void InitGraph()
    {
        m_waypointsGraph = new WaypointGraph(waypointSet);
        path = new List<int>();
        pathfinder = createPathfinder();
        pathfinder.navGraph = m_waypointsGraph.navGraph;
    }

    void Update()
    {
        if (m_EnableAgentMovement)
        {
            if (path.Count == 0)
            {
                // We don't know where to go next
                //generateNewPath();

            }
            else
            {
                // Get the next waypoint position
                GameObject next = m_waypointsGraph[path[0]];
                Vector3 there = next.transform.position;
                Vector3 here = transform.position;

                // Are we there yet?
                float distance = Vector3.Distance(here, there);
                if (distance < NEARBY)
                {
                    // We're here
                    current = path[0];
                    path.RemoveAt(0);
                    if (path.Count == 0 && m_GoalLocation != null) //reached last node, face target
                    {
                        transform.LookAt(m_GoalLocation.Value);
                    }
                    Debug.Log("Arrived at waypoint " + current);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (path.Count > 0)
        {
            GameObject next = m_waypointsGraph[path[0]];
            Vector3 position = next.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, position, speed);
            transform.LookAt(position); //update face
            if (m_ActionManager != null)
                m_ActionManager.PlayWalkAnimation();
        }
        else
        {
            if (m_ActionManager != null)
                m_ActionManager.PlayIdleAnimation();
            if(m_GoalLocation!=null)
                transform.LookAt(m_GoalLocation.Value); //update face
        }


    }

    /// <summary>
    /// true to enable the work of AI algorithm of movement and player tracking 
    /// </summary>
    public void EnableAgentMovementAlgorithm(bool isEnabled)
    {
        m_EnableAgentMovement = isEnabled;
        
    }

    public void SetPlayerLocation(Vector3 playerLocation)
    {
        m_GoalLocation = playerLocation;
        generateNewPath();
    }

    protected virtual void generateNewPath()
    {

        if (current != null)
        {
            // We know where are
            List<int> nodes = m_waypointsGraph.navGraph.nodes();

            if (nodes.Count > 0)
            {
                if (m_GoalLocation != null) //we have target
                {
                    //pick the nearest waypoint to goal (player) location
                    int? target = m_waypointsGraph.findNearest(m_GoalLocation.Value);

                    if (target != null)
                    {
                        if (target != m_GoalNodeIndex) //change than previous goal, so new path is required
                        {
                            //we have the nearest point, build path to it
                            // Find a path from here to there
                            path = pathfinder.findPath(current.Value, target.Value);
                            m_GoalNodeIndex = target;
                            Debug.Log("New m_CurrentTarget: " + target);
                            Debug.Log("New path: " + WritePath(path));
                        }
                        else //same target, continue in same path
                        {
                            Debug.Log("Same Target: " + target);
                        }
                    }
                    else
                    {
                        // There is no near Map
                        Debug.Log("No near target to current player location, can't route");
                    }
                }
            }
            else
            {
                // There are zero nodes
                Debug.Log("No m_waypointsGraph - can't select new m_CurrentTarget");
            }

        }
        else
        {
            // We don't know where we are

            // Find the nearest waypoint
            int? target = m_waypointsGraph.findNearest(transform.position);

            if (target != null)
            {
                // Go to m_CurrentTarget
                path.Clear();
                path.Add(target.Value);

                Debug.Log("Heading for nearest waypoint: " + target);
            }
            else
            {
                // Couldn't find a waypoint
                Debug.Log("Can't find nearby waypoint to m_CurrentTarget");
            }

        }
    }

    public static string WritePath(List<int> path)
    {
        var s = new StringBuilder();
        bool first = true;
        foreach (int t in path)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                s.Append(", ");
            }
            s.Append(t);
        }
        return s.ToString();
    }

}





