using nepali_calendar_picker.Services;

namespace nepali_calendar_picker
{
    /// <summary>
    /// Extension methods for registering NepaliDatePicker with the MAUI app builder.
    /// </summary>
    public static class MauiAppBuilderExtensions
    {
        /// <summary>
        /// Registers <see cref="INepaliDatePickerService"/> and the XAML namespace handler
        /// so you can use <c>xmlns:nep="clr-namespace:NepaliDatePicker;assembly=NepaliDatePicker.Maui"</c>.
        /// </summary>
        public static MauiAppBuilder AddNepaliDatePicker(this MauiAppBuilder builder)
        {
            builder.Services.AddTransient<INepaliDatePickerService, NepaliDatePickerService>();
            return builder;
        }
    }
}
