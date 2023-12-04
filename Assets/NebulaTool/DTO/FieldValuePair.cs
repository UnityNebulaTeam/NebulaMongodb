public class FieldValuePair
{
    public string FieldName { get; set; }
    public string OriginalValue { get; set; }
    public string UpdatedValue { get; set; }

    public FieldValuePair() { }
    public FieldValuePair(string fieldName, string originalValue)
    {
        FieldName = fieldName;
        OriginalValue = originalValue;
        UpdatedValue = originalValue;
    }
}
