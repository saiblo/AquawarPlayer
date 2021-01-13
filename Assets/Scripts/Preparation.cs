using System.Collections.Generic;
using System.Linq;
using Components;
using GameHelper;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class Preparation : EnhancedMonoBehaviour
{
    private enum SelectStatus
    {
        Available,
        Selected,
        Unavailable
    }

    private readonly SelectStatus[] _fishSelectStatus =
    {
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable,
        SelectStatus.Unavailable
    };

    public Button doneButton;

    public GameProfile[] profiles;

    public FishDetail fishDetailPrefab;

    public Transform backgroundBase;

    private int _playerId = 0;

    private void Awake()
    {
        if (SharedRefs.Mode == Constants.GameMode.Offline)
        {
            if (SharedRefs.ReplayCursor == 0) SharedRefs.ReplayCursor = 1;
            var myFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"][_playerId]["my_fish"];
            for (var i = 0; i < myFish.Count; i++)
                _fishSelectStatus[(int) myFish[i]["id"] - 1] = SelectStatus.Available;
        }
        else
        {
            /* var result = await SharedRefs.GameClient.Receive();
            if ((string) result["Action"] == "Pick")
            {
                var remaining = result["RemainFishs"];
            } */
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
            for (var i = 0; i < Constants.FishNum; i++)
                if (_fishSelectStatus[i] == SelectStatus.Selected)
                    chooseFishs.Add(i + 1);
            SharedRefs.GameClient.Send(
                _fishSelectStatus[11] == SelectStatus.Selected
                    ? new Pick {ChooseFishs = chooseFishs, ImitateFish = 1}
                    : new Pick {ChooseFishs = chooseFishs}
            );
        }
        SceneManager.LoadScene("Scenes/Game");
    }

    protected override void RunPerFrame()
    {
        doneButton.interactable = SharedRefs.Mode == Constants.GameMode.Offline ||
                                  _fishSelectStatus.Count(status => status == SelectStatus.Selected) == 4;
    }
}