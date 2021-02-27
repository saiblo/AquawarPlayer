using GameImpl;
using Utils;

namespace GameAnim
{
    public static class GameActionAnim
    {
        public static bool ActionAnim(this GameUI gameUI)
        {
            var actionInfo = SharedRefs.ActionInfo;
            gameUI.GameState.NormalAttack = (string) actionInfo["skill"]["type"] == "normalattack";

            var actions = gameUI.GenEventProcessor(actionInfo);

            var fishId = (gameUI.GameState.MyTurn ? gameUI.GameState.MyFishId : gameUI.GameState.EnemyFishId)
                [(int) actionInfo["ActionFish"]];
            var fishName = Constants.FishName[fishId];
            var logPrefix = $"{(gameUI.GameState.MyTurn ? 0 : 1)}号AI的{fishName}对";

            actions[0]();
            gameUI.RemoveBuff(actionInfo);
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