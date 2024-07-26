using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class Match3Visual : MonoBehaviour
{
    public event Action<State> StateChanged;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private SpriteRenderer itemprefab;
    [SerializeField] private SpriteRenderer glassPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AudioManager audioManager;

    private Grid2D<Match3.Cell> _grid;
    private Match3 _match3;
    private SpriteRenderer[,] _items;
    private State _state;
    private float _busyTimer;
    private Action _onBusyTimerElapsedAction;
    private Vector3 _startDragWorldPosition;
    private Vector3 _endDragWorldPosition;
    private int _startDragX;
    private int _startDragY;
    private float _cameraYOffset;
    private Vector2 ItemScale;

    public enum State {
        Busy,
        WaitingForUser,
        TryFindMatches,
        GameOver,
    }
    
    public void Initialize(float xCameraPos, float yCameraPos, Grid2D<Match3.Cell> grid, Match3 match3, float itemSize, float cameraYOffset)
    {
        _grid = grid;
        _match3 = match3;
        _cameraYOffset = cameraYOffset;

        ItemScale = new Vector2(itemSize, itemSize);

        _items = new SpriteRenderer[_grid.GetWidth(), _grid.GetHeight()];
        SetCameraPosition(xCameraPos, yCameraPos);
    }

    public void InitializeState() => SetState(State.WaitingForUser);

    public void ClearItems()
    {
        foreach(SpriteRenderer item in _items)
            Destroy(item.gameObject);
    }

    public void SetupItem(int x, int y)
    {
        
        _items[x, y] = Instantiate(itemprefab, _grid.GetCellCenterPosition(x,y), Quaternion.identity);
        _items[x, y].transform.SetParent(this.transform);
        _items[x, y].transform.localScale = ItemScale;
        _items[x, y].sprite = _grid.GridArray[x,y].GetItemSO().Sprite;
    }

    public void OnChangeItem(int x, int y)
    {
        _items[x, y].sprite = _grid.GridArray[x,y].GetItemSO().Sprite;
    }

    private void Update()
    {
        switch(_state)
        {
            case State.Busy:
                _busyTimer -= Time.deltaTime;
                if (_busyTimer <= 0f) {
                    _onBusyTimerElapsedAction();
                }
                break;

            case State.WaitingForUser:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    _startDragWorldPosition = worldPosition;

                    _grid.GetXY(worldPosition, out _startDragX, out _startDragY);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    _endDragWorldPosition = worldPosition;

                    float deltaX = _endDragWorldPosition.x - _startDragWorldPosition.x;
                    float deltaY = _endDragWorldPosition.y - _startDragWorldPosition.y;

                    if (Mathf.Abs(deltaX) < _grid.GetCellSize() / 4 && Mathf.Abs(deltaY) < _grid.GetCellSize() / 4)
                        break;

                    if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
                    {
                        if(deltaX > 0)
                            TrySwap(_startDragX, _startDragY, _startDragX + 1, _startDragY);
                        else
                            TrySwap(_startDragX, _startDragY, _startDragX - 1, _startDragY);
                    }
                    else
                    {
                        if(deltaY > 0)
                            TrySwap(_startDragX, _startDragY, _startDragX, _startDragY + 1);
                        else
                            TrySwap(_startDragX, _startDragY, _startDragX, _startDragY - 1);
                    }

                }
                break;
            case State.TryFindMatches:
                if (_match3.TryFindMatchesAndDestroyThem())
                {
                    audioManager.PlaySFX(audioManager.MatchSound);
                    PopItemsVisual();
                    SetBusyState(0.3f, ()=> {
                        _match3.FallItemsToEmptyCells();

                        SetBusyState(.3f, () => {
                            _match3.SpawnNewItems();
                            SetBusyState(.3f, () => SetState(State.TryFindMatches));
                        });
                    });
                }
                else
                {
                    CheckGameOverState();
                }
                break;
            case State.GameOver:

                break;
        }
    }

    private void CheckGameOverState()
    {
        switch (_match3.IsGameOver())
        {
            case Match3.GameOverState.gameContinue:
                SetState(State.WaitingForUser);
                break;

            case Match3.GameOverState.win:
                SetState(State.GameOver);
                break;

            case Match3.GameOverState.lose:
                SetState(State.GameOver);
                break;

        }
    }

    private void PopItemsVisual()
    {
        for (int x = 0; x < _grid.GetWidth(); x++)
        {
            for (int y = 0; y < _grid.GetHeight(); y++)
            {
                if (_grid.GridArray[x,y].IsEmpty)
                {
                    ParticleSystem popEffect = Instantiate(_grid.GridArray[x,y].GetItemSO().PopEffect, _grid.GetCellCenterPosition(x, y), Quaternion.identity);
                    popEffect.Play();
                    Destroy(popEffect.gameObject, 0.3f);
                    _items[x, y].enabled = false;
                }
            }
        }
    }

    public void FallItemVisual(int StartX, int StartY, int endX, int endY, float durarion)
    {
        _items[StartX, StartY].enabled = false;
        _items[endX, endY].enabled = false;

        SpriteRenderer sprite1 = Instantiate(itemprefab, _grid.GetCellCenterPosition(StartX, StartY), Quaternion.identity);
        sprite1.transform.localScale = ItemScale;
        sprite1.sprite = _grid.GridArray[StartX,StartY].GetItemSO().Sprite;

        sprite1.transform.DOMove(_grid.GetCellCenterPosition(endX,endY), durarion).OnComplete(() => {
            Destroy(sprite1.gameObject);
            _items[StartX, StartY].enabled = false;
            _items[endX, endY].enabled = true;

            _items[endX, endY].sprite = _grid.GridArray[endX,endY].GetItemSO().Sprite;
        });
    }

    public void SpawnNewItemVisual(int x, int y, float durarion)
    {
        Vector2 SpawnPosition = _grid.GetCellCenterPosition(x, _grid.GetHeight() -1);
        SpawnPosition.y += _grid.GetCellSize();

        SpriteRenderer sprite1 = Instantiate(itemprefab, SpawnPosition, Quaternion.identity);
        sprite1.transform.localScale = ItemScale;
        sprite1.sprite = _grid.GridArray[x, y].GetItemSO().Sprite;

        sprite1.transform.DOMove(_grid.GetCellCenterPosition(x, y), durarion).OnComplete(() => {
            Destroy(sprite1.gameObject);
            _items[x, y].enabled = true;
            _items[x, y].sprite = _grid.GridArray[x,y].GetItemSO().Sprite;
        });
    }

    public void SwapMoveVisual(int x1, int y1, int x2, int y2, float durarion)
    {
        _items[x1, y1].enabled = false;
        _items[x2, y2].enabled = false;

        SpriteRenderer sprite1 = Instantiate(itemprefab, _grid.GetCellCenterPosition(x1,y1), Quaternion.identity);
        sprite1.transform.localScale = ItemScale;
        sprite1.sprite = _grid.GridArray[x2,y2].GetItemSO().Sprite;

        SpriteRenderer sprite2 = Instantiate(itemprefab, _grid.GetCellCenterPosition(x2,y2), Quaternion.identity);
        sprite2.transform.localScale = ItemScale;
        sprite2.sprite = _grid.GridArray[x1,y1].GetItemSO().Sprite;

        sprite1.transform.DOMove(_grid.GetCellCenterPosition(x2,y2), durarion).OnComplete(() => {
            Destroy(sprite1.gameObject);
            _items[x1, y1].enabled = true;
            _items[x1, y1].sprite = _grid.GridArray[x1,y1].GetItemSO().Sprite;
        });
        sprite2.transform.DOMove(_grid.GetCellCenterPosition(x1,y1), durarion).OnComplete(() => {
            Destroy(sprite2.gameObject);
            _items[x2, y2].enabled = true;
            _items[x2, y2].sprite = _grid.GridArray[x2,y2].GetItemSO().Sprite;
        });
    }

    private void TrySwapVisual(int x1, int y1, int x2, int y2, float durarion)
    {
        _items[x1, y1].enabled = false;

        SpriteRenderer sprite1 = Instantiate(itemprefab, _grid.GetCellCenterPosition(x1,y1), Quaternion.identity);
        sprite1.transform.localScale = ItemScale;
        sprite1.sprite = _grid.GridArray[x1,y1].GetItemSO().Sprite;

        Vector2 position = _grid.GetCellCenterPosition(x1,y1);
        position.x = position.x + (x2 - x1) * _grid.GetCellSize() / 2;
        position.y = position.y + (y2 - y1) * _grid.GetCellSize() / 2;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(sprite1.transform.DOMove(position, durarion/2));
        sequence.Append(sprite1.transform.DOMove(_grid.GetCellCenterPosition(x1,y1), durarion/2).OnComplete(() => {
            
            Destroy(sprite1.gameObject);
            _items[x1, y1].enabled = true;
            _items[x1, y1].sprite = _grid.GridArray[x1,y1].GetItemSO().Sprite;
        }));
    }

    public void TrySwap(int startX, int startY, int endX, int endY)
    {
        if(_match3.IsValidPosition(startX, startY))
        {
            
            if(_match3.IsValidPosition(endX, endY))
            {
                _match3.Swap(_startDragX, _startDragY, endX, endY);
                if (_match3.HasMatch3Link())
                {
                    audioManager.PlaySFX(audioManager.SwapSound);
                    _match3.DoMove();
                    SwapMoveVisual(_startDragX, _startDragY, endX, endY, 0.3f);
                    SetBusyState(0.3f,() => SetState(State.TryFindMatches));
                }
                else
                {
                    audioManager.PlaySFX(audioManager.TrySwapSound);
                    _match3.Swap(_startDragX, _startDragY, endX, endY);
                    TrySwapVisual(_startDragX, _startDragY, endX, endY, 0.3f);
                    SetBusyState(0.3f,() => SetState(State.WaitingForUser));
                }
            }
            else
            {
                audioManager.PlaySFX(audioManager.TrySwapSound);
                TrySwapVisual(_startDragX, _startDragY, endX, endY, 0.3f);
                SetBusyState(0.3f,() => SetState(State.WaitingForUser));
            }
        }
    }

    public void SetGlass(int x, int y)
    {
        SpriteRenderer glass = Instantiate(glassPrefab, _grid.GetCellCenterPosition(x,y), Quaternion.identity);
        glass.transform.SetParent(this.transform);
        glass.transform.localScale = new Vector3(_grid.GetCellSize(), _grid.GetCellSize(), 1);
        _grid.GridArray[x,y].SetGlass(glass);
    }

    public void BotSwap(int startX, int startY, int endX, int endY)
    {
        _match3.Swap(startX, startY, endX, endY);

        SwapMoveVisual(startX, startY, endX, endY, 0.3f);

        SetBusyState(0.3f,() => SetState(State.TryFindMatches));
    }

    private void SetBusyState(float busyTimer, Action onBusyTimerElapsedAction) {
        SetState(State.Busy);
        _busyTimer = busyTimer;
        _onBusyTimerElapsedAction = onBusyTimerElapsedAction;
    }

    private void SetState(State state)
    {
        _state = state;
        StateChanged?.Invoke(_state);
    }

    private void SetCameraPosition(float xCameraPos, float yCameraPos)
    {
        cameraTransform.position = new Vector3(xCameraPos , yCameraPos + _cameraYOffset, cameraTransform.position.z);
    }


}
