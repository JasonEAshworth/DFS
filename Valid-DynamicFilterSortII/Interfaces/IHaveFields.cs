using System.Collections.ObjectModel;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// Interface for objects with dynamic fields
    /// </summary>
    public interface IHaveFields
    {
        /// <summary>
        /// Generic other fields
        /// </summary>
        ReadOnlyDictionary<string, object> Fields { get; }

        /// <summary>
        /// Adds or updates a value on the object
        /// </summary>
        /// <param name="key">field name</param>
        /// <param name="value">field value</param>
        /// <returns>IHaveFields Object</returns>
        IHaveFields AddOrUpdateValue(string key, object value);

        /// <summary>
        /// Tries to get a value on the object from the key provided
        /// </summary>
        /// <param name="key">field name</param>
        /// <param name="value">out object</param>
        /// <returns>bool for success; out object value</returns>
        bool TryGetValue(string key, out object value);

        /// <summary>
        /// Gets a value on the object from the key provided
        /// </summary>
        /// <param name="key">field name</param>
        /// <returns>object value</returns>
        public object GetValue(string key);

        /// <summary>
        /// Removes a value from fields
        /// </summary>
        /// <param name="key">field name</param>
        /// <returns>IHaveFields Object</returns>
        IHaveFields RemoveField(string key);
    }
}