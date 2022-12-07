using System;
using Valid_DynamicFilterSort.Enums;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// A string to type value conversion and verification service
    /// </summary>
    public interface IDataTypeValueHandler : IDisposable
    {
        /// <summary>
        /// Verifies that the string value passed can be converted to one of the simple list of data types
        /// provided in the DataTypeEnum
        /// </summary>
        /// <param name="value">value to verify if conversion is possible</param>
        /// <param name="dataType">data type from simple list to verify conversion is possible</param>
        /// <returns>bool for valid conversion</returns>
        bool Validate(string value, DataTypeEnum dataType);
        /// <summary>
        /// Verifies that the string value passed can be converted to type T
        /// </summary>
        /// <param name="value">value to verify if conversion is possible</param>
        /// <typeparam name="T">data type to verify conversion into</typeparam>
        /// <returns>bool for valid conversion</returns>
        bool Validate<T>(string value);
        /// <summary>
        /// Verifies that the string value passed can be converted to type
        /// </summary>
        /// <param name="value">value to verify if conversion is possible</param>
        /// <param name="type">data type to verify conversion into</param>
        /// <returns>bool for valid conversion</returns>
        bool Validate(string value, Type type);
        /// <summary>
        /// Verifies and converts string passed in to type T
        /// </summary>
        /// <param name="value">value to verify and convert</param>
        /// <param name="result">value converted to type T</param>
        /// <typeparam name="T">data type to verify and on success, return result as an out parameter</typeparam>
        /// <returns>bool for valid conversion; out T result</returns>
        bool ValidateAndConvert<T>(string value, out T result);
        /// <summary>
        /// Verifies and converts string passed in to type
        /// </summary>
        /// <param name="value">value to verify and convert</param>
        /// <param name="type">data type to verify and on success, return result as an out parameter</param>
        /// <param name="result">value converted to type T</param>
        /// <returns>bool for valid conversion; out object result</returns>
        bool ValidateAndConvert(string value, Type type, out object result);
        /// <summary>
        /// Verifies and converts string passed in to type
        /// </summary>
        /// <param name="value">value to verify and convert</param>
        /// <param name="dataType">data type from simple list to verify conversion is possible</param>
        /// <param name="result">value converted to type T</param>
        /// <typeparam name="T">on success, returns result as an out parameter</typeparam>
        /// <returns>bool for valid conversion; out object result</returns>
        bool ValidateAndConvert<T>(string value, DataTypeEnum dataType, out T result);
        /// <summary>
        /// Converts a string to type T
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <typeparam name="T">type to convert into</typeparam>
        /// <returns>T</returns>
        T ConvertString<T>(string value);
        /// <summary>
        /// Converts a string to type T
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="type">type to convert into</param>
        /// <returns>object</returns>
        object ConvertString(string value, Type type);
    }
}