using GameImpl;
using Utils;

namespace GameAnim
{
    public static class GameActionAnim
    {
        public static bool ActionAnim(this GameUI gameUI)
        {
            var actionInfo =
                SharedRefs.Mode == Constants.GameMode.Offline
                    ? SharedRefs.ActionInfo
                    : SharedRefs.ActionInfo[gameUI.GameState.MyTurn ? "MyAction" : "EnemyAction"];
            gameUI.GameState.NormalAttack = (string) actionInfo["skill"]["type"] == "normalattack";

            var actions = gameUI.GenEventProcessor(actionInfo);

            var fishId = (gameUI.GameState.MyTurn ? gameUI.GameState.MyFishId : gameUI.GameState.EnemyFishId)
                [(int) actionInfo["ActionFish"]];
            var fishName = SharedRefs.Mode == Constants.GameMode.Offline ||
                           gameUI.GameState.MyTurn ||
                           gameUI.GameState.EnemyFishExpose[(int) actionInfo["ActionFish"]]
                ? Constants.FishName[fishId]
                : "[未知]";
            var logPrefix = (gameUI.GameState.MyTurn ? GameUI.MeStr : GameUI.EnemyStr) +
                            $"{actionInfo["ActionFish"]}号位置的{fishName}对";

            actions[0]();
            if (gameUI.GameState.NormalAttack)
                gameUI.NormalAttackAnim(actionInfo, logPrefix);
            else
                gameUI.SkillAttackAnim(actionInfo, logPrefix);
            if (actionInfo.ContainsKey("passive"))
                gameUI.PassiveAnim(actionInfo);
            gameUI.HpAnim(actionInfo);
            actions[1]();
            return actionInfo.ContainsKey("passive");
        }
    }
}