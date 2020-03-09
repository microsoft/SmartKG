namespace SmartKG.KGBot.Data.Request
{
    public class BotRequestMessage
    {
        public string userId { get; set; }
        public string sessionId { get; set; }        
        public string query { get; set; }
    }
}
