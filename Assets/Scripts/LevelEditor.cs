using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    [SerializeField] private LevelSO _level;
    [SerializeField] private SpriteRenderer _itemPrefab;
    [SerializeField] private SpriteRenderer _glassPrefab;
    [SerializeField] private bool _randomAutoFillLevel;
    [SerializeField] private Camera _mainCamera;

    private Grid2D<Match3.Cell> _grid;
    private SpriteRenderer[,] _items;
    private Vector3 _itemScale;
    
    private void Awake()
    {
        _items = new SpriteRenderer[_level.Width, _level.Height];
        _itemScale = new Vector3(_level.ItemSize, _level.ItemSize, 1);

        _grid = new Grid2D<Match3.Cell>(_level.Width, _level.Height, _level.CellSize, (int x, int y) => new Match3.Cell(x,y));

        Vector2 cameroPos = _grid.GetCellPosition(_level.Width, _level.Height);

        _mainCamera.transform.position = new Vector3(cameroPos.x / 2f, cameroPos.y / 2 + _level.CameraYOffset, _mainCamera.transform.position.z);

        if (_randomAutoFillLevel || _level.gridCells.Count == 0)
        {
            for (int x = 0; x < _level.Width; x++)
            {
                for (int y = 0; y < _level.Height; y++)
                {
                    foreach(var cell in _level.gridCells)
                    {
                        if (cell.X == x && cell.Y == y && cell.HasGlass)
                        {
                            SpriteRenderer glass = Instantiate(_glassPrefab, _grid.GetCellCenterPosition(x,y), Quaternion.identity);
                            glass.transform.SetParent(this.transform);
                            glass.transform.localScale = new Vector3(_level.CellSize, _level.CellSize, 1);
                            _grid.GridArray[x,y].SetGlass(glass);
                        }
                    }
                }
            }
            _level.gridCells = new List<LevelSO.GridCell>();

            for (int x = 0; x < _level.Width; x++)
            {
                for (int y = 0; y < _level.Height; y++)
                {
                    var item = GetRandomItem();
                    _grid.GridArray[x,y].SetItem(item.Item1, item.Item2);
                    _level.gridCells.Add(new LevelSO.GridCell(x, y,item.Item1, item.Item2, _grid.GridArray[x,y].HasGlass));

                    SetupItem(x, y);
                }
            } 
            VerifyGrid();
        }
        else
        {
            for (int x = 0; x < _level.Width; x++)
            {
                for (int y = 0; y < _level.Height; y++)
                {
                    foreach(var item in _level.gridCells)
                    {
                        if (item.X == x && item.Y == y)
                        {
                            _grid.GridArray[x,y].SetItem(item.ItemSO, item.Index);
                            SetupItem(x, y);

                            if (item.HasGlass)
                            {
                                SpriteRenderer glass = Instantiate(_glassPrefab, _grid.GetCellCenterPosition(x,y), Quaternion.identity);
                                glass.transform.SetParent(this.transform);
                                glass.transform.localScale = new Vector3(_level.CellSize, _level.CellSize, 1);
                                _grid.GridArray[x,y].SetGlass(glass);

                            }
                        }
                        
                    }
                }
            }
            VerifyGrid();
        }
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < _level.Width && y >= 0 && y < _level.Height;
    }  

    private void SetupItem(int x, int y)
    {
        _items[x, y] = Instantiate(_itemPrefab, _grid.GetCellCenterPosition(x,y), Quaternion.identity);
        _items[x, y].transform.SetParent(this.transform);
        _items[x, y].transform.localScale = _itemScale;
        _items[x, y].sprite = _grid.GridArray[x,y].GetItemSO().Sprite;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_level);
#endif
    }

    private void Update()
    {
        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        _grid.GetXY(worldPosition, out int x, out int y);

        if (IsValidPosition(x, y)) {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                _grid.GridArray[x,y].SetItem(_level.Items[0], 0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                _grid.GridArray[x,y].SetItem(_level.Items[1], 1);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                _grid.GridArray[x,y].SetItem(_level.Items[2], 2);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                _grid.GridArray[x,y].SetItem(_level.Items[3], 3);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                _grid.GridArray[x,y].SetItem(_level.Items[4], 4);

            if (Input.GetMouseButtonDown(1)) 
            {
                if (!_grid.GridArray[x,y].HasGlass)
                {
                    SpriteRenderer glass = Instantiate(_glassPrefab, _grid.GetCellCenterPosition(x,y), Quaternion.identity);
                    glass.transform.SetParent(this.transform);
                    glass.transform.localScale = new Vector3(_level.CellSize, _level.CellSize, 1);
                    _grid.GridArray[x,y].SetGlass(glass);
                }
                else
                {    
                    Destroy(_grid.GridArray[x,y].DestroyGlass());
                }
            }

            for (int i = 0; i < _level.gridCells.Count(); i++)
            {
                if (_level.gridCells[i].X == x && _level.gridCells[i].Y == y)
                    _level.gridCells[i] =  new LevelSO.GridCell(x, y,_grid.GridArray[x,y].GetItemSO(), _grid.GridArray[x,y].GetItemIndex(), _grid.GridArray[x,y].HasGlass);
            }

            _items[x, y].sprite = _grid.GridArray[x,y].GetItemSO().Sprite;
        }
    }

    private (ItemSO, int) GetRandomItem()
    {
        int index = UnityEngine.Random.Range(0, _level.Items.Length);
        return (_level.Items[index], index);
    }

    private void VerifyGrid()
    {
        for (int x = 0; x < _level.Width; x++)
        {
            for (int y = 0; y < _level.Height; y++)
            {
                int rightLine = 1;
                for (int i = x + 1; i < _level.Width; i++)
                {
                    if(IsValidPosition(i, y) && _grid.GridArray[x,y].GetItemSO().Type == _grid.GridArray[i, y].GetItemSO().Type)
                        rightLine++;
                    else    
                        break;

                    if (rightLine > 2)
                    {
                        ResetCell(x, y);
                        break;
                    }     
                }

                int upLine = 1;
                for (int i = y + 1; i < _level.Height; i++)
                {
                    if(IsValidPosition(x, i) && _grid.GridArray[x,y].GetItemSO().Type == _grid.GridArray[x, i].GetItemSO().Type)
                        upLine++;
                    else    
                        break;

                    if (upLine > 2)
                    {
                        ResetCell(x, y);
                        break;
                    }     
                }
            }
        }
    }

    private void ResetCell(int x, int y)
    {
        HashSet<int> set = new HashSet<int>();

        for (int i = 0; i < _level.Items.Length; i++)
            set.Add(i);

        int left = x - 1;
        if (IsValidPosition(left, y) && set.Contains(_grid.GridArray[left, y].GetItemIndex()))
            set.Remove(_grid.GridArray[left,y].GetItemIndex());
        
        int right = x + 1;
        if (IsValidPosition(right, y) && set.Contains(_grid.GridArray[right, y].GetItemIndex()))
            set.Remove(_grid.GridArray[right,y].GetItemIndex());

        int down = y - 1;
        if (IsValidPosition(x, down) && set.Contains(_grid.GridArray[x, down].GetItemIndex()))
            set.Remove(_grid.GridArray[x, down].GetItemIndex());
        
        int up = y + 1;
        if (IsValidPosition(x, up) && set.Contains(_grid.GridArray[x, up].GetItemIndex()))
            set.Remove(_grid.GridArray[x, up].GetItemIndex());

        int[] indexArray = set.ToArray();

        int itemIndex = UnityEngine.Random.Range(0, indexArray.Length);
        
        _grid.GridArray[x,y].SetItem(_level.Items[indexArray[itemIndex]], indexArray[itemIndex]);
        _items[x, y].sprite = _grid.GridArray[x,y].GetItemSO().Sprite;
    }
}
