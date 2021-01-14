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

    private int _selectNum;

    public Button doneButton;

    public Button turnButton;

    public GameProfile[] profiles;

    public FishDetail fishDetailPrefab;

    public Transform backgroundBase;

    private int _playerId;

    private void Awake()
    {
        if (SharedRefs.Mode == Constants.GameMode.Offline)
        {
            if (SharedRefs.ReplayCursor == 0) SharedRefs.ReplayCursor = 1;
            var myFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"][_playerId]["my_fish"];
            for (var i = 0; i < myFish.Count; i++)
                _fishSelectStatus[(int) myFish[i]["id"] - 1] = SelectStatus.Available;
            _selectNum = 4;
        }
        else
        {
            turnButton.gameObject.SetActive(false);
            var result = SharedRefs.PickInfo;
            if ((string) result["Action"] == "Pick")
            {
                var remaining = result["RemainFishs"];
                for (var i = 0; i < remaining.Count; i++)
                    _fishSelectStatus[(int) remaining[i] - 1] = SelectStatus.Available;
            }
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
                profiles[i].GetComponent<Image>().color = new Color(1, 0, 0, 0.8f);
        }
    }

    public void ConfirmSelection()
    {
        if (SharedRefs.Mode == Constants.GameMode.Online)
        {
            var chooseFishs = new List<int>();
            SharedRefs.FishChosen = new List<int>();
            for (var i = 0; i < Constants.FishNum; i++)
                if (_fishSelectStatus[i] == SelectStatus.Selected)
                {
                    chooseFishs.Add(i + 1);
                    SharedRefs.FishChosen.Add(i);
                }
            SharedRefs.GameClient.Send(
                _fishSelectStatus[11] == SelectStatus.Selected
                    ? new PickWithImitate {ChooseFishs = chooseFishs, ImitateFish = 1}
                    : new Pick {ChooseFishs = chooseFishs}
            );
        }
        SceneManager.LoadScene("Scenes/Game");
    }

    public void ToggleSelection(int i)
    {
        switch (_fishSelectStatus[i])
        {
            case SelectStatus.Unavailable:
                break;
            case SelectStatus.Available:
                _fishSelectStatus[i] = SelectStatus.Selected;
                profiles[i].GetComponent<Image>().color = new Color(0, 1, 0, 0.8f);
                ++_selectNum;
                break;
            case SelectStatus.Selected:
                _fishSelectStatus[i] = SelectStatus.Available;
                profiles[i].GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
                --_selectNum;
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
        doneButton.interactable = _selectNum == 4;
    }
}