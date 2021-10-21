using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Linq;

namespace PatMe
{
    class EmoteReaderChat : EmoteReader
    {
        private static readonly string[] patternPetEmote = { "gently pats you", "なでた", "streichelt dich sanft", "vous caresse" };

        public EmoteReaderChat()
        {
            Service.chatGui.ChatMessage += OnChatMessage;
        }

        public override void Dispose()
        {
            Service.chatGui.ChatMessage -= OnChatMessage;
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (message != null && type == XivChatType.StandardEmote)
            {
                // pet emote payloads:
                // - instigator player, raw text (name), unknown, raw text (emote text)
                // - instigator player, raw text (name), unknown, icon, raw text (emote text)
                if (message.Payloads.Count >= 4 &&
                    message.Payloads[message.Payloads.Count - 1].Type == PayloadType.RawText)
                {
                    var textPayload = (message.Payloads[message.Payloads.Count - 1] as TextPayload);
                    var textPayloadContent = textPayload?.Text;
                    var numPlayers = message.Payloads.Count(x => x.Type == PayloadType.Player);

                    if (!string.IsNullOrEmpty(textPayloadContent) && (numPlayers == 1))
                    {
                        foreach (var testStr in patternPetEmote)
                        {
                            if (textPayloadContent.Contains(testStr))
                            {
                                var instigatorPayload = message.Payloads.Find(x => x.Type == PayloadType.Player) as PlayerPayload;
                                var instigatorName = instigatorPayload.PlayerName;

                                OnPetEmote?.Invoke(instigatorName);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
