using System;
using System.Linq;
using UnityEngine;


namespace Octree
{
    public class Mover : MonoBehaviour
    {
        private float _speed = 5f;
        private float _accuracy = 1f;
        private int _turnSpeed = 5;

        private int _currentWaypoint;

        private OctreeNode currentNode;
        Vector3 destination;
        
        public OctreeBehaviour OctreeBehaviour;
        private Graph graph;

        private void Start()
        {
            graph = OctreeBehaviour.WayPoints;
            currentNode = GetClosestNode();
            GetRandomDestination();
            
        }

        private void Update()
        {
            if (graph == null) return;

            if (graph.GetPathLength == 0 || _currentWaypoint == graph.GetPathLength)
            {
                GetRandomDestination();
                return;
            };
            
            if (Vector3.Distance(transform.position, graph.GetPathNode(_currentWaypoint).Bounds.center) < _accuracy)
            {
                _currentWaypoint++;
            }
            
            if(_currentWaypoint < graph.GetPathLength)
            {
                currentNode = graph.GetPathNode(_currentWaypoint);
                destination = currentNode.Bounds.center;
                var direction = destination - transform.position;
                direction.Normalize();
                
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), _turnSpeed * Time.deltaTime);
                transform.Translate(0, 0, _speed * Time.deltaTime);
                
            }
        }

        private void GetRandomDestination()
        {
            OctreeNode destinationNode;
            do
            {
                destinationNode = graph.Nodes.ElementAt(UnityEngine.Random.Range(0,graph.Nodes.Count)).Key;
            } while (!graph.AStar(currentNode, destinationNode));
            _currentWaypoint = 0;
        }

        private void OnDrawGizmos()
        {
            if (graph == null) return;
            if (graph.GetPathLength == 0) return;

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(graph.GetPathNode(0).Bounds.center,.5f);
            
            Gizmos.DrawWireSphere( graph.GetPathNode(graph.GetPathLength - 1).Bounds.center,.5f);
            
            for (int i = 0; i < graph.GetPathLength - 1; i++)
            {
                Gizmos.DrawWireSphere( graph.GetPathNode(i).Bounds.center,.5f);
                
                if(i < graph.GetPathLength)
                    Gizmos.DrawLine(graph.GetPathNode(i).Bounds.center, graph.GetPathNode(i + 1).Bounds.center);
                
            }
        }

        private OctreeNode GetClosestNode()
        {
            OctreeNode closest = null;
            float minDistance = float.MaxValue;
            foreach (var node in graph.Nodes)
            {
                OctreeNode n = node.Key;
                float distanceSqr = (transform.position - n.Bounds.center).sqrMagnitude;
                
                if (distanceSqr < minDistance)
                {
                    minDistance = distanceSqr;
                    closest = n;
                }
            }

            return closest;

        }
    }
}