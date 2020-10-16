using UnityEngine;

public class GameUI : MonoBehaviour
{
    public Transform statusBarPrefab;

    public Transform myStatusRoot;
    public Transform enemyStatusRoot;

    private void Awake()
    {
        for (var i = 0; i < 4; i++)
        {
            Instantiate(statusBarPrefab, myStatusRoot)
                .localPosition = new Vector3(10, -15 * i - 10);
            Instantiate(statusBarPrefab, enemyStatusRoot)
                .localPosition = new Vector3(10, -15 * i - 10);
        }
    }
}