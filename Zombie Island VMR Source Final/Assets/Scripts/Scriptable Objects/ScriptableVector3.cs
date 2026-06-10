using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableVector3", menuName = "ScriptableObjects/ScriptableVector3")]
public class ScriptableVector3 : ScriptableObject
{
    [SerializeField] public Vector3 _value;

    public Vector3 Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
        }
    }
}
