namespace DotsAndBoxes.Navigation;

public class DynamicDictionary
{
    private readonly Dictionary<string, object> _parameters;

    public DynamicDictionary(Dictionary<string, object>? baseParameters = null)
    {
        _parameters = baseParameters ?? new Dictionary<string, object>();
    }

    public DynamicDictionary(params (string, object)[] baseParameters)
    {
        _parameters = baseParameters.ToDictionary(x => x.Item1, x => x.Item2);
    }

    public T? GetValue<T>(string key)
    {
        if (!_parameters.TryGetValue(key, out var value))
        {
            return default;
        }

        if (value is T result)
        {
            return result;
        }

        return default;
    }

    public void AddValue(string key, object value)
    {
        _parameters[key] = value;
    }
}