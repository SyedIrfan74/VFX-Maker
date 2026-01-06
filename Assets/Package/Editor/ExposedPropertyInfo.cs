using System;

[Serializable]
public class ExposedPropertyInfo
{
    public string Name;
    public Type ValueType;
    public object Value;
    public object InternalParameter;

    public ExposedPropertyInfo(string name, Type valueType, object value, object internalRef)
    {
        Name = name;
        ValueType = valueType;
        Value = value;
        InternalParameter = internalRef;
    }
}