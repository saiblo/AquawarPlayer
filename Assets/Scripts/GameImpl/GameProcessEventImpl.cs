using System;
using System.Collections.Generic;
using LitJson;
using Utils;

namespace GameImpl
{
    public static class GameProcessEventImpl
    {
        private class ActionEvent
        {
            public bool Enemy;
            public int Pos;
            public int Value;
            public int Time;
            public bool Positive;
        }

        private static void AddEventsToLog(this GameUI gameUI, List<ActionEvent> events)
        {
            if (events.Count == 0) return;
            gameUI.AddLog();
            gameUI.AddLog("本轮结算清单：");
            events.ForEach(actionEvent =>
            {
                var fishId =
                    (actionEvent.Enemy ? gameUI.GameState.EnemyFishId : gameUI.GameState.MyFishId)
                    [actionEvent.Pos];
                var fishName = SharedRefs.Mode == Constants.GameMode.Offline ||
                               !actionEvent.Enemy ||
                               gameUI.GameState.EnemyFishExpose[actionEvent.Pos]
                    ? Constants.FishName[fishId]
                    : "[未知]";
                gameUI.AddLog(
                    $"  {actionEvent.Time}. {(actionEvent.Enemy ? GameUI.EnemyStr : GameUI.MeStr)}{actionEvent.Pos}号位置的{fishName}血量{(actionEvent.Positive ? "+" : "-")}{actionEvent.Value}"
                );
            });
            gameUI.AddLog();
        }

        public static Action[] GenEventProcessor(this GameUI gameUI, JsonData actionInfo)
        {
            var events = new List<ActionEvent>();

            if (actionInfo.ContainsKey("hit"))
            {
                var hitList = actionInfo["hit"];
                for (var i = 0; i < hitList.Count; i++)
                {
                    events.Add(new ActionEvent
                    {
                        Enemy = SharedRefs.Mode == Constants.GameMode.Offline
                            ? (int) hitList[i]["player"] == 1
                            : (bool) hitList[i]["isEnemy"],
                        Pos = (int) hitList[i]["target"],
                        Value = (int) hitList[i]["value"],
                        Time = (int) hitList[i]["time"],
                        Positive = false
                    });
                }
            }

            if (actionInfo.ContainsKey("passive"))
            {
                var passiveList = actionInfo["passive"];
                for (var i = 0; i < passiveList.Count; i++)
                {
                    if ((string) passiveList[i]["type"] == "heal")
                    {
                        events.Add(new ActionEvent
                        {
                            Enemy = SharedRefs.Mode == Constants.GameMode.Offline
                                ? (int) passiveList[i]["player"] == 1
                                : (bool) passiveList[i]["isEnemy"],
                            Pos = (int) passiveList[i]["source"],
                            Value = (int) (double) passiveList[i]["value"],
                            Time = (int) passiveList[i]["time"],
                            Positive = true
                        });
                    }
                }
            }

            events.Sort((x, y) => x.Time - y.Time);

            return new Action[] {() => { gameUI.AddEventsToLog(events); }};
        }
    }
}