namespace AntiNuke
{
    internal class Settings
    {
        public static string Token = ""; //token here, i recommend the owner account so there is no problem banning the nukers
        public static ulong serverId = 1; //serverid you want the bot to monitor
        public static bool checkChannelDeletions = true; //checks for channel deletions
        public static bool checkBannedUsers = true; //checks for users being banned
        public static bool checkKickedUsers = true; //checks for users being kicked
        public static bool checkRoleDeletions = true; //checks for roles being deleted
        public static int maxChannelDeletions = 2; //channel deletions before the user gets banned
        public static int maxBannedUsers = 2; //banned users before the user gets banned
        public static int maxKickedUsers = 2; //kicked users before the user gets banned
        public static int maxRoleDeletions = 2; //role deletions before the user gets banned
        public static int Timeframe = 3; //in seconds
    }
}