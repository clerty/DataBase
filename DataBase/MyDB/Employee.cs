using System.Collections.Generic;
using DB;

namespace MyDB
{
    public class Employee : Record
    {
        public Employee()
            : base()
        {
            this.Add("Surname", new Field(typeof(string)));//FieldType.Varchar
            this.Add("Position", new Field(typeof(string)));//FieldType.Varchar
            this.Add("Salary", new Field(typeof(int)));//FieldType.Integer
            Key = new List<Field>();
            Key.Add(this["Surname"]);
            Key.Add(this["Position"]);
            Key.Add(this["Salary"]);
        }
    }
}
