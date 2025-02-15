using System;
using UnityEngine;

namespace Octree
{
    public class OctreeBehaviour : MonoBehaviour
    {
        public GameObject[] objects;
        public float minNodeSize = 1f;
        private Octree _octree;

        public readonly Graph WayPoints = new();
        
        private void Awake()
        {
            _octree = new Octree(objects,minNodeSize,WayPoints);
        }

        private void OnDrawGizmos()
        {
            if(!Application.isPlaying) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_octree.Bounds.center,_octree.Bounds.size);
            _octree.Root.DrawNode();
            _octree.Graph.DrawGraph();
        }
    }
}