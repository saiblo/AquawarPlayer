using GameHelper;
using GameImpl;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class GameUI : GameBridge
{
    public readonly GameStates GameState;

    public readonly GameObjectManager Gom;

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

    public void DoneAndGoBackToPreparation()
    {
        SceneManager.LoadScene(SharedRefs.Mode == Constants.GameMode.Online ? "Scenes/Preparation" :
            (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["rounds"] == 3 ? "Scenes/Welcome" : "Scenes/Game");
    }

    public void ToggleLog(GameObject logObj)
    {
        logObj.SetActive(!logObj.activeSelf);
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
            ++SharedRefs.ReplayCursor;
        ++SharedRefs.ReplayCursor;
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

    public void SetAttackType(bool normal)
    {
        GameState.NormalAttack = normal;
        GameState.GameStatus = Constants.GameStatus.SelectEnemyFish;
        normalAttackButton.GetComponent<Image>().overrideSprite = normal ? darkBlue : lightBlue;
        skillAttackButton.GetComponent<Image>().overrideSprite = normal ? lightBlue : darkBlue;
    }

    public void ConfirmAttack()
    {
        this.ChangeStatus();
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