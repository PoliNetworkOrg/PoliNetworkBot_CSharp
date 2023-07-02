#region

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class FileSerialization
{
    /// <summary>
    ///     Writes the given object instance to a binary file.
    ///     <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
    ///     <para>
    ///         To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be
    ///         applied to properties.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">The type of object being written to the binary file.</typeparam>
    /// <param name="filePath">The file path to write the object instance to.</param>
    /// <param name="objectToWrite">The object instance to write to the binary file.</param>
    /// <param name="append">
    ///     If false the file will be overwritten if it already exists. If true the contents will be appended
    ///     to the file.
    /// </param>
    public static Tuple<bool, Exception?> WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
    {
        Stream? stream = null;
        try
        {
            stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create);
            SerializeObjectToJsonFile(objectToWrite, ref stream);
            stream?.Close();
            return new Tuple<bool, Exception?>(true, null);
        }
        catch (Exception e)
        {
            try
            {
                stream?.Close();
            }
            catch
            {
                // ignored
            }

            return new Tuple<bool, Exception?>(false, e);
        }
    }

    /// <summary>
    ///     Reads an object instance from a binary file.
    /// </summary>
    /// <typeparam name="T">The type of object to read from the binary file.</typeparam>
    /// <param name="filePath">The file path to read the object instance from.</param>
    /// <returns>Returns a new instance of the object read from the binary file.</returns>
    public static T? ReadFromJsonFile<T>(string filePath)
    {
        try
        {
            var s = File.ReadAllText(filePath);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s);
        }
        catch
        {
            return default;
        }
    }

    internal static void SerializeObjectToJsonFile<T>(T objectToWrite, ref Stream? stream)
    {
        try
        {
            var s = JsonConvert.SerializeObject(objectToWrite);
            var buffer = Encoding.UTF8.GetBytes(s);
            stream?.Write(buffer);
        }
        catch
        {
            ;
        }
    }
}