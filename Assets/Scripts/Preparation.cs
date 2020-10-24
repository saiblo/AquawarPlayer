using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Preparation : MonoBehaviour
{
    public Transform fishPrefab;

    public Transform allFishRoot;

    private readonly MeshRenderer[] _fishMeshRenderers = new MeshRenderer[18];

    private readonly EventTrigger[] _fishEventTriggers = new EventTrigger[18];

    private readonly bool[] _fishSelected =
    {
        false, false, false, false, false, false,
        false, false, false, false, false, false,
        false, false, false, false, false, false
    };

    public Button doneButton;

    private void Awake()
    {
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                var fish = Instantiate(fishPrefab, allFishRoot);
                fish.localPosition = new Vector3(j * 3 - 7.5f, -i * 3);
                var id = i * 6 + j;
                _fishMeshRenderers[id] = fish.GetComponent<MeshRenderer>();
                _fishMeshRenderers[id].material.color = Color.gray;
                _fishEventTriggers[id] = fish.GetComponent<EventTrigger>();
            }
        }
        for (var i = 0; i < Constants.BanNum; i++)
        {
            var id = i;
            _fishMeshRenderers[id].material.color = Color.red;
        }
        ActivateFishTriggers();
    }

    private void ActivateFishTriggers()
    {
        for (var i = Constants.BanNum; i < Constants.FishNum; i++)
        {
            var id = i;
            var trigger = new EventTrigger.Entry();
            trigger.callback.AddListener(delegate
            {
                _fishSelected[id] = !_fishSelected[id];
                _fishMeshRenderers[id].material.color = _fishSelected[id] ? Color.green : Color.gray;
            });
            _fishEventTriggers[id].triggers.Add(trigger);
        }
    }

    public void ConfirmSelection()
    {
        SceneManager.LoadScene("Scenes/Game");
    }

    private void Update()
    {
        doneButton.interactable = _fishSelected.Count(b => b) == 4;
    }
}