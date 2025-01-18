using UnityEngine;
using System.Collections;
using System.IO;

namespace Internal
{
    public class FakeKeyGenerator : MonoBehaviour
    {
        private const int fakeKeyCount = 10;

#if !UNITY_EDITOR
        private const float GenerationCooldown = 2f;
#else
        private const float GenerationCooldown = 0f;
#endif

        public void StartDelayedFakeKeyGeneration()
        {
            try
            {
                StartCoroutine(GenerateFakeKeysCoroutine());
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        private IEnumerator GenerateFakeKeysCoroutine()
        {
            Debug.Log("<color=green>[FakeKeyGenerator] Starting fake key generation...</color>");

            for (var i = 0; i < fakeKeyCount; i++)
            {
                FakeKeyUtility.GenerateAndSaveFakeKeys(i + 1, Path.Combine(Application.persistentDataPath, "AppKeys"));

                yield return new WaitForSeconds(GenerationCooldown);
            }

            for (var i = 0; i < fakeKeyCount; i++)
            {
                FakeKeyUtility.GenerateAndSaveFakeKeys(i + 1);

                yield return new WaitForSeconds(GenerationCooldown);
            }

            FakeKeyUtility.GenerateAndSaveFakeKeys(1, Path.Combine(Application.persistentDataPath, "Game"));
            yield return new WaitForSeconds(GenerationCooldown);
            FakeKeyUtility.GenerateAndSaveFakeKeys(2, Path.Combine(Application.persistentDataPath, "SequreStorage"));
            yield return new WaitForSeconds(GenerationCooldown);
            FakeKeyUtility.GenerateAndSaveFakeKeys(2, Path.Combine(Application.persistentDataPath, "InApps"));
            yield return new WaitForSeconds(GenerationCooldown);
            FakeKeyUtility.GenerateAndSaveFakeKeys(3, Path.Combine(Application.persistentDataPath, "SecretFolder"));

            Debug.Log("<color=green>[FakeKeyGenerator] Fake key generation completed!</color>");
        }
    }
}