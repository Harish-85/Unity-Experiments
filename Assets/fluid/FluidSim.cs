using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace fluid
{
    [Serializable,StructLayout(LayoutKind.Sequential,Size = 44)]
    public struct Particle
    {
        public float Pressure;
        public float Density;
        public Vector3 CurrentForce;
        public Vector3 Velocity;
        public Vector3 Position;
    }
    
    public class FluidSim : MonoBehaviour
    {
        [Header("General")]
        public bool showSphere = true;
        public Vector3Int numToSpawn = new Vector3Int(10, 10, 10);
        
        private int TotalParticles => numToSpawn.x * numToSpawn.y * numToSpawn.z;
        
        public Vector3 boxSize = new Vector3(5, 5, 5);
        public Vector3 boxCenter = new Vector3(0, 0, 0);
        public float radius = 0.1f;
        public float spawnJitter = .1f;
        
        [Header("REndering")]
        public Mesh particleMesh;
        public float particleRenderSize = 1f;
        public Material material;
        
        [Header("Compute")]
        public ComputeShader computeShader;

        [Header("Fluid Constants")]
        public float boundDamper = -0.3f;
        public float viscosity = -0.003f;
        public float particleMass = 1f;
        public float gasConstant = 2f;
        public float restingDensity = 1f;
        public float timeStep = 0.005f;
        
        
        
        public Particle[] particles;

        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _particleBuffer;
        
        private int _kernelIndex;
        private int _computeForceKernel;
        private int _densityKernel;

        private static readonly int SizeProperty = Shader.PropertyToID("_size");
        private static readonly int ParticlesBufferProperty = Shader.PropertyToID("_particlesBuffer");

        
        void SetupBuffer()
        {
            _kernelIndex = computeShader.FindKernel("Integrate");
            _computeForceKernel = computeShader.FindKernel("ComputeForces");
            _densityKernel = computeShader.FindKernel("ComputeDensityPressure");
            
            computeShader.SetInt("particleLength", TotalParticles);
            computeShader.SetFloat("particlesMass", particleMass);
            computeShader.SetFloat("gasConstant", gasConstant);
            computeShader.SetFloat("restDensity", restingDensity);
            computeShader.SetFloat("viscosity", viscosity);
            computeShader.SetFloat("boundDamping", boundDamper);
            computeShader.SetVector("boxSize", boxSize);
            computeShader.SetFloat("timeStep", timeStep);
            
            computeShader.SetFloat("pi", Mathf.PI);
            computeShader.SetFloat("radius1", radius);
            computeShader.SetFloat("radius2", radius*radius);
            computeShader.SetFloat("radius3", radius*radius*radius);
            computeShader.SetFloat("radius4", Mathf.Pow(radius,4));
            computeShader.SetFloat("radius5", Mathf.Pow(radius,5));
            
           computeShader.SetBuffer(_densityKernel,"particles",_particleBuffer);
            computeShader.SetBuffer(_computeForceKernel,"particles",_particleBuffer);
            computeShader.SetBuffer(_kernelIndex,"particles",_particleBuffer);
            
        }
        
        private void Awake()
        {
            SpawnParticlesInBox();
            
            uint[] args =
            {
                particleMesh.GetIndexCount(0),
                (uint)TotalParticles,
                particleMesh.GetIndexStart(0),
                particleMesh.GetBaseVertex(0),
                0
            };
            _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(args);
            
            _particleBuffer = new ComputeBuffer(TotalParticles, 44);
            _particleBuffer.SetData(particles);
            SetupBuffer();
        }

        private void SpawnParticlesInBox()
        {
            Vector3 spawnpoint = boxCenter;
            List<Particle> tempParticles = new List<Particle>();

            for (int x = 0; x < numToSpawn.x; x++)
            {
                for (int y = 0; y < numToSpawn.y; y++)
                {
                    for (int z = 0; z < numToSpawn.z; z++)
                    {
                        Vector3 spawnPos = spawnpoint + new Vector3(x * radius * 2, y * radius * 2, z * radius * 2);
                        spawnPos += new Vector3(Random.Range(-spawnJitter, spawnJitter), Random.Range(-spawnJitter, spawnJitter), Random.Range(-spawnJitter, spawnJitter));
                        Particle p = new Particle
                        {
                            Position = spawnPos
                        };
                        tempParticles.Add(p);
                    }
                }
            }

            particles = tempParticles.ToArray();
        }

        private void Update()
        {
            material.SetFloat(SizeProperty,particleRenderSize);
            material.SetBuffer(ParticlesBufferProperty,_particleBuffer);

            if (showSphere)
            {
                Graphics.DrawMeshInstancedIndirect(particleMesh,0,material,new Bounds(Vector3.zero,boxSize),_argsBuffer,castShadows: ShadowCastingMode.Off);
            }
        }

        private void FixedUpdate()
        {
            computeShader.SetVector("boxSize", boxSize);
            computeShader.SetFloat("timeStep", timeStep);
            
            computeShader.Dispatch(_densityKernel, TotalParticles/100, 1, 1);
            computeShader.Dispatch(_computeForceKernel, TotalParticles/100, 1, 1);
            computeShader.Dispatch(_kernelIndex, TotalParticles/100, 1, 1);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);


            if (!Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(boxCenter,radius);
            }
            
        }
    }
}