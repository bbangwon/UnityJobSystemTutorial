/*
 * Copyright (c) 2020 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

public class WaveGenerator : MonoBehaviour
{
    [Header("Wave Parameters")]
    public float waveScale; //Perlin 노이즈 크기
    public float waveOffsetSpeed;   //Perlin 노이즈 이동속도
    public float waveHeight;    //Perlin 노이즈 높이

    [Header("References and Prefabs")]
    public MeshFilter waterMeshFilter;
    private Mesh waterMesh;

    //잡 시스템에 구현된 안전 시스템과 함께 작동하는 NativeArray를 사용한다.
    //스레드의 안전을 보장하기 위해 읽고 쓴 내용을 추적함
    //프로세스가 병렬로 진행되기 때문에 중요함
    NativeArray<Vector3> waterVertices;
    NativeArray<Vector3> waterNormals;

    private void Start()
    {
        waterMesh = waterMeshFilter.mesh;
        waterMesh.MarkDynamic();

        //Allocator.Persistent : 할당은 느리지만, 프로그램 전체 수명동안 지속됨
        waterVertices = new NativeArray<Vector3>(waterMesh.vertices, Allocator.Persistent);
        waterNormals = new NativeArray<Vector3>(waterMesh.normals, Allocator.Persistent);
    }

    private void Update()
    {
       
    }

    private void LateUpdate()
    {
      
    }

    private void OnDestroy()
    {
        //영구할당자를 사용하고 있으므로, OnDestroy에서 Dispose를 호출하여 메모리를 해제해준다.
        waterVertices.Dispose();
        waterNormals.Dispose();
    }
}