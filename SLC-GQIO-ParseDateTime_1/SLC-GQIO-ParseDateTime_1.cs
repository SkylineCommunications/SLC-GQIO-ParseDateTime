using Skyline.DataMiner.Analytics.GenericInterface;
using System;
using System.Globalization;

[GQIMetaData(Name = "Parse date/time")]
public sealed class DateTimeParser : IGQIColumnOperator, IGQIRowOperator, IGQIInputArguments
{
    private static readonly string[] _cultureOptions = Localization.GetCultureOptions();
    private static readonly string[] _timeZoneOptions = Localization.GetTimeZoneOptions();

    private readonly GQIArgument<GQIColumn> _columnArgument;
    private readonly GQIArgument<string> _formatArgument;
    private readonly GQIArgument<string> _cultureArgument;
    private readonly GQIArgument<string> _timeZoneArgument;

    private GQIColumn<string> _stringColumn;
    private GQIColumn<DateTime> _dateTimeColumn;
    private string _format;
    private CultureInfo _culture;
    private DateTimeStyles _dateTimeStyles;
    private TimeZoneInfo _timeZone;

    public DateTimeParser()
    {
        _columnArgument = new GQIColumnDropdownArgument("Column")
        {
            Types = new[] { GQIColumnType.String },
            IsRequired = true,
        };
        _formatArgument = new GQIStringArgument("Format") { IsRequired = true };
        _cultureArgument = new GQIStringDropdownArgument("Culture", _cultureOptions);
        _timeZoneArgument = new GQIStringDropdownArgument("Time zone", _timeZoneOptions);
    }

    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[]
        {
            _columnArgument,
            _formatArgument,
            _cultureArgument,
            _timeZoneArgument,
        };
    }

    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _stringColumn = (GQIColumn<string>)args.GetArgumentValue(_columnArgument);
        args.TryGetArgumentValue(_formatArgument, out _format);

        args.TryGetArgumentValue(_cultureArgument, out string cultureValue);
        _culture = Localization.GetCulture(cultureValue);

        args.TryGetArgumentValue(_timeZoneArgument, out string timeZoneValue);

        // When time zone is specified in the format, convert to UTC
        _dateTimeStyles = DateTimeStyles.AdjustToUniversal;
        if (string.IsNullOrEmpty(timeZoneValue))
        {
            // Assume formats without time zone are UTC
            _dateTimeStyles |= DateTimeStyles.AssumeUniversal;
        }
        else
        {
            // Assume formats without time zone use this local time zone
            _timeZone = Localization.GetTimeZone(timeZoneValue);
        }

        return default;
    }

    public void HandleColumns(GQIEditableHeader header)
    {
        header.DeleteColumns(_stringColumn);

        var originalName = _stringColumn.Name;
        _dateTimeColumn = new GQIDateTimeColumn($"DATETIME({originalName})");
        header.AddColumns(_dateTimeColumn);
    }

    public void HandleRow(GQIEditableRow row)
    {
        var stringValue = row.GetValue(_stringColumn);

        if (!DateTime.TryParseExact(stringValue, _format, _culture, _dateTimeStyles, out var dateTimeValue))
            return;

        if (dateTimeValue.Kind == DateTimeKind.Unspecified)
        {
            // Assume selected time zone
            dateTimeValue = TimeZoneInfo.ConvertTimeToUtc(dateTimeValue, _timeZone);
        }

        row.SetValue(_dateTimeColumn, dateTimeValue);
    }
}