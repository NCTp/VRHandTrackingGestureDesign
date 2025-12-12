using System.Collections.Generic;
using System.IO;   // 파일 저장을 위해 필수
using System.Text; // 인코딩 처리를 위해 사용
using Unity.VisualScripting;
using UnityEngine;

public class ExperimentManager : Singleton<ExperimentManager>
{
    [Header("Experiment Settings")]
    public int spawnCount = 10;
    public MoonObjectSpawner.SpawnDensity spawnDensity = MoonObjectSpawner.SpawnDensity.Normal; // MoonObjectSpawner 참조가 필요하다면 주석 해제
    public int experimentCode;

    private List<float> tctList = new List<float>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartExperiment();
        }
    } 

    void StartExperiment()
    {
        MoonObjectSpawner.Instance.ReGenerateObjects();
        Debug.LogWarning("Start Experiment");
    }

    // 외부에서 실험 종료 시 이 함수를 호출하면 CSV 저장 후 종료됩니다.
    public void EndExperiment()
    {
        Debug.LogWarning("End Experiment - Saving Data...");

        // --- CSV 저장 로직 시작 ---
        SaveToCSV(); 
        // -----------------------

        // #if UNITY_EDITOR : 유니티 에디터 환경에서만 실행되는 코드
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    
    // #else : 빌드된 앱 환경에서 실행되는 코드
    #else
        Application.Quit();
    #endif
    }

    private void SaveToCSV()
    {
        // 1. 파일 이름 설정 (날짜_시간_Result.csv) 중복 방지
        string fileName = $"ExperimentResult_{experimentCode}.csv";
        
        // 2. 저장 경로 설정 (Assets 폴더 경로)
        // 빌드 후에는 실행 파일 옆 데이터 폴더 등에 저장됨
        string filePath = Path.Combine(Application.dataPath, fileName);

        try
        {
            // 3. StreamWriter를 사용해 파일 작성
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // 헤더(컬럼 이름) 작성
                writer.WriteLine("Trial_Index,Task_Completion_Time");

                // 리스트에 있는 데이터 한 줄씩 작성
                for (int i = 0; i < tctList.Count; i++)
                {
                    // 예: 1, 3.452
                    // 인덱스는 0부터 시작하므로 +1을 해서 1부터 시작하게 함
                    string line = $"{i + 1},{tctList[i]}";
                    writer.WriteLine(line);
                }
            }

            Debug.LogWarning($"CSV 파일이 성공적으로 저장되었습니다: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSV 저장 중 오류 발생: {e.Message}");
        }
    }

    public void SaveTaskCompletionTimeEachTrial(float time)
    {
        if(tctList.Count < spawnCount)
        {
            tctList.Add(time);
            Debug.LogWarning((tctList.Count) + " 번째 Trial 에서의 TCT는 : " + tctList[tctList.Count - 1]);
        }
        else
        {
            // 리스트가 꽉 찼을 때 처리 (이미 외부에서 EndExperiment를 호출한다면 여기서 리턴만 해도 무방)
            return;
        }
    }
}