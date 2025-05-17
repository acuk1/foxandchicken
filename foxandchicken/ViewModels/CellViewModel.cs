using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace foxandchicken.ViewModels
{
    public class CellViewModel : INotifyPropertyChanged
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string DisplayText { get; set; }

        private Brush _background;
        public Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                OnPropertyChanged();
            }
        }

        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                _isHighlighted = value;
             
                if (_isHighlighted)
                {
                    Background = Brushes.LightBlue;
                }
                else
                {
                    if (DisplayText == "Л")
                        Background = Brushes.Orange;
                    else if (DisplayText == "К")
                        Background = Brushes.LightGray;
                    else
                        Background = Brushes.White;
                }
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}