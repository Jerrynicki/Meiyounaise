using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace Meiyounaise.Modules
{
    public class MessageModule : BaseCommandModule
    {
        [Command("say"), RequireOwner, Hidden]
        public async Task Say(CommandContext ctx, [RemainingText] string text)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var guilds = new Dictionary<int, DiscordGuild>();
            var i = 1;
            foreach (var guild in ctx.Client.Guilds)
            {
                guilds.TryAdd(i, guild.Value);
                i++;
            }

            var g = await ctx.RespondAsync($"Found Guilds\n{string.Join("\n", guilds)}\nChoose one via the number!");
            var gResponse = await interactivity.WaitForMessageAsync(x => x.Author == ctx.User);
            if (gResponse == null)
            {
                await ctx.RespondAsync("I didn't get your choice!");
                return;
            }

            await g.DeleteAsync();
            await gResponse.Message.DeleteAsync();

            if (gResponse.Message.Content.ToLower() == "abort")
            {
                return;
            }

            var target = guilds[Convert.ToInt32(gResponse.Message.Content)];
            var channels = new Dictionary<int, DiscordChannel>();
            var channelNames = new List<string>();
            var j = 1;
            foreach (var channel in target.Channels)
            {
                if (channel.Value.Type != ChannelType.Text) continue;
                channels.TryAdd(j, channel.Value);
                channelNames.Add(j + " - " + channel.Value.Name);
                j++;
            }

            var c = await ctx.RespondAsync(
                $"Listing text channels in guild {target.Name}\n{string.Join("\n", channelNames)}\nChoose one via the number!");
            var cResponse = await interactivity.WaitForMessageAsync(x => x.Author == ctx.User);
            if (cResponse.Message.Content.ToLower() == "abort")
            {
                await c.DeleteAsync();
                await cResponse.Message.DeleteAsync();
                return;
            }

            var targetChannel = channels[Convert.ToInt32(cResponse.Message.Content)];
            await targetChannel.SendMessageAsync(text);
            await c.DeleteAsync();
            await cResponse.Message.DeleteAsync();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }

        [Command("purge"), Aliases("prune"), RequireUserPermissions(Permissions.ManageMessages),
         Description("Bulk delete messages")]
        public async Task Purge(CommandContext ctx, int amount,
            [Description("If provided will only delete messages from this user (Needs to be the user id)")]
            DiscordUser user)
        {
            await ctx.Message.DeleteAsync();
            IReadOnlyList<DiscordMessage> messagesToDelete;
            if (user == null)
            {
                messagesToDelete = await ctx.Channel.GetMessagesAsync(amount);
                await ctx.Channel.DeleteMessagesAsync(messagesToDelete);
                var toDel = await ctx.RespondAsync($"Deleted {messagesToDelete.Count} messages!");
                await Task.Delay(2500);
                await toDel.DeleteAsync();
            }
            else
            {   
                messagesToDelete = await ctx.Channel.GetMessagesAsync(amount);
                var filteredMessages = messagesToDelete.ToList();
                filteredMessages.RemoveAll(x => x.Author.Id != user.Id);
                await ctx.Channel.DeleteMessagesAsync(filteredMessages);
                var toDel = await ctx.RespondAsync($"Deleted {filteredMessages.Count} messages!");
                await Task.Delay(2500);
                await toDel.DeleteAsync();
            }
        }
    }
}
