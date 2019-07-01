using Freitas.XControls.Calendar.Localization;
using System;
using System.Globalization;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Freitas.XControls.Calendar
{
    public class Translator
    {
        private Translator()
        {
        }

        private static Translator instance;
        public static Translator Instance
        {
            get
            {
                if (object.ReferenceEquals(instance, null))
                    instance = new Translator();

                return instance;
            }
        }

        private ResourceManager resourceManager = new ResourceManager(typeof(Dictionary));
        private CultureInfo cultureInfo;

        public void Configure(CalendarLanguage language)
        {
            switch(language)
            {
                case CalendarLanguage.En_US:
                    cultureInfo = new CultureInfo("en-US");
                    break;
                case CalendarLanguage.Pt_BR:
                    cultureInfo = new CultureInfo("pt-BR");
                    break;
            }
        }

        public string GetResource(string key)
        {
            var text = resourceManager.GetString(key, cultureInfo);
            return string.IsNullOrEmpty(text) ? $"[ {key} ]" : text;
        }
    }

    [ContentProperty("Text")]
    public class TranslatorExtension : IMarkupExtension
    {
        public string Text { get; set; }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Translator.Instance.GetResource(Text);
        }
    }

}
