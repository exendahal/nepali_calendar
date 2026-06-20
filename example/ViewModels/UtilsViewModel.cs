using CommunityToolkit.Mvvm.ComponentModel;
using NepaliDatePicker.Models;
using NepaliDatePicker.Services;
using NepaliDatePicker.Formatting;

namespace NepaliDatePickerDemo;

public partial class UtilsViewModel : ObservableObject
{
    // ── Formatter section ─────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Fmt_dMMMMyyy))]
    [NotifyPropertyChangedFor(nameof(Fmt_ddMMyyy))]
    [NotifyPropertyChangedFor(nameof(Fmt_EEEEdMMMMyyy))]
    [NotifyPropertyChangedFor(nameof(Fmt_EEEdMMMyyy))]
    [NotifyPropertyChangedFor(nameof(Fmt_dMMMMyyyNp))]
    [NotifyPropertyChangedFor(nameof(Fmt_EEEEdMMMMyyyNp))]
    [NotifyPropertyChangedFor(nameof(MomentResult))]
    [NotifyPropertyChangedFor(nameof(MomentResultNepali))]
    private NepaliDate? _PickedDate;

    // ── Moment section ────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MomentResult))]
    [NotifyPropertyChangedFor(nameof(MomentResultNepali))]
    private NepaliDate? _ReferenceDate;

    // ── Formatter computed properties ─────────────────────────────────────────

    public string Fmt_dMMMMyyy       => PickedDate?.Format("d MMMM yyyy")               ?? "—";
    public string Fmt_ddMMyyy        => PickedDate?.Format("dd/MM/yyyy")                 ?? "—";
    public string Fmt_EEEEdMMMMyyy   => PickedDate?.Format("EEEE, d MMMM yyyy")         ?? "—";
    public string Fmt_EEEdMMMyyy     => PickedDate?.Format("EEE d MMM, yy")             ?? "—";
    public string Fmt_dMMMMyyyNp     => PickedDate?.Format("d MMMM yyyy", true)         ?? "—";
    public string Fmt_EEEEdMMMMyyyNp => PickedDate?.Format("EEEE, d MMMM yyyy", true)   ?? "—";

    // ── Moment computed properties ────────────────────────────────────────────

    public string MomentResult       => PickedDate is null ? "—" : NepaliMoment.Elapsed(PickedDate, ReferenceDate);
    public string MomentResultNepali => PickedDate is null ? "—" : NepaliMoment.Elapsed(PickedDate, ReferenceDate, nepali: true);

    // ── Constructor ───────────────────────────────────────────────────────────

    public UtilsViewModel()
    {
        _PickedDate    = BsAdConverter.AdToBs(DateTime.Today);
        _ReferenceDate = null;   // defaults to today inside NepaliMoment
    }
}
