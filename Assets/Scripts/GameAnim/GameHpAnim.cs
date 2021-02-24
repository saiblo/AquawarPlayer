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
    }
}