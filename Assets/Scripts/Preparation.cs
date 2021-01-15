using System;
using System.Collections.Generic;
using Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class Preparation : MonoBehaviour
{
    private enum SelectStatus
    {
        Unavailable,
        Available,
        Selected
    }

    private readonly SelectStatus[] _fishSelectStatus = new SelectStatus[Constants.FishNum];

    public Button doneButton;

    public Button turnButton;

    public GameProfile[] profiles;

    public FishDetail fishDetailPrefab;

    public Transform backgroundBase;

    public Image[] queue;

    private readonly List<int> _selectedList = new List<int>();

    private readonly Color _unavailable = new Color(1, 0, 0, 0.8f);
    private readonly Color _available = new Color(0, 0, 0, 0.5f);
    private readonly Color _selected = new Color(0, 1, 0, 0.8f);

    private int _playerId;

    private bool _imitating;

    private int _imitateId;

    private void DisplayQueue()
    {
        for (var i = 0; i < 4; i++)
            queue[i].overrideSprite = i < _selectedList.Count
                ? SharedRefs.FishAvatars[_selectedList[i] == 11 ? _imitateId : _selectedList[i]]
                : null;
    }

    private void Push(int id)
    {
        _selectedList.Add(id);
        _fishSelectStatus[id] = SelectStatus.Selected;
        profiles[id].GetComponent<Image>().color = _selected;
        DisplayQueue();
    }

    public void Drop(int pos)
    {
        if (_imitating || pos >= _selectedList.Count) return;
        var id = _selectedList[pos];
        _selectedList.RemoveAt(pos);
        _fishSelectStatus[id] = SelectStatus.Available;
        profiles[id].GetComponent<Image>().color = _available;
        DisplayQueue();
    }

    private void Awake()
    {
        turnButton.gameObject.SetActive(false);
        var result = SharedRefs.PickInfo;
        if ((string) result["Action"] == "Pick")
        {
            var remaining = result["RemainFishs"];
            for (var i = 0; i < remaining.Count; i++)
                _fishSelectStatus[(int) remaining[i] - 1] = SelectStatus.Available;
        }
        for (var i = 0; i < Constants.FishNum; i++)
        {
            var detail = Instantiate(fishDetailPrefab, backgroundBase);
            detail.GetComponent<Transform>().localPosition = new Vector3(1470, 410);
            detail.SetupFish(i);
            detail.gameObject.SetActive(false);
            profiles[i].SetupFish(i, detail);
            profiles[i].SetHp(400);
            profiles[i].SetAtk(100);
            if (_fishSelectStatus[i] == SelectStatus.Unavailable)
                profiles[i].GetComponent<Image>().color = _unavailable;
        }
    }

    public void ConfirmSelection()
    {
        var chooseFishs = new List<int>();
        SharedRefs.FishChosen = new List<int>();
        foreach (var i in _selectedList)
        {
            chooseFishs.Add(i + 1);
            SharedRefs.FishChosen.Add(i);
        }
        if (_fishSelectStatus[11] == SelectStatus.Selected)
            SharedRefs.GameClient.Send(new PickWithImitate {ChooseFishs = chooseFishs, ImitateFish = _imitateId + 1});
        else
            SharedRefs.GameClient.Send(new Pick {ChooseFishs = chooseFishs});
        SceneManager.LoadScene("Scenes/Game");
    }

    public void ToggleSelection(int i)
    {
        if (_imitating)
        {
            _imitating = false;
            _imitateId = i;
            Push(11);
            for (var j = 0; j < Constants.FishNum; j++)
            {
                Color c;
                switch (_fishSelectStatus[j])
                {
                    case SelectStatus.Unavailable:
                        c = _unavailable;
                        break;
                    case SelectStatus.Available:
                        c = _available;
                        break;
                    case SelectStatus.Selected:
                        c = _selected;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                profiles[j].GetComponent<Image>().color = c;
            }
        }
        else if (_selectedList.Count < 4 && _fishSelectStatus[i] == SelectStatus.Available)
        {
            if (i == 11)
            {
                _imitating = true;
                for (var j = 0; j < 10; j++) profiles[j].GetComponent<Image>().color = _available;
                profiles[11].GetComponent<Image>().color = _unavailable;
            }
            else
            {
                Push(i);
            }
        }
    }

    public void BackToWelcome()
    {
        SceneManager.LoadScene("Scenes/Welcome");
    }

    public void SwitchPlayer()
    {
        _playerId = 1 - _playerId;
    }

    private void Update()
    {
        doneButton.interactable = _selectedList.Count == 4;
    }
}