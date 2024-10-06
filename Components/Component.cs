using Microsoft.Xna.Framework;

namespace PandaGameLibrary.Components;

public abstract class Component
{
	internal bool _startPending = true;

	public Guid ComponentId { get; set; }


	//public Guid gameobjectId { get; set; }

	public GameObject gameObject { get; set; }

	public string Tag { get; set; }

	public bool IsEnabled { get; set; } = true;


	//this constracter is working dont remove it
	public Component()
	{
		ComponentId = Guid.NewGuid();
	}

	public void ToggleEnable()
	{
		IsEnabled = !IsEnabled;
	}

	public virtual void Awake()
	{
		
	}

	public virtual void Start()
	{
	}

	public virtual void Update(GameTime gameTime)
	{
	}

	
}
