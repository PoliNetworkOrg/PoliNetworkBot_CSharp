#region

using System;

#endregion

namespace InstagramApiSharp.Classes;

public interface IRequestDelay
{
    TimeSpan Value { get; }
    bool Exist { get; }

    void Enable();

    void Disable();
}