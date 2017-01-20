using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;

public delegate float Heuristic(int a, int b);

public class AStarPathfinder : Pathfinder
{

    protected Heuristic guessCost;

    public AStarPathfinder(Heuristic h)
    {
        guessCost = h;
    }

    public override List<int> findPath(int start, int goal)
    {
        if (guessCost == null)
            return null;//failure

        /*references:
        1- https://en.wikipedia.org/wiki/A*_search_algorithm
        2- https://blog.nobel-joergensen.com/2011/02/26/a-path-finding-algorithm-in-unity/
        3- http://www.leniel.net/2009/06/astar-pathfinding-search-csharp-part-2.html#sthash.H9Qq0Y1V.dpbs
        4- http://www.redblobgames.com/pathfinding/a-star/introduction.html
        */
        List<int> allNodes = navGraph.nodes();
        SimplePriorityQueue<int> priorityQueue = new SimplePriorityQueue<int>();      
        List<int> closedSet = new List<int>();
        Dictionary<int, float> distancesFromStart = new Dictionary<int,float>(),
            heauriticsToGoal = new Dictionary<int, float>();

        List<int> path = new List<int>();
        //initial distances
        foreach (int node in allNodes)
        {
            distancesFromStart.Add(node, float.MaxValue);
            heauriticsToGoal.Add(node, float.MaxValue);
        }

        distancesFromStart[start] = 0;//first node
        heauriticsToGoal[start] = guessCost(start, goal);
        priorityQueue.Enqueue(start, distancesFromStart[start]);

        while (priorityQueue.Count > 0)
        {
            int currentNode = priorityQueue.Dequeue();
          
            if (closedSet.Contains(currentNode))
                continue;

            path.Add(currentNode);

            if (currentNode == goal)
                break;
               // return path;

            closedSet.Add(currentNode);
            List<int> neighbors = navGraph.neighbours(currentNode);

            foreach (int next in neighbors)
            {
                if (closedSet.Contains(next))
                    continue;
                float childCurrentCost = heauriticsToGoal[next];
                double? edgeLength = navGraph.getCost(currentNode, next);
                if (edgeLength == null)
                    continue;
                //float alternativeHeuristic = heauriticsToGoal[currentNode] + (float) edgeLength.Value;
                float alternativeHeuristic = guessCost(next, goal);

                float minimumHeuristic = Mathf.Min(childCurrentCost, alternativeHeuristic);
              

                heauriticsToGoal[next] = minimumHeuristic;

                if (!priorityQueue.Contains(next))
                    priorityQueue.Enqueue(next, minimumHeuristic);
                else
                    priorityQueue.UpdatePriority(next, minimumHeuristic);
            }
        }
       
        return path;
    }
}

