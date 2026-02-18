using System;

public interface IEventChannel : IDisposable
{
    void Update();
}
