using System;
using System.Linq;
using Valid_DynamicFilterSort.Extensions;

namespace Valid_DynamicFilterSort
{
    public class DynamicFilterSortErrors
    {
        private static T MakeException<T>(string defaultMessage, string appendMessage = "") where T : SystemException, new()
        {
            var message = defaultMessage;
            if (!string.IsNullOrWhiteSpace(appendMessage))
            {
                message += Environment.NewLine + appendMessage;
            }

            var messageCtor = typeof(T).GetConstructors()
                .First(f =>
                    f.GetParameters().Length == 1 &&
                    f.GetParameters().First().ParameterType.IsString() &&
                    f.GetParameters().First().Name.Equals("message", StringComparison.InvariantCultureIgnoreCase));

            var exception = (T) messageCtor.Invoke(new object[] {message});
            
            return exception;
        }

        public static InvalidOperationException DFS_ALREADY_CONFIGURED(string msg = "") =>
            MakeException<InvalidOperationException>("Dynamic Filter Sort may only be configured once", msg);

        public static FormatException PARSE_SYNTAX_GENERAL_ERROR (string msg = "") =>
            MakeException<FormatException>("There was an error parsing the provided syntax into a usable format", msg);

        public static FormatException PARSE_SYNTAX_SINGLE_ERROR (string msg = "") =>
            MakeException<FormatException>("There was an error parsing a parameter", msg);

        public static FormatException PARSE_SYNTAX_INVALID_OPERATOR(string msg = "") =>
            MakeException<FormatException>("Invalid operator found when parsing the provided syntax", msg);

        public static InvalidOperationException PARSER_NOT_FOUND_FOR_TYPE (string msg = "")=>
            MakeException<InvalidOperationException>("A syntax parser could not be found for this type", msg);

        public static FormatException PARSE_SYNTAX_INVALID_SORT_DIRECTION(string msg = "") =>
            MakeException<FormatException>("Sort direction could not be determined from the provided syntax", msg);

        public static ArgumentException DATA_INTERFACE_CONFIG_TYPE_MISMATCH(string msg = "") => 
            MakeException<ArgumentException>("Data Interface Types Must Match", msg);

        public static SystemException DATA_TYPE_INTERFACE_NOT_FOUND(string msg = "") =>
            MakeException<SystemException>("Unable to find a data interface for the provided request", msg);

        public static SystemException DATA_ACCESSOR_NOT_FOUND (string msg = "")=>
            MakeException<SystemException>("Unable to find a data accessor for the provided request", msg);

        public static ArgumentException PROPERTY_NAME_INVALID (string msg = "")=>
            MakeException<ArgumentException>("A property name was found to be invalid or missing from the data model", msg);

        public static SystemException DATA_SYNTAX_BUILDER_GENERAL_ERROR (string msg = "")=> 
            MakeException<SystemException>("Unable to build parameter syntax for data source", msg);

        public static SystemException DATA_SYNTAX_BUILDER_NO_MATCHING_SERVICE(string msg = "") =>
            MakeException<SystemException>("Unable to find a data syntax builder for the provided parameter type", msg);
    }
}