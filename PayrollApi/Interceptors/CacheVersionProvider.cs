using System.Collections.Concurrent;

namespace PayrollApi.Interceptors;

public interface ICacheVersionProvider
{
	int GetVersion(Type type);
	void Bump(Type type);
}

public class CacheVersionProvider : ICacheVersionProvider
{
	private readonly ConcurrentDictionary<string, int> _versions = new();

	public int GetVersion(Type type)
	{
		var key = type.FullName ?? type.Name;
		return _versions.GetOrAdd(key, 1);
	}

	public void Bump(Type type)
	{
		var key = type.FullName ?? type.Name;
		_ = _versions.AddOrUpdate(key, 2, (_, v) => v + 1);
	}
}

