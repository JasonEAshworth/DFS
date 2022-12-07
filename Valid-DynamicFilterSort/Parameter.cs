using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using valid.error.management.exceptions;

namespace Valid_DynamicFilterSort
{
    internal enum FilterTypeEnum
    {
        PRIMARY, SECONDARY
    }

    internal class Parameter
    {
        public Parameter(string Key, string Value, Type PropertyType, string Operator = "=", FilterTypeEnum FilterType = FilterTypeEnum.SECONDARY)
        {
            this.Key = Key;
            this.Value = Value;
            this.Operator = Operator;
            this.FilterType = FilterType;
            this.PropertyType = PropertyType;
        }

        public string Id { get; set; }
        public string JsonExtensionDataAttributeName { get; set; }
        public FilterTypeEnum? FilterType { get; set; }
        public char FirstCharValue => Value[0];
        public string Key { get; set; }
        public char LastCharValue => Value[Value.Length - 1];
        public string Operator { get; set; }
        public Type PropertyType { get; set; }
        public Type TruePropertyType
        {
            get
            {
                var ut = Nullable.GetUnderlyingType(PropertyType);
                return ut != null ? ut : PropertyType;
            }
        }

        public bool IsNullableType
        {
            get
            {
                var ut = Nullable.GetUnderlyingType(PropertyType);
                return ut != null;
            }
        }

        public string Value { get; set; }

        public string ValueLowerInvariant => Value.ToLower();

        public object ValueObject {
            get
            {
                if (Nullable.GetUnderlyingType(PropertyType) != null &&
                    Value.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                
                if (IsPartialComparison())
                {
                    return ValueLowerInvariant;
                }

                if (bool.TryParse(ValueLowerInvariant, out var b) && PropertyType.IsNumericOrBoolean())
                {
                    return b;
                }
                
                if (double.TryParse(ValueLowerInvariant, out var num) && PropertyType.IsNumericOrBoolean())
                {
                    return num;
                }

                if(Guid.TryParse(ValueLowerInvariant, out var guid))
                {
                    return guid;
                }
                
                if (DateTime.TryParse(ValueLowerInvariant, out var dt) && IsDateTime())
                {
                    return dt;
                }

                if (PropertyType.IsEnum)
                {
                    try
                    {
                        return Enum.Parse(PropertyType, ValueLowerInvariant, true);
                    }
                    catch
                    {
                        throw new ValidBadRequestException(Errors.CANNOT_CONVERT.Message, Errors.CANNOT_CONVERT.Code);
                    }
                }
                
                return ValueLowerInvariant;
            }
        }
        public bool IsDateTime()
        {
            var propertyType = TruePropertyType;
            return (Type.GetTypeCode(propertyType) == TypeCode.DateTime);
        }

        public bool IsDictionary()
        {
            return PropertyType.IsDictionary();
        }

        public bool IsPartialComparison()
        {
            return (FirstCharValue == '%' || LastCharValue == '%');
        }

        public bool NeedsQuotes()
        {
            if (IsNullableType && ValueLowerInvariant == "null")
            {
                return false;
            }
            
            return !PropertyType.IsNumericOrBoolean() || 
                   PropertyType.IsEnum || 
                   IsPartialComparison() ||
                   IsDictionary();
        }
        public bool IsValidConvert()
        {
            try
            {
                if (PropertyType.IsEnum)
                {
                    return !IsPartialComparison() && ValueObject.GetType().IsEnum;
                }
                
                if (PropertyType != typeof(string))
                {
                    var underlyingType = Nullable.GetUnderlyingType(PropertyType);
                    
                    if ( underlyingType != null && Value.Equals("null",StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                    
                    var propertyType = underlyingType ?? PropertyType;
                    Convert.ChangeType(ValueObject, propertyType);
                }
            }
            catch
            {
                return IsPartialComparison() || IsDictionary();
            }

            return true;
        }
    }
}