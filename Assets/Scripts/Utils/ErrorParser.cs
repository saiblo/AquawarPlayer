using System.Linq;
using LitJson;
using UnityEngine;

namespace Utils
{
    public static class ErrorParser
    {
        private static string Parse(JsonData e)
        {
            var player = (int) e["player"];
            var type = (int) e["type"];
            switch (type)
            {
                case 0:
                    return $"在{player}号玩家的回合中Judger出现错误。";
                case 1:
                    return $"{player}号玩家发送的消息无法解析。";
                case 2:
                    return $"{player}号玩家发生运行时错误（RE）。";
                case 3:
                    return $"{player}号玩家超时（TLE）。";
                case 4:
                    return $"{player}号玩家发送的消息中存在非法项。";
                case 5:
                    return $"{player}号玩家发送的消息中缺少了必要的项。";
                case 6:
                    return $"{player}号玩家发送的消息中，Action项的值非法。";
                case 7:
                    return $"{player}号玩家发来的信息中，某些整数的范围不对。";
                case 8:
                    return $"{player}号玩家发来的信息中，有些列表包含重复元素。";
                case 9:
                    return $"选择阶段中，逻辑收到了{player}号玩家多次发来的结束信息。";
                case 10:
                    return $"选择阶段中，{player}号玩家选择了少于或多于4条鱼。";
                case 11:
                    return $"选择阶段中，{player}号玩家选择了之前上场过的鱼。";
                case 12:
                    return $"断言阶段中，{player}号玩家选择断言一条已死亡的鱼。";
                case 13:
                    return $"断言阶段中，{player}号玩家选择断言一条已暴露的鱼。";
                case 14:
                    return $"行动阶段中，{player}号玩家的操作涉及已死亡的鱼。";
                case 15:
                    var actionFishName = Constants.FishName[(int) e["actionfish"] - 1];
                    var actionRuleErrorType = (int) e["action_rules_error_type"];
                    var prefix = $"行动阶段中，{player}号玩家选择了{actionFishName}发动主动技能，但操作违规了：";
                    switch (actionRuleErrorType)
                    {
                        case 1:
                            return $"{prefix}玩家选择了友方目标，但{actionFishName}的主动技能不能对友方目标发动。";
                        case 2:
                            return $"{prefix}玩家没有选择友方目标，但{actionFishName}的主动技能应该对友方目标发动。";
                        case 3:
                            return $"{prefix}玩家选择了敌方目标，但{actionFishName}的主动技能不能对敌方目标发动。";
                        case 4:
                            return $"{prefix}玩家没有选择敌方目标，但{actionFishName}的主动技能应该对敌方目标发动";
                        case 5:
                            return $"{prefix}玩家选择了多个友方目标，但{actionFishName}的主动技能只应该对一个友方目标发动。";
                        case 6:
                            return $"{prefix}玩家选择了多个敌方目标，但{actionFishName}的主动技能只应该对一个敌方目标发动。";
                        case 7:
                            return $"{prefix}玩家发动了{actionFishName}的伤害队友技能，但选择的目标是{actionFishName}自己。";
                        case 8:
                            return $"{prefix}玩家发动了{actionFishName}的给队友上buff技能，但选择的目标是{actionFishName}自己。";
                        case 9:
                            return $"{prefix}玩家发动了{actionFishName}的锁定最低血暴击技能，但选择的目标生命值不是最低。";
                        default:
                            return "";
                    }
                default:
                    return "";
            }
        }

        public static bool HandleErrorCheck(GameUI gameUI)
        {
            var state = SharedRefs.ReplayJson[SharedRefs.ReplayCursor];
            if (!state.ContainsKey("errors")) return false;
            gameUI.resultText.text = string.Join("\n", state["errors"].OfType<JsonData>().Select(ErrorParser.Parse));
            gameUI.gameOverText.text = "回到首页";
            SharedRefs.ErrorFlag = true;
            gameUI.gameOverMask.SetActive(true);
            return true;
        }
    }
}