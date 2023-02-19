using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン読み込み制御
/// </summary>
public class SceneLoadControl : MonoBehaviour
{
    // ローディングシーン名
    public static string SceneName = "LoadingScene";

    // インスタンス
    private static SceneLoadControl instance = null;
    public static SceneLoadControl Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject();
                obj.name = "SceneLoadControl";
                instance = obj.AddComponent<SceneLoadControl>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    // ローディングインスタンス
    private LoadingSequence loadingInstance = null;
    public LoadingSequence LoadingInstance
    {
        set
        {
            loadingInstance = value;
        }
    }

    // シーン状態
    enum SceneState
    {
        Idle,           // 待機中
        FadeOut,        // フェードアウト中
        SceneLoad,      // 次シーンの読み込み中
        FadeIn,         // フェードイン
    }
    private SceneState sceneState = SceneState.Idle;

    private string currentSceneName = "";       // 現在シーン名
    private string nextSceneName = "";          // 次シーン名

    private bool isInitialized = false;
    public bool IsInitialized
    {
        get { return isInitialized; }
        set { isInitialized = value; }
    }

    /// <summary>
    /// シーン読み込み
    /// </summary>
    /// <param name="sceneName">シーン名</param>
    /// <returns>読み込み開始の成否</returns>
    public bool LoadScene(string sceneName)
    {
        // 待機中でなければ失敗
        if (sceneState != SceneState.Idle)
        {
            return false;
        }

        // 情報更新
        sceneState = SceneState.FadeOut;
        currentSceneName = SceneManager.GetActiveScene().name;
        nextSceneName = sceneName;
        isInitialized = false;

        // ローディングシーンを追加表示
        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);

        StartCoroutine(UpdateCoroutine());

        return true;
    }

    /// <summary>
    /// 更新制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            switch (sceneState)
            {
                // フェードアウト中
                case SceneState.FadeOut:
                    if (loadingInstance == null)
                    {
                        // ローディングシーンの開始待ち
                        break;
                    }
                    if (loadingInstance.IsLoading())
                    {
                        // ロード中になったらシーン読み込み開始
                        sceneState = SceneState.SceneLoad;
                    }
                    break;

                // 次シーンの読み込み中
                case SceneState.SceneLoad:
                    yield return SceneManager.UnloadSceneAsync(currentSceneName);
                    yield return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
                    while (!isInitialized)
                    {
                        yield return null;
                    }
                    // ロード完了したらフェードイン開始
                    loadingInstance.LoadComplete();
                    sceneState = SceneState.FadeIn;
                    break;

                // フェードイン中
                case SceneState.FadeIn:
                    if (loadingInstance.IsComplete())
                    {
                        // フェードイン完了したら破棄
                        SceneManager.UnloadSceneAsync("LoadingScene");
                        instance = null;
                        Destroy(gameObject);
                        yield break;
                    }
                    break;

                default:
                    break;
            }

            yield return null;
        }
    }
}
