using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Components
{
    public class ActionButtons : MonoBehaviour
    {
        public Image normalButtonImage;
        public Image skillButtonImage;

        public Text skillText;

        public GameUI gameUI;

        public void Setup(int fishId)
        {
            skillText.text = Constants.SkillTable[fishId == 11 ? SharedRefs.MyImitate : fishId];
        }

        public void ResetButtons()
        {
            normalButtonImage.overrideSprite = gameUI.lightBlue;
            skillButtonImage.overrideSprite = gameUI.lightBlue;
        }

        private void ClearTargets()
        {
            for (var i = 0; i < 4; i++)
                gameUI.GameState.MyFishSelectedAsTarget[i] = gameUI.GameState.EnemyFishSelectedAsTarget[i] = false;
        }

        public void Normal()
        {
            gameUI.GameState.NormalAttack = true;
            gameUI.GameState.GameStatus = Constants.GameStatus.SelectEnemyFish;
            normalButtonImage.overrideSprite = gameUI.darkBlue;
            skillButtonImage.overrideSprite = gameUI.lightBlue;
            ClearTargets();
        }

        public void Skill()
        {
            gameUI.GameState.NormalAttack = false;
            gameUI.GameState.GameStatus = Constants.GameStatus.SelectEnemyFish;
            normalButtonImage.overrideSprite = gameUI.lightBlue;
            skillButtonImage.overrideSprite = gameUI.darkBlue;
            ClearTargets();

            var fishId = gameUI.GameState.MyFishId[gameUI.GameState.MyFishSelected];
            var skill = Constants.SkillDict[fishId == 11 ? SharedRefs.MyImitate : fishId];
            if (skill != Constants.Skill.Aoe &&
                !(skill == Constants.Skill.Clown &&
                  (fishId == 11 ? gameUI.GameState.ImitateUsed : gameUI.GameState.ClownUsed) < 3)) return;

            for (var i = 0; i < 4; i++)
                if (gameUI.GameState.EnemyFishAlive[i])
                    gameUI.GameState.EnemyFishSelectedAsTarget[i] = true;
        }
    }
}