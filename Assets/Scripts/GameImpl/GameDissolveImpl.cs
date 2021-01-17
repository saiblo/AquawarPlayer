using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace GameImpl
{
    public static class GameDissolveImpl
    {
        public static void Dissolve(this GameUI gameUI, bool enemy, int pos)
        {
            if (enemy) gameUI.GameState.EnemyFishAlive[pos] = false;
            else gameUI.GameState.MyFishAlive[pos] = false;

            var meshRenderers = (enemy ? gameUI.Gom.EnemyFishMeshRenderers : gameUI.Gom.MyFishMeshRenderers)[pos];
            var fish = (enemy ? gameUI.Gom.EnemyFishTransforms : gameUI.Gom.MyFishTransforms)[pos];
            var fog = (enemy ? gameUI.Gom.EnemyFogs : gameUI.Gom.MyFogs)[pos];
            (enemy ? gameUI.Gom.EnemyFishParticleSystems : gameUI.Gom.MyFishParticleSystems)[pos]?.Play();

            foreach (var meshRenderer in meshRenderers) meshRenderer.material = gameUI.dissolveEffect;
            gameUI.Repeat(cnt =>
                {
                    foreach (var meshRenderer in meshRenderers)
                        meshRenderer.material.SetFloat(gameUI.DissolveShaderProperty,
                            gameUI.fadeIn.Evaluate(Mathf.InverseLerp(0, 3, cnt / 25f)));
                }, () =>
                {
                    if (fish != null) Object.Destroy(fish.gameObject);
                    fog.gameObject.SetActive(false);
                },
                75, 0, 40);

            var name = SharedRefs.Mode == Constants.GameMode.Offline || !enemy || gameUI.GameState.EnemyFishExpose[pos]
                ? Constants.FishName[(enemy ? gameUI.GameState.EnemyFishId : gameUI.GameState.MyFishId)[pos]]
                : "鱼";
            gameUI.AddLog($"{(enemy ? GameUI.EnemyStr : GameUI.MeStr)}{pos}号位置的{name}死亡。");
        }
    }
}