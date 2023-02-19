using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトルシーンのシーケンス制御
/// </summary>
public class ActionSequence : MonoBehaviour
{
    [SerializeField]
    private GameObject[] mapSetList;            // マップセットのプレハブリスト

    [SerializeField]
    private int mapAddCount = 5;                // マップセット配置数

    [SerializeField]
    private GameObject[] obstacleSetList;       // 障害物セットのプレハブリスト

    [SerializeField]
    private int obstacleAddCount = 10;          // 障害物セット配置数

    [SerializeField]
    private float obstacleIntervalPos = 50f;    // 障害物セット調整間隔

    [SerializeField]
    private float levelInterval = 100f;         // レベル調整間隔

    [SerializeField]
    private int levelRankMax = 10;              // レベル調整最大値

    private const string STR_START_STRING = "Start!";       // スタート文字列
    private const string STR_LEVEL_FORMAT = "Lv. {0}";      // レベル文字列フォーマット

    // シーケンス種別
    enum SequenceState
    {
        FadeWait,               // フェード待ち
        CountDown,              // カウントダウン中
        Playing,                // プレイ中
        Finished,               // フィニッシュ
        Waiting,                // 待機中
    }
    private SequenceState seqState = SequenceState.FadeWait;

    private CharacterControl charaControl;
    private Transform transMapRoot;
    private Transform transObstacleRoot;
    private TextMeshProUGUI textLevel;
    private TextMeshProUGUI textDistance;

    private Vector3 nextMapOffset;
    private List<Vector3> offsetMapList = new List<Vector3>();
    private Vector3 nextObstacleOffset;
    private List<Vector3> offsetObstacleList = new List<Vector3>();

    private int prevLevel;              // 前回レベル

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    IEnumerator Start()
    {
        charaControl = GameObject.Find("PlayerCharacter").GetComponent<CharacterControl>();
        transMapRoot = GameObject.Find("MapRoot").transform;
        transObstacleRoot = GameObject.Find("ObstacleRoot").transform;
        textLevel = GameObject.Find("Text_Level").GetComponent<TextMeshProUGUI>();
        textDistance = GameObject.Find("Text_Distance").GetComponent<TextMeshProUGUI>();

        prevLevel = GetLevel(true);

        // マップ生成
        yield return GenerateMap();

        // 更新制御を開始
        StartCoroutine(UpdateCoroutine());

        // 初期化完了
        SceneLoadControl.Instance.IsInitialized = true;
    }

    /// <summary>
    /// 更新制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateCoroutine()
    {
        while (true)
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
                    seqState = SequenceState.CountDown;
                    break;

                // カウントダウン中
                case SequenceState.CountDown:
                    yield return CountDownSequence();
                    seqState = SequenceState.Playing;
                    break;

                // プレイ中
                case SequenceState.Playing:
                    yield return PlayingSequence();
                    seqState = SequenceState.Finished;
                    break;

                // フィニッシュ
                case SequenceState.Finished:
                    yield return FinishedSequence();
                    seqState = SequenceState.Waiting;
                    break;

                default:
                    break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// マップ生成
    /// </summary>
    /// <returns></returns>
    private IEnumerator GenerateMap()
    {
        if (mapSetList.Length == 0)
        {
            // マップセットが無くては生成できない
            yield break;
        }

        // 既存のものを削除
        foreach (Transform child in transMapRoot)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in transObstacleRoot)
        {
            Destroy(child.gameObject);
        }
        yield return null;

        // マップセットの配置
        nextMapOffset = Vector3.zero;
        for (int i = 0; i < mapAddCount; ++i)
        {
            AddMapSet();
        }

        // 障害物セットの配置
        nextObstacleOffset = new Vector3(0f, 0f, obstacleIntervalPos);
        for (int i = 0; i < obstacleAddCount; ++i)
        {
            AddObstacleSet();
        }
    }

    /// <summary>
    /// マップセットの配置
    /// </summary>
    private void AddMapSet()
    {
        // プレハブから生成
        int index = Random.Range(0, mapSetList.Length);
        GameObject obj = Instantiate(mapSetList[index], nextMapOffset, Quaternion.identity);
        obj.transform.parent = transMapRoot;

        // 次位置の調整
        Vector3 offset = obj.GetComponent<MapSetStatus>().NextOffset;
        nextMapOffset += offset;
        offsetMapList.Add(nextMapOffset + offset * 0.2f);
    }

    /// <summary>
    /// 障害物セットの配置
    /// </summary>
    private void AddObstacleSet()
    {
        // プレハブから生成
        int index = Random.Range(0, obstacleSetList.Length);
        GameObject obj = Instantiate(obstacleSetList[index], nextObstacleOffset, Quaternion.identity);
        obj.transform.parent = transObstacleRoot;

        // 次位置の調整
        Vector3 offset = obj.GetComponent<MapSetStatus>().NextOffset;
        int difficult = levelRankMax - GetLevel(true);
        offset.z += obstacleIntervalPos / levelRankMax * difficult;
        nextObstacleOffset += offset;
        offsetObstacleList.Add(nextObstacleOffset + offset * 0.2f);
    }

    /// <summary>
    /// カウントダウン シーケンス制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator CountDownSequence()
    {
        int count = 3;
        TextMeshProUGUI textCountDown = GameObject.Find("Canvas").transform.Find("Text_CountDown").GetComponent<TextMeshProUGUI>();

        // カウントダウンテキストの表示
        textCountDown.text = count.ToString();
        textCountDown.gameObject.SetActive(true);

        while (count > 0)
        {
            yield return new WaitForSeconds(1.0f);

            // カウントダウンテキストの変更
            --count;
            textCountDown.text = count.ToString();
        }

        // カウントダウンテキストの変更
        textCountDown.text = STR_START_STRING;

        // プレイヤーの移動開始
        charaControl.enabled = true;
    }

    /// <summary>
    /// プレイ中 シーケンス制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayingSequence()
    {
        while (true)
        {
            // 走行距離を表示
            textDistance.text = string.Format(RecordControl.STR_DISTANCE_FORMAT, GetDistance());

            // レベルを更新
            int level = GetLevel(false);
            textLevel.text = string.Format(STR_LEVEL_FORMAT, level);
            if ((prevLevel != level) && (level > levelRankMax))
            {
                charaControl.SpeedUp();
                prevLevel = level;
            }

            // 現在地を確認してマップを更新
            if (charaControl.transform.localPosition.z >= offsetMapList[0].z)
            {
                Destroy(transMapRoot.GetChild(0).gameObject);
                offsetMapList.RemoveAt(0);
                AddMapSet();
            }

            // 現在地を確認して障害物を更新
            if (charaControl.transform.localPosition.z >= offsetObstacleList[0].z)
            {
                Destroy(transObstacleRoot.GetChild(0).gameObject);
                offsetObstacleList.RemoveAt(0);
                AddObstacleSet();
            }

            // 激突確認
            if (charaControl.IsDamage())
            {
                GameObject.Find("Canvas").transform.Find("Text_Finish").gameObject.SetActive(true);
                yield break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// レベルの取得
    /// </summary>
    /// <returns>レベル</returns>
    private int GetLevel(bool isMaxCut)
    {
        int level = (int)(charaControl.transform.localPosition.z / levelInterval) + 1;
        if ((isMaxCut) && (level > levelRankMax))
        {
            level = levelRankMax;
        }
        return level;
    }

    /// <summary>
    /// 走行距離の取得
    /// </summary>
    /// <returns>走行距離</returns>
    private float GetDistance()
    {
        return charaControl.transform.localPosition.z / 2f;
    }

    /// <summary>
    /// フィニッシュ シーケンス制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator FinishedSequence()
    {
        // プレイ結果を保存
        RecordControl.Instance.SaveRecord(GetDistance());

        // 少し待機してからタイトルシーンへ
        yield return new WaitForSeconds(4.0f);
        SceneLoadControl.Instance.LoadScene("TitleScene");
    }
}
