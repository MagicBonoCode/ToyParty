using UnityEngine;
using UnityEngine.SceneManagement;

public class Managers : Singleton<Managers>
{
    #region Core

    private EventManager _event = new EventManager();
    public static EventManager Event { get { return Instance?._event; } }

    private PoolManager _pool = new PoolManager();
    public static PoolManager Pool { get { return Instance?._pool; } }

    private ResourceManager _resource = new ResourceManager();
    public static ResourceManager Resource { get { return Instance?._resource; } }

    private SceneManagerEx _scene = new SceneManagerEx();
    public static SceneManagerEx Scene { get { return Instance?._scene; } }

    private SoundManager _sound = new SoundManager();
    public static SoundManager Sound { get { return Instance?._sound; } }

    private UIManager _ui = new UIManager();
    public static UIManager UI { get { return Instance?._ui; } }

    #endregion

    #region Game

    private MatchManager _match = new MatchManager();
    public static MatchManager Match { get { return Instance?._match; } }

    private ObjectManager _object = new ObjectManager();
    public static ObjectManager Object { get { return Instance?._object; } }

    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    public static void Clear()
    { 
    
    }
}
