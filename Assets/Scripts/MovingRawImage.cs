using UnityEngine;
using UnityEngine.UI;

public class MovingRawImage : MonoBehaviour
{
    [SerializeField] private RawImage image;
    [SerializeField, Range(0, 10)] private float speed;
    [SerializeField, Range(-1, 1)] private float xDirection;
    [SerializeField, Range(-1, 1)] private float yDirection;
    //[SerializeField] private float image;

    private void Update()
    {
        image.uvRect = new Rect(image.uvRect.position + new Vector2(- xDirection * speed, yDirection * speed) * Time.deltaTime, image.uvRect.size);
    }
}
