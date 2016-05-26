using System.Collections.Generic;
using DB;

namespace MyDB
{
    public class Customer_Contract : Relationship
    {
        public Customer_Contract()
        {
            Cardinality = Cardinality.OneToMany;
        }
    }
}
