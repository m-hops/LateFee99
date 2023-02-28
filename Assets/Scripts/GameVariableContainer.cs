using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
public class GameVariableContainer : VariableStorageBehaviour
{
    Dictionary<string, string> StringValues = new();
    Dictionary<string, float> FloatValues = new();
    Dictionary<string, bool> BoolValues = new();
    public override void Clear()
    {
        throw new System.NotImplementedException();
    }

    public override bool Contains(string variableName)
    {
        throw new System.NotImplementedException();
    }

    public override (Dictionary<string, float>, Dictionary<string, string>, Dictionary<string, bool>) GetAllVariables()
        => (FloatValues, StringValues, BoolValues);

    public override void SetAllVariables(Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools, bool clear = true)
    {
        FloatValues = floats;
        StringValues = strings;
        BoolValues = bools;
    }

    public override void SetValue(string variableName, string stringValue)
    {
        StringValues[variableName] = stringValue;
        FloatValues.Remove(variableName);
        BoolValues.Remove(variableName);
    }

    public override void SetValue(string variableName, float floatValue)
    {
        FloatValues[variableName] = floatValue;
        StringValues.Remove(variableName);
        BoolValues.Remove(variableName);
    }

    public override void SetValue(string variableName, bool boolValue)
    {
        BoolValues[variableName] = boolValue;
        FloatValues.Remove(variableName);
        StringValues.Remove(variableName);
    }

    public override bool TryGetValue<T>(string variableName, out T result)
    {
        if(StringValues.TryGetValue(variableName, out var stringValue))
        {
            result = (T)System.Convert.ChangeType(stringValue, typeof(T));
            return true;
        }
        if (FloatValues.TryGetValue(variableName, out var floatValue))
        {
            result = (T)System.Convert.ChangeType(floatValue, typeof(T));
            return true;
        }
        if (BoolValues.TryGetValue(variableName, out var boolValue))
        {
            result = (T)System.Convert.ChangeType(boolValue, typeof(T));
            return true;
        }
        result = default;
        return false;
    }
}
