using System;
using System.Linq;
using Utils;

namespace GameImpl
{
    public static class GameRunPerFrameImpl
    {
        public static void RunPerFrameImpl(this GameUI gameUI)
        {
            if (SharedRefs.Mode == Constants.GameMode.Offline) gameUI.roundText.text = $"操作数：{SharedRefs.ReplayCursor}";

            gameUI.logObject.SetActive(gameUI.logActive);

            gameUI.playButtonImage.overrideSprite = SharedRefs.AutoPlay ? gameUI.pauseIcon : gameUI.playIcon;

            if (!gameUI.Gom.Initialized || gameUI.GameState.GameStatus == Constants.GameStatus.WaitAssertion) return;

            for (var i = 0; i < 4; i++)
            {
                gameUI.assertionButtons[i].SetActive(SharedRefs.Mode == Constants.GameMode.Online &&
                                                     gameUI.GameState.MyTurn &&
                                                     gameUI.GameState.GameStatus == Constants.GameStatus.DoAssertion &&
                                                     i == gameUI.GameState.Assertion);
                gameUI.actionButtons[i].gameObject
                    .SetActive(SharedRefs.Mode == Constants.GameMode.Online &&
                               gameUI.GameState.MyTurn &&
                               (gameUI.GameState.GameStatus == Constants.GameStatus.SelectMyFish ||
                                gameUI.GameState.GameStatus == Constants.GameStatus.SelectEnemyFish) &&
                               i == gameUI.GameState.MyFishSelected);
            }

            gameUI.doNotAssertButton.SetActive(SharedRefs.Mode == Constants.GameMode.Online &&
                                               gameUI.GameState.MyTurn &&
                                               gameUI.GameState.GameStatus == Constants.GameStatus.DoAssertion);


            gameUI.confirmActionGroup.SetActive(
                SharedRefs.Mode == Constants.GameMode.Online &&
                gameUI.GameState.GameStatus == Constants.GameStatus.SelectEnemyFish &&
                gameUI.GameState.MyTurn
            );

            gameUI.confirmAttackButton.interactable =
                SharedRefs.Mode == Constants.GameMode.Online &&
                gameUI.GameState.GameStatus == Constants.GameStatus.SelectEnemyFish &&
                (gameUI.GameState.NormalAttack &&
                 gameUI.GameState.MyFishSelectedAsTarget.Count(b => b) == 0 &&
                 gameUI.GameState.EnemyFishSelectedAsTarget.Count(b => b) == 1 ||
                 !gameUI.GameState.NormalAttack &&
                 gameUI.GameState.MyFishSelectedAsTarget.Count(b => b) +
                 gameUI.GameState.EnemyFishSelectedAsTarget.Count(b => b) > 0);

            for (var i = 0; i < 4; i++)
            {
                if (gameUI.GameState.MyFishAlive[i])
                    gameUI.Gom.MyFishTransforms[i].localScale =
                        gameUI.GameState.MyFishSelectedAsTarget[i] || gameUI.GameState.MyFishSelected == i
                            ? gameUI.Gom.Large
                            : gameUI.Gom.Small;

                if (gameUI.GameState.EnemyFishAlive[i])
                    gameUI.Gom.EnemyFishTransforms[i].localScale =
                        gameUI.GameState.EnemyFishSelectedAsTarget[i] || gameUI.GameState.EnemyFishSelected == i
                            ? gameUI.Gom.Large
                            : gameUI.Gom.Small;
            }

            var text = "请等待动画放完。";

            if (SharedRefs.Mode == Constants.GameMode.Online && gameUI.GameState.MyTurn)
            {
                switch (gameUI.GameState.GameStatus)
                {
                    case Constants.GameStatus.DoAssertion:
                        text = "请选择你要断言的敌方鱼，或点击屏幕右下方的放弃断言。";
                        break;
                    case Constants.GameStatus.WaitAssertion:
                        break;
                    case Constants.GameStatus.SelectMyFish:
                        text = "请选择我方鱼，并选择使用的技能。";
                        break;
                    case Constants.GameStatus.SelectEnemyFish:
                        text = "请选择行动的作用对象，或点击屏幕右下方的重新选鱼。";
                        break;
                    case Constants.GameStatus.WaitingAnimation:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            gameUI.hintText.text = text;
        }
    }
}