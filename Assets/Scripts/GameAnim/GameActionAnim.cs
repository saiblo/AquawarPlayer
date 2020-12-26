namespace GameAnim
{
    public static class GameActionAnim
    {
        public static void ActionAnim(this GameUI gameUI)
        {
            gameUI.GameState.PassiveList.Clear();
            if (gameUI.GameState.NormalAttack)
                gameUI.NormalAttackAnim(gameUI.GameState.EnemyFishSelected >= 0 && gameUI.GameState.EnemyFishSelected < 4);
            else
                gameUI.SkillAttackAnim(gameUI.GameState.EnemyFishSelected >= 0 && gameUI.GameState.EnemyFishSelected < 4);
            gameUI.PassiveAnim();
        }
    }
}