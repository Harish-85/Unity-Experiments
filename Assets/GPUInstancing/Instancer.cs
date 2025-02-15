using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GPUInstancing
{
    public class Instancer : MonoBehaviour
    {
        public int Instances;
        public Mesh mesh;
        
        public Material material;
        
        public List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();
        
        public void RenderBatches()
        {
            for (int i = 0; i < batches.Count; i++)
            {
                Graphics.DrawMeshInstanced(mesh, 0, material, batches[i]);
            }
        }

        private void Update()
        {
            RenderBatches();
        }

        private void Start()
        {
            int added = 0;
            batches = new List<List<Matrix4x4>>();
            batches.Add(new List<Matrix4x4>());
            for (int i = 0; i < Instances; i++)
            {
                if (added < 1000)
                {
                    batches[batches.Count-1].Add(Matrix4x4.Translate(new Vector3(Random.Range(0,50),Random.Range(0,50),Random.Range(0,50))));
                    added++;
                }
                else
                {
                    batches.Add(new List<Matrix4x4>());
                    added = 0;
                }
            }
        }
    }
}
