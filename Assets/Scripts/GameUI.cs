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

        var meshRenderer = (enemy ? Gom.EnemyFishRenderers : Gom.MyFishRenderers)[pos];
        var fish = (enemy ? Gom.EnemyFishTransforms : Gom.MyFishTransforms)[pos];
        var question = (enemy ? Gom.EnemyQuestions : Gom.MyQuestions)[pos];
        (enemy ? Gom.EnemyFishParticleSystems : Gom.MyFishParticleSystems)[pos].Play();

        meshRenderer.material = dissolveEffect;
        Repeat(cnt =>
            {
                meshRenderer.material.SetFloat(DissolveShaderProperty,
                    fadeIn.Evaluate(Mathf.InverseLerp(0, 3, cnt / 100f)));
            },
            () =>
            {
                if (fish != null) Destroy(fish.gameObject);
                if (question != null) Destroy(question.gameObject);
            },
            300, 0, 10);
    }

    public void DisplayHpOnline()
    {
        for (var i = 0; i < 4; i++)
        {
            myStatus[i].Current = GameState.MyFishOnlineHp[i];
            enemyStatus[i].Current = GameState.EnemyFishOnlineHp[i];
        }
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

    /// <summary>
    ///   WARNING: NO CURSOR MOVEMENT INVOLVED!
    /// </summary>
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
        SharedRefs.ReplayCursor -= 2;
        if ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 1]["gamestate"] == 2)
            prevStepButton.interactable = false;
        UpdateFishStatus(SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"]);
    }

    public void NextStep()
    {
        prevStepButton.interactable = false;
        nextStepButton.interactable = false;
        this.MoveCursor();
    }

    public void ToggleAutoPlay()
    {
        SharedRefs.AutoPlay = !SharedRefs.AutoPlay;
        if (SharedRefs.AutoPlay && nextStepButton.interactable) NextStep();
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