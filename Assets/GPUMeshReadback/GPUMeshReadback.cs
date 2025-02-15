using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GPUMeshReadback : MonoBehaviour
{
    public ComputeShader computeShader;
    public MeshFilter meshFilter;
    public MeshCollider collider;

    [StructLayout(LayoutKind.Sequential)]
    struct VertexData
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }

    private Mesh _mesh;
    private ComputeBuffer _buff;
    private int _kernal;
    private int _dispatchCount;
    private NativeArray<VertexData> _vertData;
    private AsyncGPUReadbackRequest _request;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!SystemInfo.supportsAsyncGPUReadback)
        {
            Debug.LogError("Async GPU Readback is not supported on this platform");
            return;
        }
        _mesh = meshFilter.mesh;
        _kernal = computeShader.FindKernel("CSMain");
        uint threadX = 0, threadY = 0, threadZ = 0;
        computeShader.GetKernelThreadGroupSizes(_kernal, out threadX, out threadY, out threadZ);
        _dispatchCount = Mathf.CeilToInt(_mesh.vertexCount / (float)threadX);

        _vertData = new NativeArray<VertexData>(_mesh.vertexCount, Allocator.Temp);
        for (int i = 0; i < _mesh.vertexCount; i++)
        {
            VertexData v = new VertexData();
            v.position = _mesh.vertices[i];
            v.normal = _mesh.normals[i];
            v.uv = _mesh.uv[i];

            _vertData[i] = v;
        }

        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position,
                _mesh.GetVertexAttributeFormat(VertexAttribute.Position), 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal,
                _mesh.GetVertexAttributeFormat(VertexAttribute.Normal), 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                _mesh.GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2)

        };
        _mesh.SetVertexBufferParams(_mesh.vertexCount,layout);

        _buff = new ComputeBuffer(_mesh.vertexCount, 8 * 4);
        if(_vertData.IsCreated) _buff.SetData(_vertData);
        computeShader.SetBuffer(_kernal,"vertexBuffer",_buff);

        _request = AsyncGPUReadback.Request(_buff);

    }

    // Update is called once per frame
    void Update()
    {
        computeShader.SetFloat("_time",Time.time);
        computeShader.Dispatch(_kernal,_dispatchCount,1,1);

        if (_request.done && !_request.hasError)
        {
            _vertData = _request.GetData<VertexData>();
            
            _mesh.MarkDynamic();
            _mesh.SetVertexBufferData(_vertData,0,0,_vertData.Length);
            _mesh.RecalculateNormals();
            collider.sharedMesh = _mesh;
            _request = AsyncGPUReadback.Request(_buff);
        }
        
    }

    private void OnDestroy()
    {
        _buff.Release();
    }
}
