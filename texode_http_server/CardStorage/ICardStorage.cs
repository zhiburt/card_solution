using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using texode_http_server.Models.Card;

namespace texode_http_server.CardStorage
{
    public interface ICardStorage
    {
        IEnumerable<ICard> GetCards();
        bool SaveCard(ICard newCard);
        bool UpdateCard(ICard oldCard, ICard newCard);
        int RemoveCard(ICard card);
    } 
}
