using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class JobParallelForSample : MonoBehaviour
{
    /// <summary>
    /// Excute의 Index를 result 배열에 넣어주는 단순한 예제
    /// IJob이 한번에 여러개의 워커 스레드에서 실행된다는 것 외에 기본적인 개념은 IJob과 동일
    /// 한번에 여러개의 Transform을 처리하는 IJobParallelForTransform 도 있음.
    /// </summary>
    struct JobParallel : IJobParallelFor
    {
        public NativeArray<int> result;
        public void Execute(int index)
        {
            result[index] = index;
        }
    }

    // Job을 실행할 횟수
    int count = 10000;
    // 하나의 워커 스레드당 설정할 Job의 개수
    // 워커스레드가 3개인 경우 10000개의 Job을 10개씩 나누어 3개의 워커 스레드에 나눈다.
    // 10000/10 = 1000/3 = 333개 묶음으로 나뉨
    int batchCount = 10;

    private void Start()
    {
        NativeArray<int> result = new NativeArray<int>(count, Allocator.TempJob);

        JobParallel jobParallel = new JobParallel();
        jobParallel.result = result;

        // count, batchCount에 따라 워커 스레드에게 Job을 나누어 줌
        JobHandle handle = jobParallel.Schedule(count, batchCount);

        handle.Complete();
        result.Dispose();
    }



}
