namespace texode_http_server.Models.Card.CardImage
{
    public interface ICardImage
    {
        string Png { get; set; }
        int Size { get; set; }
        byte[] ImageSource { get; set; }
    }
}