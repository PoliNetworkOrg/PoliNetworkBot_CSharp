#region

using PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;
using System;
using System.Linq;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

[Serializable]
public class Conversation
{
    private string course;
    private string path;
    private string school;

    private UserState? state;

    public Conversation()
    {
        state = UserState.START;
    }

    public void SetState(UserState? var)
    {
        state = var;
    }

    public UserState? GetState()
    {
        return state;
    }

    internal void SetCourse(string courseToSet)
    {
        course = courseToSet;
    }

    internal void SetSchool(string schoolToSet)
    {
        school = schoolToSet;
    }

    internal string GetSchool()
    {
        return school;
    }

    internal string GetCourse()
    {
        return course;
    }

    internal void PathDroppedOneLevel(string droppedTo)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = droppedTo;
            return;
        }

        path += "/" + droppedTo;
    }

    internal void ResetPath()
    {
        path = null;
    }

    internal string GetPath()
    {
        return path;
    }

    internal string GetGit()
    {
        return GetPath().Split(@"/").First();
    }
}