using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace texode_http_server.Models.Card.CardImage
{
    public class CardImage : ICardImage
    {
        public string Png { get; set; }
        public int Size { get; set; }
        public byte[] ImageSource { get; set; }
    }
}
