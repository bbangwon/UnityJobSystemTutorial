using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

//간단히 z값만 변경하여 파도 효과를 만들어낸다.
public class WaveGeneratorUpdate : MonoBehaviour
{
    [Header("Wave Parameters")]
    public float waveScale;
    public float waveOffsetSpeed;
    public float waveHeight;

    [Header("References and Prefabs")]
    public MeshFilter waterMeshFilter;
    private Mesh waterMesh;

    private List<Vector3> waterVertices;
    private List<Vector3> waterNormals;

    void Start()
    {
        waterMesh = waterMeshFilter.mesh;

        //메시가 동적 버퍼를 사용하게 되며, 메시가 자주 바뀔때 사용하면 좋다.
        waterMesh.MarkDynamic();

        waterVertices = new List<Vector3>(waterMesh.vertices);
        waterNormals = new List<Vector3>(waterMesh.normals);        
    }

    void Update()
    {
        MakeNoise();
        waterMesh.SetVertices(waterVertices);
        waterMesh.RecalculateNormals();
    }

    private void MakeNoise()
    {
        for(int i = 0; i < waterVertices.Count; i++)
        {
            if (waterNormals[i].z <= 0f)
                continue;

            var vertex = waterVertices[i];

            var waveOffsetSpeedMutiplyTime = waveOffsetSpeed * Time.time;
            var x = vertex.x * waveScale + waveOffsetSpeedMutiplyTime;
            var y = vertex.y * waveScale + waveOffsetSpeedMutiplyTime;
            float2 pos = math.float2(x, y);
            float noiseValue = noise.snoise(pos);

            waterVertices[i] = new Vector3(vertex.x, vertex.y, noiseValue * waveHeight + 0.3f);
        }
    }
}
