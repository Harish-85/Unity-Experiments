using UnityEngine;

namespace GPUInstancing
{
    public class InstancerIndirect : MonoBehaviour
    {
        public int population;
        public float range;
        public Material mat;
        
        private ComputeBuffer _meshPropertiesBuffer;
        private ComputeBuffer _argsBuffer;

        private Mesh mesh;
        private Bounds bounds;

        private struct MeshProperties
        {
            public Matrix4x4 matrix;
            public Vector4 color;
            
            public static int Size()
            {
                return sizeof(float) * 16 + sizeof(float) * 4;
            }
        }

        private void Setup()
        {
        }

    }
}