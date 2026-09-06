using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using HoldfastGame;
using UnityEngine;

// Kept in step with BepInPlugin below; package.ps1 reads this back off the built DLL.
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.1.0")]

namespace ServerSilencer
{
    // Hides the chat lines the server posts on the admin channel - slays, revives, teleports, weapon grants -
    // which a server mod can generate hundreds of in one session.
    [BepInPlugin(Guid, "ServerSilencer", "1.0.1")]
    public class ServerSilencerMod : BaseUnityPlugin
    {
        // Also names the config file, BepInEx/config/com.ryannlt.serversilencer.cfg.
        public const string Guid = "com.ryannlt.serversilencer";

        internal static ConfigEntry<bool> BlockAdminChat;
        internal static ConfigEntry<bool> BlockNotifications;

        private DateTime _stamp;
        private float _nextCheck;

        private void Awake()
        {
            BlockAdminChat = Config.Bind("General", "BlockAdminChat", true,
                "Hide chat lines the server posts on the admin channel. Private messages are never affected.");
            BlockNotifications = Config.Bind("General", "BlockNotifications", false,
                "Also hide the centre-screen admin notification popups.");

            _stamp = Stamp();

            // BepInEx applies no Harmony patches itself, and without this the mod loads cleanly and does nothing.
            Harmony.CreateAndPatchAll(typeof(ServerSilencerMod).Assembly, Guid);

            Logger.LogInfo($"Ready. BlockAdminChat={BlockAdminChat.Value} BlockNotifications={BlockNotifications.Value}");
        }

        // BepInEx never watches its own cfg, so an external edit only lands if the mod goes looking for it.
        private void Update()
        {
            if (Time.unscaledTime < _nextCheck) return;
            _nextCheck = Time.unscaledTime + 1f;

            DateTime stamp = Stamp();
            if (stamp == _stamp) return;

            _stamp = stamp;
            Config.Reload();
        }

        private DateTime Stamp()
        {
            try
            {
                return File.GetLastWriteTimeUtc(Config.ConfigFilePath);
            }
            catch (Exception)
            {
                // A locked or half-written file just means no reload this second.
                return _stamp;
            }
        }
    }

    // Every chat route funnels through this one overload: the string overload delegates to it, and so does the
    // bcm RPC that receives server messages. Argument types are spelled out because the name is overloaded.
    [HarmonyPatch(typeof(ClientChatHandler), nameof(ClientChatHandler.AddChatEntry),
        new[] { typeof(int), typeof(string), typeof(TextChatEntryType), typeof(TextChatChannel), typeof(string) })]
    internal static class ChatEntryPatch
    {
        private static bool Prefix(int playerID, TextChatEntryType entryType, TextChatChannel channel)
        {
            return !ShouldHide(playerID, entryType, channel);
        }

        private static bool ShouldHide(int playerID, TextChatEntryType entryType, TextChatChannel channel)
        {
            if (!ServerSilencerMod.BlockAdminChat.Value) return false;

            // Private messages are always let through: the server mods answer commands on this channel.
            if (entryType == TextChatEntryType.PrivateMessage || entryType == TextChatEntryType.PrivateMessageAdmin)
                return false;

            // Both tests describe the same thing. bcm sets AdminAction exactly when the message arrives on the
            // admin channel with no sender, and playerID -1 means the server rather than a person.
            return entryType == TextChatEntryType.AdminAction
                   || (channel == TextChatChannel.Admin && playerID == -1);
        }
    }

    // The popups are a separate surface from the chat pane, so they get their own switch, off by default.
    [HarmonyPatch(typeof(AdminFancyUINotificationPanel), nameof(AdminFancyUINotificationPanel.ShowAdminNotification))]
    internal static class AdminNotificationPatch
    {
        private static bool Prefix(FancyNotificationType notificationType)
        {
            if (!ServerSilencerMod.BlockNotifications.Value) return true;

            // AdminPM mirrors the private-message exemption above.
            return notificationType == FancyNotificationType.AdminPM;
        }
    }
}
