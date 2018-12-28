using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using texode_client_wpf.Commands;
using texode_client_wpf.Models.Card;
using texode_client_wpf.Models.CardStorage;

namespace texode_client_wpf.ViewModels.CardViewModel
{
    /// <summary>
    /// CardsViewModel model for Cards
    /// </summary>
    public class CardsViewModel : INotifyPropertyChanged
    {
        private readonly CardStorage cardStorage;
        private Card selectedCardWithautChanges;
        private Card selectedCard;

        #region ctors

        public CardsViewModel()
        {
            cardStorage = new CardStorage();
            PublicCards = cardStorage.PublicCards;
            //cardStorage.PropertyChanged += (sender, args) =>
            //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PublicCards"));

            AddCard = new RelayCommand(async (strTitle) =>
            {
                var file = GetFileBodyCreate();
                if (file == null) return;
                await cardStorage.AddValueAsync((string)strTitle, file);
            });

            RemoveCard = new ActionCommand(async () =>
            {
                if (SelectedCard == null) return;

                await cardStorage.RemoveCard(SelectedCard);
                SelectedCard = null;
            });

            UpdateCard = new ActionCommand(async () =>
            {
                if (SelectedCard == null) return;
                var image = GetFileBodyUpdate();

                await cardStorage.UpdateCard(selectedCardWithautChanges, SelectedCard, image);
                SelectedCard = null;
            });

            SortCards = new ActionCommand(() =>
            {
                cardStorage.Order();
            });
        }

        #endregion

        #region props

        public ReadOnlyObservableCollection<Card> PublicCards { get; }

        public Card SelectedCard
        {
            get => selectedCard;
            set
            {
                selectedCard = value;
                selectedCardWithautChanges = (Card)selectedCard?.Clone();
                OnSelectedBookChanged("SelectedCard");
            }
        }

        public ICommand AddCard { get; }
        public ICommand RemoveCard { get; }
        public ICommand UpdateCard { get; }
        public ICommand SortCards { get; }

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        void OnSelectedBookChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #region private 

        private byte[] GetFileBodyCreate()
        {
            if (MainWindow.NewFileContent == null)
                return null;

            var filename = MainWindow.NewFileContent;
            MainWindow.NewFileContent = null;

            return File.ReadAllBytes(filename);
        }

        private byte[] GetFileBodyUpdate()
        {
            if (MainWindow.NewFileContentForUpdate == null)
                return null;

            var filename = MainWindow.NewFileContentForUpdate;
            MainWindow.NewFileContentForUpdate = null;

            return File.ReadAllBytes(filename);
        }

        #endregion
    }
}