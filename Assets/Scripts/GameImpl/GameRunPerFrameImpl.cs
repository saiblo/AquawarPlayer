using System;
using System.Linq;
using UnityEngine;
using Utils;

namespace GameImpl
{
    public static class GameRunPerFrameImpl
    {
        public static void RunPerFrameImpl(this GameUI gameUI)
        {
            if (gameUI.Gom.Initialized && gameUI.GameState.GameStatus != Constants.GameStatus.WaitAssertion)
            {
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
            }

            gameUI.changeStatusButton.interactable =
                SharedRefs.Mode == Constants.GameMode.Online &&
                (gameUI.GameState.GameStatus == Constants.GameStatus.DoAssertion ||
                 gameUI.GameState.GameStatus == Constants.GameStatus.SelectMyFish &&
                 gameUI.GameState.MyFishSelected != -1 ||
                 gameUI.GameState.GameStatus == Constants.GameStatus.SelectEnemyFish &&
                 (gameUI.GameState.MyFishSelectedAsTarget.Any(b => b) ||
                  gameUI.GameState.EnemyFishSelectedAsTarget.Any(b => b)));

            string title;
            switch (gameUI.GameState.GameStatus)
            {
                case Constants.GameStatus.DoAssertion:
                    title = gameUI.GameState.Assertion == -1 ? "放弃断言" : "进行断言";
                    break;
                case Constants.GameStatus.WaitAssertion:
                    title = "请等待动画放完";
                    break;
                case Constants.GameStatus.SelectMyFish:
                    title = "选择我方鱼";
                    break;
                case Constants.GameStatus.SelectEnemyFish:
                    title = "选择作用对象";
                    break;
                case Constants.GameStatus.WaitingAnimation:
                    title = "请等待动画放完";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            gameUI.changeStatusPrompt.text = title;

            (gameUI.GameState.NormalAttack ? gameUI.normalButton : gameUI.skillButton).color = Color.green;
            (gameUI.GameState.NormalAttack ? gameUI.skillButton : gameUI.normalButton).color = Color.blue;
        }
    }
}