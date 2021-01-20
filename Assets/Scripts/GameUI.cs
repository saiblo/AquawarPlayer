using GameHelper;
using GameImpl;
using LitJson;
using UnityEngine.SceneManagement;
using Utils;

public class GameUI : GameBridge
{
    public readonly GameStates GameState;

    public readonly GameObjectManager Gom;

    public static string MeStr => SharedRefs.Mode == Constants.GameMode.Offline ? "0号AI" : "我方";

    public static string EnemyStr => SharedRefs.Mode == Constants.GameMode.Offline ? "1号AI" : "敌方";

    // Dissolve effect

    internal int DissolveShaderProperty;

    public void UpdateFishStatus(JsonData players)
    {
        for (var i = 0; i < 4; i++)
        {
            myStatus[i].Current = (int) players[0]["fight_fish"][i]["state"] == 2
                ? 0
                : (int) players[0]["fight_fish"][i]["hp"];
            enemyStatus[i].Current = (int) players[1]["fight_fish"][i]["state"] == 2
                ? 0
                : (int) players[1]["fight_fish"][i]["hp"];
            myProfiles[i].SetHp(myStatus[i].Current);
            enemyProfiles[i].SetHp(enemyStatus[i].Current);
            myProfiles[i].SetAtk((int) players[0]["fight_fish"][i]["atk"]);
            enemyProfiles[i].SetAtk((int) players[1]["fight_fish"][i]["atk"]);
        }
    }

    public void AddLog(string s)
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
        SceneManager.LoadScene(SharedRefs.Mode == Constants.GameMode.Online ? "Scenes/Preparation" :
            (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["rounds"] == 3 ? "Scenes/Welcome" : "Scenes/Game");
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
        if (SharedRefs.ReplayCursor > 1 &&
            (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["gamestate"] != 2 &&
            (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 1]["gamestate"] == 2)
            SharedRefs.ReplayCursor -= 3;
        while ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["gamestate"] != 2)
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
        ++SharedRefs.ReplayCursor;
        if (ErrorParser.HandleErrorCheck(this)) return;
        DoneAndGoBackToPreparation();
    }

    public void BackHome()
    {
        SceneManager.LoadScene("Scenes/Welcome");
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

    // Extension methods

    // void MakeAGuess(bool enemy, int timeout)

    // async void NewRound()

    // void MoveCursor()

    // async void ChangeStatus()

    private void Awake()
    {
        this.AwakeImpl();
    }

    protected override void RunPerFrame()
    {
        this.RunPerFrameImpl();
    }

    public GameUI()
    {
        GameState = new GameStates();
        Gom = new GameObjectManager(GameState);
    }
}