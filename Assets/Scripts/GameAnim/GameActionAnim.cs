namespace GameAnim
{
    public static class GameActionAnim
    {
        public static void ActionAnim(this GameUI gameUI)
        {
            gameUI.GameState.PassiveList.Clear();
            if (gameUI.GameState.NormalAttack)
                gameUI.NormalAttackAnim();
            else
                gameUI.SkillAttackAnim();
            gameUI.PassiveAnim();
        }
    }
}