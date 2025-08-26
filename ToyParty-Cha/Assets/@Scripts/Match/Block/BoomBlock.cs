using UnityEngine;

public abstract class BoomBlock : Block
{
    public enum BoomType { Rainbow, Rocket, }

    public BoomType Boom { get; protected set; }
}
