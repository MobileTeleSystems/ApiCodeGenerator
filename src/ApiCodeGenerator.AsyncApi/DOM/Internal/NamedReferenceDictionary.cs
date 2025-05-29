using System.Collections;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM.Internal;

internal sealed class NamedReferenceDictionary<T> : IDictionary<string, NamedReference<T>>, IDictionary
   where T : IJsonReference
{
    private readonly Dictionary<string, NamedReference<T>> _dictionary = new();

    public ICollection<string> Keys => _dictionary.Keys;

    public ICollection<NamedReference<T>> Values => _dictionary.Values;

    public int Count => _dictionary.Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<string, NamedReference<T>>>)_dictionary).IsReadOnly;

    public bool IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;

    public bool IsSynchronized => ((ICollection)_dictionary).IsSynchronized;

    public object SyncRoot => ((ICollection)_dictionary).SyncRoot;

    ICollection IDictionary.Keys => ((IDictionary)_dictionary).Keys;

    ICollection IDictionary.Values => ((IDictionary)_dictionary).Values;

    public NamedReference<T> this[string key]
    {
        get => _dictionary[key];
        set
        {
            _dictionary[key] = value;
            if (value != null)
            {
                value.ObjectId = key;
            }
        }
    }

    public object this[object key]
    {
        get => ((IDictionary)_dictionary)[key];
        set
        {
            ((IDictionary)_dictionary)[key] = value;
            if (value is NamedReference<T> n)
            {
                n.ObjectId = key.ToString();
            }
        }
    }

    public void Add(string key, NamedReference<T> value)
    {
        _dictionary.Add(key, value);
        value.ObjectId = key;
    }

    public void Add(KeyValuePair<string, NamedReference<T>> item)
    {
        ((ICollection<KeyValuePair<string, NamedReference<T>>>)_dictionary).Add(item);
        item.Value.ObjectId = item.Key;
    }

    public void Add(object key, object value)
    {
        ((IDictionary)_dictionary).Add(key, value);

        if (value is NamedReference<T> n)
        {
            n.ObjectId = key.ToString();
        }
    }

    public void Clear() => ((ICollection<KeyValuePair<string, NamedReference<T>>>)_dictionary).Clear();

    public bool Contains(KeyValuePair<string, NamedReference<T>> item) => ((ICollection<KeyValuePair<string, NamedReference<T>>>)_dictionary).Contains(item);

    public bool Contains(object key) => ((IDictionary)_dictionary).Contains(key);

    public bool ContainsKey(string key) => ((IDictionary<string, NamedReference<T>>)_dictionary).ContainsKey(key);

    public void CopyTo(KeyValuePair<string, NamedReference<T>>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, NamedReference<T>>>)_dictionary).CopyTo(array, arrayIndex);

    public void CopyTo(Array array, int index) => ((ICollection)_dictionary).CopyTo(array, index);

    public IEnumerator<KeyValuePair<string, NamedReference<T>>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, NamedReference<T>>>)_dictionary).GetEnumerator();

    public bool Remove(string key) => ((IDictionary<string, NamedReference<T>>)_dictionary).Remove(key);

    public bool Remove(KeyValuePair<string, NamedReference<T>> item) => ((ICollection<KeyValuePair<string, NamedReference<T>>>)_dictionary).Remove(item);

    public void Remove(object key) => ((IDictionary)_dictionary).Remove(key);

    public bool TryGetValue(string key, out NamedReference<T> value) => ((IDictionary<string, NamedReference<T>>)_dictionary).TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionary).GetEnumerator();

    IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)_dictionary).GetEnumerator();
}
