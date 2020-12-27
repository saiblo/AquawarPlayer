using LitJson;
using Utils;

namespace GameAnim
{
    public static class GameProcessActionInfo
    {
        public static void ProcessActionInfo(this GameUI gameUI, JsonData actionInfo)
        {
            /* if (gameUI.GameState.MyTurn)
                gameUI.GameState.MyFishSelected = (int) actionInfo["ActionFish"];
            else
                gameUI.GameState.EnemyFishSelected = (int) actionInfo["ActionFish"]; */
            if (!gameUI.GameState.MyTurn)
                gameUI.GameState.EnemyFishSelected = 0;
            
            gameUI.GameState.NormalAttack = !actionInfo.ContainsKey("skill");
            gameUI.GameState.MyFishSelectedAsTarget[0] = true;

            var hitList = actionInfo["hit"];
            for (var i = 0; i < hitList.Count; i++)
            {
                // if ((int) hitList[i]["player"] == 0)
                if (!gameUI.GameState.MyTurn)
                {
                    for (var j = 0; j < 4; j++)
                        if (gameUI.GameState.MyFishId[j] == (int) hitList[i]["target"])
                        {
                            if ((bool) hitList[i]["traceable"])
                                gameUI.GameState.MyFishSelectedAsTarget[j] = true;
                            gameUI.GameState.MyFishOnlineHp[j] -= (int) hitList[i]["value"];
                        }
                }
                else
                {
                    for (var j = 0; j < 4; j++)
                        if (gameUI.GameState.EnemyFishId[j] == (int) hitList[i]["target"])
                        {
                            if ((bool) hitList[i]["traceable"])
                                gameUI.GameState.EnemyFishSelectedAsTarget[j] = true;
                            gameUI.GameState.EnemyFishOnlineHp[j] -= (int) hitList[i]["value"];
                        }
                }
            }

            if (!gameUI.GameState.NormalAttack)
            {
                switch ((string) actionInfo["skill"]["type"])
                {
                    case "aoe":
                        gameUI.GameState.ActionSkill = Constants.Skill.Aoe;
                        break;
                    case "infight":
                        gameUI.GameState.ActionSkill = Constants.Skill.Infight;
                        break;
                    case "crit_value":
                        gameUI.GameState.ActionSkill = Constants.Skill.CritValue;
                        break;
                    case "crit_percent":
                        gameUI.GameState.ActionSkill = Constants.Skill.CritPercent;
                        break;
                    case "subtle":
                        gameUI.GameState.ActionSkill = Constants.Skill.Subtle;
                        break;
                }
            }
            
            gameUI.RunOnUiThread(gameUI.DisplayHpOnline);
        }
    }
}