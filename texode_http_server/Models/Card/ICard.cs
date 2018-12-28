using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using texode_http_server.Models.Card.CardImage;

namespace texode_http_server.Models.Card
{
    public interface ICard
    {
        String Title { get; set; }
        ICardImage Image { get; set; }
    }
}
