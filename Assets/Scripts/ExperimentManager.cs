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
    public string experimentCode;

    private List<float> tctList = new List<float>();
    private List<bool> bList = new List<bool>();
    private int _successCount = 0;
    private int _failureCount = 0;

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
        // 1. 파일 이름 설정 (기존과 동일)
        string fileName = $"ExperimentResult_{experimentCode}_{spawnDensity}.csv";

        // 2. 저장 폴더 경로 설정 (Assets/ExperimentResult)
        // Application.dataPath는 프로젝트의 "Assets" 폴더를 가리킵니다.
        string folderPath = Path.Combine(Application.dataPath, "ExperimentResult");

        // 3. 해당 폴더가 없으면 새로 생성
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("ExperimentResult 폴더가 생성되었습니다.");
        }

        // 4. 최종 파일 경로 설정
        string filePath = Path.Combine(folderPath, fileName);

        float totalTCT = 0f;

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // [섹션 1] Trial 별 상세 데이터
                writer.WriteLine("Trial_Index,Task_Completion_Time,isSuccess");
                
                for (int i = 0; i < tctList.Count; i++)
                {
                    string line = $"{i + 1},{tctList[i]}, {bList[i]}";
                    writer.WriteLine(line);
                    totalTCT += tctList[i];
                }

                // 평균 계산
                float averageTCT = tctList.Count > 0 ? totalTCT / tctList.Count : 0f;

                // [섹션 2] 실험 요약 데이터
                writer.WriteLine(); 
                writer.WriteLine("Metric,Value"); 
                
                writer.WriteLine($"Average_TCT,{averageTCT}");
                writer.WriteLine($"Total_Success,{_successCount}");
                writer.WriteLine($"Total_Failure,{_failureCount}");
                writer.WriteLine($"Total_Trials,{tctList.Count}");
            }

            Debug.LogWarning($"CSV 파일이 저장되었습니다: {filePath}");
            
            // Unity 에디터에서 생성된 파일/폴더를 즉시 인식하도록 새로고침
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
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

    public void AddCount(bool input)
    {
        if(input == true)
        {
            _successCount += 1;
            bList.Add(true);
        }
        else
        {
            _failureCount += 1;
            bList.Add(false);
        }
    }
}