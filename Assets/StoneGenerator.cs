using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoneGenerator : MonoBehaviour
{
    [Header("돌 프리팹 설정")]
    [Tooltip("작은 크기의 돌 프리팹 배열")]
    public GameObject[] stones_small;
    [Tooltip("중간 크기의 돌 프리팹 배열")]
    public GameObject[] stones_normal;
    [Tooltip("큰 크기의 돌 프리팹 배열")]
    public GameObject[] stones_big;

    [Header("생성할 돌 크기 선택")]
    [Tooltip("true일 경우 작은 돌을 생성합니다.")]
    public bool spawnSmall = true;
    [Tooltip("true일 경우 중간 크기 돌을 생성합니다.")]
    public bool spawnNormal = false;
    [Tooltip("true일 경우 큰 돌을 생성합니다.")]
    public bool spawnBig = false;

    [Header("생성 옵션")]
    [Tooltip("생성할 돌의 총 개수")]
    public int stoneNum = 10;
    [Tooltip("돌이 생성될 반경")]
    public float radius = 10f;
    [Tooltip("돌 사이의 최소 거리")]
    public float minDistance = 1.5f;
    [Tooltip("돌 사이의 최대 거리. 이 거리를 넘어서는 위치에 돌이 생성되지 않도록 하여 군집도를 높입니다.")]
    public float maxDistance = 5f;
    [Tooltip("각 돌의 위치를 찾기 위한 최대 시도 횟수")]
    public int numSamplesBeforeRejection = 30;

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        GenerateStones();
    }

    public void GenerateStones()
    {
        GameObject[] selectedStones = null;

        // 크기 선택 로직 (큰 사이즈 우선)
        if (spawnBig)
        {
            selectedStones = stones_big;
        }
        else if (spawnNormal)
        {
            selectedStones = stones_normal;
        }
        else if (spawnSmall)
        {
            selectedStones = stones_small;
        }

        if (selectedStones == null || selectedStones.Length == 0)
        {
            Debug.LogError("선택된 크기의 돌 프리팹 배열이 비어있습니다. Inspector에서 프리팹을 할당하거나 생성할 크기를 선택해주세요.");
            return;
        }

        // 기존에 생성된 돌이 있다면 삭제
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedPositions.Clear();

        for (int i = 0; i < stoneNum; i++)
        {
            bool foundPosition = false;
            for (int j = 0; j < numSamplesBeforeRejection; j++)
            {
                // 구형 영역 내에서 무작위 위치 후보 생성 (XYZ 공간)
                Vector3 randomPoint = Random.insideUnitSphere * radius;
                Vector3 candidatePosition = transform.position + randomPoint;

                if (IsValidPosition(candidatePosition))
                {
                    spawnedPositions.Add(candidatePosition);
                    // 선택된 배열에서 무작위 프리팹 선택하여 생성
                    GameObject stonePrefab = selectedStones[Random.Range(0, selectedStones.Length)];
                    Instantiate(stonePrefab, candidatePosition, Quaternion.identity, this.transform);
                    foundPosition = true;
                    break;
                }
            }

            if (!foundPosition)
            {
                Debug.LogWarning($"돌 {i + 1}의 적절한 위치를 찾지 못했습니다. 생성된 돌의 수가 요청한 것보다 적을 수 있습니다.");
            }
        }
    }

    private bool IsValidPosition(Vector3 candidate)
    {
        if (!spawnedPositions.Any())
        {
            return true; // 첫 번째 돌은 항상 유효
        }

        float closestDistance = float.MaxValue;
        foreach (var pos in spawnedPositions)
        {
            float distance = Vector3.Distance(candidate, pos);
            if (distance < minDistance)
            {
                return false; // 다른 돌과 너무 가까우면 무효
            }
            closestDistance = Mathf.Min(closestDistance, distance);
        }

        // 가장 가까운 돌이 maxDistance보다 멀리 있으면 무효 (너무 흩어지지 않게)
        if (closestDistance > maxDistance)
        {
            return false;
        }

        return true;
    }
}
