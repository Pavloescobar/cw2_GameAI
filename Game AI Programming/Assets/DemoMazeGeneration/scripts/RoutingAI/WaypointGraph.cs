using UnityEngine;
using System.Collections.Generic;

public class WaypointGraph
{

    public Graph navGraph;
    protected List<GameObject> waypoints;

    //maps each game object for waypoint to its index
    protected Dictionary<GameObject, int> m_waypointsIndices = new Dictionary<GameObject, int>();
    public GameObject this[int i]
    {
        get { return waypoints[i]; }
        set { waypoints[i] = value; }
    }

    public WaypointGraph(GameObject waypointSet)
    {

        waypoints = new List<GameObject>();
        navGraph = new AdjacencyListGraph();

        findWaypoints(waypointSet);
        buildGraph();
    }

    private void findWaypoints(GameObject waypointSet)
    {

        if (waypointSet != null)
        {
            int index = 0;
            foreach (Transform t in waypointSet.transform)
            {
                waypoints.Add(t.gameObject);
                m_waypointsIndices.Add(t.gameObject, index);
                index++;
            }
            Debug.Log("Found " + waypoints.Count + " m_waypointsGraph.");

        }
        else
        {
            Debug.Log("No m_waypointsGraph found.");

        }
    }

    private void buildGraph()
    {

        int n = waypoints.Count;

        navGraph = new AdjacencyListGraph();
        for (int i = 0; i < waypoints.Count; i++)
        {
            //use index as ID for node
            navGraph.addNode(i);
        }

        // ADD APPROPRIATE EDGES
        double cost;
        int startNodeIndex, endNodeIndex;
        foreach (GameObject nodeStart in waypoints)
        {
            WayPoint startPointComponent = nodeStart.GetComponent<WayPoint>();
            startNodeIndex = m_waypointsIndices[nodeStart];

            if (startPointComponent != null)
            {
                //get neighbors (Directed graph)
                List<GameObject> neighbors = startPointComponent.m_neighborWaypoints;
                foreach (GameObject nodeEnd in neighbors)
                {
                    endNodeIndex = m_waypointsIndices[nodeEnd];
                    cost = CalculateCost(startNodeIndex, endNodeIndex);
                    navGraph.addEdge(startNodeIndex, endNodeIndex, cost);
                }
            }
        }
        Debug.Log("Done ");
    }

    private double CalculateCost(int idA, int idB)
    {
        Vector3 startPoint = this[idA].transform.position;
        Vector3 endPoint = this[idB].transform.position;
        double cost = Vector3.Distance(startPoint, endPoint);
        return cost;
    }


    public int? findNearest(Vector3 here)
    {
        int? nearest = null;

        if (waypoints.Count > 0)
        {
            nearest = 0;
            Vector3 there = waypoints[0].transform.position;
            float minDistance = Vector3.Distance(here, there);

            for (int i = 1; i < waypoints.Count; i++)
            {
                there = waypoints[i].transform.position;
                float distance = Vector3.Distance(here, there);
                if (distance < minDistance)
                {
                    nearest = i;
                    minDistance = distance;//NOTE: Bug solved
                }
            }
        }
        return nearest;
    }

   
}

