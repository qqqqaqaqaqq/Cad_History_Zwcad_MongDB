using System.ComponentModel;
using System.Windows;

namespace CadEye_WebVersion.Models
{
    public class NaviTapHost : INotifyPropertyChanged
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        private string? _pageName;
        public string? PageName
        {
            get => _pageName;
            set
            {
                if (_pageName != value)
                {
                    _pageName = value;
                    OnPropertyChanged(nameof(PageName));
                }
            }
        }

        public int id { get; set; }
        public object? Page { get; set; }
        public Visibility PageVisibility { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
