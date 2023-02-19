using UnityEngine;

/// <summary>
/// ローディングシーンのシーケンス制御
/// </summary>
public class LoadingSequence : MonoBehaviour
{
    // シーケンス種別
    enum SequenceState
    {
        FadeIn,             // フェードイン中
        Loading,            // ロード中
        FadeOut,            // フェードアウト中
        Complete,           // 完了
    }
    private SequenceState seqState = SequenceState.FadeIn;

    private Animator    animBackGround;
    private Transform   transLoading;

    static int bgOutState = Animator.StringToHash("Base Layer.LoadingBgOut");

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        animBackGround = GameObject.Find("BackGround").GetComponent<Animator>();
        transLoading = animBackGround.transform.Find("LoadingRoot");

        SceneLoadControl.Instance.LoadingInstance = this;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        switch (seqState)
        {
            // フェードイン中
            case SequenceState.FadeIn:
                if (transLoading.gameObject.activeSelf)
                {
                    // フェードイン完了時にロード中へ
                    seqState = SequenceState.Loading;
                }
                break;

            // フェードアウト中
            case SequenceState.FadeOut:
                AnimatorStateInfo state = animBackGround.GetCurrentAnimatorStateInfo(0);
                if (state.fullPathHash == bgOutState)
                {
                    if (state.normalizedTime > 1f)
                    {
                        // 再生完了時にシーン破棄
                        seqState = SequenceState.Complete;
                    }
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// ロード中かどうか
    /// </summary>
    /// <returns>ロード中の是非</returns>
    public bool IsLoading()
    {
        return seqState == SequenceState.Loading;
    }

    /// <summary>
    /// 完了したかどうか
    /// </summary>
    /// <returns>完了の是非</returns>
    public bool IsComplete()
    {
        return seqState == SequenceState.Complete;
    }

    /// <summary>
    /// ロード完了
    /// </summary>
    /// <returns>フェードアウト開始の是非</returns>
    public bool LoadComplete()
    {
        if (!IsLoading())
        {
            return false;
        }

        // フェードアウト開始
        animBackGround.SetTrigger("IsOut");
        seqState = SequenceState.FadeOut;

        return true;
    }
}
