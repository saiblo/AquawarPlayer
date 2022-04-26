using System;
using System.Runtime.InteropServices;
using System.Text;
using GameAnim;
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
    private static extern string GetToken();

    [DllImport("__Internal")]
    private static extern void ConnectSaiblo(string tokenDecoded, string tokenEncoded);

    [DllImport("__Internal")]
    public static extern void SendWsMessage(string message);

    [DllImport("__Internal")]
    private static extern string GetPlayers();

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
        for (var i = 0; i < Constants.FishNum; i++)
        {
            SharedRefs.FishPrefabs[i] = fishPrefabSamples[i];
            SharedRefs.FishAvatars[i] = fishAvatars[i];
        }
        if (SharedRefs.PlayerNames != null)
        {
            playerNamesText.text = SharedRefs.PlayerNames;
        }
        if (SharedRefs.ReplayJson != null || SharedRefs.OnlineToken != null)
        {
            this.AwakeImpl();
        }
    }

    protected override void RunPerFrame()
    {
        if (SharedRefs.PlayerNames == null)
        {
            var players = GetPlayers();
            if (players.Length > 0)
            {
                SharedRefs.PlayerNames = players;
                playerNamesText.text = players;
            }
        }
        playerNamesBox.gameObject.SetActive(SharedRefs.OnlineToken == null);
        if (SharedRefs.ReplayJson == null && SharedRefs.OnlineToken == null)
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
            else if (SharedRefs.OnlineToken == null)
            {
                var tokenEncoded = GetToken();
                if (tokenEncoded.Length > 0 && SharedRefs.OnlineToken == null)
                {
                    SharedRefs.OnlineToken = tokenEncoded;
                    var tokenDecoded = tokenEncoded;
                    try
                    {
                        tokenDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenEncoded));
                    }
                    catch (Exception e)
                    {
                        JsAlert($"连接失败：{e.Message}");
                    }
                    if (tokenDecoded == tokenEncoded)
                    {
                        JsAlert("连接失败：token 解析失败");
                    }
                    SharedRefs.OnlineWaiting = 1;
                    ConnectSaiblo(tokenDecoded, tokenEncoded);
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

    public void OnWsMessage(string message)
    {
        var messageObject = JsonMapper.ToObject(message);
        switch (SharedRefs.OnlineWaiting)
        {
            case 1:
                SharedRefs.PickInfo = messageObject; // PICK
                SharedRefs.Mode = Constants.GameMode.Online;
                SharedRefs.OnlineWin = SharedRefs.OnlineLose = 0;
                SceneManager.LoadScene("Scenes/Preparation");
                break;
            case 2:
                SharedRefs.ActionInfo = messageObject; // ASSERT
                if ((string)SharedRefs.ActionInfo["Action"] == "EarlyFinish")
                {
                    resultText.text = (string)SharedRefs.ActionInfo["Result"] == "Win" ? $"{MeStr}获胜" : $"{EnemyStr}获胜";
                    this.GameOver((string)SharedRefs.ActionInfo["Result"] == "Win", true);
                    return;
                }
                GameState.MyTurn = (int)SharedRefs.PickInfo["FirstMover"] == 1;
                for (var i = 0; i < Constants.FishNum; i++)
                    assertionModal.SetupFish(i, Constants.FishState.Using, assertionExt, this);
                this.NewRound();
                break;
            case 3:
                SharedRefs.ActionInfo = messageObject; // ASSERT
                if ((string)SharedRefs.ActionInfo["Action"] == "EarlyFinish")
                {
                    resultText.text = (string)SharedRefs.ActionInfo["Result"] == "Win" ? $"{MeStr}获胜" : $"{EnemyStr}获胜";
                    this.GameOver((string)SharedRefs.ActionInfo["Result"] == "Win", true);
                }
                else
                {
                    // And now the animation part
                    var hasPassive = this.ActionAnim();

                    if (SharedRefs.Mode == Constants.GameMode.Offline || !GameState.MyTurn ||
                        SharedRefs.ActionInfo.ContainsKey("EnemyAssert"))
                    {
                        // Now go for a new round
                        GameState.MyTurn = !GameState.MyTurn;
                        SetTimeout(this.NewRound, hasPassive ? 2000 : 1000);
                    }
                    else
                    {
                        // Game over
                        resultText.text = (string) SharedRefs.ActionInfo["Result"] == "Win" ? $"{MeStr}获胜" : $"{EnemyStr}获胜";
                        this.GameOver((string) SharedRefs.ActionInfo["Result"] == "Win");
                    }
                }
                break;
            case 4:
            {
                var end = false; // ACTION
                switch ((string)messageObject["Action"])
                {
                    case "Finish":
                        // You assert your way to death
                        end = true;
                        resultText.text = (string)messageObject["Result"] == "Win" ? $"{MeStr}获胜" : $"{EnemyStr}获胜";
                        this.GameOver((string)messageObject["Result"] == "Win");
                        break;
                    case "EarlyFinish":
                        resultText.text = (string)messageObject["Result"] == "Win" ? $"{MeStr}获胜" : $"{EnemyStr}获胜";
                        this.GameOver((string)messageObject["Result"] == "Win", true);
                        return;
                    default:
                    {
                        var info = messageObject["GameInfo"];
                        for (var i = 0; i < 4; i++)
                        {
                            var id = i;
                            myStatus[i].Current = (int)info["MyHP"][i];
                            enemyStatus[i].Current = (int)info["EnemyHP"][i];
                            myProfiles[i].SetAtk((int)info["MyATK"][i]);
                            if (GameState.MyFishAlive[i] && myStatus[i].Current <= 0)
                                SetTimeout(() => { this.Dissolve(false, id); }, 500);
                            if (GameState.EnemyFishAlive[i] &&
                                enemyStatus[i].Current <= 0)
                                SetTimeout(() => { this.Dissolve(true, id); }, 500);
                        }
                        break;
                    }
                }
                GameState.AssertionPlayer = 0;
                GameState.OnlineAssertionHit = !end && (bool)(messageObject["AssertReply"]["AssertResult"] ?? false);
                
                // When either side made an assertion, play the animation.
                if (GameState.Assertion != -1) this.AssertionAnim();

                if (SharedRefs.Mode == Constants.GameMode.Online && !GameState.MyTurn)
                {
                    if ((string)SharedRefs.ActionInfo["Action"] == "Finish")
                    {
                        end = true;
                        resultText.text =
                            (string)SharedRefs.ActionInfo["Result"] == "Win" ? $"{MeStr}获胜" : $"{EnemyStr}获胜";
                        this.GameOver((string)SharedRefs.ActionInfo["Result"] == "Win");
                    }
                    else
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            var id = i;
                            if (GameState.MyFishAlive[i] && myStatus[i].Current <= 0)
                                SetTimeout(() => { this.Dissolve(false, id); }, 500);
                            if (GameState.EnemyFishAlive[i] && enemyStatus[i].Current <= 0)
                                SetTimeout(() => { this.Dissolve(true, id); }, 500);
                        }
                    }
                }

                if (end) break;

                // Enter `WaitAssertion` branch
                SetTimeout(() =>
                {
                    GameState.Assertion = -1;
                    this.ChangeStatus();
                }, GameState.Assertion == -1 ? 200 : 1000);
                break;
            }
            case 5:
            default:
                SharedRefs.PickInfo = messageObject; // PICK
                gameOverMask.SetActive(true);
                break;
        }
        SharedRefs.OnlineWaiting = 0;
    }
}