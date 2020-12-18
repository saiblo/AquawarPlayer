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