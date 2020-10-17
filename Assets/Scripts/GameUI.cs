using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Transform fishPrefab;
    public Transform statusBarPrefab;

    public Transform allFishRoot;
    public Transform myStatusRoot;
    public Transform enemyStatusRoot;

    public Button changeStatusButton;
    public Text changeStatusPrompt;

    private enum SelectStatus
    {
        SelectMyFish,
        SelectEnemyFish
    }

    private SelectStatus _selectStatus = SelectStatus.SelectMyFish;

    private readonly List<MeshRenderer> _myFishMeshRenderers = new List<MeshRenderer>();
    private readonly List<MeshRenderer> _enemyFishMeshRenderers = new List<MeshRenderer>();

    private int _myFishSelected = -1;

    private readonly bool[] _myFishSelectedAsTarget = {false, false, false, false};
    private readonly bool[] _enemyFishSelectedAsTarget = {false, false, false, false};

    public void ChangeStatus()
    {
        if (_selectStatus == SelectStatus.SelectMyFish)
        {
            _selectStatus = SelectStatus.SelectEnemyFish;
        }
        else
        {
            _selectStatus = SelectStatus.SelectMyFish;
            _myFishSelected = -1;
            for (var i = 0; i < 4; i++)
                _myFishSelectedAsTarget[i] = _enemyFishSelectedAsTarget[i] = false;
        }
    }

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
            myFishTrigger.callback.AddListener(delegate
            {
                if (_selectStatus == SelectStatus.SelectMyFish)
                    _myFishSelected = _myFishSelected == j ? -1 : j;
                else
                    _myFishSelectedAsTarget[j] = !_myFishSelectedAsTarget[j];
            });
            myFish.GetComponent<EventTrigger>().triggers.Add(myFishTrigger);
            _myFishMeshRenderers.Add(myFish.GetComponent<MeshRenderer>());

            var enemyFish = Instantiate(fishPrefab, allFishRoot);
            enemyFish.localPosition = new Vector3(2 * (i + 2), 0, 2 - i);
            var enemyFishTrigger = new EventTrigger.Entry();
            enemyFishTrigger.callback.AddListener(delegate
            {
                if (_selectStatus == SelectStatus.SelectEnemyFish)
                    _enemyFishSelectedAsTarget[j] = !_enemyFishSelectedAsTarget[j];
            });
            enemyFish.GetComponent<EventTrigger>().triggers.Add(enemyFishTrigger);
            _enemyFishMeshRenderers.Add(enemyFish.GetComponent<MeshRenderer>());
        }
    }

    private void Update()
    {
        for (var i = 0; i < 4; i++)
        {
            if (_myFishSelectedAsTarget[i])
                _myFishMeshRenderers[i].material.color = Color.red;
            else if (_myFishSelected == i)
                _myFishMeshRenderers[i].material.color = Color.green;
            else
                _myFishMeshRenderers[i].material.color = Color.blue;

            _enemyFishMeshRenderers[i].material.color =
                _enemyFishSelectedAsTarget[i] ? Color.red : Color.blue;
        }

        changeStatusButton.interactable =
            _selectStatus == SelectStatus.SelectMyFish && _myFishSelected != -1 ||
            _selectStatus == SelectStatus.SelectEnemyFish &&
            (_myFishSelectedAsTarget.Any(b => b) ||
             _enemyFishSelectedAsTarget.Any(b => b));
        changeStatusPrompt.text = _selectStatus == SelectStatus.SelectMyFish ? "选择我方鱼" : "选择作用对象";
    }
}