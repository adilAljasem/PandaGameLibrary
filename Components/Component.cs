using Microsoft.Xna.Framework;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace PandaGameLibrary.Components;

public abstract class Component
{
	internal bool _startPending = true;
	public Guid ComponentId { get; private set; }
	public GameObject gameObject { get; set; }
	public string Tag { get; set; }
	public bool IsEnabled { get; set; } = true;
	public Action OnComponentDestroyEvent { get; set; }

	//this constracter is working dont remove it
	public Component()
	{
		ComponentId = Guid.NewGuid();
		OnComponentDestroyEvent += OnComponentDestroy;
	}
   
    public void ToggleEnable()
	{
		IsEnabled = !IsEnabled;
	}

	internal void Destroy()
	{
        //// Prevent circular destruction
        //var go = gameObject;
        //gameObject = null;  // Break the reference first

        OnComponentDestroyEvent?.Invoke();
	}

	protected virtual void OnComponentDestroy()
	{
	}

    /// <summary>
    /// First Method Get Called In Component is Awake
    /// </summary>
    public virtual void Awake()
	{
		
	}

    /// <summary>
    /// Start Method Get Called After Awake and before First Update
    /// </summary>
    public virtual void Start()
	{
	}
    /// <summary>
    /// Update Method Called EachFrame
    /// </summary>
    public virtual void Update(GameTime gameTime)
	{
	}

	
}
