using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Valid_DynamicFilterSort.Interfaces;

namespace Valid_DynamicFilterSort.Base
{
    public abstract class BaseFieldObject : IHaveFields
    {
        public ReadOnlyDictionary<string, object> Fields => new ReadOnlyDictionary<string, object>(_fields);
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public IHaveFields AddOrUpdateValue(string key, object value)
        {
            // get type and props
            var myType = GetType();
            var myProps = myType.GetProperties();
            
            // find prop or null
            var pi = myProps
                .FirstOrDefault(a => 
                    a.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            // if null, we can add to the field dict
            if (pi == null)
            {
                _fields[key] = value;
                return this;
            }
            
            // if we can't write to the prop or the prop isn't the right type, throw
            if (!pi.CanWrite || !pi.PropertyType.IsInstanceOfType(value))
            {
                throw new InvalidCastException($"Unable to set value for {key};");
            }
            
            // set prop
            pi.SetValue(this, value);

            return this;
        }

        public bool TryGetValue(string key, out object value)
        {
            try
            {
                value = GetValue(key);
                return true;
            }
            catch(ArgumentException)
            {
                value = null;
                return false;
            }
        }

        public object GetValue(string key)
        {
            // check fields
            if (Fields.ContainsKey(key))
            {
                return Fields[key];
            }
            
            // get type and props
            var myType = GetType();
            var myProps = myType.GetProperties();
            
            // find prop or null
            var pi = myProps
                .FirstOrDefault(a =>
                    a.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            
            // if we can't write to the prop or the prop isn't the right type, throw
            if (pi == null || !pi.CanWrite)
            {
                throw new ArgumentException($"Could not find property with key: {key};");
            }

            var value = pi.GetValue(this);

            return value;
        }

        public IHaveFields RemoveField(string key)
        {
            if (Fields.ContainsKey(key))
            {
                _fields.Remove(key);
            }

            return this;
        }
    }
}