using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "ScriptableObjects/ItemSO")]
public class ItemSO : ScriptableObject 
{
    [SerializeField] private ItemType type;
    [SerializeField] private Sprite sprite;
    [SerializeField] private ParticleSystem popEffect;

    public ItemType Type => type;
    public Sprite Sprite => sprite;
    public ParticleSystem PopEffect => popEffect;

}

