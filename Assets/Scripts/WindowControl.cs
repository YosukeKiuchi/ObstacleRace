using UnityEngine;

/// <summary>
/// ウィンドウ制御
/// </summary>
[RequireComponent(typeof(Animator))]
public class WindowControl : MonoBehaviour
{
    private Animator animWindow;

    static int outState = Animator.StringToHash("Base Layer.WindowAnimOut");

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        animWindow = GetComponent<Animator>();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // 再生完了時にオブジェクト非表示
        AnimatorStateInfo state = animWindow.GetCurrentAnimatorStateInfo(0);
        if (state.fullPathHash == outState)
        {
            if (state.normalizedTime > 1f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// アウトトリガーの設定
    /// </summary>
    public void SetOutTrigger()
    {
        animWindow.SetTrigger("IsOut");
    }
}
