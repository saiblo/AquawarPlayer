using GameHelper;
using UnityEngine;
using Utils;

namespace GameImpl
{
    public static class GameMakeAGuessImpl
    {
        /// <summary>
        ///   <para>Currently shows a fish on the top of the asserted fish.</para>
        /// </summary>
        /// <param name="gameUI">`this` for extension methods</param>
        /// <param name="enemy">Whether the fish is shown on the enemy side</param>
        public static void MakeAGuess(this GameUI gameUI, bool enemy)
        {
            gameUI.Gom.GuessFish = Object.Instantiate(
                SharedRefs.FishPrefabs[gameUI.GameState.AssertionTarget],
                GameObjectManager.FishRelativePosition(enemy, gameUI.GameState.Assertion) + new Vector3(0, 6, 0),
                Quaternion.Euler(new Vector3(0, 180, 0)),
                gameUI.allFishRoot
            ).gameObject;
        }
    }
}