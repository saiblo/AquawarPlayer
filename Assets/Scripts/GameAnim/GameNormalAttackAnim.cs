using System;
using GameHelper;

namespace GameAnim
{
    public static class GameNormalAttackAnim
    {
        public static void NormalAttackAnim(this GameUI gameUI, bool enemy)
        {
            var selected = enemy ? gameUI.GameState.EnemyFishSelected : gameUI.GameState.MyFishSelected;
            var target = 0;
            for (var i = 0; i < 4; i++)
            {
                // ReSharper disable once InvertIf
                if (enemy && gameUI.GameState.MyFishSelectedAsTarget[i] ||
                    gameUI.GameState.EnemyFishSelectedAsTarget[i])
                {
                    target = i;
                    break;
                }
            }
            var distance =
                GameObjectManager.FishRelativePosition(enemy, selected) -
                GameObjectManager.FishRelativePosition(!enemy, target);
            gameUI.Repeat(cnt =>
            {
                (enemy ? gameUI.Gom.EnemyFishTransforms : gameUI.Gom.MyFishTransforms)[selected].localPosition =
                    GameObjectManager.FishRelativePosition(!enemy, target) + Math.Abs(cnt - 40f) / 40f * distance;
            }, () => { }, 81, 0, 10);
            gameUI.GameState.PassiveList.Add(target);
        }
    }
}