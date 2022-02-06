using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZeiControl.Core;

namespace ZeiControl.MVVM.ViewModel
{
    internal class MainWindowViewModel : ObservableObject
    {

        public BitmapImage DefaultStreamImage { get; set; }

        private object _streamSource;

        public object StreamSource
        {
            get { return _streamSource; }
            set
            {
                _streamSource = value;
                OnPropertyChanged();
            }
        }


        public MainWindowViewModel()
        {
            DefaultStreamImage = new BitmapImage(new Uri("pack://application:,,,/Images/NoImage.png"));
            StreamSource = DefaultStreamImage;
        }
    }
}
