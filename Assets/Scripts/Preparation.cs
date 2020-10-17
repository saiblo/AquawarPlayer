using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Preparation : MonoBehaviour
{
    public Transform fishPrefab;

    public Transform allFishRoot;

    private readonly bool[] _fishSelected =
        {false, false, false, false, false, false, false, false, false, false, false, false};

    public Button doneButton;

    private void Awake()
    {
        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                var fish = Instantiate(fishPrefab, allFishRoot);
                fish.localPosition = new Vector3(j * 3 - 7.5f, -i * 3);
                var fishMeshRenderer = fish.GetComponent<MeshRenderer>();
                var trigger = new EventTrigger.Entry();
                var id = i * 6 + j;
                trigger.callback.AddListener(delegate
                {
                    _fishSelected[id] = !_fishSelected[id];
                    fishMeshRenderer.material.color = _fishSelected[id] ? Color.green : Color.gray;
                });
                fishMeshRenderer.material.color = Color.gray;
                fish.GetComponent<EventTrigger>().triggers.Add(trigger);
            }
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