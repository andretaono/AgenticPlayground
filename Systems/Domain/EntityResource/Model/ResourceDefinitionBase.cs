using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.EntityResource.Model;

public abstract class ResourceDefinitionBase : IResourceDefinition
{
	private bool _isRegistered;
	private float _currentAmount;

	protected ResourceDefinitionBase(
		Type resourceType,
		ResourceId resourceId,
		string name,
		float maximumAmount,
		float regenerationRate,
		float depletionRate,
		float initialAmount)
	{
		ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
		ResourceId = resourceId;
		Name = name;
		MaximumAmount = maximumAmount;
		RegenerationRate = regenerationRate;
		DepletionRate = depletionRate;
		InitialAmount = initialAmount;
		_currentAmount = initialAmount;
	}

	public Type ResourceType { get; }
	public ResourceId ResourceId { get; }
	public string Name { get; }
	public float MaximumAmount { get; }
	public float RegenerationRate { get; }
	public float DepletionRate { get; }
	public float InitialAmount { get; }

	public float CurrentAmount
	{
		get
		{
			EnsureRegistered();
			return _currentAmount;
		}
	}

	public bool IsDepleted
	{
		get
		{
			EnsureRegistered();
			return _currentAmount <= 0f;
		}
	}

	public bool IsFull
	{
		get
		{
			EnsureRegistered();
			return _currentAmount >= MaximumAmount;
		}
	}

	internal EntityId Owner { get; private set; }

	internal void Register(EntityId entityId)
	{
		if (_isRegistered)
			throw new InvalidOperationException($"Resource '{ResourceId}' is already registered.");

		Owner = entityId;
		_currentAmount = InitialAmount;
		_isRegistered = true;
	}

	internal void Unregister() => _isRegistered = false;

	internal void AdvanceSimulation(float deltaTime)
	{
		EnsureRegistered();
		var netChange = (RegenerationRate - DepletionRate) * deltaTime;
		_currentAmount = Math.Clamp(_currentAmount + netChange, 0f, MaximumAmount);
	}

	public void Increase(float amount)
	{
		if (amount < 0f)
			throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

		EnsureRegistered();
		_currentAmount = Math.Min(_currentAmount + amount, MaximumAmount);
	}

	public void Decrease(float amount)
	{
		if (amount < 0f)
			throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

		EnsureRegistered();
		_currentAmount = Math.Max(_currentAmount - amount, 0f);
	}

	public void Set(float amount)
	{
		EnsureRegistered();
		_currentAmount = Math.Clamp(amount, 0f, MaximumAmount);
	}

	public ResourceSnapshot GetSnapshot()
	{
		EnsureRegistered();
		return new ResourceSnapshot(
			ResourceId,
			Name,
			_currentAmount,
			MaximumAmount,
			RegenerationRate,
			DepletionRate);
	}

	private void EnsureRegistered()
	{
		if (!_isRegistered)
			throw new InvalidOperationException($"Resource '{ResourceId}' is not registered.");
	}
}
