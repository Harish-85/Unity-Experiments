using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Octree
{
    public class Node
    {
        static int _idCounter;
        public readonly int Id;
        
        public float H,F,G;
        
        public List<Edge> Edges = new();
        
        
        public OctreeNode OctreeNode;
        
        public Node From;

        public Node(OctreeNode node)
        {
            this.Id = _idCounter++;
            OctreeNode = node;
        }

        public override bool Equals(object obj)
        {
            return obj is Node other && other.Id == Id;
        }

        public override int GetHashCode() => Id.GetHashCode();

        [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetCounter()
        {
            _idCounter = 0;
        }
    }

    public class Edge
    {
        public readonly Node A, B;
        
        public Edge(Node a, Node b)
        {
            A = a;
            B = b;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && (other.A == A && other.B == B || other.A == B && other.B == A);
        }
    }
    
    public class Graph
    {
        public readonly Dictionary< OctreeNode , Node> Nodes = new();
        public readonly HashSet<Edge> Edges = new();

        private List<Node> _pathList = new();
        
        public int GetPathLength => _pathList.Count;

        public OctreeNode GetPathNode(int index)
        {
            if(_pathList == null) return null;
            
            if (index < 0 || index >= _pathList.Count) return null;
            return _pathList[index].OctreeNode;
        }
        
        public void AddNode(OctreeNode node)
        {
            if (Nodes.ContainsKey(node)) return;
            
            Nodes.Add(node, new Node(node));
        }

        public void AddEdge(OctreeNode a, OctreeNode b)
        {
            Node nodeA = FindNode(a);
            Node nodeB = FindNode(b);

            if (nodeA == null || nodeB == null) return;
            
            var edge = new Edge(nodeA, nodeB);
            if (Edges.Add(edge))
            {
                nodeA.Edges.Add(edge);
                nodeB.Edges.Add(edge);
            }
        }
        
        Node FindNode(OctreeNode node)
        {
            Nodes.TryGetValue(node, out Node n);
            return n;
        }
        
        public bool AStar(OctreeNode start, OctreeNode end)
        {
            _pathList.Clear();
            Node startNode = FindNode(start);
            Node endNode = FindNode(end);
            if (startNode == null || endNode == null) return false;
            
            SortedSet<Node> openSet = new SortedSet<Node>(new NodeComparer());
            HashSet<Node> closedSet = new HashSet<Node>();
            
            int iterations = 0;
            startNode.G = 0;
            startNode.H = Heuristic(startNode, endNode);
            startNode.F = startNode.G + startNode.H;
            startNode.From = null;
            
            openSet.Add(startNode);
            
            iterations = 0;
            while (openSet.Count > 0)
            {
                if (iterations > 100)
                {
                    Debug.LogError("A* iterations exceeded 1000");
                    return false;
                }
                iterations++;
                
                Node current = openSet.First();
                openSet.Remove(current);
                
                if (current.Equals(endNode))
                {
                    ReconstructPath(current);
                    return true;
                }
                
                closedSet.Add(current);

                foreach (Edge edge in current.Edges)
                {
                    Node neighbour = Equals(edge.A, current) ? edge.B : edge.A;
                    
                    if (closedSet.Contains(neighbour)) continue;
                    
                    float tentativeG = current.G + Heuristic( current, neighbour);
                    
                    if(tentativeG < neighbour.G || !openSet.Contains(neighbour))
                    {
                        neighbour.G = tentativeG;
                        neighbour.H = Heuristic(neighbour, endNode);
                        neighbour.F = neighbour.G + neighbour.H;
                        neighbour.From = current;
                        openSet.Add(neighbour);
                       
                    }
                }
                
            }
            Debug.LogError("A* failed to find path");
            return false;
        }
        
        void ReconstructPath(Node current)
        {
            while (current != null)
            {
                _pathList.Add(current);
                current = current.From;
            }
            _pathList.Reverse();
        }
        
        float Heuristic(Node a, Node b) => ( a.OctreeNode.Bounds.center - b.OctreeNode.Bounds.center).sqrMagnitude;
        
        class NodeComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                if (x == null || y == null) return 0;

                var compare = x.F.CompareTo(y.F);
                if (compare == 0)
                {
                    return x.Id.CompareTo(y.Id);
                }
                return compare;
            }
        }
        
        public void DrawGraph()
        {
            Gizmos.color = Color.blue;
            foreach(Edge edge in Edges)
            {
                Gizmos.DrawLine( edge.A.OctreeNode.Bounds.center, edge.B.OctreeNode.Bounds.center);
            }
            foreach( var node in Nodes.Values)
            {
                Gizmos.DrawWireSphere(node.OctreeNode.Bounds.center,0.2f);
            }
            
        }
    }
}