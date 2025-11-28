using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    public Transform playerCameraTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(playerCameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if(mainCamera != null)
            {
                playerCameraTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogWarning("FacePlayer 스크립트: 플레이어 카메라 Transform이 할당되지 않았고, 'MainCamera' 태그를 가진 카메라를 찾을 수 없습니다. 이 스크립트는 작동하지 않을 것입니다.");
                enabled = false; // 스크립트 비활성화
            }
        }
    }

    void LateUpdate()
    {
        if(playerCameraTransform == null)
        {
            return;
        }

        Vector3 directionToCamera = playerCameraTransform.position - transform.position;
        directionToCamera.y = 0;

        if(directionToCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            transform.rotation = targetRotation;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
