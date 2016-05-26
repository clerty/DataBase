using System.Collections.Generic;
using DB;

namespace MyDB
{
    public class ContractEmployeeRelationship : Relationship
    {
        public void AddMembers(Dictionary<Contract, Employee> executors)
        {
            Cardinality = Cardinality.ManyToMany;
            foreach (KeyValuePair<Contract, Employee> executor in executors)
            {
                Connections.Add(executor.Key, executor.Value);
            }
        }
    }
}
