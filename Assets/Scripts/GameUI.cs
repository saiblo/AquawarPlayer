using UnityEngine;

public class GameUI : MonoBehaviour
{
    public Transform fishPrefab;
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
            Instantiate(fishPrefab, transform)
                .localPosition = new Vector3(2 * (i + 2), 0, 2 - i);
            Instantiate(fishPrefab, transform)
                .localPosition = new Vector3(-2 * (i + 2), 0, 2 - i);
        }
    }
}