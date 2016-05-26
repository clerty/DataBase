using System.Collections.Generic;
using DB;

namespace MyDB
{
    public class Department_Employee : Relationship
    {
        public Department_Employee()
        {
            Cardinality = Cardinality.OneToMany;
        }
    }
}
