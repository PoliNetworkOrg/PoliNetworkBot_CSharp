#region

using System.Threading.Tasks;

#endregion

namespace InstagramApiSharp.API.Processors;

public interface IPushProcessor
{
    /// <summary>
    ///     Registers application for push notifications
    /// </summary>
    /// <returns></returns>
    Task<bool> RegisterPush();
}