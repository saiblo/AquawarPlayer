namespace Utils
{
    public static class Constants
    {
        public const int FishNum = 12;

        public enum GameMode
        {
            Online,
            Offline
        }

        public enum GameStatus
        {
            DoAssertion,
            WaitAssertion,
            SelectMyFish,
            SelectEnemyFish,
            WaitingAnimation
        }

        public enum Skill
        {
            Aoe,
            Infight,
            CritValue,
            CritPercent,
            Subtle
        }
    }
}