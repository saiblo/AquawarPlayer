using System;
using System.Runtime.InteropServices;
using GameHelper;
using GameImpl;
using LitJson;
using UnityEngine.SceneManagement;
using Utils;

public class GameUI : GameBridge
{
    public readonly GameStates GameState;

    public readonly GameObjectManager Gom;

    [DllImport("__Internal")]
    private static extern string GetReplay();

    [DllImport("__Internal")]
    private static extern void JsAlert(string message);

    private bool _alerted;

    public static string MeStr => SharedRefs.Mode == Constants.GameMode.Offline ? "0号AI" : "我方";

    public static string EnemyStr => SharedRefs.Mode == Constants.GameMode.Offline ? "1号AI" : "敌方";

    // Dissolve effect

    internal int DissolveShaderProperty;

    private void UpdateFishStatus(JsonData players)
    {
        for (var i = 0; i < 4; i++)
        {
            myStatus[i].Current = (int) players[0]["fight_fish"][i]["state"] == 2
                ? 0
                : (int) players[0]["fight_fish"][i]["hp"];
            enemyStatus[i].Current = (int) players[1]["fight_fish"][i]["state"] == 2
                ? 0
                : (int) players[1]["fight_fish"][i]["hp"];
            myProfiles[i].SetAtk((int) players[0]["fight_fish"][i]["atk"]);
            enemyProfiles[i].SetAtk((int) players[1]["fight_fish"][i]["atk"]);
        }
    }

    public void AddLog(string s = "")
    {
        Instantiate(logItem, logContent).SetText(s);
    }

    public void DoneAndGoBackToPreparation()
    {
        if (SharedRefs.ErrorFlag)
        {
            SharedRefs.ErrorFlag = false;
            SceneManager.LoadScene("Scenes/Welcome");
            return;
        }
        if (SharedRefs.Mode == Constants.GameMode.Online)
            SceneManager.LoadScene(SharedRefs.OnlineLose + SharedRefs.OnlineWin == 3
                ? "Scenes/Welcome"
                : "Scenes/Preparation");
        else
            SceneManager.LoadScene((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["rounds"] == 3
                ? "Scenes/Welcome"
                : "Scenes/Game");
    }

    public void OnlineCancelBackHome()
    {
        exitConfirmMask.SetActive(false);
    }

    public void ToggleLog()
    {
        logActive = !logActive;
    }

    public void PrevStep()
    {
        Gom.CheckReviveOnBackwards(this);
        SharedRefs.ReplayCursor -= 2;
        if ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 1]["gamestate"] == 2)
            prevStepButton.interactable = false;
        UpdateFishStatus(SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"]);
    }

    public void NextStep()
    {
        prevStepButton.interactable = false;
        nextStepButton.interactable = false;
        prevRoundButton.interactable = false;
        nextRoundButton.interactable = false;
        this.MoveCursor();
    }

    public void PrevRound()
    {
        if (SharedRefs.ReplayCursor > 2 &&
            (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["gamestate"] != 2 &&
            (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 1]["gamestate"] == 2)
            SharedRefs.ReplayCursor -= 3;
        while ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["gamestate"] != 2)
            --SharedRefs.ReplayCursor;
        --SharedRefs.ReplayCursor;
        DoneAndGoBackToPreparation();
    }

    public void NextRound()
    {
        while ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["gamestate"] != 2)
        {
            ++SharedRefs.ReplayCursor;
            if (ErrorParser.HandleErrorCheck(this)) return;
        }
        DoneAndGoBackToPreparation();
    }

    public void ToggleAutoPlay()
    {
        SharedRefs.AutoPlay = !SharedRefs.AutoPlay;
        if (SharedRefs.AutoPlay && nextStepButton.interactable) NextStep();
    }

    public void DoAssertion(int id)
    {
        GameState.Assertion = id;
        assertionModal.gameObject.SetActive(true);
    }

    public void CloseAssertionModal()
    {
        assertionModal.gameObject.SetActive(false);
        assertionExt.gameObject.SetActive(false);
    }

    public void GiveUpAssertion()
    {
        GameState.Assertion = -1;
        CloseAssertionModal();
        doNotAssertButton.SetActive(false);
        this.ChangeStatus();
    }

    public void ConfirmAttack()
    {
        for (var i = 0; i < 4; i++) actionButtons[i].ResetButtons();
        this.ChangeStatus();
    }

    public void ReSelect()
    {
        for (var i = 0; i < 4; i++) actionButtons[i].ResetButtons();
        GameState.GameStatus = Constants.GameStatus.SelectMyFish;
        GameState.MyFishSelected = -1;
    }

    public void ShowHint()
    {
        hintImage.SetActive(true);
    }

    public void HideHint()
    {
        hintImage.SetActive(false);
    }

    // Extension methods

    // void MakeAGuess(bool enemy, int timeout)

    // async void NewRound()

    // void MoveCursor()

    // async void ChangeStatus()

    private void Awake()
    {
        if (SharedRefs.GameClient != null) SharedRefs.GameClient.GameUI = this;
        for (var i = 0; i < Constants.FishNum; i++)
        {
            SharedRefs.FishPrefabs[i] = fishPrefabSamples[i];
            SharedRefs.FishAvatars[i] = fishAvatars[i];
        }
    }

    private void OnDestroy()
    {
        if (SharedRefs.GameClient != null) SharedRefs.GameClient.GameUI = null;
    }

    protected override void RunPerFrame()
    {
        if (SharedRefs.ReplayJson == null)
        {
            var replay = GetReplay();
            if (replay.Length > 0)
            {
                try
                {
                    var replayJson = JsonMapper.ToObject(replay);
                    if (Validators.ValidateJson(replayJson))
                    {
                        if (SharedRefs.ReplayJson == null)
                        {
                            SharedRefs.ReplayCursor = 0;
                            SharedRefs.ReplayJson = replayJson;
                            SharedRefs.Mode = Constants.GameMode.Offline;
                            this.AwakeImpl();
                        }
                    }
                    else
                    {
                        if (!_alerted)
                        {
                            _alerted = true;
                            JsAlert("文件解析失败！");
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!_alerted)
                    {
                        _alerted = true;
                        JsAlert(e.Message);
                    }
                }
            }
        }
        this.RunPerFrameImpl();
    }

    public GameUI()
    {
        GameState = new GameStates();
        Gom = new GameObjectManager(GameState);
    }
}