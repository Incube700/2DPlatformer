using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRestarter : MonoBehaviour
{
    [SerializeField] private float _restartDelay = 0.35f;

    private bool _isRestarting;

    public void RestartLevel()
    {
        if (_isRestarting)
        {
            return;
        }

        StartCoroutine(RestartLevelCoroutine());
    }

    private IEnumerator RestartLevelCoroutine()
    {
        _isRestarting = true;

        if (_restartDelay > 0f)
        {
            yield return new WaitForSeconds(_restartDelay);
        }

        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
            yield break;
        }

        if (string.IsNullOrEmpty(activeScene.path) == false)
        {
            SceneManager.LoadScene(activeScene.path);
            yield break;
        }

        SceneManager.LoadScene(activeScene.name);
    }
}
