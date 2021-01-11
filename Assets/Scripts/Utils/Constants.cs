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

        public static readonly string[] SkillTable =
        {
            "AOE",
            "伤害队友",
            "AOE",
            "伤害队友",
            "易伤暴击",
            "伤害减免",
            "有限暴击",
            "伤害减免",
            "弱者暴击",
            "弱者暴击",
            "有限AOE",
            "拟态"
        };
    }
}