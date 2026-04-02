using Newtonsoft.Json;

namespace PiggyGame.Data.ValueObjects.CallbackData;

public class CallbackData
{
    public int Id { get; set; }
    public int Command { get; set; }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this);
    }
}