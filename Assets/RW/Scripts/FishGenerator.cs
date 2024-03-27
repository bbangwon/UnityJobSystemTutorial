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

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

using random = Unity.Mathematics.Random;

public class FishGenerator : MonoBehaviour
{
    /// <summary>
    /// Transform을 추적하기 위한 IJobParallelForTransform 구현
    /// </summary>
    [BurstCompile]
    struct PositionUpdateJob : IJobParallelForTransform
    {
        public NativeArray<Vector3> objectVelocities;

        public Vector3 bounds;
        public Vector3 center;

        public float jobDeltaTime;
        public float time;
        public float swimSpeed;
        public float turnSpeed;
        public int swimChangeFrequency;

        public float seed;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 currentVelocity = objectVelocities[index];

            //Unity Math 라이브러리를 이용하여 인덱스와 시스템 시간을 사용하여 시드를 생성하는 의사난수 생성기를 만듦
            random randomGen = new random((uint)(index * time + 1 + seed));

            //localToWorldMatrix를 사용하여 로컬 정방향을 따라 변환
            transform.position += transform.localToWorldMatrix.MultiplyVector(new Vector3(0, 0, 1)) * swimSpeed * jobDeltaTime * randomGen.NextFloat(0.3f, 1.0f);

            if (currentVelocity != Vector3.zero)
            {
                //currentVelocity 방향으로 회전
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(currentVelocity), turnSpeed * jobDeltaTime);
            }

            Vector3 currentPosition = transform.position;
            bool randomise = true;

            ///경계를 벗어나면 속도가 중심을 향해 반전
            if(currentPosition.x > center.x + bounds.x / 2 ||
                currentPosition.x < center.x - bounds.x / 2 || 
                currentPosition.z > center.z + bounds.z / 2 || 
                currentPosition.z < center.z - bounds.z / 2)
            {
                Vector3 internalPosition = new Vector3(center.x + randomGen.NextFloat(-bounds.x / 2, bounds.x / 2)/1.3f, 0, 
                                                        center.z + randomGen.NextFloat(-bounds.z / 2, bounds.z / 2)/1.3f);

                currentVelocity = (internalPosition- currentPosition).normalized;
                objectVelocities[index] = currentVelocity;

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(currentVelocity), turnSpeed * jobDeltaTime * 2);               

                randomise = false;
            }

            //변환이 경계 내에 있는 경우 자연스러운 움직임을 제공하기 위해 방향이 바뀔 가능성이 적음
            if(randomise)
            {
                if(randomGen.NextInt(0, swimChangeFrequency) <= 2)
                {
                    objectVelocities[index] = new Vector3(randomGen.NextFloat(-1, 1), 0, randomGen.NextFloat(-1, 1));
                }
            }
        }
    }

    [Header("References")]
    public Transform waterObject;
    public Transform objectPrefab;

    [Header("Spawn Settings")]
    public int amountOfFish;
    public Vector3 spawnBounds;
    public float spawnHeight;
    public int swimChangeFrequency;

    [Header("Settings")]
    public float swimSpeed;
    public float turnSpeed;

    NativeArray<Vector3> velocities;
    TransformAccessArray transformAccessArray;

    private PositionUpdateJob positionUpdateJob;
    private JobHandle positionUpdateJobHandle;

    private void Start()
    {
        //속도 초기화
        velocities = new NativeArray<Vector3>(amountOfFish, Allocator.Persistent);

        transformAccessArray = new TransformAccessArray(amountOfFish);

        for (int i = 0; i < amountOfFish; i++)
        {
            float distanceX = Random.Range(-spawnBounds.x / 2, spawnBounds.x / 2);
            float distanceZ = Random.Range(-spawnBounds.z / 2, spawnBounds.z / 2);

            Vector3 spawnPoint = (transform.position + Vector3.up * spawnHeight) + new Vector3(distanceX, 0, distanceZ);

            //생성위치 내에 Fish 생성
            Transform t = Instantiate(objectPrefab, spawnPoint, Quaternion.identity);
            
            //transformAccessArray에 Fish 추가
            transformAccessArray.Add(t);
        }
    }

    private void Update()
    {
        //Job 데이터 설정
        positionUpdateJob = new PositionUpdateJob()
        {
            objectVelocities = velocities,
            jobDeltaTime = Time.deltaTime,
            swimSpeed = this.swimSpeed,
            turnSpeed = this.turnSpeed,
            time = Time.time,
            swimChangeFrequency = swimChangeFrequency,
            center = waterObject.position,
            bounds = spawnBounds,
            seed = System.DateTimeOffset.Now.Millisecond    //각 호출에 대해 다른 시드를 보장하기 위해 시스템 시간에서 현재 밀리초를 가져옴
        };

        positionUpdateJobHandle = positionUpdateJob.Schedule(transformAccessArray);
    }

    private void LateUpdate()
    {
        //업데이트 주기로 이동하기 전에 Job이 완료됨
        positionUpdateJobHandle.Complete();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position + Vector3.up * spawnHeight, spawnBounds);
    }

    private void OnDestroy()
    {
        transformAccessArray.Dispose();
        velocities.Dispose();
    }
}