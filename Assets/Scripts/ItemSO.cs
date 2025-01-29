using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "ScriptableObjects/ItemSO")]
public class ItemSO : ScriptableObject 
{
    [SerializeField] private ItemType _type;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private ParticleSystem _popEffect;

    public ItemType Type => _type;
    public Sprite Sprite => _sprite;
    public ParticleSystem PopEffect => _popEffect;

}

