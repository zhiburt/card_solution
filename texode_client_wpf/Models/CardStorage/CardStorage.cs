using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using texode_client_wpf.Models.Card;

namespace texode_client_wpf.Models.CardStorage
{
    //todo may create method Cards or smth. else for only update server and change smth
    /// <summary>
    /// Card Storage is Wrap to Service
    /// </summary>
    public class CardStorage : INotifyPropertyChanged
    {
        private const string RequestUri = @"https://localhost:5001/api/cards";
        private HttpClient httpClient = new HttpClient();

        private readonly ObservableCollection<Card.Card> _bookCards = new ObservableCollection<Card.Card>();

        public readonly ReadOnlyObservableCollection<Card.Card> PublicCards;
        
        public CardStorage()
        {
            UpdateCards();
            PublicCards = new ReadOnlyObservableCollection<Card.Card>(_bookCards);
        }

        public void UpdateCards()
        {
            _bookCards.Clear();
            foreach (var card in GetFromServiceCards())
            {
                AddValue(card);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PublicCards"));
        }

        public async Task AddValueAsync(string title, byte[] source)
        {
            //_bookCards.Add(new Card.Card(title, GetImageFromSource(source))); // there is
            await SendImageOnServicAsync(title, source);
            UpdateCards();
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AddValue"));
        }

        public async Task RemoveCard(Card.Card card)
        {
            await RemoveCardInServiceAsync(card);
            UpdateCards();
        }

        public void AddValue(Card.Card card)
        {
            _bookCards.Add(card);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AddValue"));
        }

        public async Task UpdateCard(Card.Card oldCard, Card.Card newCard, byte[] image = null)
        {
            if (image != null) newCard.Image = GetImageFromSource(image);

            await UpdateCardInServiceAsync(oldCard, newCard);
            UpdateCards();
        }

        public void Order()
        {
            var sortableCollection = _bookCards.OrderBy(c => c.Title).ToList();

            for (int i = 0; i < sortableCollection.Count; i++)
            {
                _bookCards.Move(_bookCards.IndexOf(sortableCollection[i]), i);
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Private

        private IEnumerable<Card.Card> GetFromServiceCards()
        {
            var resp = httpClient.GetAsync(RequestUri).Result;
            var body = resp.Content.ReadAsStreamAsync().Result;
            var cards = ParsFromJson(body);

            return cards;
        }

        private IEnumerable<Card.Card> ParsFromJson(Stream body)
        {
            //JsonConvert.DeserializeObject<IEnumerable<Card.Card>>(Encoding.UTF8.GetString(body));
            List<Card.Card> cards = new List<Card.Card>();
            using (TextReader textReader = new StreamReader(body))
            using (JsonTextReader jsonReader = new JsonTextReader(textReader))
            {
                var jArray = JArray.Load(jsonReader);
                foreach (var jCard in jArray)
                {
                    string titleCard = (string)jCard["Title"];
                    byte[] imageSource = (byte[])jCard["Image"]["ImageSource"];

                    cards.Add(new Card.Card(titleCard, GetImageFromSource(imageSource)));
                }
            }

            return cards;
        }

        private static Image GetImageFromSource(byte[] bytes)
        {
            BitmapImage bi = new BitmapImage();

            bi.BeginInit();
            bi.StreamSource = new MemoryStream(bytes);
            bi.EndInit();

            Image img = new Image();
            img.Source = bi;
            return img;
        }

        private async Task SendImageOnServicAsync(string title, byte[] source)
        {
            using (var content = new MultipartFormDataContent())
            {
                var size = GetSizeBySource(ImageToByteArray(GetImageFromSource(source))); // for syncrohnus values size in Delete context

                content.Add(new StringContent(title),
                    String.Format("\"{0}\"", "title"));
                content.Add(new StringContent(size.ToString()),
                    String.Format("\"{0}\"", "size"));

                content.Add(new StreamContent(new MemoryStream(source)), "image", "file.png");

                await httpClient.PostAsync(RequestUri, content);
            }
        }
        
        private async Task RemoveCardInServiceAsync(Card.Card card)
        {
            using (var content = new MultipartFormDataContent())
            using (var req = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{ RequestUri }/{card.Title}")
            })
            {
                int size = GetSizeBySource(ImageToByteArray(card.Image));
                content.Add(new StringContent(size.ToString()),
                            String.Format("\"{0}\"", "size"));
                req.Content = content;

                await httpClient.SendAsync(req);
            }
        }

        private async Task UpdateCardInServiceAsync(Card.Card oldCard, Card.Card newCard)
        {
            using (var content = new MultipartFormDataContent())
            {
                int sizeNewImage = GetSizeBySource(ImageToByteArray(newCard.Image));
                int sizeOldImage = GetSizeBySource(ImageToByteArray(oldCard.Image));

                content.Add(new StringContent(sizeNewImage.ToString()),
                            String.Format("\"{0}\"", "newSize"));
                content.Add(new StringContent(sizeOldImage.ToString()),
                            String.Format("\"{0}\"", "oldSize"));
                content.Add(new StringContent(newCard.Title.ToString()), "newTitle");
                content.Add(new StringContent(oldCard.Title.ToString()),"oldTitle");

                if (newCard.Image != null)
                    content.Add(new StreamContent(new MemoryStream(ImageToByteArray(newCard.Image))), "image", "file.png");


                await httpClient.PutAsync(RequestUri, content);
            }
        }

        private int GetSizeBySource(byte[] source)
        {
            return source.Length;
        }

        private byte[] ImageToByteArray(Image imageIn)
        {
            var bitmap = imageIn.Source as BitmapImage;
            MemoryStream memStream = new MemoryStream();
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(memStream);
            memStream.Flush();
            return memStream.ToArray();
        }

        #endregion
    }
}
