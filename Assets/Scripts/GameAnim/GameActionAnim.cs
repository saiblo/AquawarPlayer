using System.Collections.Generic;
using Utils;

namespace GameAnim
{
    public static class GameActionAnim
    {
        private static readonly Dictionary<string, string> SkillSuffixDict = new Dictionary<string, string>();

        static GameActionAnim()
        {
            SkillSuffixDict.Add("aoe", "AOE攻击。");
            SkillSuffixDict.Add("infight", "伤害队友攻击。");
            SkillSuffixDict.Add("crit", "暴击伤害。");
            SkillSuffixDict.Add("subtle", "无作为攻击。");
        }

        public static void ActionAnim(this GameUI gameUI)
        {
            var actionInfo = SharedRefs.ActionInfo[gameUI.GameState.MyTurn ? "MyAction" : "EnemyAction"];
            gameUI.GameState.NormalAttack = !actionInfo.ContainsKey("skill");

            // Logging related starts.
            var fishId = (gameUI.GameState.MyTurn ? gameUI.GameState.MyFishId : gameUI.GameState.EnemyFishId)
                [(int) actionInfo["ActionFish"]];
            var fishName = gameUI.GameState.EnemyFishExpose[(int) actionInfo["ActionFish"]]
                ? Constants.FishName[fishId]
                : "未知";
            var logStr = (gameUI.GameState.MyTurn ? "我方" : "敌方") +
                         $"{actionInfo["ActionFish"]}号位置的{fishName}发起了";
            if (gameUI.GameState.NormalAttack) logStr += "普通攻击。";
            else logStr += SkillSuffixDict[(string) actionInfo["skill"]["type"]];
            gameUI.GameState.Logs.Enqueue(logStr);
            // Logging related ends.

            if (gameUI.GameState.NormalAttack)
                gameUI.NormalAttackAnim(actionInfo);
            else
                gameUI.SkillAttackAnim(actionInfo);
            gameUI.PassiveAnim();
            gameUI.HpAnim(actionInfo);
        }
    }
}