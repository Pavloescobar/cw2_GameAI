using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// the possible states for FSM agent
/// </summary>
public enum FSMStates
{
    /// <summary>
    /// a state that the agent stops movements without any action
    /// </summary>
    StopIdle,

    /// <summary>
    /// a state to move always with picking random points as targets
    /// </summary>
    RandomContinousMove,

    /// <summary>
    /// a state to go to start node (way point 0) and then stop
    /// </summary>
    MoveToStartPoint, 


    /// <summary>
    /// a state to search for the farest point from it current position, and seek for it, and stops when arrive
    /// </summary>
    GoToFarestPoint,
}

/*
 * this agent is based on PathAgent (but not inherited from it), 
 * however, it is implemented specificly with A* path finder and FSM additional steps
 * 
 */
public class FinalAgentMonoBehaviour : MonoBehaviour
{
    #region basic variable from PathAgent
    // Set from inspector
    public GameObject waypointSet;

    // Waypoints
    protected WaypointGraph waypoints;
    protected int? current;

    protected List<int> path;
    protected AStarPathfinder pathfinder;

    public float speed;
    protected static float NEARBY = 0.2f;
    protected static System.Random rnd = new System.Random();
    #endregion

    //_____________________________________________________
    #region extra FSM and visualization variables
    //current state of agent
    protected FSMStates m_CurrentState = FSMStates.StopIdle;

    protected int? m_CurrentTarget = null ;
    //used to control agent color
    protected Renderer m_render;

    //colors for each state in agent
    private Dictionary<FSMStates, Color> m_StatesColors = new Dictionary<FSMStates, Color>();
    #endregion
    //_____________________________________________________
    void Start()
    {
        waypoints = new WaypointGraph(waypointSet);
        path = new List<int>();
        
        //use A* finder with heuristic function of basic euclidean distance
        pathfinder = new AStarPathfinder(EuclideanDistance);
        pathfinder.navGraph = waypoints.navGraph;

        //render and state colors
        m_render = GetComponent<Renderer>();

        /* 4 states colors:
         * black: stop, magenta: random move, green: move to start, blue: go to farest
         */
        m_StatesColors.Add(FSMStates.StopIdle, Color.black);
        m_StatesColors.Add(FSMStates.RandomContinousMove, Color.magenta);
        m_StatesColors.Add(FSMStates.MoveToStartPoint, Color.green);
        m_StatesColors.Add(FSMStates.GoToFarestPoint, Color.blue);

        //initiall set state to stop
        ChangeState(FSMStates.StopIdle);
    }

    //_____________________________________________________
    /*Heuristic function that return the  euclidean distance
     *  between a node and a goal in graph*/
    float EuclideanDistance(int next, int goal)
    {
        Vector3 nextPoint = this.waypoints[next].transform.position;
        Vector3 goalPoint = this.waypoints[goal].transform.position;

        return Vector3.Distance(nextPoint, goalPoint);
    }
    //_____________________________________________________
    /// <summary>
    /// change the state of agent
    /// </summary>
    void ChangeState(FSMStates NewState)
    {
        m_CurrentState = NewState;
        ChangeColor(NewState);

        switch (m_CurrentState)
        {
            case FSMStates.StopIdle:
                path.Clear();
                break;
            case FSMStates.MoveToStartPoint:
                m_CurrentTarget = 0;// first point in graph
                generateNewPath();
                break;
            case FSMStates.RandomContinousMove:
                m_CurrentTarget = null;// no specific target
                generateNewPath();
                break;
            case FSMStates.GoToFarestPoint:

                m_CurrentTarget = SearchForFarestNodeIndex();// go for the Farest node
                generateNewPath();
                break;

        }
    }
    //_______________________________________
    /// <summary>
    /// searches for farest node in the graph
    /// </summary>
    /// <returns>index of the farest node to use as a target</returns>
    private int SearchForFarestNodeIndex()
    {
        Vector3 currentPosition = this.transform.position;

        //initially consider the first node is the farest node
        int farestIndex = 0;
        float farestDistance = Vector3.Distance(currentPosition, 
            waypoints[0].transform.position);

        //loop other nodes
        for (int i = 1; i < waypoints.navGraph.nodes().Count; i++)
        {
            float temp = Vector3.Distance(currentPosition, waypoints[i].transform.position);
            if (temp > farestDistance)
            {
                farestDistance = temp;
                farestIndex = i;
            }
        }
        return farestIndex;
    }

    //_____________________________________________________
    /// <summary>
    /// change the color of agent with respect to state color
    /// </summary>
    private void ChangeColor(FSMStates newState)
    {
       Color stateColor = m_StatesColors[newState];
        m_render.material.color = stateColor;
    }

    void Update()
    {

        /*controling states by keyboards as:
         * press (S): stop Idle,
         * press (R): Random Continous Move,
         * press (A): go to start point,
         * press (F): go to the farest point from current location
         */
        if (Input.GetKeyDown(KeyCode.S))
            ChangeState(FSMStates.StopIdle);
        else if (Input.GetKeyDown(KeyCode.R))
            ChangeState(FSMStates.RandomContinousMove);
        else if (Input.GetKeyDown(KeyCode.A))
            ChangeState(FSMStates.MoveToStartPoint);
        else if (Input.GetKeyDown(KeyCode.F))
            ChangeState(FSMStates.GoToFarestPoint);

        if (m_CurrentState == FSMStates.StopIdle)
            return; //no action
        else
        {
            if (path.Count == 0)
            {
                if (m_CurrentState == FSMStates.GoToFarestPoint ||
                    m_CurrentState == FSMStates.MoveToStartPoint)
                {
                    //now the agent has reached its goal, switch state to "stop"
                    ChangeState(FSMStates.StopIdle);
                }
                else
                {
                    //randome continous move is required
                    //and We don't know where to go next
                    m_CurrentTarget = null;// no specific target
                    generateNewPath();
                }
            }
            else
            {
                // Get the next waypoint position
                GameObject next = waypoints[path[0]];
                Vector3 there = next.transform.position;
                Vector3 here = transform.position;

                // Are we there yet?
                float distance = Vector3.Distance(here, there);
                if (distance < NEARBY)
                {
                    // We're here
                    current = path[0];
                    path.RemoveAt(0);

                    Debug.Log("Arrived at waypoint " + current);
                }
            }
        }

    }

    void FixedUpdate()
    {
        if (path.Count > 0)
        {
            GameObject next = waypoints[path[0]];
            Vector3 position = next.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, position, speed);
        }
    }

    protected void generateNewPath()
    {
        // We know where are
        List<int> nodes = waypoints.navGraph.nodes();
        // Go to m_CurrentTarget
        path.Clear();

        if (current != null)
        {
            
            if (nodes.Count > 0)
            {
                // Pick a random node to aim for
                if(m_CurrentTarget==null)
                {
                    m_CurrentTarget = nodes[rnd.Next(nodes.Count)];
                }
                Debug.Log("New m_CurrentTarget: " + m_CurrentTarget.Value);
                // Find a path from here to there
                path = pathfinder.findPath(current.Value, m_CurrentTarget.Value);
                Debug.Log("New path: " + writePath(path));

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
            if (current == null)
            {
                current = waypoints.findNearest(transform.position);
                path.Add(current.Value);
            }    

            if (current != null)
            {
                // Pick a random node to aim for
                if (m_CurrentTarget == null)
                {
                    m_CurrentTarget = nodes[rnd.Next(nodes.Count)];
                }

                

                //find new path from current to target
                path.AddRange( pathfinder.findPath(current.Value, m_CurrentTarget.Value));
                Debug.Log("Heading for nearest waypoint: " + m_CurrentTarget);
            }
            else
            {
                // Couldn't find a waypoint
                Debug.Log("Can't find nearby waypoint to m_CurrentTarget");
            }

        }
    }

    public static string writePath(List<int> path)
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

