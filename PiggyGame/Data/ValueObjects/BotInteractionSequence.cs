using PiggyGame.Common.Constants.TelegramBot;
using PiggyGame.Data.ValueObjects.InteractionsData;

namespace PiggyGame.Data.ValueObjects;

public class BotInteractionSequence
{
    public string Name { get; init; }
    public string Data { get; init; }

    private BotInteractionSequence()
    {
        Name = BotSequenceCommands.Empty;
        Data = string.Empty;
    }
    
    private BotInteractionSequence(string name, InteractionData data)
    {
        Name = name;
        Data = data.Serialize();
    }
    
    public static BotInteractionSequence Empty()
    {
        return new BotInteractionSequence(BotSequenceCommands.Empty, new EmptyData());
    }
}