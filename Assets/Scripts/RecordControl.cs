using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class RecordControl
{
    // インスタンス
    private static RecordControl instance = null;
    public static RecordControl Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RecordControl();
            }
            return instance;
        }
    }

    public static string STR_DISTANCE_FORMAT = "{0:#,0.00} m";      // 走行距離フォーマット
    public static int RECORD_NUM = 5;                               // 保存レコード数

    private const string RECORD_FILE_NAME = "record.dat";           // 保存ファイル名

    /// <summary>
    /// 結果保存
    /// </summary>
    /// <param name="record">レコード値</param>
    /// <returns>保存の成否</returns>
    public int SaveRecord(float record)
    {
        // ファイルを開く
        string path = Path.Combine(Application.dataPath, RECORD_FILE_NAME);
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        try
        {
            // 元レコードを読み込み
            List<float> list = GetRecord(stream);
            stream.Position = 0;

            // 追加して降順ソート
            list.Add(record);
            list.Sort((a, b) => b.CompareTo(a));

            // 上下を超えた分は削除
            if (list.Count > RECORD_NUM)
            {
                list.RemoveAt(list.Count - 1);
            }

            // シリアライズして保存
            BinaryFormatter bf = new BinaryFormatter();
            foreach (float val in list)
            {
                bf.Serialize(stream, val);
            }
        }
        finally
        {
            stream.Close();
        }

        return 0;
    }

    /// <summary>
    /// 結果取得
    /// </summary>
    /// <param name="stream">ファイルストリーム</param>
    /// <returns>レコードリスト</returns>
    public List<float> GetRecord(FileStream stream = null)
    {
        List<float> list = new List<float>();

        // ファイルを開く
        bool isOpen = false;
        if (stream == null)
        {
            string path = Path.Combine(Application.dataPath, RECORD_FILE_NAME);
            stream = new FileStream(path, FileMode.OpenOrCreate);
            isOpen = true;
        }

        // 元レコードを読み込み
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            while (stream.Position < stream.Length)
            {
                list.Add((float)bf.Deserialize(stream));
            }
        }
        finally
        {
            if (isOpen)
            {
                stream.Close();
            }
        }

        return list;
    }
}
