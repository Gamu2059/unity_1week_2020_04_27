using UnityEngine;

public class GlobalSingletonMonoBehavior<T> : SingletonMonoBehavior<T> where T : MonoBehaviour
{
	protected override void OnAwake()
	{
		DontDestroyOnLoad( gameObject );
	}
}