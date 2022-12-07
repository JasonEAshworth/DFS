using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]

namespace Valid_DynamicFilterSort
{
    public class Errors
    {
        public static readonly VError BAD_FIELD_VALUE_QUERY = new VError("QRY004", "Invalid Field in Query String.");
        public static readonly VError BAD_FILTER_QUERY = new VError("QRY001", "Invalid Filter Query String.");
        public static readonly VError BAD_SORT_ORDER_QUERY = new VError("QRY003", "Invalid Sort Order in Query String.");
        public static readonly VError BAD_SORT_QUERY = new VError("QRY002", "Invalid Sort Query String.");
        public static readonly VError DAPPER_MAP_NOT_FOUND = new VError("QRY009", "A map was not found for this entity type");
        public static readonly VError INVALID_DATETIME = new VError("QRY008", "DateTime Format could not be converted into a usable format.");
        public static readonly VError INVALID_FILTER_TYPE = new VError("QRY006", "Invalid Filter Type.");
        public static readonly VError INVALID_OPERATOR = new VError("QRY007", "Invalid Operator in Query String");
        public static readonly VError MISSING_VALUE_QUERY = new VError("QRY005", "Value Missing in Query String.");
        public static readonly VError CANNOT_CONVERT = new VError("QRY010","Unable to convert a value in the filter");
    }

    public class VError
    {
        public VError(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; set; }
        public string Message { get; set; }
    }
}