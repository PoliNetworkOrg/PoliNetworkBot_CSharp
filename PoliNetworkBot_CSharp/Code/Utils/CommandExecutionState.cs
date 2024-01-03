namespace PoliNetworkBot_CSharp.Code.Utils;

public enum CommandExecutionState
{
    SUCCESSFUL,
    NOT_TRIGGERED,
    UNMET_CONDITIONS,
    INSUFFICIENT_PERMISSIONS,
    ERROR_NOT_ENABLED,
    ERROR_DEFAULT
}