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
            switch (Constants.SkillDict[gameUI.GameState.MyFishId[gameUI.GameState.MyFishSelected]])
            {
                case Constants.Skill.Aoe:
                    for (var i = 0; i < 4; i++)
                        gameUI.GameState.EnemyFishSelectedAsTarget[i] = true;
                    break;
            }
        }
    }
}