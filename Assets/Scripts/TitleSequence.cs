using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトルシーンのシーケンス制御
/// </summary>
public class TitleSequence : MonoBehaviour
{
    // シーケンス種別
    enum SequenceState
    {
        FadeWait,               // フェード待ち
        PressAnyKey,            // キー入力待ち
        SelectContent,          // コンテンツ選択
        Record,                 // レコード表示
        NextContent,            // 次のコンテンツへ
        Waiting,                // 待機中
    }
    private SequenceState seqState = SequenceState.FadeWait;

    enum NextContent
    {
        GameStart = 0,          // ゲーム開始
        Record,                 // レコード表示

        SelectContent,          // コンテンツ選択
    }

    private Transform transWindowContent;
    private Transform transPylon;
    private Transform transWindowRecord;

    // カーソル位置リスト
    private List<float> listPylonPos = new List<float>();
    private int contentIndex = 0;

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        // 初期化完了
        SceneLoadControl.Instance.IsInitialized = true;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialze()
    {
        transWindowContent = GameObject.Find("Canvas").transform.Find("Window_Content");
        transPylon = transWindowContent.Find("Content/Image_Pylon");
        transWindowRecord = GameObject.Find("Canvas").transform.Find("Window_Record");

        // カーソル位置リストの取得
        foreach (Transform child in transWindowContent.Find("Content"))
        {
            if (child.name.StartsWith("Text_"))
            {
                listPylonPos.Add(child.localPosition.y);
            }
        }

        // レコードを取得して設定
        List<float> list = RecordControl.Instance.GetRecord();

        TextMeshProUGUI textRecord = transWindowRecord.Find("Content/Text_Record").GetComponent< TextMeshProUGUI>();
        textRecord.text = "";

        for (int i = 0; i < list.Count; ++i)
        {
            if (textRecord.text.Length > 0)
            {
                textRecord.text += "\n";
            }
            textRecord.text += string.Format(RecordControl.STR_DISTANCE_FORMAT, list[i]);
        }
        for (int i = list.Count; i < RecordControl.RECORD_NUM; ++i)
        {
            if (textRecord.text.Length > 0)
            {
                textRecord.text += "\n";
            }
            textRecord.text += string.Format(RecordControl.STR_DISTANCE_FORMAT, 0f);
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        switch (seqState)
        {
            // フェード待ち
            case SequenceState.FadeWait:
                if (SceneManager.GetSceneByName(SceneLoadControl.SceneName).IsValid())
                {
                    // ローディングシーンの破棄待ち
                    break;
                }
                Initialze();
                seqState = SequenceState.PressAnyKey;
                break;

            // キー入力待ち
            case SequenceState.PressAnyKey:
                if (Input.GetButtonDown("Submit"))
                {
                    // 入力受付
                    seqState = SequenceState.SelectContent;
                    GameObject.Find("Text_PressAnyKey").SetActive(false);
                    transWindowContent.gameObject.SetActive(true);
                }
                break;

            // コンテンツ選択
            case SequenceState.SelectContent:
                SelectContentSequence();
                break;

            // レコード表示
            case SequenceState.Record:
                RecordSequence();
                break;

            // 次のコンテンツへ
            case SequenceState.NextContent:
                if (!transWindowContent.gameObject.activeSelf && !transWindowRecord.gameObject.activeSelf)
                {
                    switch (contentIndex)
                    {
                        // ゲーム開始
                        case (int)NextContent.GameStart:
                            SceneLoadControl.Instance.LoadScene("ActionScene");
                            seqState = SequenceState.Waiting;
                            break;

                        // レコード表示
                        case (int)NextContent.Record:
                            transWindowRecord.gameObject.SetActive(true);
                            seqState = SequenceState.Record;
                            break;

                        // コンテンツ選択
                        case (int)NextContent.SelectContent:
                            contentIndex = (int)NextContent.Record;
                            transWindowContent.gameObject.SetActive(true);
                            seqState = SequenceState.SelectContent;
                            break;

                        default:
                            break;
                    }
                }
                break;

            // 待機中
            case SequenceState.Waiting:
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// コンテンツ選択 シーケンス制御
    /// </summary>
    private void SelectContentSequence()
    {
        if (Input.GetButtonDown("Submit"))
        {
            // 項目決定
            seqState = SequenceState.NextContent;
            transWindowContent.GetComponent<WindowControl>().SetOutTrigger();
        }
        else if (Input.GetButtonDown("Vertical"))
        {
            float v = Input.GetAxis("Vertical");
            if (v > 0f)
            {
                // 項目を一つ上へ
                --contentIndex;
                if (contentIndex < 0)
                {
                    contentIndex = listPylonPos.Count - 1;
                }
            }
            else
            {
                // 項目を一つ下へ
                ++contentIndex;
                if (contentIndex >= listPylonPos.Count)
                {
                    contentIndex = 0;
                }
            }
            transPylon.localPosition = new Vector3(transPylon.localPosition.x, listPylonPos[contentIndex], 0f);
        }
    }

    /// <summary>
    /// レコード表示 シーケンス制御
    /// </summary>
    private void RecordSequence()
    {
        if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel"))
        {
            // 項目決定
            contentIndex = (int)NextContent.SelectContent;
            seqState = SequenceState.NextContent;
            transWindowRecord.GetComponent<WindowControl>().SetOutTrigger();
        }
    }
}
