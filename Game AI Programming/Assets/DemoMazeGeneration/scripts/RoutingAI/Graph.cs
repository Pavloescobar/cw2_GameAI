using System.Collections.Generic;

public interface Graph {

    bool addNode(int a);          // true if node added
    
    bool addEdge(int a, int b);   // true if edge added
    bool addEdge(int a, int b, double cost);   // true if edge added
    double? getCost(int a, int b);// double value for cost, or null if no edge between them

    List<int> nodes();

    List<int> neighbours(int a);

}