using System.Collections.Generic;
using Utils;

namespace GameHelper
{
    /// <summary>
    ///   <para>Records the transient states of the game.</para>
    /// </summary>
    public class GameStates
    {
        public readonly int[] MyFishId = {0, 0, 0, 0};
        public readonly int[] EnemyFishId = {0, 0, 0, 0};

        public Constants.GameStatus GameStatus = Constants.GameStatus.DoAssertion;

        public bool MyTurn = true;
        public bool NormalAttack = true;

        public readonly int[] MyFishOnlineHp = {0, 0, 0, 0};
        public readonly int[] EnemyFishOnlineHp = {0, 0, 0, 0};

        public Constants.Skill ActionSkill = Constants.Skill.Subtle;

        public int MyFishSelected = -1;

        public int EnemyFishSelected = -1;

        public int Assertion = -1; // The asserted position
        public int AssertionPlayer = 0; // The player that performs the assertion
        public int AssertionTarget; // Which fish do you think it is?

        public bool OnlineAssertionHit; // Made a online assertion, and hit

        public readonly List<int> PassiveList = new List<int>();

        public readonly bool[] MyFishSelectedAsTarget = {false, false, false, false};
        public readonly bool[] EnemyFishSelectedAsTarget = {false, false, false, false};

        public readonly bool[] MyFishAlive = {true, true, true, true};
        public readonly bool[] EnemyFishAlive = {true, true, true, true};

        public readonly bool[] MyFishExpose = {false, false, false, false};
        public readonly bool[] EnemyFishExpose = {false, false, false, false};
        
        public readonly List<int> MyFishPicked = new List<int>();
        public readonly List<int> EnemyFishPicked = new List<int>();
        public readonly List<int> MyFishAvailable = new List<int>();
        public readonly List<int> EnemyFishAvailable = new List<int>();

        public readonly Queue<string> Logs = new Queue<string>();
    }
}