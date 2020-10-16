using UnityEngine;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour
{
    public Transform fishPrefab;
    public Transform statusBarPrefab;

    public Transform allFishRoot;
    public Transform myStatusRoot;
    public Transform enemyStatusRoot;

    private void Awake()
    {
        for (var i = 0; i < 4; i++)
        {
            var j = i;
            Instantiate(statusBarPrefab, myStatusRoot)
                .localPosition = new Vector3(10, -15 * i - 10);
            Instantiate(statusBarPrefab, enemyStatusRoot)
                .localPosition = new Vector3(10, -15 * i - 10);
            var myFish = Instantiate(fishPrefab, allFishRoot);
            myFish.localPosition = new Vector3(-2 * (i + 2), 0, 2 - i);
            var myFishTrigger = new EventTrigger.Entry();
            myFishTrigger.callback.AddListener(delegate { Debug.Log($"My fish {j} clicked!"); });
            myFish.GetComponent<EventTrigger>().triggers.Add(myFishTrigger);
            var enemyFish = Instantiate(fishPrefab, allFishRoot);
            enemyFish.localPosition = new Vector3(2 * (i + 2), 0, 2 - i);
            var enemyFishTrigger = new EventTrigger.Entry();
            enemyFishTrigger.callback.AddListener(delegate { Debug.Log($"Enemy fish {j} clicked!"); });
            enemyFish.GetComponent<EventTrigger>().triggers.Add(enemyFishTrigger);
        }
    }
}