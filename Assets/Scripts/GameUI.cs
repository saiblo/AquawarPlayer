using GameHelper;
using GameImpl;
using LitJson;
using UnityEngine;

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

    // UI related

    public void SwitchToNormal()
    {
        GameState.NormalAttack = true;
    }

    public void SwitchToSkill()
    {
        GameState.NormalAttack = false;
    }

    public void DummyChangeStatus()
    {
        this.ChangeStatus();
    }

    public void DisplayHp(JsonData players)
    {
        for (var i = 0; i < 4; i++)
        {
            Gom.MyStatus[i].value = (float) players[0]["fight_fish"][i]["hp"] / GameState.MyFishFullHp[i];
            Gom.EnemyStatus[i].value = (float) players[1]["fight_fish"][i]["hp"] / GameState.EnemyFishFullHp[i];
        }
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