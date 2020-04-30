using UnityEngine;

/// <summary>
/// シングルトンパターンを実装するための基底クラス。
/// </summary>
public abstract class Singleton<T> where T : class, new()
{
	public static T Instance { get; private set; }

	protected Singleton() { }

	public static T Create()
	{
		if (Instance != null)
		{
			return Instance;
		}

		Instance = new T();
		return Instance;
	}

	public void OnFinalize()
	{
		if (Instance == null)
		{
			return;
		}

		if (Instance.Equals(this))
		{
			Instance = null;
		}
	}
}