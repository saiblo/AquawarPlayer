using System.Linq;
using Utils;

namespace GameImpl
{
    public static class GameRunPerFrameImpl
    {
        public static void RunPerFrameImpl(this GameUI gameUI)
        {
            gameUI.roundText.text = $"操作数：{SharedRefs.ReplayCursor}";

            gameUI.logObject.SetActive(gameUI.logActive);

            gameUI.playButtonImage.overrideSprite = SharedRefs.AutoPlay ? gameUI.pauseIcon : gameUI.playIcon;

            if (!gameUI.Gom.Initialized || gameUI.GameState.GameStatus == Constants.GameStatus.WaitAssertion) return;

            for (var i = 0; i < 4; i++)
                gameUI.actionButtons[i].gameObject
                    .SetActive(gameUI.GameState.MyTurn &&
                               (gameUI.GameState.GameStatus == Constants.GameStatus.SelectMyFish ||
                                gameUI.GameState.GameStatus == Constants.GameStatus.SelectEnemyFish) &&
                               i == gameUI.GameState.MyFishSelected);

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
        }
    }
}