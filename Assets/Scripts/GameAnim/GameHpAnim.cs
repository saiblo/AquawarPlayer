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
                    ((bool) hitList[i]["isEnemy"] ? gameUI.enemyStatus : gameUI.myStatus)
                        [(int) hitList[i]["target"]].Current -= (int) hitList[i]["value"];
            }

            for (var i = 0; i < 4; i++)
            {
                gameUI.myProfiles[i].SetHp(gameUI.myStatus[i].Current);
                gameUI.enemyProfiles[i].SetHp(gameUI.enemyStatus[i].Current);
            }
        }
    }
}