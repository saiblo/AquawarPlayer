using LitJson;
using Utils;

namespace GameAnim
{
    public static class GameProcessActionInfo
    {
        public static void ProcessActionInfo(this GameUI gameUI, JsonData actionInfo)
        {
            if (gameUI.GameState.MyTurn)
                gameUI.GameState.MyFishSelected = (int) actionInfo["ActionFish"];
            else
                gameUI.GameState.EnemyFishSelected = (int) actionInfo["ActionFish"];

            if (actionInfo.ContainsKey("hit"))
            {
                var hitList = actionInfo["hit"];
                for (var i = 0; i < hitList.Count; i++)
                {
                    var target = (int) hitList[i]["target"];
                    ((bool) hitList[i]["isEnemy"] ? gameUI.enemyStatus[target] : gameUI.myStatus[target])
                        .Current -= (int) hitList[i]["value"];
                }
            }

            gameUI.GameState.NormalAttack = !actionInfo.ContainsKey("skill");

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

            for (var i = 0; i < 4; i++)
            {
                gameUI.myProfiles[i].SetHp(gameUI.myStatus[i].Current);
                gameUI.enemyProfiles[i].SetHp(gameUI.enemyStatus[i].Current);
            }
        }
    }
}