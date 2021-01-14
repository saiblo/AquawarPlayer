using GameHelper;
using GameImpl;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class GameUI : GameBridge
{
    public readonly GameStates GameState;

    public readonly GameObjectManager Gom;

    // Dissolve effect

    internal int DissolveShaderProperty;

    public void Dissolve(bool enemy, int pos)
    {
        if (enemy) GameState.EnemyFishAlive[pos] = false;
        else GameState.MyFishAlive[pos] = false;

        var meshRenderers = (enemy ? Gom.EnemyFishMeshRenderers : Gom.MyFishMeshRenderers)[pos];
        var fish = (enemy ? Gom.EnemyFishTransforms : Gom.MyFishTransforms)[pos];
        var fog = (enemy ? Gom.EnemyFogs : Gom.MyFogs)[pos];
        (enemy ? Gom.EnemyFishParticleSystems : Gom.MyFishParticleSystems)[pos].Play();

        foreach (var meshRenderer in meshRenderers) meshRenderer.material = dissolveEffect;
        Repeat(cnt =>
            {
                foreach (var meshRenderer in meshRenderers)
                    meshRenderer.material.SetFloat(DissolveShaderProperty,
                        fadeIn.Evaluate(Mathf.InverseLerp(0, 3, cnt / 100f)));
            },
            () =>
            {
                if (fish != null) Destroy(fish.gameObject);
                fog.gameObject.SetActive(false);
            },
            300, 0, 10);
    }

    public void UpdateFishStatus(JsonData players)
    {
        for (var i = 0; i < 4; i++)
        {
            myStatus[i].Current = (int) players[0]["fight_fish"][i]["hp"];
            enemyStatus[i].Current = (int) players[1]["fight_fish"][i]["hp"];
            myProfiles[i].SetHp(myStatus[i].Current);
            enemyProfiles[i].SetHp(enemyStatus[i].Current);
            myProfiles[i].SetAtk((int) players[0]["fight_fish"][i]["atk"]);
            enemyProfiles[i].SetAtk((int) players[1]["fight_fish"][i]["atk"]);
        }
    }

    public void DoneAndGoBackToPreparation()
    {
        SceneManager.LoadScene("Scenes/Preparation");
    }

    public void ToggleLog(GameObject logObj)
    {
        logObj.SetActive(!logObj.activeSelf);
    }

    public void PrevStep()
    {
        Gom.CheckReviveOnBackwards();
        SharedRefs.ReplayCursor -= 2;
        if ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 1]["gamestate"] == 2)
        {
            prevStepButton.interactable = false;
            replayStepButton.interactable = false;
        }
        UpdateFishStatus(SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"]);
    }

    public void NextStep()
    {
        prevStepButton.interactable = false;
        nextStepButton.interactable = false;
        replayStepButton.interactable = false;
        prevRoundButton.interactable = false;
        nextRoundButton.interactable = false;
        this.MoveCursor();
    }

    public void PrevRound()
    {
        while ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["gamestate"] != 2)
            --SharedRefs.ReplayCursor;
        DoneAndGoBackToPreparation();
    }

    public void NextRound()
    {
        while ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["gamestate"] != 2)
            ++SharedRefs.ReplayCursor;
        DoneAndGoBackToPreparation();
    }

    public void ReplayStep()
    {
        PrevStep();
        NextStep();
    }

    public void ToggleAutoPlay()
    {
        SharedRefs.AutoPlay = !SharedRefs.AutoPlay;
        if (SharedRefs.AutoPlay && nextStepButton.interactable) NextStep();
    }

    public void DoAssertion(int id)
    {
        GameState.Assertion = id;
        this.ChangeStatus();
    }

    public void GiveUpAssertion()
    {
        GameState.Assertion = -1;
        this.ChangeStatus();
    }

    public void FakeAttack()
    {
        GameState.MyFishSelected = 0;
        this.ChangeStatus();
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