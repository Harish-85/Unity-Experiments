using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Octree
{
    public class OctreeNode
    {
        public List<OctreeObject> Objects = new();

        public readonly int Id;
        
        static int _idCounter = 0;

        public Bounds Bounds;
        private Bounds[] _childBounds = new Bounds[8];
        public OctreeNode[] Children;
        public bool IsLeaf => Children == null;
    
        private float _minNodeSize;
        
        public OctreeNode(Bounds bounds, float minNodeSize)
        {
            
            Id = _idCounter++;
            Bounds = bounds;
            _minNodeSize = minNodeSize;
            
            Vector3 newSize = bounds.size / 2f;
            Vector3 centerOffset = newSize / 2f;
            Vector3 parentCenter = bounds.center;

            for (int i = 0; i < 8; i++)
            {
                Vector3 childCenter = parentCenter;
                // HOW THIS WITCHCRAFT WORKS - i & 1 == 0 
                //
                //So all numbers from 0 to 7 can be represented using 3 bits.
                // 0 = 000 1 = 001 2 = 010 3 = 011 4 = 100 5 = 101 6 = 110 7 = 111
                //
                // The & symbol is the bitwise AND operator.
                // what & 1 does is it checks if the last bit of the number is 1 or 0.
                // If it is 1, then the number is odd, if it is 0, then the number is even.
                //
                //  similar for i & 2 and i & 4 , they check the second and third bit respectively.
                
                childCenter.x += centerOffset.x * ((i & 1) == 0 ? -1 : 1);
                childCenter.y += centerOffset.y * ((i & 2) == 0 ? -1 : 1);
                childCenter.z += centerOffset.z * ((i & 4) == 0 ? -1 : 1);
                _childBounds[i] = new Bounds(childCenter, newSize);
            }
        }

        
        public void DrawNode()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Bounds.center,Bounds.size);
            
            foreach( var obj in Objects)
            {
                if (obj.Intersects(Bounds))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(obj.Bounds.center, obj.Bounds.size);
                }
            }
            
            if(Children != null)
            {
                foreach (var child in Children)
                {
                    child?.DrawNode();
                }
            }
            
        }
        
        
        //reset the id counter upon entering play mode
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetCounter()
        {
            _idCounter = 0;
        }

        public void Divide(GameObject gameObject)
        {
            Divide(new OctreeObject(gameObject));   
        }

        private void Divide(OctreeObject octreeObject)
        {
            if (Bounds.size.x <= _minNodeSize)
            {
                Objects.Add( octreeObject);
                return;
            }
            
            Children ??= new OctreeNode[8];
            
            bool intersectsChild = false;

            for (int i = 0; i < 8; i++)
            {
                Children[i] ??= new OctreeNode(_childBounds[i], _minNodeSize);
                
                if(octreeObject.Intersects(_childBounds[i]))
                {
                    Children[i].Divide(octreeObject);
                    intersectsChild = true;
                }
            }

            if (!intersectsChild)
            {
                Objects.Add(octreeObject);
            }
        }
    }
    

    public class Octree
    {
        public OctreeNode Root;
        private Bounds _bounds;
        public Graph Graph;
        
        //getters
        public Bounds Bounds => _bounds;
        private List<OctreeNode> _emptyNodes = new();
        
        public Octree(GameObject[] objects, float minNodeSize,Graph graph)
        {
            Graph = graph;
            
            CalculateBounds( objects);
            CreateTree(objects, minNodeSize);
            GetEmptyLeaveNodes(Root);
            GetEdges();
        }

        void GetEdges()
        {
            foreach (var leaf in _emptyNodes)
            {
                foreach (var otherLeaf in _emptyNodes)
                {
                    if(leaf.Bounds.Intersects(otherLeaf.Bounds))
                    {
                        Graph.AddEdge(leaf,otherLeaf);
                    }
                }
            }
        }
        
        private void GetEmptyLeaveNodes(OctreeNode node)
        {
            if(node.IsLeaf && node.Objects.Count == 0)
            {
                _emptyNodes.Add(node);
                Graph.AddNode(node);
                return;
            }

            if (node.Children == null) return;

            foreach (var child in node.Children)
            {
                GetEmptyLeaveNodes(child);
            }
            
            for (int i = 0; i < node.Children.Length; i++)
            {
                for (int j = i +1 ; j < node.Children.Length; j++)
                {
                    Graph.AddEdge(node.Children[i],node.Children[j]);
                }
            }
            
        }
        

        void CreateTree(GameObject[] worldObjs, float minNodeSize)
        {
            Root = new OctreeNode(_bounds, minNodeSize);
            
            
            foreach (var obj in worldObjs)
            {
                Root.Divide(obj);
            }
            
        }
        void CalculateBounds(GameObject[] objects)
        {
            foreach (var obj in objects)
            {
                _bounds.Encapsulate(obj.GetComponent<Collider>().bounds);
            }
            
            Vector3 size = Vector3.one * Mathf.Max(_bounds.size.x, _bounds.size.y, _bounds.size.z) * .6f;
            _bounds.SetMinMax( _bounds.center - size, _bounds.center + size);
        }
    }

    public class OctreeObject
    {
        Bounds _bounds;
        
        public Bounds Bounds => _bounds;
        
        public OctreeObject(GameObject obj)
        {
            _bounds = obj.GetComponent<Collider>().bounds;
        }
        
        public bool Intersects(Bounds bounds)
        {
            return _bounds.Intersects(bounds);
        }
    }
}