using UnityEngine;
using System.Collections;

/// <summary>
/// ランダムで待機アニメーションを再生する
/// キャラクターコントロール
/// </summary>
[RequireComponent(typeof(Animator))]
public class IdleControl : MonoBehaviour
{
	private const string PARAM_SEED = "Seed";	// Seed パラメーター名

	[SerializeField]
	private float interval = 10f;               // ランダム判定のインターバル

	private Animator animMotion;

	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start()
	{
		// 各参照の初期化
		animMotion = GetComponent<Animator>();

		// ランダム変更開始
		StartCoroutine(RandomChange());
	}

	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update()
	{
		if (animMotion.GetInteger(PARAM_SEED) > 0)
		{
			animMotion.SetInteger(PARAM_SEED, 0);
		}
	}

	/// <summary>
	/// ランダム変更
	/// </summary>
	IEnumerator RandomChange()
	{
		// 無限ループ開始
		while (true)
		{
			// 次の判定までインターバルを置く
			yield return new WaitForSeconds(interval);

			// ランダムシードを設定をする
			if (animMotion.runtimeAnimatorController.animationClips.Length > 1)
			{
				animMotion.SetInteger(PARAM_SEED, Random.Range(1, animMotion.runtimeAnimatorController.animationClips.Length));
			}
		}
	}
}
