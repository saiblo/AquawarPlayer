using LitJson;

namespace GameAnim
{
    public static class GameHpAnim
    {
        public static void HpAnim(this GameUI gameUI, JsonData actionInfo)
        {
            if (actionInfo.ContainsKey("hit"))
            {
                var hitList = actionInfo["hit"];
                for (var i = 0; i < hitList.Count; i++)
                    ((int) hitList[i]["player"] == 1 ? gameUI.enemyStatus : gameUI.myStatus)
                        [(int) hitList[i]["target"]].Current -= (int) hitList[i]["value"];
            }

            if (actionInfo.ContainsKey("passive"))
            {
                var passiveList = actionInfo["passive"];
                for (var i = 0; i < passiveList.Count; i++)
                    if ((string) passiveList[i]["type"] == "heal")
                        ((int) passiveList[i]["player"] == 1 ? gameUI.enemyStatus : gameUI.myStatus)
                            [(int) passiveList[i]["source"]].Current += (int) (double) passiveList[i]["value"];
            }

            for (var i = 0; i < 4; i++)
            {
                if (gameUI.myStatus[i].Current < 0) gameUI.myStatus[i].Current = 0;
                if (gameUI.enemyStatus[i].Current < 0) gameUI.enemyStatus[i].Current = 0;
            }
        }

        public static void RemoveBuff(this GameUI gameUI, JsonData actionInfo)
        {
            // NEW FEATURE: Buff
            bool[] meSkipped = {false, false, false, false};
            bool[] enemySkipped = {false, false, false, false};

            bool[] meHit = {false, false, false, false};
            bool[] enemyHit = {false, false, false, false};

            if (actionInfo.ContainsKey("hit"))
            {
                var hitList = actionInfo["hit"];
                for (var i = 0; i < hitList.Count; i++)
                {
                    if ((int) hitList[i]["value"] == 0)
                        ((int) hitList[i]["player"] == 1 ? enemySkipped : meSkipped)[(int) hitList[i]["target"]] = true;
                    else
                        ((int) hitList[i]["player"] == 1 ? enemyHit : meHit)[(int) hitList[i]["target"]] = true;
                }
            }

            for (var i = 0; i < 4; i++)
            {
                if (meHit[i])
                {
                    gameUI.GameState.MyBuff[i].Clear();
                    gameUI.GameState.MyComboStop[i] = true;
                }
                if (meSkipped[i] && !gameUI.GameState.MyComboStop[i])
                {
                    ++gameUI.GameState.MyComboSkip[i];
                }
                if (enemyHit[i])
                {
                    gameUI.GameState.EnemyBuff[i].Clear();
                    gameUI.GameState.EnemyComboStop[i] = true;
                }
                if (enemySkipped[i] && !gameUI.GameState.EnemyComboStop[i])
                {
                    ++gameUI.GameState.EnemyComboSkip[i];
                }
            }
        }
    }
}