using System.Reflection;
using HarmonyLib;
using HoldfastGame;
using MelonLoader;

[assembly: MelonInfo(typeof(QuietAdmin.QuietAdminMod), "QuietAdmin", "1.0.0", "Ryannlt")]
[assembly: MelonGame("Anvil Game Studio", "Holdfast NaW")]

// Kept in step with MelonInfo above; release.ps1 reads this back off the built DLL.
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace QuietAdmin
{
    // Hides the chat lines the server posts on the admin channel - slays, revives, teleports, weapon grants -
    // which a server mod can generate hundreds of in one session.
    public class QuietAdminMod : MelonMod
    {
        internal static MelonPreferences_Entry<bool> BlockAdminChat;
        internal static MelonPreferences_Entry<bool> BlockNotifications;

        public override void OnInitializeMelon()
        {
            MelonPreferences_Category category = MelonPreferences.CreateCategory("QuietAdmin", "QuietAdmin");

            BlockAdminChat = category.CreateEntry(
                "BlockAdminChat", true,
                description: "Hide chat lines the server posts on the admin channel. Private messages are never affected.");

            BlockNotifications = category.CreateEntry(
                "BlockNotifications", false,
                description: "Also hide the centre-screen admin notification popups.");

            LoggerInstance.Msg($"Ready. BlockAdminChat={BlockAdminChat.Value} BlockNotifications={BlockNotifications.Value}");
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
            if (!QuietAdminMod.BlockAdminChat.Value) return false;

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
            if (!QuietAdminMod.BlockNotifications.Value) return true;

            // AdminPM mirrors the private-message exemption above.
            return notificationType == FancyNotificationType.AdminPM;
        }
    }
}
