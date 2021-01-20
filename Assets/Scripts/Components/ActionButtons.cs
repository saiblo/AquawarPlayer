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
            skillText.text = Constants.SkillTable[fishId];
        }

        public void Normal()
        {
            gameUI.GameState.NormalAttack = true;
            gameUI.GameState.GameStatus = Constants.GameStatus.SelectEnemyFish;
            normalButtonImage.overrideSprite = gameUI.darkBlue;
            skillButtonImage.overrideSprite = gameUI.lightBlue;
        }

        public void Skill()
        {
            gameUI.GameState.NormalAttack = false;
            gameUI.GameState.GameStatus = Constants.GameStatus.SelectEnemyFish;
            normalButtonImage.overrideSprite = gameUI.lightBlue;
            skillButtonImage.overrideSprite = gameUI.darkBlue;
        }
    }
}