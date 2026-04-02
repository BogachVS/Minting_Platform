using Newtonsoft.Json;

namespace PiggyGame.Data.ValueObjects.InteractionsData;

public abstract class InteractionData
{
    public string Serialize()
    {
        return JsonConvert.SerializeObject(this);
    }
}
