using LitJson;

namespace GameAnim
{
    public static class GamePassiveAnim
    {
        public static void PassiveAnim(this GameUI gameUI, JsonData actionInfo)
        {
            var actionFish = (int) actionInfo["ActionFish"];
            var passiveList = actionInfo["passive"];
            for (var i = 0; i < passiveList.Count; i++)
            {
                switch ((string) passiveList[i]["type"])
                {
                    case "counter":
                        break;
                    case "deflect":
                        break;
                    case "reduce":
                        break;
                    case "heal":
                        break;
                    case "explode":
                        break;
                }
            }
        }
    }
}