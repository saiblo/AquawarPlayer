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
    private readonly bool[] _fishSelected =
        {false, false, false, false, false, false, false, false, false, false, false, false};

    public Button doneButton;

    public GameProfile[] profiles;

    public FishDetail fishDetailPrefab;

    public Transform backgroundBase;

    private void Awake()
    {
        if (SharedRefs.Mode == Constants.GameMode.Offline)
        {
            if (SharedRefs.ReplayCursor == 0) SharedRefs.ReplayCursor = 1;
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
        }
    }

    public void ConfirmSelection()
    {
        if (SharedRefs.Mode == Constants.GameMode.Online)
        {
            var chooseFishs = new List<int>();
            for (var i = 0; i < Constants.FishNum; i++)
                if (_fishSelected[i])
                    chooseFishs.Add(i + 1);
            SharedRefs.GameClient.Send(
                _fishSelected[11]
                    ? new Pick {ChooseFishs = chooseFishs, ImitateFish = 1}
                    : new Pick {ChooseFishs = chooseFishs}
            );
        }
        SceneManager.LoadScene("Scenes/Game");
    }

    protected override void RunPerFrame()
    {
        doneButton.interactable = SharedRefs.Mode == Constants.GameMode.Offline || _fishSelected.Count(b => b) == 4;
    }
}