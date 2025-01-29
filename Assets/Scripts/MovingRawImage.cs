using UnityEngine;
using UnityEngine.UI;

public class MovingRawImage : MonoBehaviour
{
    [SerializeField] private RawImage _image;
    [SerializeField, Range(0, 10)] private float _speed = 0.03f;
    [SerializeField, Range(-1, 1)] private float _xDirection = 1;
    [SerializeField, Range(-1, 1)] private float _yDirection = 1;

    private void Update()
    {
        _image.uvRect = new Rect(_image.uvRect.position + new Vector2(- _xDirection * _speed, _yDirection * _speed) * Time.deltaTime, _image.uvRect.size);
    }
}
