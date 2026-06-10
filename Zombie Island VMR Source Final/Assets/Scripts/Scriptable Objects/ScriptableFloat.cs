using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableFloat", menuName = "ScriptableObjects/ScriptableFloat")]

public class ScriptableFloat : ScriptableObject
{
    [SerializeField] private float _value;

    public float Value
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
