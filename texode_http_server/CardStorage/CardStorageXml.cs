using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using texode_http_server.Models.Card;
using texode_http_server.Models.Card.CardImage;

namespace texode_http_server.CardStorage
{
    public class CardStorageXml : ICardStorage
    {
        private readonly string filePath;

        /// <summary>
        /// save images in filePath/images
        /// </summary>
        /// <param name="filePath">filePath to folder</param>
        public CardStorageXml(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// Get cards from XmlFile
        /// </summary>
        /// <returns>Cards</returns>
        public IEnumerable<ICard> GetCards()
        {
            var cards = ReadCardsFromXml();
            foreach (var card in cards)
            {
                card.Image.ImageSource = GetImageSource(card.Image.Png + ".png");
            }

            return cards;
        }

        /// <summary>
        /// Remove card
        /// in XML and images
        /// </summary>
        /// <param name="card">Card</param>
        /// <returns>amaunt changes</returns>
        public int RemoveCard(ICard card)
        {
            int changes = 0;
            foreach (var rmCard in GetCardsByCard(card))
            {
                RemoveImageBy(rmCard);
                changes += RemoveCardFromXml(rmCard);
            }

            return changes;
        }

        /// <summary>
        /// SaveCard to XML
        /// and save image
        /// </summary>
        /// <param name="newCard"></param>
        /// <returns></returns>
        public bool SaveCard(ICard newCard)
        {
            var imageName = SaveImage(newCard.Image.ImageSource);
            newCard.Image.Png = imageName;
            TrySaveNewCard(newCard);

            return true;
        }

        /// <summary>
        /// Update card that the same like change card
        /// if newCard.Image == null don't change image
        /// </summary>
        /// <param name="old">old card that'll be changed</param>
        /// <param name="newCard">new card</param>
        /// <returns>status</returns>
        public bool UpdateCard(ICard old, ICard newCard)
        {
            var newImageName = newCard.Image?.ImageSource == null ? null : SaveImage(newCard.Image.ImageSource);
            foreach (var oldCard in GetCardsByCard(old))
            {
                if (newImageName == null)
                {
                    newCard.Image.Png = oldCard.Image.Png;
                }
                else
                {
                    newCard.Image.Png = newImageName;
                    RemoveImageBy(oldCard);             //might there's problem not troble but problem
                }
                UpdateCardInXml(oldCard, newCard);
            }

            return true;
        }
        /// <summary>
        /// Save image in path/images
        /// </summary>
        /// <param name="imageSource">image source</param>
        /// <returns>name file(image)</returns>
        protected string SaveImage(byte[] imageSource)
        {
            var hash = string.Format(@"{0}_{1}", DateTime.Now.Ticks, Guid.NewGuid()); ;    // may there'll be mistake here ?
            var nameImage = Path.GetFileName(hash);
            File.WriteAllBytes(GetPathToImages(nameImage + ".png"), imageSource);

            return nameImage;
        }

        /// <summary>
        /// Get cards From All cards
        /// When Title and Image.Size is Eaqaleted
        /// </summary>
        /// <param name="card">card</param>
        /// <returns>cards</returns>
        protected IEnumerable<ICard> GetCardsByCard(ICard card)
        {
            return GetCards()?.Where(c => c.Title == card.Title &&
                                          c.Image.Size == card.Image.Size);
        }

        #region private

        private IEnumerable<ICard> ReadCardsFromXml()
        {
            var cards = new List<ICard>();
            XDocument xdoc = XDocument.Load(GetPath());
            var xCards = from card in xdoc.Descendants().Elements("Card") select card;

            foreach (var xElement in xCards)
            {
                cards.Add(new Card()
                {
                    Title = xElement.Element("Title").Value,
                    Image = new CardImage()
                    {
                        Png = xElement.Element("Image").Value,
                        Size = int.Parse(xElement.Element("Image").Attribute("size").Value)
                    }
                });
            }

            return cards;
        }

        private int RemoveCardFromXml(ICard card)
        {
            XDocument xdoc = XDocument.Load(GetPath());
            var removeCards = xdoc.Descendants().Elements("Card").Where(xCard => xCard.Element("Title").Value == card.Title &&
                                                                                 xCard.Element("Image").Value == card.Image.Png &&
                                                                                 xCard.Element("Image").Attribute("size").Value ==
                                                                                    card.Image.Size.ToString());
            int amauntRemoves = removeCards.Count();
            removeCards.Remove();
            xdoc.Save(GetPath());

            return amauntRemoves;
        }

        private void RemoveImageBy(ICard card)
        {
            XDocument xdoc = XDocument.Load(GetPath());
            var removeCards = xdoc.Descendants().Elements("Card").Where(xCard => xCard.Element("Title").Value == card.Title &&
                                                                                 xCard.Element("Image").Value == card.Image.Png &&
                                                                                 xCard.Element("Image").Attribute("size").Value ==
                                                                                    card.Image.Size.ToString());

            string imageName = null;
            foreach (var rCard in removeCards)
            {
                imageName = rCard.Element("Image").Value;
                File.Delete(GetPathToImages(imageName + ".png"));
            }
        }

        private bool UpdateCardInXml(ICard oldCard, ICard newCard)
        {
            XDocument xdoc = XDocument.Load(GetPath());
            var updateCards = xdoc.Descendants().Elements("Card").Where(xCard => xCard.Element("Title").Value == oldCard.Title &&
                                                                                 xCard.Element("Image").Value == oldCard.Image.Png &&
                                                                                 xCard.Element("Image").Attribute("size").Value ==
                                                                                    oldCard.Image.Size.ToString());
            if (updateCards == null) return false;

            foreach (var updateCard in updateCards)
            {
                updateCard.Element("Image").Value = newCard.Image.Png;
                updateCard.Element("Image").Attribute("size").Value = newCard.Image.Size.ToString();
                updateCard.Element("Title").Value = newCard.Title;
            }
            xdoc.Save(GetPath());

            return true;
        }

        private void TrySaveNewCard(ICard newCard)
        {
            var card = new XElement("Card");
            card.Add(new XElement("Title", newCard.Title));
            var image = new XElement("Image", newCard.Image.Png);
            image.Add(new XAttribute("size", newCard.Image.Size));
            card.Add(image);

            XDocument xdoc = XDocument.Load(GetPath());
            xdoc.Element("Cards").Add(card);
            xdoc.Save(GetPath());
        }

        private byte[] GetImageSource(string nameImage)
        {
            return File.ReadAllBytes(GetPathToImages(nameImage));
        }

        private string GetPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), filePath);
        }

        private string GetPathToImages(string nameImage)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "cards", "images", nameImage);
        }


        #endregion
    }
}
