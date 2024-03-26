using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class JobHandleSample : MonoBehaviour
{
    /// <summary>
    /// a + b를 더하여 result에 값을 넣는 단순한 Job 예제
    /// </summary>
    struct JobSingle : IJob
    {
        /// <summary>
        /// Job에서 사용할 수 있는 변수는 값 타입
        /// </summary>
        public int a;
        public int b;

        /// <summary>
        /// 메인 스레드와 메모리를 공유하기 위해서는 NativeContainer라는 특수한 변수를 사용해야 함
        /// attributes:
        /// [ReadOnly] : 메모리에서 읽기만 가능하고, 이 경우 다른 Job에서도 이 Container에 접근할 수 있음
        /// [WriteOnly] : 메모리에 쓰기만 가능하고, 이 경우 다른 Job에서는 이 Container에 접근할 수 없음
        /// </summary>
        public NativeArray<int> result;

        /// <summary>
        /// 워커 스레드에서 실행되는 함수
        /// </summary>
        public void Execute()
        {
            result[0] = a + b;
        }
    }

    struct JobAdd : IJob
    {
        public NativeArray<int> result;
        public void Execute()
        {
            result[0] = result[0] + 1;
        }
    }

    void Start()
    {
        var result = new NativeArray<int>(1, Allocator.TempJob);

        // Job을 생성한 후 a, b에 값을 할당
        JobSingle jobSingle = new JobSingle();
        jobSingle.a = 1;
        jobSingle.b = 2;

        //메인 스레드에서 값을 사용하기 위해 NativeArray를 생성
        // Allocator.Temp : 가장 빠른 할당에 사용되지만, 수명이 1프레임 이하 작업에 적합. 하나의 함수 안에서 Job을 생성하고, 메인스레드에서 기다리게 하는 작업에 적합
        // Allocator.TempJob : Temp보다 느리지만, P{ersistent보다 빠름. 4프레임 이하일 때 적합.
        // Allocator.Persistent : 가장 느린 할당. 메모리를 계속 유지하므로, 사용에 유의해야 한다.

        jobSingle.result = result;

        // Job을 실행 할 수 있도록 워커 스레드에 예약함.
        // 메인 스레드에서는 Schedule만 호출할 수 있음.
        // JobHandle : Job의 Schedule을 호출하면 Job을 컨트롤 할 수 있는 JobHandle을 리턴함.
        JobHandle handle = jobSingle.Schedule();

        JobAdd jobAdd = new JobAdd();
        jobAdd.result = result;

        // jobAdd는 jobSingle이 완료된 후에 실행되어야 하므로, jobAdd에 대한 JobHandle을 jobSingle의 JobHandle에 추가함
        // 하지만 최대한 종속성은 피하는 것이 좋음
        JobHandle handleAdd = jobAdd.Schedule(handle);

        // JobSingle이 완료될때까지 메인 스레드가 동작하지 않도록
        // Complete()를 호출하여 대기함. Job 실행이 완료된 후에
        // 메인스레드에서 안전하게 NativeContainer에 접근할 수 있음.
        handleAdd.Complete();

        Debug.LogWarning("result : " + result[0]);

        // NativeContainer 사용한 후에는 Dispose를 호출해서 메모리에서 삭제해야함
        // Dispose를 호출하지 않으면 Unity에서 에러를 냄
        jobSingle.result.Dispose();
    }
}
