using Freitas.XControls.Calendar.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
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
            Translator.Instance.Configure(Language);
            InitializeGestures();
            InitializeDayLabels();
            InitializeDayOfWeekLabels();
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
            public Color? FontColor { get; set; }
        }

        class DayOfWeekLabel : Label { }

        #endregion

        #region Fields

        private IList<DayLabel> allDayLabels;
        private IList<DayLabel> AllDayLabels
        {
            get
            {
                if (allDayLabels == null)
                {
                    allDayLabels = new List<DayLabel>();
                    foreach (var d in CalendarGrid.Children.Where(f => f is DayLabel))
                        allDayLabels.Add((DayLabel)d);
                }
                return allDayLabels;
            }
        }

        private IList<Month> _allMonths;
        private IList<int> _allYears;

        private TapGestureRecognizer dayLabelTapGesture = new TapGestureRecognizer() { NumberOfTapsRequired = 1 };
        private TapGestureRecognizer dayLabelDoubleTapGesture = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };

        private bool _isInternalChangingDisplayMonthAndYear = false;
        private int _minYear = 1900, _maxYear = 2100;
        private CalendarLanguage _language;

        #endregion

        #region Properties

        public string MonthLabelName
        {
            get => Translator.Instance.GetResource("Month_Label");
        }
        public string YearLabelName
        {
            get => Translator.Instance.GetResource("Year_Label");
        }

        public CalendarLanguage Language
        {
            get => _language;
            set
            {
                _language = value;
                ResetLanguage();
            }
        }

        public bool NextYearIsEnabled
        {
            get
            {
                return DisplayYear < _maxYear;
            }
        }

        public bool PreviousYearIsEnabled
        {
            get
            {
                return DisplayYear > _minYear;
            }
        }

        public bool PreviousMonthIsEnabled
        {
            get
            {
                return (DisplayYear == _minYear
                        && DisplayMonth.Code > 1)
                    || DisplayYear > _minYear;
            }
        }

        public bool NextMonthIsEnabled
        {
            get
            {
                return (DisplayYear == _maxYear
                        && DisplayMonth.Code < 12)
                    || DisplayYear < _maxYear;
            }
        }

        public IList<Month> AllMonths
        {
            get
            {
                if (_allMonths == null)
                {
                    var array = new Month[] {
                                new Month { Code = 1, Name = Translator.Instance.GetResource("Month_Name_0_Jan") },
                                new Month { Code = 2, Name = Translator.Instance.GetResource("Month_Name_1_Feb") },
                                new Month { Code = 3, Name = Translator.Instance.GetResource("Month_Name_2_Mar") },
                                new Month { Code = 4, Name = Translator.Instance.GetResource("Month_Name_3_Apr") },
                                new Month { Code = 5, Name = Translator.Instance.GetResource("Month_Name_4_May") },
                                new Month { Code = 6, Name = Translator.Instance.GetResource("Month_Name_5_Jun") },
                                new Month { Code = 7, Name = Translator.Instance.GetResource("Month_Name_6_Jul") },
                                new Month { Code = 8, Name = Translator.Instance.GetResource("Month_Name_7_Aug") },
                                new Month { Code = 9, Name = Translator.Instance.GetResource("Month_Name_8_Sep") },
                                new Month { Code = 10, Name = Translator.Instance.GetResource("Month_Name_9_Oct") },
                                new Month { Code = 11, Name = Translator.Instance.GetResource("Month_Name_10_Nov") },
                                new Month { Code = 12, Name = Translator.Instance.GetResource("Month_Name_11_Dec") }};

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
                    for (int year = _minYear; year <= _maxYear; year++)
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
            set
            {
                SetValue(DisplayMonthProperty, value);
                RaiseNextAndPreviousMonthAndYearIsEnabled();
            }
        }

        public int DisplayYear
        {
            get { return (int)GetValue(DisplayYearProperty); }
            set
            {
                SetValue(DisplayYearProperty, value);
                RaiseNextAndPreviousMonthAndYearIsEnabled();
            }
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

        public ICommand OnDayLabelDoubleTapCommand
        {
            get => (ICommand)GetValue(OnDayLabelDoubleTapCommandProperty);
            set => SetValue(OnDayLabelDoubleTapCommandProperty, value);
        }

        #endregion

        #region Bindable Properties

        public static BindableProperty DisplayMonthProperty = BindableProperty.Create(nameof(DisplayMonth), typeof(Month), typeof(Calendar), propertyChanged: OnDisplayMonthChanged, defaultValueCreator: CrateDefaultValueForDisplayMonth);
        public static BindableProperty DisplayYearProperty = BindableProperty.Create(nameof(DisplayYear), typeof(int), typeof(Calendar), propertyChanged: OnDisplayYearChanged, defaultValueCreator: CrateDefaultValueForDisplayYear);
        public static BindableProperty SelectedDatesProperty = BindableProperty.Create(nameof(SelectedDates), typeof(IList<DateTime>), typeof(Calendar), propertyChanged: OnSelectedDatesChanged, defaultValue: new List<DateTime>());
        public static BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(CalendarSelectionMode), typeof(Calendar), propertyChanged: OnSelectionModeChanged);
        public static BindableProperty PredefinedDatesProperty = BindableProperty.Create(nameof(PredefinedDates), typeof(IList<CalendarPredefinedDate>), typeof(Calendar), propertyChanged: OnPredefinedDatesChanged, defaultValueCreator: CrateDefaultValueForPredefinedDates);
        public static BindableProperty CalendarDefaultFontColorProperty = BindableProperty.Create(nameof(CalendarDefaultFontColor), typeof(Color), typeof(Calendar), propertyChanged: OnCalendarDefaultFontColorChanged, defaultValue: Color.Black);
        public static BindableProperty OnDayLabelDoubleTapCommandProperty = BindableProperty.Create(nameof(OnDayLabelDoubleTapCommand), typeof(ICommand), typeof(Calendar));

        #endregion

        #region Private Methods

        private void ResetLanguage()
        {
            Translator.Instance.Configure(Language);
            InitializeDayOfWeekLabels();
            _allMonths = null;
            OnPropertyChanged(nameof(AllMonths));
            DisplayMonth = AllMonths.FirstOrDefault(f => f.Code == DisplayMonth.Code);
            OnPropertyChanged(nameof(DisplayMonth));
            OnPropertyChanged(nameof(MonthLabelName));
            OnPropertyChanged(nameof(YearLabelName));
        }

        private void InitializeDayLabels()
        {
            for (int row = 1; row <= 6; row++)
            {
                for (int column = 0; column <= 6; column++)
                {
                    var label = new DayLabel
                    {
                        Row = row,
                        Column = column,
                        Text = "0"
                    };
                    AllDayLabels.Add(label);
                    CalendarGrid.Children.Add(label, column, row);
                }
            }
        }

        private void InitializeDayOfWeekLabels()
        {
            var allDayOfWeekLabels = CalendarGrid.Children.Where(f => f is DayOfWeekLabel).ToList();
            foreach (var label in allDayOfWeekLabels)
                CalendarGrid.Children.Remove(label);

            for (int day = 0; day <= 6; day++)
            {
                var label = new DayOfWeekLabel()
                {
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Color.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = Translator.Instance.GetResource($"DayOfWeek_NickName_{day}_{((DayOfWeek)day).ToString()}")
                };

                CalendarGrid.Children.Add(label, day, 0);
            }
        }

        private void RaiseNextAndPreviousMonthAndYearIsEnabled()
        {
            OnPropertyChanged(nameof(PreviousMonthIsEnabled));
            OnPropertyChanged(nameof(NextMonthIsEnabled));
            OnPropertyChanged(nameof(PreviousYearIsEnabled));
            OnPropertyChanged(nameof(NextYearIsEnabled));
        }

        private void InitializeGestures()
        {
            dayLabelTapGesture.Tapped += OnDayLabelSingleTapped;
            dayLabelDoubleTapGesture.Tapped += OnDayLabelDoubleTapped;
        }

        private void BuildCalendar()
        {
            int currentRowIndex = 1,
                totalDaysInMonth = DateTime.DaysInMonth(DisplayYear, DisplayMonth.Code);

            DateTime firstDayOfCurrentMonth = new DateTime(DisplayYear, DisplayMonth.Code, 1);
            int currentColumnIndex = (int)firstDayOfCurrentMonth.DayOfWeek;

            if (currentColumnIndex > 0)
                AddBlankLabel(currentRowIndex, currentRowIndex, 0, currentColumnIndex - 1);

            for (int i = 1; i <= totalDaysInMonth; i++)
            {
                AddDayLabelToGrid(new DateTime(DisplayYear, DisplayMonth.Code, i), currentRowIndex, currentColumnIndex);
                SetCurrentRowAndColumn(ref currentRowIndex, ref currentColumnIndex);
            }

            if (currentColumnIndex <= 6)
                AddBlankLabel(currentRowIndex, 6, currentColumnIndex, 6);
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

        private void AddBlankLabel(int rowIndexFrom, int rowIndexTo, int columnIndexFrom, int columnIndexTo)
        {
            for (int row = rowIndexFrom; row <= rowIndexTo; row++)
            {
                for (int column = columnIndexFrom; column <= columnIndexTo; column++)
                {
                    var label = AllDayLabels.FirstOrDefault(f => f.Row == row && f.Column == column);
                    if (label == null)
                        continue;

                    SetDayLabel(ref label, null);
                    label.BackgroundColor = Color.LightGray;
                }
                columnIndexFrom = 0;
                columnIndexTo = 6;
            }
        }

        private void AddDayLabelToGrid(DateTime day, int rowIndex, int columnIndex)
        {
            var label = AllDayLabels.FirstOrDefault(f => f.Row == rowIndex && f.Column == columnIndex);
            if (label != null)
                SetDayLabel(ref label, day);
        }

        private void SetDayLabel(ref DayLabel dayLabel, DateTime? date)
        {
            CalendarPredefinedDate item = null;
            if (date.HasValue)
                item = PredefinedDates.FirstOrDefault(f => f.Date == date.Value);

            dayLabel.Date = date;
            dayLabel.Text = date.HasValue ? date.Value.Day.ToString() : string.Empty;
            dayLabel.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            dayLabel.TextColor = item != null && item.FontColor.HasValue ? (Color)item.FontColor.Value : CalendarDefaultFontColor;
            dayLabel.HorizontalOptions = LayoutOptions.FillAndExpand;
            dayLabel.VerticalOptions = LayoutOptions.FillAndExpand;
            dayLabel.HorizontalTextAlignment = TextAlignment.Center;
            dayLabel.IsSelected = IsDateInSelectedDates(date);

            if (date.HasValue)
            {
                dayLabel.GestureRecognizers.Clear();

                dayLabel.GestureRecognizers.Add(dayLabelTapGesture);
                dayLabel.GestureRecognizers.Add(dayLabelDoubleTapGesture);
            }
        }

        private void AddSelectedDate(DateTime date)
        {
            if (IsDateInSelectedDates(date))
                return;

            SelectedDates.Add(date);
            DayLabel dayLabel = (DayLabel)CalendarGrid.Children.FirstOrDefault(f => f is DayLabel && ((DayLabel)f).Date == date);

            if (dayLabel != null)
                dayLabel.IsSelected = true;
        }

        private void RemoveSelectedDate(DateTime date)
        {
            if (!IsDateInSelectedDates(date))
                return;

            SelectedDates.Remove(date);
            DayLabel dayLabel = (DayLabel)CalendarGrid.Children.FirstOrDefault(f => f is DayLabel && ((DayLabel)f).Date == date);

            if (dayLabel != null)
                dayLabel.IsSelected = false;
        }

        private void ClearAllSelectedDates()
        {
            if (!SelectedDates.Any())
                return;

            List<DateTime> datesToRemove = new List<DateTime>(SelectedDates);

            foreach (var d in datesToRemove)
                RemoveSelectedDate(d);
        }

        private bool IsDateInSelectedDates(DateTime? date)
        {
            return SelectedDates.FirstOrDefault(f => f == date) != DateTime.MinValue;
        }

        private void HandleSingleSelection(DateTime date)
        {
            bool existentDate = IsDateInSelectedDates(date);
            ClearAllSelectedDates();

            if (!existentDate)
                AddSelectedDate(date);
        }

        private void HandleMultipleSelection(DateTime date)
        {
            if (!IsDateInSelectedDates(date))
                AddSelectedDate(date);
            else
                RemoveSelectedDate(date);
        }

        private void HandleSpanSelection(DateTime date)
        {
            if (!SelectedDates.Any())
                AddSelectedDate(date);
            else
            {
                var firstDate = SelectedDates[0];
                ClearAllSelectedDates();
                if (date > firstDate)
                    for (DateTime d = firstDate; d <= date; d = d.AddDays(1))
                        AddSelectedDate(d);
                else if (date < firstDate)
                    for (DateTime d = date; d <= firstDate; d = d.AddDays(1))
                        AddSelectedDate(d);
            }
        }

        private void HandleSelectedDate(DateTime date)
        {
            switch (SelectionMode)
            {
                case CalendarSelectionMode.Single:
                    HandleSingleSelection(date);
                    break;
                case CalendarSelectionMode.Multiple:
                    HandleMultipleSelection(date);
                    break;
                case CalendarSelectionMode.Span:
                    HandleSpanSelection(date);
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
            var control = (Calendar)bindable;
            if (!control._isInternalChangingDisplayMonthAndYear)
                control.BuildCalendar();
        }

        private static void OnDisplayYearChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (Calendar)bindable;
            if (!control._isInternalChangingDisplayMonthAndYear)
                control.BuildCalendar();
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

        private void OnDayLabelSingleTapped(object sender, EventArgs e)
        {
            var label = (DayLabel)sender;
            if (SelectionMode != CalendarSelectionMode.None)
                HandleSelectedDate(label.Date.Value);
            else
                OnDayLabelDoubleTapCommand?.Execute(label.Date);
        }

        private void OnDayLabelDoubleTapped(object sender, EventArgs e)
        {
            var label = (DayLabel)sender;
            OnDayLabelDoubleTapCommand?.Execute(label.Date);
        }

        private void PreviousMonth()
        {
            _isInternalChangingDisplayMonthAndYear = true;
            if (DisplayMonth.Code == 1)
            {
                DisplayMonth = AllMonths[11];
                DisplayYear--;
            }
            else
            {
                DisplayMonth = AllMonths.First(f => f.Code == DisplayMonth.Code - 1);
            }

            BuildCalendar();

            _isInternalChangingDisplayMonthAndYear = false;
        }

        private void NextMonth()
        {
            _isInternalChangingDisplayMonthAndYear = true;
            if (DisplayMonth.Code == 12)
            {
                DisplayMonth = AllMonths[0];
                DisplayYear++;
            }
            else
            {
                DisplayMonth = AllMonths.First(f => f.Code == DisplayMonth.Code + 1);
            }

            BuildCalendar();

            _isInternalChangingDisplayMonthAndYear = false;
        }

        private void PreviousYear()
        {
            if (DisplayYear > _minYear)
                DisplayYear--;
        }

        private void NextYear()
        {
            if (DisplayYear < _maxYear)
                DisplayYear++;
        }

        private void NextMonthTapLabelGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            NextMonth();
        }

        private void PreviousMonthLabelTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            PreviousMonth();
        }

        private void PreviousYearLabelTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            PreviousYear();
        }

        private void NextYearTapLabelGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            NextYear();
        }

        #endregion
    }
}