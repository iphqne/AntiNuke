using System;
using System.Threading;
using Discord.Gateway;
using Discord;
using System.Collections.Generic;

namespace AntiNuke
{
    internal class Program
    {
        public static Timer? Timer;
        public static Timer? Timer2;
        public static List<ulong> Channels = new List<ulong>();
        public static int deletedChannels = 0;
        public static int bannedUsers = 0;
        public static ulong Offender;
        public static DiscordSocketClient DiscordSocketClient = new DiscordSocketClient(new DiscordSocketConfig { ApiVersion = 9, Cache = false } );

        static void Main(string[] args)
        {
            try
            {
                DiscordSocketClient.Login(Settings.Token);
                Console.WriteLine($"Logged into {DiscordSocketClient.User}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error logging in\n{e}");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Timer = new Timer(TimerTick, null, 0, Settings.Timeframe * 1000);
            Timer2 = new Timer(Timer2Tick, null, 0, 1);

            DiscordSocketClient.OnChannelDeleted += DiscordSocketClient_OnChannelDeleted;
            DiscordSocketClient.OnUserBanned += DiscordSocketClient_OnUserBanned;

            try
            {
                DiscordSocketClient.GetGuild(Settings.serverId);
            }
            catch
            {
                Console.WriteLine("Error fetching guild");
                Console.ReadKey();
                Environment.Exit(0);
            }

            int count = 0;
            int refCount = DiscordSocketClient.GetGuildChannels(Settings.serverId).Count;

            foreach (var Channel in DiscordSocketClient.GetGuildChannels(Settings.serverId))
            {
                if (count == refCount) break;
                Channels.Add(Channel.Id);
                count = count + 1;
            }

            Console.WriteLine($"Monitoring {DiscordSocketClient.GetGuild(Settings.serverId).Name}");

            Thread.Sleep(-1);
        }

        public static void TimerTick(Object Object)
        {
            deletedChannels = 0;
            bannedUsers = 0;
        }

        public static void Timer2Tick(Object Object)
        {
            if (Settings.checkChannelDeletions)
            {
                if (deletedChannels >= Settings.maxChannels)
                {
                    punishmentHandler("channels", DiscordSocketClient);
                    deletedChannels = 0;
                }
            }

            if (Settings.checkBannedUsers)
            {
                if (bannedUsers >= Settings.maxBannedUsers)
                {
                    punishmentHandler("bans", DiscordSocketClient);
                    bannedUsers = 0;
                }
            }
        }

        public static AuditLogFilters AuditLogFilters = new AuditLogFilters() { Limit = 1 };

        public static void DiscordSocketClient_OnChannelDeleted(DiscordSocketClient DiscordSocketClient, ChannelEventArgs ChannelEventArgs)
        {
            if (Settings.checkChannelDeletions)
            {
                if (Channels.Contains(ChannelEventArgs.Channel.Id))
                {
                    deletedChannels = deletedChannels + 1;
                    foreach (var audit in DiscordSocketClient.GetAuditLog(Settings.serverId, AuditLogFilters))
                    {
                        if (audit.Type == AuditLogActionType.ChannelDelete && audit.TargetId == ChannelEventArgs.Channel.Id)
                        {
                            Offender = audit.ChangerId;
                        }
                    }
                }
            }
        }

        public static void DiscordSocketClient_OnUserBanned(DiscordSocketClient DiscordSocketClient, BanUpdateEventArgs BanUpdateEventArgs)
        {
            if (Settings.checkBannedUsers)
            {
                if (BanUpdateEventArgs.Guild.Id == Settings.serverId)
                {
                    bannedUsers = bannedUsers + 1;
                    foreach (var audit in DiscordSocketClient.GetAuditLog(Settings.serverId, AuditLogFilters))
                    {
                        if (audit.Type == AuditLogActionType.MemberBan && audit.ChangerId != DiscordSocketClient.User.Id)
                        {
                            Offender = audit.ChangerId;
                        }
                    }
                }
            }
        }

        public static void punishmentHandler(string type, DiscordSocketClient DiscordSocketClient)
        {
            if (type == "channels")
            {
                try
                {
                    if (deletedChannels > 0)
                    {
                        DiscordSocketClient.BanGuildMember(Settings.serverId, Offender, $"Detected by AntiNuke (deleting channels)");
                        Console.WriteLine($"Banned {DiscordSocketClient.GetUser(Offender).Username} for deleting channels");
                        deletedChannels = 0;
                    }
                }
                catch (Exception) { deletedChannels = 0; } //just going to assume there is no permissions to ban the user so leaving this like this
            }
            else if (type == "bans")
            {
                try
                {
                    if (bannedUsers > 0)
                    {
                        DiscordSocketClient.BanGuildMember(Settings.serverId, Offender, $"Detected by AntiNuke (banning users)");
                        Console.WriteLine($"Banned {DiscordSocketClient.GetUser(Offender).Username} for banning users");
                        bannedUsers = 0;
                    }
                }
                catch (Exception) { bannedUsers = 0; } //just going to assume there is no permissions to ban the user so leaving this like this
            }

            deletedChannels = 0;
            bannedUsers = 0;
        }
    }
}