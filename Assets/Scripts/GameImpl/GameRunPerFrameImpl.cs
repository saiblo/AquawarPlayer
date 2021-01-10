using Utils;

namespace GameImpl
{
    public static class GameRunPerFrameImpl
    {
        public static void RunPerFrameImpl(this GameUI gameUI)
        {
            gameUI.logText.text = string.Join("\n", gameUI.GameState.Logs);

            if (!gameUI.Gom.Initialized || gameUI.GameState.GameStatus == Constants.GameStatus.WaitAssertion) return;

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