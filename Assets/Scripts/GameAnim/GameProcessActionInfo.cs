using LitJson;
using Utils;

namespace GameAnim
{
    public static class GameProcessActionInfo
    {
        public static void ProcessActionInfo(this GameUI gameUI, JsonData actionInfo)
        {
            var fishId = (gameUI.GameState.MyTurn ? gameUI.GameState.MyFishId : gameUI.GameState.EnemyFishId)
                [(int) actionInfo["ActionFish"]];
            var fishName = gameUI.GameState.EnemyFishExpose[(int) actionInfo["ActionFish"]]
                ? Constants.FishName[fishId]
                : "未知";
            var logStr = (gameUI.GameState.MyTurn ? "我方" : "敌方") +
                         $"{actionInfo["ActionFish"]}号位置的{fishName}发起了";
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

            if (gameUI.GameState.NormalAttack)
            {
                logStr += "普通攻击。";
            }
            else
            {
                switch ((string) actionInfo["skill"]["type"])
                {
                    case "aoe":
                        gameUI.GameState.ActionSkill = Constants.Skill.Aoe;
                        logStr += "AOE攻击。";
                        break;
                    case "infight":
                        gameUI.GameState.ActionSkill = Constants.Skill.Infight;
                        logStr += "伤害队友攻击。";
                        break;
                    case "crit_value":
                        gameUI.GameState.ActionSkill = Constants.Skill.CritValue;
                        logStr += "暴击伤害。";
                        break;
                    case "crit_percent":
                        gameUI.GameState.ActionSkill = Constants.Skill.CritPercent;
                        logStr += "暴击伤害。";
                        break;
                    case "subtle":
                        gameUI.GameState.ActionSkill = Constants.Skill.Subtle;
                        logStr += "无作为攻击。";
                        break;
                }
            }

            gameUI.GameState.Logs.Enqueue(logStr);

            for (var i = 0; i < 4; i++)
            {
                gameUI.myProfiles[i].SetHp(gameUI.myStatus[i].Current);
                gameUI.enemyProfiles[i].SetHp(gameUI.enemyStatus[i].Current);
            }
        }
    }
}