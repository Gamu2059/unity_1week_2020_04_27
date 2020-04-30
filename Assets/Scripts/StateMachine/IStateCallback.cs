using System;

public interface IStateCallback<T>
{
    Action<T> ChangeStateAction { get; set; }
}
