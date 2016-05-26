using System.Collections.Generic;
using DB;

namespace MyDB
{
    public class Department : Record
    {
        public Department()
            : base()
        {
            this.Add("Name", new Field(typeof(string)));//FieldType.Varchar
            this.Add("NumberOfEmployees", new Field(typeof(int)));//FieldType.Integer
            Key = new List<Field>();
            Key.Add(this["Name"]);
        }
    }
}
