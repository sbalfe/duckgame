namespace Server.Portals
{
    /* Connection status logic */
    public enum ConnectStatus
    {
        Undefined,
        Success,
        ServerFull,
        GameInProgress,
        LoggedInAgain,
        UserRequestedDisconnect,
        GenericDisconnect
    }
}