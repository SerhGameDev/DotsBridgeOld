using System.Collections.Generic;

namespace DotsBridge.UI
{
    public enum PopupRowType { Header, Text, Value, Status }

    public struct PopupRowData
    {
        public PopupRowType Type;
        public string Label;
        public string StringValue;
        public float FloatValue;
        public bool BoolValue;
    }

    /// <summary>
    /// Fluent API для удобной сборки содержимого всплывающего окна.
    /// </summary>
    public class PopupBuilder
    {
        public List<PopupRowData> Rows { get; } = new List<PopupRowData>();

        public PopupBuilder Clear()
        {
            Rows.Clear();
            return this;
        }

        public PopupBuilder AddHeader(string title)
        {
            Rows.Add(new PopupRowData { Type = PopupRowType.Header, Label = title });
            return this;
        }

        public PopupBuilder AddText(string label, string value)
        {
            Rows.Add(new PopupRowData { Type = PopupRowType.Text, Label = label, StringValue = value });
            return this;
        }

        public PopupBuilder AddValue(string label, float value, string unit = "")
        {
            Rows.Add(new PopupRowData { Type = PopupRowType.Value, Label = label, FloatValue = value, StringValue = unit });
            return this;
        }

        public PopupBuilder AddStatus(string label, bool isActive)
        {
            Rows.Add(new PopupRowData { Type = PopupRowType.Status, Label = label, BoolValue = isActive });
            return this;
        }
    }
}