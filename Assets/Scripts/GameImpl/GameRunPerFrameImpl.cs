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

                var myFishId = gameUI.GameState.MyFishId[i];
                var myFishRealId = myFishId == 11 ? SharedRefs.MyImitate : myFishId;
                gameUI.myExtensions[i].UpdateText(
                    $"主动：{Constants.SkillDescription[myFishRealId]}\n\n被动：{Constants.PassiveDescription[myFishRealId]}\n\n用过的主动：{string.Join(",", gameUI.GameState.MyUsedSkills[i])}\n\n用过的被动：{string.Join(",", gameUI.GameState.MyUsedPassives[i])}"
                );

                var enemyFishId = gameUI.GameState.EnemyFishId[i];
                var enemyFishRealId = enemyFishId == 11 ? SharedRefs.EnemyImitate : enemyFishId;
                gameUI.enemyExtensions[i].UpdateText(
                    $"主动：{Constants.SkillDescription[enemyFishRealId]}\n\n被动：{Constants.PassiveDescription[enemyFishRealId]}\n\n用过的主动：{string.Join(",", gameUI.GameState.EnemyUsedSkills[i])}\n\n用过的被动：{string.Join(",", gameUI.GameState.EnemyUsedPassives[i])}"
                );
            }
        }
    }
}