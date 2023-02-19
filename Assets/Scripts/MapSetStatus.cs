using UnityEngine;

public class MapSetStatus : MonoBehaviour
{
    [SerializeField]
    private Vector3 nextOffset = Vector3.zero;

    public Vector3 NextOffset
    {
        get { return nextOffset; }
        set { nextOffset = value; }
    }
}
