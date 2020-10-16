using UnityEngine;

namespace Game
{
    public class AllFish : MonoBehaviour
    {
        public Transform fishPrefab;

        private void Awake()
        {
            for (var i = 0; i < 4; i++)
            {
                Instantiate(fishPrefab, transform)
                    .localPosition = new Vector3(2 * (i + 2), 0, 2 - i);
                Instantiate(fishPrefab, transform)
                    .localPosition = new Vector3(-2 * (i + 2), 0, 2 - i);
            }
        }
    }
}