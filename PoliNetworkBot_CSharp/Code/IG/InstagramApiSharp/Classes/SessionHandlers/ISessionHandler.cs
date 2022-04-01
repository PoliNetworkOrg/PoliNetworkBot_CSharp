#region

using InstagramApiSharp.API;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;

#endregion

#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace InstagramApiSharp.Classes.SessionHandlers
{
    public interface ISessionHandler
    {
        InstaApi InstaApi { get; set; }
#if WINDOWS_UWP
        /// <summary>
        ///     File => Optional
        ///     <para>If you didn't set this, InstagramApiSharp will choose it automatically based on <see cref="InstaApi"/> username!</para>
        /// </summary>
        StorageFile File { get; set; }
#else

        /// <summary>
        ///     Path to file
        /// </summary>
        string FilePath { get; set; }

#endif

        /// <summary>
        ///     Load and Set StateData to InstaApi
        /// </summary>
        void Load();

        /// <summary>
        ///     Save current StateData from InstaApi
        /// </summary>
        void Save();
    }
}