using UnityEngine;

/// <summary>
/// キャラクター制御
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterControl : MonoBehaviour
{
	[SerializeField]
	private float animSpeed = 1.5f;             // アニメーション再生速度設定

	[SerializeField]
	private float forwardSpeed = 7.0f;          // 前進速度

	[SerializeField]
	private float backwardSpeed = 2.0f;         // 後退速度

	[SerializeField]
	private float rotateSpeed = 2.0f;           // 旋回速度

	[SerializeField, Range(-1, 1)]
	private short autoMoveVertical = 0;			// 前後のオート移動

	[SerializeField, Range(-1, 1)]
	private short autoMoveHorizontal = 0;		// 左右のオート移動

	private const string STR_ROAD_PREFAB_NAME = "sptp_road_segment_";   // 道オブジェクト名
	private const string PARAM_DAMAGE = "Damage";                       // Damage パラメーター名

	private Animator animator;                  // アニメーター参照

	private bool isDamage = false;              // ダメージフラグ

	/// <summary>
	/// Start is called before the first frame update
	/// </summary>
	void Start()
	{
		animator = GetComponent<Animator>();
	}

	/// <summary>
	/// FixedUpdate is called every fixed frame-rate frame
	/// </summary>
	void FixedUpdate()
	{
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		if (isDamage)
		{
			// ダメージ時は移動させない
			h = v = 0f;
		}
		else
		{
			if (autoMoveVertical != 0)
            {
				// 前後のオート移動
				v = 1.0f;
			}
			if (autoMoveHorizontal != 0)
            {
				// 左右のオート移動
				h = 1.0f;
			}
		}

		// アニメーション設定
		animator.SetFloat("Speed", v);                         
		animator.SetFloat("Direction", h);                     
		animator.speed = animSpeed;                            

		// 移動処理
		Vector3 velocity = new Vector3(0, 0, v);
		velocity = transform.TransformDirection(velocity);
		if (v > 0.1)
		{
			// 速度調整
			velocity *= forwardSpeed;  
		}
		else if (v < -0.1)
		{
			// 速度調整
			velocity *= backwardSpeed; 
		}
		transform.localPosition += velocity * Time.fixedDeltaTime;
		transform.Rotate(0, h * rotateSpeed, 0);
	}

	/// <summary>
	/// 接触判定
	/// </summary>
	/// <param name="collision">接触コリジョン</param>
	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.name.StartsWith(STR_ROAD_PREFAB_NAME))
		{
			// 道オブジェクトは無視
			return;
		}

		// 障害物に衝突したのでダメージ
		isDamage = true;
		animator.SetTrigger(PARAM_DAMAGE);
	}

	/// <summary>
	/// ダメージ判定
	/// </summary>
	/// <returns></returns>
	public bool IsDamage()
	{
		return isDamage;
	}

	/// <summary>
	/// 速度アップ
	/// </summary>
	public void SpeedUp()
	{
		forwardSpeed += 1f;
		animSpeed += 0.2f;
	}
}
