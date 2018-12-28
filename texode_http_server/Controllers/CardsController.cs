using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using texode_http_server.CardStorage;
using texode_http_server.Models.Card;
using texode_http_server.Models.Card.CardImage;

namespace texode_http_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly ICardStorage _cardStorage;

        public CardsController(ICardStorage cardStorage)
        {
            this._cardStorage = cardStorage;
        }

        /// <summary>
        /// Http Get All Cards
        /// If In service 0 cards
        /// Return not null list cards (lenght == 0)
        /// </summary>
        /// <returns>card</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ICard>> Get()
        {
            var cards = _cardStorage.GetCards();
            return new JsonResult(cards, new JsonSerializerSettings(){Formatting = Formatting.Indented});
        }
        
        /// <summary>
        /// Add new Card
        /// There's param file
        /// </summary>
        /// <param name="title">title card</param>
        /// <param name="size">card image size</param>
        /// <returns>StatusCode</returns>
        [HttpPost]
        public StatusCodeResult Post([FromForm] string title, [FromForm] int size)
        {
            var image = Request?.Form?.Files?.FirstOrDefault();
            if (string.IsNullOrEmpty(title)) return BadRequest();
            if (image is null) return BadRequest();

            var card = new Card()
            {
                Image = new CardImage()
                {
                    ImageSource = GetImage(image),
                    Size = size
                },
                Title = title
            };

            bool status = _cardStorage.SaveCard(card);

            return Ok();
        }

        /// <summary>
        /// Update some card
        /// param file can be
        /// </summary>
        /// <param name="oldTitle">title of card in service</param>
        /// <param name="oldSize">image size of card in service</param>
        /// <param name="newTitle">title of new card in service</param>
        /// <param name="newSize">image size of new card in service</param>
        /// <returns>StatusCode</returns>
        [HttpPut]
        public StatusCodeResult Put([FromForm] string oldTitle, [FromForm] int oldSize, [FromForm]  string newTitle, [FromForm]  int newSize)
        {
            var image = Request.Form?.Files?.FirstOrDefault();
            if (string.IsNullOrEmpty(oldTitle)) return BadRequest();
            if (string.IsNullOrEmpty(newTitle)) return BadRequest();

            byte[] imageSource = image is null ? null : GetImage(image);
            var oldCard = new Card()
            {
                Image = new CardImage()
                {
                    Size = oldSize
                },
                Title = oldTitle
            };
            var newCard = new Card()
            {
                Image = new CardImage()
                {
                    ImageSource = imageSource,
                    Size = newSize
                },
                Title = newTitle
            };

            bool status = _cardStorage.UpdateCard(oldCard, newCard);

            return Ok();
        }

        /// <summary>
        /// Delete some card in service
        /// </summary>
        /// <param name="title">title of card in service</param>
        /// <param name="size">image size of card in service</param>
        /// <returns>JsonResult</returns>
        [HttpDelete("{title}")]
        public IActionResult Delete(string title, [FromForm] int size)
        {
            if (string.IsNullOrEmpty(title)) return BadRequest();

            var card = new Card()
            {
                Title = title,
                Image = new CardImage() { Size = size }
            };
            var amauntChanges = _cardStorage.RemoveCard(card);

            return new JsonResult(new {Changes = amauntChanges});
        }

        /// <summary>
        /// Get Image from request
        /// </summary>
        /// <param name="file">file in request</param>
        /// <returns>content file(image)</returns>
        private byte[] GetImage(IFormFile file)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Flush();

                return stream.ToArray();
            }
        }
    }
}
