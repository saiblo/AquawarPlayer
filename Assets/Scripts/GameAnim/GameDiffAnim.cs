using System.Collections.Generic;
using GameImpl;
using UnityEngine;

namespace GameAnim
{
    public static class GameDiffAnim
    {
        public static void DiffAnim(this GameUI gameUI, IEnumerable<GameProcessEventImpl.ActionEvent> events)
        {
            foreach (var e in events)
            {
                gameUI.SetTimeout(() =>
                {
                    Object.Instantiate(
                        gameUI.diffPrefab,
                        (e.Enemy ? gameUI.enemyStatus : gameUI.myStatus)[e.Pos].transform
                    ).Setup(e.Value, e.Positive);
                }, e.Time * 300);
            }
        }
    }
}