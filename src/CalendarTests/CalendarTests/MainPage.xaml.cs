using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace CalendarTests
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private ICommand _openDayDetailsCommand;
        public ICommand OpenDayDetailsCommand
        {
            get
            {
                if (_openDayDetailsCommand == null)
                {
                    _openDayDetailsCommand = new DelegateCommand<DateTime?>(ExecuteDayDetails);
                }

                return _openDayDetailsCommand;
            }
        }

        private void ExecuteDayDetails(DateTime? date)
        {
            
        }
    }
}
