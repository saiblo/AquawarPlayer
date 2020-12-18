using System.Collections.Generic;

public class Ok
{
    public static string Action = "OK";
}

public class Pick
{
    public static string Action = "Pick";
    public List<int> ChooseFishs { get; set; }
    public int ImitateFish { get; set; }
}

public class Assert
{
    public static string Action = "Assert";
    public int Pos { get; set; }
    public int Id { get; set; }
}

public class Null
{
    public static string Action = "Null";
}