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
            {
                gameUI.assertionButtons[i].SetActive(false);
                gameUI.actionButtons[i].gameObject.SetActive(false);
            }
            gameUI.doNotAssertButton.SetActive(false);
            gameUI.confirmActionGroup.SetActive(false);
            gameUI.confirmAttackButton.interactable = false;

            for (var i = 0; i < 4; i++)
            {
                if (gameUI.GameState.MyFishAlive[i])
                    gameUI.Gom.MyFishTransforms[i].localScale =
                        gameUI.GameState.GameStatus == Constants.GameStatus.SelectMyFish &&
                        gameUI.GameState.MyFishSelected == i ||
                        gameUI.GameState.GameStatus == Constants.GameStatus.SelectEnemyFish &&
                        gameUI.GameState.MyFishSelectedAsTarget[i]
                            ? gameUI.Gom.Large
                            : gameUI.Gom.Small;

                if (gameUI.GameState.EnemyFishAlive[i])
                    gameUI.Gom.EnemyFishTransforms[i].localScale =
                        gameUI.GameState.GameStatus == Constants.GameStatus.SelectMyFish &&
                        gameUI.GameState.EnemyFishSelected == i ||
                        gameUI.GameState.GameStatus == Constants.GameStatus.SelectEnemyFish &&
                        gameUI.GameState.EnemyFishSelectedAsTarget[i]
                            ? gameUI.Gom.Large
                            : gameUI.Gom.Small;
            }
        }
    }
}