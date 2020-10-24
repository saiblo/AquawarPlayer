using System;
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
        DoAssertion,
        SelectMyFish,
        SelectEnemyFish
    }

    private SelectStatus _selectStatus = SelectStatus.DoAssertion;

    private readonly List<MeshRenderer> _myFishMeshRenderers = new List<MeshRenderer>();
    private readonly List<MeshRenderer> _enemyFishMeshRenderers = new List<MeshRenderer>();

    private int _myFishSelected = -1;

    private int _assertion = -1;

    private readonly bool[] _myFishSelectedAsTarget = {false, false, false, false};
    private readonly bool[] _enemyFishSelectedAsTarget = {false, false, false, false};

    public void ChangeStatus()
    {
        switch (_selectStatus)
        {
            case SelectStatus.DoAssertion:
                _selectStatus = SelectStatus.SelectMyFish;
                // if (_assertion != -1)
                // {
                _assertion = -1;
                // }
                break;
            case SelectStatus.SelectMyFish:
                _selectStatus = SelectStatus.SelectEnemyFish;
                break;
            case SelectStatus.SelectEnemyFish:
                _selectStatus = SelectStatus.SelectMyFish;
                _myFishSelected = -1;
                for (var i = 0; i < 4; i++)
                    _myFishSelectedAsTarget[i] = _enemyFishSelectedAsTarget[i] = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
                switch (_selectStatus)
                {
                    case SelectStatus.DoAssertion:
                        break;
                    case SelectStatus.SelectMyFish:
                        _myFishSelected = _myFishSelected == j ? -1 : j;
                        break;
                    case SelectStatus.SelectEnemyFish:
                        _myFishSelectedAsTarget[j] = !_myFishSelectedAsTarget[j];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            myFish.GetComponent<EventTrigger>().triggers.Add(myFishTrigger);
            _myFishMeshRenderers.Add(myFish.GetComponent<MeshRenderer>());

            var enemyFish = Instantiate(fishPrefab, allFishRoot);
            enemyFish.localPosition = new Vector3(2 * (i + 2), 0, 2 - i);
            var enemyFishTrigger = new EventTrigger.Entry();
            enemyFishTrigger.callback.AddListener(delegate
            {
                switch (_selectStatus)
                {
                    case SelectStatus.DoAssertion:
                        _assertion = _assertion == j ? -1 : j;
                        break;
                    case SelectStatus.SelectMyFish:
                        break;
                    case SelectStatus.SelectEnemyFish:
                        _enemyFishSelectedAsTarget[j] = !_enemyFishSelectedAsTarget[j];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
                _enemyFishSelectedAsTarget[i] || i == _assertion ? Color.red : Color.blue;
        }

        changeStatusButton.interactable =
            _selectStatus == SelectStatus.DoAssertion ||
            _selectStatus == SelectStatus.SelectMyFish && _myFishSelected != -1 ||
            _selectStatus == SelectStatus.SelectEnemyFish &&
            (_myFishSelectedAsTarget.Any(b => b) ||
             _enemyFishSelectedAsTarget.Any(b => b));

        string title;
        switch (_selectStatus)
        {
            case SelectStatus.DoAssertion:
                title = _assertion == -1 ? "放弃断言" : "进行断言";
                break;
            case SelectStatus.SelectMyFish:
                title = "选择我方鱼";
                break;
            case SelectStatus.SelectEnemyFish:
                title = "选择作用对象";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        changeStatusPrompt.text = title;
    }
}