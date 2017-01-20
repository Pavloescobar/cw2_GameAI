using System;
using System.Collections.Generic;

class AdjacencyListGraph: Graph
{
    protected Dictionary<int, List<int>> m_AdjacencyList = new Dictionary<int, List<int>>();

    /*dictionary of main start nodes, 
     * then each node contains dictionary of neighbor,
     * then each neighbor contains the distance to start node*/
    protected Dictionary<int, Dictionary<int, double>> m_WeightedAdjacencyList =
        new Dictionary<int, Dictionary<int, double>>();

    public  bool addNode(int a){
        if (m_AdjacencyList.ContainsKey(a))
            return false; //duplicated key (id(
        else
        {
            //add new node with empty neighbors list
            m_AdjacencyList.Add(a, new List<int>());
            m_WeightedAdjacencyList.Add(a, new Dictionary<int, double>());
            return true;// true if node added
        }
        
    }        
    public  bool addEdge(int a, int b)
    {
        //get neighbors list for both (a) and (b) nodes
        List<int> listA = m_AdjacencyList[a];
        List<int> listB = m_AdjacencyList[b];

        //asuming - bidrectional graph
        //normal it should be 2 edges (a->b, and b->a), if 0 edges then this edges is already existing
        short addedEdges = 0;

        //add two way edge (a->b, and b->a) - bidrectional graph
        if (!listA.Contains(b))
        {
            listA.Add(b);
            addedEdges++;
        }

        if (!listB.Contains(a))
        {
            listB.Add(a);
            addedEdges++;
        }
        return addedEdges > 0;// true if edge added (at least in one direction)

    }

    public bool addEdge(int a, int b, double cost)
    {
        if (this.addEdge(a, b))
        {
            //add cost from A to B
            Dictionary<int, double> neighborsA = m_WeightedAdjacencyList[a];
            if (neighborsA.ContainsKey(b))
                neighborsA[b] = cost;
            else
                neighborsA.Add(b, cost);

            //add vice versa cost (B to A)
            Dictionary<int, double> neighborsB = m_WeightedAdjacencyList[b];
            if (neighborsB.ContainsKey(a))
                neighborsB[a] = cost;
            else
                neighborsB.Add(a, cost);
            return true;
        }
        else return false;
    }

    public double? getCost(int a, int b)
    {
        //add cost from A to B
        Dictionary<int, double> neighborsA = m_WeightedAdjacencyList[a];
        if (neighborsA.ContainsKey(b))
            return neighborsA[b];
        else
            return null;//no path between the 2 nodes
    }

    public List<int> nodes()
    {
        List<int> nodesList = new List<int>();
        var keys = m_AdjacencyList.Keys;
        foreach (int k in keys)
            nodesList.Add(k);

        return nodesList;
    }

    public List<int> neighbours(int a)
    {
        if (m_AdjacencyList.ContainsKey(a))
            return m_AdjacencyList[a];
        else
            return null;//no key found
    }
}