public class GameStates
{
    public readonly int[] MyFishId = {0, 0, 0, 0};
    public readonly int[] EnemyFishId = {0, 0, 0, 0};

    public Constants.GameStatus GameStatus = Constants.GameStatus.DoAssertion;

    public bool MyTurn = true;
    public bool NormalAttack = true;

    public readonly int[] MyFishFullHp = {0, 0, 0, 0};
    public readonly int[] EnemyFishFullHp = {0, 0, 0, 0};
    
    public int MyFishSelected = -1;

    public int EnemyFishSelected = -1;
}