using LitJson;

namespace Utils
{
    public static class Validators
    {
        public static bool ValidateJson(JsonData json)
        {
            if (!json.IsArray) return false;
            var len = json.Count;
            while (json[len - 1] == null) --len;
            for (var i = 0; i < len; i++)
            {
                var obj = json[i];
                if (obj == null) return false;
                if (!obj["winner"].IsInt) return false;
                if ((int) obj["winner"] != -1 && (int) obj["winner"] != 0 && (int) obj["winner"] != 1) return false;
                if (!obj["players"].IsArray) return false;
                if (!obj["players"][0]["id"].IsInt) return false;
                if ((int) obj["players"][0]["id"] != 0) return false;
                if (!obj["players"][1]["id"].IsInt) return false;
                if ((int) obj["players"][1]["id"] != 1) return false;
                if (!obj["state"].IsInt) return false;
                // if ((int) obj["state"] != i + 1) return false;
                if (!obj["gamestate"].IsInt) return false;
                if (!obj["cur_turn"].IsInt) return false;
                if ((int) obj["cur_turn"] != 0 && (int) obj["cur_turn"] != 1) return false;
                if (!obj["over"].IsBoolean) return false;
                if (!obj["score"].IsInt) return false;
                if (!obj["rounds"].IsInt) return false;
                if (!obj["operation"].IsArray) return false;
            }
            return true;
        }
    }
}