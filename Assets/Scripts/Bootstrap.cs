using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private Match3 match3;

    private PlayerData _playerData;

    void Start()
    {
        InitializeData();

        InitializeMatch3();
    }

    private void InitializeData()
    {
        _playerData = new PlayerData();
    }

    private void InitializeMatch3()
    {
        match3.Initialize(_playerData);
    }

}
