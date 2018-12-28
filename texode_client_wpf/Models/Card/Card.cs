using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace texode_client_wpf.Models.Card
{
    /// <summary>
    /// Card is model
    /// Gets from service(service)
    /// </summary>
    public class Card : INotifyPropertyChanged, ICloneable
    {

        public Card(string title, Image image)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Image = image ?? throw new ArgumentNullException(nameof(image));
        }

        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));
            }
        }

        private Image image;
        public Image Image {
            get => image;
            set
            {
                image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Image"));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public object Clone()
        {
            return new Card(title, image);
        }
    }
}
