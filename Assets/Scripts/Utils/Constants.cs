namespace Utils
{
    public static class Constants
    {
        public const int FishNum = 12;

        public const int DefaultHp = 400;
        public const int DefaultAtk = 100;

        public enum GameStatus
        {
            DoAssertion,
            PeekAssertion,
            WaitAssertion,
            SelectMyFish,
            SelectEnemyFish,
            WaitingAnimation
        }

        public enum FishState
        {
            Used,
            Using,
            Free
        }

        public enum Buff
        {
            Reduce,
            Heal,
            Deflect
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

        public static readonly string[] PassiveTable =
        {
            "护友反伤",
            "护友反伤",
            "队友承伤",
            "队友承伤",
            "闪避",
            "闪避",
            "三层金衣",
            "受伤回血",
            "受伤回血",
            "死亡爆炸",
            "残血反伤",
            "拟态"
        };
    }
}