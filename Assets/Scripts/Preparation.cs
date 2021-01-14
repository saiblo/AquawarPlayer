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

    private readonly List<int> _selected = new List<int>();

    private int _playerId;

    private void Push(int id)
    {
        _selected.Add(id);
        _fishSelectStatus[id] = SelectStatus.Selected;
        profiles[id].GetComponent<Image>().color = new Color(0, 1, 0, 0.8f);
        for (var i = 0; i < 4; i++)
            queue[i].overrideSprite = i < _selected.Count ? SharedRefs.FishAvatars[_selected[i]] : null;
    }

    public void Drop(int pos)
    {
        if (pos >= _selected.Count) return;
        var id = _selected[pos];
        _selected.RemoveAt(pos);
        _fishSelectStatus[id] = SelectStatus.Available;
        profiles[id].GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        for (var i = 0; i < 4; i++)
            queue[i].overrideSprite = i < _selected.Count ? SharedRefs.FishAvatars[_selected[i]] : null;
    }

    private void Awake()
    {
        if (SharedRefs.Mode == Constants.GameMode.Offline)
        {
            if (SharedRefs.ReplayCursor == 0) SharedRefs.ReplayCursor = 1;
            var myFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"][_playerId]["my_fish"];
            var pickFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor + 1]["operation"][0]["Fish"];
            for (var i = 0; i < myFish.Count; i++)
            {
                _fishSelectStatus[(int) myFish[i]["id"] - 1] = SelectStatus.Available;
                Push((int) pickFish[0][i]["id"] - 1);
            }
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
        if (_selected.Count < 4 && _fishSelectStatus[i] == SelectStatus.Available) Push(i);
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
        doneButton.interactable = _selected.Count == 4;
    }
}