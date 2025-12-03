using UnityEngine;

public class Singleton <T> : MonoBehaviour where T: Component
{
    private static T _instance;

    public static T Instance
    {
        get // get 생성자로 public static 속성 구현 
        {
            if(_instance == null) // 새로운 인스턴스를 초기화하기 전 다른 인스턴스가 있는지 확인
            {
                _instance = FindObjectOfType<T>(); // 지정된 타입의 첫 번째로 로드된 오브젝트 검색
                if(_instance == null) // 만약 없다면, 새로은 게임 오브젝트 생성
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name; // 이름을 바꾼 후
                    _instance = obj.AddComponent<T>(); // 지정된 타입의 컴포넌트를 추가
                }
            }
            return _instance;
        }
    }
    public virtual void Awake() // 파생 클래스에서 재정의 가능
    {
        if(_instance == null) // 메모리에 초기화된 자신의 인스턴스가 이미 있는지 확인
        {
            _instance = this as T; // 없다면 자기 자신의 현재 인스턴스가 됨.
            DontDestroyOnLoad(gameObject); // 새로운 씬이 로드될 때 오브젝트 파괴를 막음
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있다면 복제를 피하기 위해 스스로를 제거
        }
    }
}