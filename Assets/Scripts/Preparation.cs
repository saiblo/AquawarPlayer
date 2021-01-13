using System.Collections.Generic;
using System.Linq;
using GameHelper;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class Preparation : EnhancedMonoBehaviour
{
    private readonly bool[] _fishSelected =
        {false, false, false, false, false, false, false, false, false, false, false, false};

    public Button doneButton;

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