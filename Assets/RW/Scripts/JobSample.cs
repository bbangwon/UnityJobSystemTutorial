using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class JobSample : MonoBehaviour
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

    void Start()
    {
        // Job을 생성한 후 a, b에 값을 할당
        JobSingle jobSingle = new JobSingle();
        jobSingle.a = 1;
        jobSingle.b = 2;

        //메인 스레드에서 값을 사용하기 위해 NativeArray를 생성
        jobSingle.result = new NativeArray<int>(1, Allocator.TempJob);

        // Job을 실행 할 수 있도록 워커 스레드에 예약함.
        // 메인 스레드에서는 Schedule만 호출할 수 있음.
        JobHandle handle = jobSingle.Schedule();

        // JobSingle이 완료될때까지 메인 스레드가 동작하지 않도록
        // Complete()를 호출하여 대기함. Job 실행이 완료된 후에
        // 메인스레드에서 안전하게 NativeContainer에 접근할 수 있음.
        handle.Complete();

        Debug.LogWarning("result : " + jobSingle.result[0]);

        // NativeContainer 사용한 후에는 Dispose를 호출해서 메모리에서 삭제해야함
        // Dispose를 호출하지 않으면 Unity에서 에러를 냄
        jobSingle.result.Dispose();
    }
}
