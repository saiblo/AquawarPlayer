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

        public static readonly string[] FishName =
        {
            "射水鱼",
            "喷火鱼",
            "电鳗",
            "翻车鱼",
            "海狼鱼",
            "蝠鲼",
            "海龟",
            "章鱼",
            "大白鲨",
            "锤头鲨",
            "小丑鱼",
            "拟态鱼"
        };
    }
}