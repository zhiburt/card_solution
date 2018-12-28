using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using texode_http_server.Models.Card.CardImage;

namespace texode_http_server.Models.Card
{
    public class Card : ICard
    {
        public string Title { get; set; }
        public ICardImage Image { get; set; }
    }
}
