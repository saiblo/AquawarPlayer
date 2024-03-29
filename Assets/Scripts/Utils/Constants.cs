﻿namespace Utils
{
    public static class Constants
    {
        public const int FishNum = 12;

        public const int DefaultHp = 400;
        public const int DefaultAtk = 100;

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

        public enum FishState
        {
            Used,
            Using,
            Free
        }

        public enum Skill
        {
            Aoe,
            InFight,
            Crit,
            InHelp,
            Turtle,
            MinCrit,
            Clown,
            Imitate,
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
            "护友膨胀",
            "护友膨胀",
            "队友承伤",
            "队友承伤",
            "闪避",
            "闪避",
            "三层金衣",
            "受伤回血",
            "受伤回血",
            "死亡爆炸",
            "残血膨胀",
            "拟态"
        };

        public static readonly string[] SkillDescription =
        {
            "对敌方所有目标发动攻击，每个造成35的伤害",
            "对一名队友发动攻击造成50伤害，并使自身攻击力永久提升70",
            "对敌方所有目标发动攻击，每个造成35的伤害",
            "对一名队友发动攻击造成50伤害，并使自身攻击力永久提升70",
            "对一名敌人造成120的暴击伤害",
            "选择一名队友（可以是自己），令其下次被攻击时减免70%伤害，并使自身攻击力永久提升20",
            "选择一名队友，令其下次被攻击后恢复20血量，如果主动技能发动次数不超过三次，还应该选择对一名敌人造成120的暴击伤害",
            "选择一名队友（可以是自己），令其下次被攻击时减免70%伤害，并使自身攻击力永久提升20",
            "选择一名场上血量最少的敌人造成120%攻击力的暴击伤害，当目标血量低于其生命值40%（即160）时改为造成140%攻击力的暴击伤害",
            "选择一名场上血量最少的敌人造成120%攻击力的暴击伤害，当目标血量低于其生命值40%（即160）时改为造成140%攻击力的暴击伤害",
            "选择一名队友，令其下次被攻击后，如果有除其以外的己方角色存活，其只承受70%伤害，其余己方角色平摊30%伤害，如果主动技能发动次数不超过三次，还应该对所有敌人分别造成35的伤害",
            "无"
        };

        public static readonly string[] PassiveDescription =
        {
            "队友被直接攻击后若其生命值少于30%（即120），对来源造成30的伤害",
            "队友被直接攻击后若其生命值少于30%（即120），对来源造成30的伤害",
            "自身受到敌方直接攻击时，如果还有队友存活，会让队友帮忙承受伤害（自身承受70%，其余队友平摊30%）；每次受到伤害时，若伤害值超过200，自身攻击值永久提升20",
            "自身受到敌方直接攻击时，如果还有队友存活，会让队友帮忙承受伤害（自身承受70%，其余队友平摊30%）；每次受到伤害时，若伤害值超过200，自身攻击值永久提升20",
            "每次受到伤害时有30%概率躲避伤害（不受到伤害，触发时机包括敌方直接攻击，队友或敌方发动技能造成的伤害等）",
            "每次受到伤害时有30%概率躲避伤害（不受到伤害，触发时机包括敌方直接攻击，队友或敌方发动技能造成的伤害等）",
            "拥有三层无敌护盾抵御三次伤害；三层护盾失去后，每次受到伤害时有30%概率躲避伤害",
            "受到伤害时自动恢复20血量",
            "受到伤害时自动恢复20血量",
            "因为受到敌方直接攻击而死亡时会爆炸，对来源造成40的伤害；血量低于20%（即80）时，自身攻击力永久提高15",
            "自身被直接攻击后若生命值少于30%（即120），对来源造成30的伤害",
            "战斗开始前，选择其余11条鱼中的任意一条，获得其的被动与主动技能"
        };

        public static readonly Skill[] SkillDict =
        {
            Skill.Aoe,
            Skill.InFight,
            Skill.Aoe,
            Skill.InFight,
            Skill.Crit,
            Skill.InHelp,
            Skill.Turtle,
            Skill.InHelp,
            Skill.MinCrit,
            Skill.MinCrit,
            Skill.Clown,
            Skill.Imitate
        };
    }
}