using System;
using UnityEngine.Events;

public class EventUtility
{

	public static void SafeInvokeAction( Action action )
	{
		if( action != null )
			action.Invoke();
	}

	public static void SafeInvokeAction<T>( Action<T> action, T p1 )
	{
		if( action != null )
			action.Invoke( p1 );
	}

	public static void SafeInvokeAction<T, U>( Action<T, U> action, T p1, U p2 )
	{
		if( action != null )
			action.Invoke( p1, p2 );
	}

	public static void SafeInvokeAction<T, U, V>( Action<T, U, V> action, T p1, U p2, V p3 )
	{
		if( action != null )
			action.Invoke( p1, p2, p3 );
	}

	public static void SafeInvokeAction<T, U, V, W>( Action<T, U, V, W> action, T p1, U p2, V p3, W p4 )
	{
		if( action != null )
			action.Invoke( p1, p2, p3, p4 );
	}

	public static void SafeInvokeUnityEvent( UnityEvent uEvent )
	{
		if( uEvent != null )
			uEvent.Invoke();
	}

	public static void SafeInvokeUnityEvent<T>( UnityEvent<T> uEvent, T p1 )
	{
		if( uEvent != null )
			uEvent.Invoke( p1 );
	}

	public static void SafeInvokeUnityEvent<T, U>( UnityEvent<T, U> uEvent, T p1, U p2 )
	{
		if( uEvent != null )
			uEvent.Invoke( p1, p2 );
	}

	public static void SafeInvokeUnityEvent<T, U, V>( UnityEvent<T, U, V> uEvent, T p1, U p2, V p3 )
	{
		if( uEvent != null )
			uEvent.Invoke( p1, p2, p3 );
	}

	public static void SafeInvokeUnityEvent<T, U, V, W>( UnityEvent<T, U, V, W> uEvent, T p1, U p2, V p3, W p4 )
	{
		if( uEvent != null )
			uEvent.Invoke( p1, p2, p3, p4 );
	}
}