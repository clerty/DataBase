using System.Collections.Generic;
using System;

namespace DB
{
    //public enum FieldType { Integer, Double, Char, Varchar, DateTime }
    public class Field
    {
        //string _value;
        public Type Type { get; private set; }
        public string Value { get; set; }
        //public string Value
        //{
        //    get
        //    {
        //        return _value;
        //    }
        //    set
        //    {
        //        switch (Type)
        //        {
        //            case FieldType.Integer:
        //                _value = value;
        //                break;
        //            case FieldType.Double:
        //                _value = value;
        //                break;
        //            case FieldType.Char:
        //                _value = value;
        //                break;
        //            case FieldType.Varchar:
        //                _value = value;
        //                break;
        //            case FieldType.DateTime:
        //                _value = value;
        //                break;
        //            default:
        //                _value = value;
        //                break;
        //        }
        //    }
        //}
        public Field(Type type)
        {
            Type = type;
        }
        public override string ToString()
        {
            return Value;
        }
    }
}
