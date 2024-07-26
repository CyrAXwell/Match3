using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSO", menuName = "ScriptableObjects/LevelSO")]
public class LevelSO : ScriptableObject
{
    [SerializeField] private ItemSO[] items;
    [SerializeField] private bool isRandomLevelFill;
    [SerializeField] private int targetScore;
    [SerializeField] private TargetType target;
    [SerializeField] private int moves;
    [SerializeField, Range(6,16)] private int width = 6;
    [SerializeField, Range(6,13)] private int height = 6;
    [SerializeField, Range(0.7f,1f)] private float itemSize;
    [SerializeField, Range(0.6f,1f)] private float cellSize;
    [SerializeField] private float cameraYOffset;

    public ItemSO[] Items => items;
    public bool IsRandomLevelFill => isRandomLevelFill;
    public int TargetScore => targetScore;
    public TargetType Target => target;
    public int Moves => moves;
    public int Width => width;
    public int Height => height;
    public float ItemSize => itemSize;
    public float CellSize => cellSize;
    public float CameraYOffset => cameraYOffset;
    public List<GridCell> gridCells;

    [System.Serializable]
    public class GridCell
    {
        public int X;
        public int Y;
        public ItemSO ItemSO;
        public int Index;
        public bool HasGlass;

        public GridCell(int x, int y, ItemSO itemSO, int index, bool hasGlass)
        {
            X = x;
            Y = y;
            ItemSO = itemSO;
            Index = index;
            HasGlass = hasGlass;
        }
        
    }
}
