using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ShowFullscreenAdv();
    
    public event Action OnSetLevel;
    public event Action OnScoreChange;
    public event Action OnMove;
    public event Action<bool> OnGameOver;

    [SerializeField] private Match3Visual match3Visual;
    [SerializeField] private LevelSO[] levels;
    [SerializeField] private AudioManager audioManager;

    private Grid2D<Cell> _grid;
    private LevelSO _levelSO;
    private PlayerData _playerData;
    private int _moves;
    private int _score = 0;
    private int _glassScore = 0;
    private int _targetGlassScore;

    public enum GameOverState
    {
        gameContinue,
        win,
        lose
    }

    public void Initialize(PlayerData playerData)
    {
        _playerData = playerData;
        
        _levelSO = levels[_playerData.GetCurrentLevel()];
        _moves = _levelSO.Moves;

        _grid = new Grid2D<Cell>(_levelSO.Width, _levelSO.Height, _levelSO.CellSize, (int x, int y) => new Cell(x,y));
        match3Visual.Initialize(_grid.GetCellPosition(_levelSO.Width, _levelSO.Height).x / 2f, _grid.GetCellPosition(_levelSO.Width, _levelSO.Height).y / 2 , _grid, this, _levelSO.ItemSize, _levelSO.CameraYOffset);

        FillGrid();
        VerifyGrid();

        match3Visual.InitializeState();
        OnSetLevel?.Invoke();
    }

    public LevelSO GetLevelSO() => _levelSO;

    public TargetType GetTarget() => _levelSO.Target;

    public int GetLevel() => _playerData.GetCurrentLevel();

    public int GetScore() => _levelSO.Target == TargetType.glass ? _glassScore : _score;
    
    public int GetTargetScore() => _levelSO.Target == TargetType.glass ? _targetGlassScore : _levelSO.TargetScore;

    public int GetMoves() => _moves;

    public void OnLoadLevelButton(bool isNextLevel)
    {
        if (isNextLevel && _playerData.GetCurrentLevel() < levels.Length - 1)
        {
            _playerData.UnlockNewLevel();
        }
        else if (_playerData.GetCurrentLevel() == levels.Length - 1)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            GameObject yandexSDK = GameObject.FindGameObjectWithTag("ySDK");
            if (yandexSDK != null)
                ShowFullscreenAdv();
#endif
        }     
    }

    private void FillGrid()
    {
        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                if (_levelSO.IsRandomLevelFill || _levelSO.gridCells.Count() == 0)
                {
                    var item = GetRandomItem();
                    _grid.GridArray[x,y].SetItem(item.Item1, item.Item2);
                }
                else
                {
                    FillGridFromLevelSO(x, y); 
                }
                match3Visual.SetupItem(x, y);

                foreach (var cell in _levelSO.gridCells)
                {
                    if (cell.X == x && cell.Y == y && cell.HasGlass)
                    {
                        _targetGlassScore ++;
                        match3Visual.SetGlass(x,y);
                    }
                }
            }
        } 
    }

    private void FillGridFromLevelSO(int x, int y)
    {
        foreach (var cell in _levelSO.gridCells)
        {
            if (cell.X == x && cell.Y == y)
            {
                _grid.GridArray[x,y].SetItem(cell.ItemSO, cell.Index);
            }
        }
    }

    private int GetRightLineMatchLenght(int x, int y)
    {
        int rightLine = 1;
        for (int i = x + 1; i < _levelSO.Width; i++)
        {
            if(IsValidPosition(i, y) && _grid.GridArray[x,y].GetItemSO().Type == _grid.GridArray[i, y].GetItemSO().Type)
                rightLine++;
            else    
                break;
        }

        return rightLine;
    }

    private int GetUpLineMatchLenght(int x, int y)
    {
        int upLine = 1;
        for (int i = y + 1; i < _levelSO.Height; i++)
        {
            if(IsValidPosition(x, i) && _grid.GridArray[x,y].GetItemSO().Type == _grid.GridArray[x, i].GetItemSO().Type)
                upLine++;
            else    
                break;
        }

        return upLine;
    }

    private void VerifyGrid()
    {
        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                if (GetRightLineMatchLenght(x, y) > 2 || GetUpLineMatchLenght(x, y) > 2)
                    ResetCell(x, y);
            }
        }
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < _levelSO.Width && y >= 0 && y < _levelSO.Height;
    }

    public GameOverState IsGameOver()
    {
        GameOverState gameOver = GameOverState.gameContinue;

        if ((_levelSO.Target != TargetType.glass && _score >= _levelSO.TargetScore) || (_levelSO.Target == TargetType.glass && _glassScore >= _targetGlassScore))
        {
            gameOver = GameOverState.win;  
            OnGameOver?.Invoke(true);
        }
        else if (_moves < 1 || !HasPossibleMove())
        {
            gameOver = GameOverState.lose;
            OnGameOver?.Invoke(false);
        }
        
        return gameOver;
    }

    public bool HasMatch3Link()
    {
        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                if (GetRightLineMatchLenght(x, y) > 2 || GetUpLineMatchLenght(x, y) > 2)
                    return true; 
            }
        }

        return false;
    }

    public void Swap(int startX, int startY, int endX, int endY)
    {   
        Item tempItem = new Item(_grid.GridArray[startX, startY].GetItem());

        _grid.GridArray[startX, startY].SwapItem(_grid.GridArray[endX, endY].GetItem());
        _grid.GridArray[endX, endY].SwapItem(tempItem);
    }

    public void DoMove()
    {
        _moves--;
        OnMove?.Invoke();
    }

    public bool TryFindMatchesAndDestroyThem()
    {
        HashSet<Cell> set = new HashSet<Cell>();

        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                int rightLine = GetRightLineMatchLenght(x, y);
                if(rightLine > 2)
                {  
                    _score += GetMatchPossibleScore(rightLine);
                    for (int i = x; i < x + rightLine; i++)
                    {
                        if (!set.Contains(_grid.GridArray[i,y]))
                            set.Add(_grid.GridArray[i,y]);
                        
                        if (_grid.GridArray[i,y].HasGlass)
                        {
                            Destroy(_grid.GridArray[i,y].DestroyGlass());
                            _glassScore ++;
                        }
                    }      
                }

                int upLine = GetUpLineMatchLenght(x, y);
                if (upLine > 2)
                {
                    _score += GetMatchPossibleScore(upLine);
                    for (int i = y; i < y + upLine; i++)
                    {
                        if (!set.Contains(_grid.GridArray[x,i]))
                            set.Add(_grid.GridArray[x,i]);

                        if (_grid.GridArray[x,i].HasGlass)
                        {
                            Destroy(_grid.GridArray[x,i].DestroyGlass());
                            _glassScore ++;
                        }
                    }   
                }
            }
        }
        bool hasMatches = set.Count > 0;

        foreach (Cell cell in set)
            cell.ClearItem();

        OnScoreChange?.Invoke();

        return hasMatches;
    }

    public void FallItemsToEmptyCells()
    {
        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                if (!_grid.GridArray[x, y].IsEmpty && IsValidPosition(x, y - 1) && _grid.GridArray[x, y - 1].IsEmpty)
                {
                    int emptyCells = 1;
                    while (IsValidPosition(x, y - emptyCells) && _grid.GridArray[x, y - emptyCells].IsEmpty)
                    {
                        emptyCells++;
                    }
                    emptyCells--;

                    _grid.GridArray[x, y - emptyCells].FillCell(_grid.GridArray[x, y].GetItem());
                    _grid.GridArray[x, y].ClearItem();

                    match3Visual.FallItemVisual(x, y, x, y - emptyCells, 0.3f);
                }
            }
        }
    }

    public void SpawnNewItems()
    {
        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                if (_grid.GridArray[x, y].IsEmpty)
                {
                    var item = GetRandomItem();
                    _grid.GridArray[x,y].SetItem(item.Item1, item.Item2);
                    
                    match3Visual.SpawnNewItemVisual(x, y, 0.3f);
                }
            }
        }
    }

    private (ItemSO, int) GetRandomItem()
    {
        int index = UnityEngine.Random.Range(0, _levelSO.Items.Length);
        return (_levelSO.Items[index], index);
    }

    private void ResetCell(int x, int y)
    {
        HashSet<int> set = new HashSet<int>();

        for (int i = 0; i < _levelSO.Items.Length; i++)
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
        
        _grid.GridArray[x,y].SetItem(_levelSO.Items[indexArray[itemIndex]], indexArray[itemIndex]);
        match3Visual.OnChangeItem(x, y);
    }

    private int GetMatchPossibleScore(int match)
    {
        int score = 0;
        switch (match)
        {
            case 3:
                score = 100;
                break;
            case 4:
                score = 150;
                break;
            case 5:
                score = 200;
                break;
            default:
                score = 250;
                break;
        }
        return score;
    }

    public int GetMatch3LinkScore()
    {
        int matchScore = 0;

        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                int rightLine = GetRightLineMatchLenght(x, y);
                if (rightLine > 2)
                    matchScore += GetMatchPossibleScore(rightLine);

                int upLine = GetUpLineMatchLenght(x, y);
                if (upLine > 2)
                    matchScore += GetMatchPossibleScore(upLine);
            }
        }
        return matchScore;
    }

    public bool HasPossibleMove()
    {
        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                List<PossibleMove> moves = new List<PossibleMove>();
                moves.Add(new PossibleMove(x, y, x + 1, y));
                moves.Add(new PossibleMove(x, y, x - 1, y));
                moves.Add(new PossibleMove(x, y, x, y + 1));
                moves.Add(new PossibleMove(x, y, x, y - 1));

                foreach(PossibleMove move in moves)
                {
                    if(IsValidPosition(move.EndX, move.EndY))
                    {
                        Swap(move.StartX, move.StartY, move.EndX, move.EndY);

                        if (GetMatch3LinkScore() > 0)
                        {
                            Swap(move.StartX, move.StartY, move.EndX, move.EndY);
                            return true;
                        }

                        Swap(move.StartX, move.StartY, move.EndX, move.EndY);
                    }
                }  
            }
        }
        return false;
    }

    public int GetMatch3LinkGlassScore()
    {
        int glassScore = 0;
        HashSet<Cell> set = new HashSet<Cell>();

        for (int x = 0; x < _levelSO.Width; x++)
        {
            for (int y = 0; y < _levelSO.Height; y++)
            {
                int rightLine = GetRightLineMatchLenght(x, y);
                if (rightLine > 2)
                {
                    for (int i = x; i < x + rightLine; i++)
                    {          
                        if (_grid.GridArray[i,y].HasGlass && !set.Contains(_grid.GridArray[i,y]))
                            glassScore++;  
                    }  
                }

                int upLine = GetUpLineMatchLenght(x, y);
                if (upLine > 2)
                {
                    for (int i = y; i < y + upLine; i++)
                    {          
                        if (_grid.GridArray[x,i].HasGlass && !set.Contains(_grid.GridArray[x,i]))
                            glassScore++;  
                    }  
                }    
            }
        }
        return glassScore;
    }

    public class PossibleMove
    {
        public int StartX;
        public int StartY;
        public int EndX;
        public int EndY;

        public PossibleMove(int startX, int startY, int endX, int endY) {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
        }
    }

    public class Cell
    {
        private int _x;
        private int _y;
        private Item _item;
        private bool _isEmpty;
        private bool _hasGlass;
        private SpriteRenderer _glass;

        public bool IsEmpty => _isEmpty;
        public bool HasGlass => _hasGlass;

        public Cell (int x, int y)
        {
            _x = x;
            _y = y;
        }

        public void SetItem(ItemSO item, int index)
        {
            _item = new Item(item, index);
            _isEmpty = false;
        }

        public void SetGlass(SpriteRenderer glass)
        {
            _glass = glass;
            _hasGlass = true;
        }

        public ItemSO GetItemSO() => _item.ItemSO;

        public int GetItemIndex() => _item.Index;

        public Item GetItem() => _item;

        public void SwapItem(Item item) => _item = item;

        public void ClearItem() => _isEmpty = true;

        public void FillCell(Item item)
        {
            _item = item;
            _isEmpty = false;
        }

        public SpriteRenderer DestroyGlass()
        {
            _hasGlass = false;
            return _glass;
        }
    }

    public class Item
    {
        private ItemSO _itemSO;
        private int _itemIndex;

        public ItemSO ItemSO => _itemSO;
        public int Index => _itemIndex;

        public Item(ItemSO item, int index)
        {
            _itemSO = item;
            _itemIndex = index;
        }

        public Item(Item item)
        {
            _itemSO = item.ItemSO;
            _itemIndex = item.Index;
        }
    }
}
