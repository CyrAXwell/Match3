using System;
using UnityEngine;

public class Grid2D<TGridObject>
{
    private int _width;
    private int _height;
    private float _cellSize;
    private TGridObject[,] _gridArray;

    public TGridObject[,] GridArray => _gridArray;

    public Grid2D(int width, int height, float cellSize, Func<int, int, TGridObject> createGridObject) 
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;

        _gridArray = new TGridObject[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _gridArray[x, y] = createGridObject(x, y);
            }
        }

        bool showDebug = true;
        if (showDebug)
            DisplayGridLines();
    }

    public int GetWidth() => _width;

    public int GetHeight() => _height;

    public float GetCellSize() => _cellSize;

    private void DisplayGridLines()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Debug.DrawLine(GetCellPosition(x, y), GetCellPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(GetCellPosition(x, y), GetCellPosition(x + 1, y), Color.white, 100f);
            }
        }

        Debug.DrawLine(GetCellPosition(0, _height), GetCellPosition(_width, _height), Color.white, 100f);
        Debug.DrawLine(GetCellPosition(_width, 0), GetCellPosition(_width, _height), Color.white, 100f);
    }


    public Vector2 GetCellPosition(int x, int y)
    {
        return new Vector2(x * _cellSize, y * _cellSize);
    }

    public Vector2 GetCellCenterPosition(int x, int y)
    {
        return new Vector2(x * _cellSize + _cellSize / 2, y * _cellSize + _cellSize / 2);
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x / _cellSize);
        y = Mathf.FloorToInt(worldPosition.y / _cellSize);
    }
}
