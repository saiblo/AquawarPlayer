using System.Collections.Generic;

namespace Utils
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class Pick
    {
        public static string Action = "Pick";
        public List<int> ChooseFishs { get; set; }
    }

    public class PickWithImitate
    {
        public static string Action = "Pick";
        public List<int> ChooseFishs { get; set; }
        public int ImitateFish { get; set; }
    }

    public class Assert
    {
        public static string Action = "Assert";
        public int Pos { get; set; }
        public int ID { get; set; }
    }

    public class Null
    {
        public static string Action = "Null";
    }

    public class NormalAction
    {
        public static string Action = "Action";
        public static int Type = 0;
        public int MyPos { get; set; }
        public int EnemyPos { get; set; }
    }

    public class SkillAction
    {
        public static string Action = "Action";
        public static int Type = 1;
        public int MyPos { get; set; }
        public List<int> MyList { get; set; }
        public List<int> EnemyList { get; set; }
    }
}