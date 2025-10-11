namespace SharedKernel;

public abstract class Entity
{
	public Guid Id { get; protected set; }
	public DateTime CreatedAt { get; protected set; }
	public DateTime? UpdatedAt { get; protected set; }
	public bool IsDeleted { get; protected set; }

	public virtual bool Delete()
	{
		if (!IsDeleted)
		{
			IsDeleted = true;
			return true;
		}

		return false;
	}

	public virtual void SetTimestamps(DateTime now)
	{
		if (CreatedAt == default)
			CreatedAt = now;
		else
			UpdatedAt = now;
	}

	public bool Equals(Entity entity) => entity.Id == Id;
}