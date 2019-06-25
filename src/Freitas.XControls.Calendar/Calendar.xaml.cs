using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Freitas.XControls.Calendar
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Calendar : ContentView
    {
        #region Constructors

        public Calendar()
        {
            InitializeComponent();
            BuildCalendar();
        }

        #endregion

        #region Nested Types

        public struct Month
        {
            public int Code { get; set; }
            public string Name { get; set; }
        }

        public enum CalendarSelectionMode
        {
            None,
            Single,
            Multiple,
            Span
        }

        public class CalendarPredefinedDate
        {
            public DateTime Date { get; set; }
            public bool IsEnabled { get; set; }
            public Color? FontColor { get; set; }
        }

        public class DayLabel : Label
        {
            public DateTime? Date { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    if (_isSelected)
                        BackgroundColor = Color.White;
                    else
                        BackgroundColor = Color.LightSkyBlue;
                }
            }
        }

        #endregion

        #region Fields

        private IList<Month> _allMonths;
        private IList<int> _allYears;

        #endregion

        #region Properties

        public IList<Month> AllMonths
        {
            get
            {
                if (_allMonths == null)
                {
                    var array = new Month[] {
                                new Month { Code = 1, Name = "Janeiro" },
                                new Month { Code = 2, Name = "Fevereiro" },
                                new Month { Code = 3, Name = "Março" },
                                new Month { Code = 4, Name = "Abril" },
                                new Month { Code = 5, Name = "Maio" },
                                new Month { Code = 6, Name = "Junho" },
                                new Month { Code = 7, Name = "Julho" },
                                new Month { Code = 8, Name = "Agosto" },
                                new Month { Code = 9, Name = "Setembro" },
                                new Month { Code = 10, Name = "Outubro" },
                                new Month { Code = 11, Name = "Novembro" },
                                new Month { Code = 12, Name = "Dezembro" }};

                    _allMonths = new List<Month>(array);
                }

                return _allMonths;
            }
        }

        public IList<int> AllYears
        {
            get
            {
                if (_allYears == null || _allYears.Count == 0)
                {
                    _allYears = new List<int>();
                    for (int year = 1900; year <= 2100; year++)
                    {
                        _allYears.Add(year);
                    }
                    _allYears = _allYears.OrderByDescending(f => f).ToList();
                }

                return _allYears;
            }
        }

        public IList<CalendarPredefinedDate> PredefinedDates
        {
            get => (IList<CalendarPredefinedDate>)GetValue(PredefinedDatesProperty);
            set => SetValue(PredefinedDatesProperty, value);
        }

        public Month DisplayMonth
        {
            get { return (Month)GetValue(DisplayMonthProperty); }
            set { SetValue(DisplayMonthProperty, value); }
        }

        public int DisplayYear
        {
            get { return (int)GetValue(DisplayYearProperty); }
            set { SetValue(DisplayYearProperty, value); }
        }

        public IList<DateTime> SelectedDates
        {
            get => (IList<DateTime>)GetValue(SelectedDatesProperty);
            set => SetValue(SelectedDatesProperty, value);
        }

        public CalendarSelectionMode SelectionMode
        {
            get => (CalendarSelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        public Color CalendarDefaultFontColor
        {
            get => (Color)GetValue(CalendarDefaultFontColorProperty);
            set => SetValue(CalendarDefaultFontColorProperty, value);
        }

        public DelegateCommand<DateTime?> OnDayLabelClickCommand
        {
            get => (DelegateCommand<DateTime?>)GetValue(OnDayLabelClickCommandProperty);
            set => SetValue(OnDayLabelClickCommandProperty, value);
        }

        #endregion

        #region Bindable Properties

        public static BindableProperty DisplayMonthProperty = BindableProperty.Create(nameof(DisplayMonth), typeof(Month), typeof(Calendar), propertyChanged: OnDisplayMonthChanged, defaultValueCreator: CrateDefaultValueForDisplayMonth);
        public static BindableProperty DisplayYearProperty = BindableProperty.Create(nameof(DisplayYear), typeof(int), typeof(Calendar), propertyChanged: OnDisplayYearChanged, defaultValueCreator: CrateDefaultValueForDisplayYear);
        public static BindableProperty SelectedDatesProperty = BindableProperty.Create(nameof(SelectedDates), typeof(IList<DateTime>), typeof(Calendar), propertyChanged: OnSelectedDatesChanged, defaultValue: new List<DateTime>());
        public static BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(CalendarSelectionMode), typeof(Calendar), propertyChanged: OnSelectionModeChanged);
        public static BindableProperty PredefinedDatesProperty = BindableProperty.Create(nameof(PredefinedDates), typeof(IList<CalendarPredefinedDate>), typeof(Calendar), propertyChanged: OnPredefinedDatesChanged, defaultValueCreator: CrateDefaultValueForPredefinedDates);
        public static BindableProperty CalendarDefaultFontColorProperty = BindableProperty.Create(nameof(CalendarDefaultFontColor), typeof(Color), typeof(Calendar), propertyChanged: OnCalendarDefaultFontColorChanged, defaultValue: Color.Black);
        public static BindableProperty OnDayLabelClickCommandProperty = BindableProperty.Create(nameof(OnDayLabelClickCommand), typeof(DelegateCommand<DateTime?>), typeof(Calendar));

        #endregion

        #region Private Methods

        private void ClearCalendar()
        {
            foreach (var control in CalendarGrid.Children)
            {
                if (control is DayLabel)
                {
                    var label = (DayLabel)control;
                    label.Text = string.Empty;
                    label.BackgroundColor = Color.LightGray;
                    label.Date = null;
                }
            }
        }

        private void BuildCalendar()
        {
            ClearCalendar();

            int currentRowIndex = 1,
                totalDaysInMonth = DateTime.DaysInMonth(DisplayYear, DisplayMonth.Code);

            DateTime firstDayOfCurrentMonth = new DateTime(DisplayYear, DisplayMonth.Code, 1);
            int currentColumnIndex = (int)firstDayOfCurrentMonth.DayOfWeek;

            if (currentColumnIndex > 0)
                AddBlankLabel(currentRowIndex, 0, currentColumnIndex - 1);

            for (int i = 1; i <= totalDaysInMonth; i++)
            {
                AddDayLabelToGrid(new DateTime(DisplayYear, DisplayMonth.Code, i), currentRowIndex, currentColumnIndex);
                SetCurrentRowAndColumn(ref currentRowIndex, ref currentColumnIndex);
            }

            if (currentColumnIndex > 0 && currentColumnIndex <= 6)
                AddBlankLabel(currentRowIndex, currentColumnIndex, 6);
        }

        private void SetCurrentRowAndColumn(ref int rowIndex, ref int columnIndex)
        {
            if (columnIndex >= 6)
            {
                rowIndex++;
                columnIndex = 0;
            }
            else
            {
                columnIndex++;
            }
        }

        private void AddBlankLabel(int rowIndex, int columnIndexFrom, int columnIndexTo)
        {
            for (int i = columnIndexFrom; i <= columnIndexTo; i++)
            {
                var label = CreateDayLabel(null);
                label.BackgroundColor = Color.LightGray;
                CalendarGrid.Children.Add(label, i, rowIndex);
            }
        }

        private void AddDayLabelToGrid(DateTime day, int rowIndex, int columnIndex)
        {
            var label = CreateDayLabel(day);
            CalendarGrid.Children.Add(label, columnIndex, rowIndex);
        }

        private DayLabel CreateDayLabel(DateTime? date)
        {
            CalendarPredefinedDate item = null;
            if (date.HasValue)
                item = PredefinedDates.FirstOrDefault(f => f.Date == date.Value);

            var dayLabel = new DayLabel()
            {
                Date = date,
                Text = date.HasValue ? date.Value.Day.ToString() : string.Empty,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                TextColor = item != null && item.FontColor.HasValue ? (Color)item.FontColor.Value : CalendarDefaultFontColor,
                IsEnabled = item != null ? item.IsEnabled : false,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Color.White
            };

            var dayLabelTapGesture = new TapGestureRecognizer();
            dayLabelTapGesture.Tapped += OnDayLabelTapped;
            dayLabel.GestureRecognizers.Add(dayLabelTapGesture);

            return dayLabel;
        }

        private void AddSelectedDate(DateTime date)
        {
            if (SelectedDates.FirstOrDefault(f => f == date) != null)
                return;

            DayLabel label = (DayLabel)CalendarGrid.Children.FirstOrDefault(f => f is DayLabel && ((DayLabel)f).Date == date);

            if (label == null)
                return;

            label.IsSelected = true;
            SelectedDates.Add(date);
        }

        private void RemoveSelectedDate(DateTime date)
        {
            if (SelectedDates.FirstOrDefault(f => f == date) == null)
                return;

            DayLabel label = (DayLabel)CalendarGrid.Children.FirstOrDefault(f => f is DayLabel && ((DayLabel)f).Date == date);

            if (label == null)
                return;

            label.IsSelected = false;
            SelectedDates.Remove(date);
        }

        private void ClearAllSelectedDates()
        {
            if (!SelectedDates.Any())
                return;

            foreach (var selected in SelectedDates)
                RemoveSelectedDate(selected.Date);
        }

        private void HandleSelectedDate(DateTime date)
        {
            switch (SelectionMode)
            {
                case CalendarSelectionMode.Single:
                    ClearAllSelectedDates();
                    AddSelectedDate(date);
                    break;
                case CalendarSelectionMode.Multiple:
                    AddSelectedDate(date);
                    break;
                case CalendarSelectionMode.Span:
                    if (!SelectedDates.Any())
                    {
                        AddSelectedDate(date);
                    }
                    else
                    {
                        var firstDate = SelectedDates[0];
                        ClearAllSelectedDates();
                        if (date > firstDate)
                        {
                            for (DateTime d = firstDate; d <= date; date = date.AddDays(1))
                            {
                                AddSelectedDate(d);
                            }
                        }
                        else if (date < firstDate)
                        {
                            for (DateTime d = date; d <= firstDate; date = date.AddDays(1))
                            {
                                AddSelectedDate(d);
                            }
                        }
                    }
                    break;
            }
        }

        private static object CrateDefaultValueForPredefinedDates(BindableObject bindable)
        {
            return new List<CalendarPredefinedDate>();
        }

        private static object CrateDefaultValueForDisplayMonth(BindableObject bindable)
        {
            return ((Calendar)bindable).AllMonths.FirstOrDefault(f => f.Code == DateTime.Today.Month);
        }

        private static object CrateDefaultValueForDisplayYear(BindableObject bindable)
        {
            return ((Calendar)bindable).AllYears.FirstOrDefault(f => f == DateTime.Today.Year);
        }

        private static void OnDisplayMonthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((Calendar)bindable).BuildCalendar();
        }

        private static void OnDisplayYearChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((Calendar)bindable).BuildCalendar();
        }

        private static void OnSelectedDatesChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }

        private static void OnSelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }

        private static void OnPredefinedDatesChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }

        private static void OnCalendarDefaultFontColorChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }

        #endregion

        #region Public Methods

        public void OnDayLabelTapped(object sender, EventArgs e)
        {
            var label = (DayLabel)sender;

            HandleSelectedDate(label.Date.Value);

            OnDayLabelClickCommand?.Execute(label.Date);
        }

        #endregion
    }
}