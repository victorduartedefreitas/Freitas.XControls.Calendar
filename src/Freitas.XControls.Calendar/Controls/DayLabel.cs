using System;
using Xamarin.Forms;

namespace Freitas.XControls.Calendar.Controls
{
    internal class DayLabel : Label
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public DateTime? Date { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (_isSelected)
                    BackgroundColor = Color.LightSkyBlue;
                else
                    BackgroundColor = Color.White;
            }
        }
    }
}
