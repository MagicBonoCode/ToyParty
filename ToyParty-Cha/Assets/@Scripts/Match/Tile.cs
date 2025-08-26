using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType { None, Normal, }
    public enum TileColor { Type_0, Type_1, }

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Color32 _colorType_0 = new Color32(10, 20, 65, 255);
    [SerializeField]
    private Color32 _colorType_1 = new Color32(20, 30, 65, 255);

    public TileType Type { get; set; } = TileType.Normal;
    public TileColor Color { get; private set; } = TileColor.Type_0;
    public bool IsSpawnPoint { get; private set; }

    public bool IsActive => Type != TileType.None;

    public void Init(TileType type, TileColor color, bool isSpawnPoint)
    {
        Type = type;
        Color = color;
        IsSpawnPoint = isSpawnPoint;

        if(Type == TileType.None)
        {
            _spriteRenderer.enabled = false;
            return;
        }

        switch(Color)
        {
            case TileColor.Type_0:
                _spriteRenderer.color = _colorType_0;
                break;
            case TileColor.Type_1:
                _spriteRenderer.color = _colorType_1;
                break;

            default:
                break;
        }
    }
}
