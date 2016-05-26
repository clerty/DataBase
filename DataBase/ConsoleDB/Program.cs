using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyDB;
using DB;

namespace ConsoleDB
{
    class Program
    {
        static Сompany cmp;
        static void Main(string[] args)
        {
            cmp = new Сompany();
            cmp.Fill<Сompany>(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Сompany"));
            //cmp.CreateNew(System.AppDomain.CurrentDomain.BaseDirectory);
            //Record cus1 = cmp.Create<Customer>("Daniil", "DU");
            //Record cus2 = cmp.Create<Customer>("Daniil", "Ufa");
            //Record contr1 = cmp.Create<Contract>("1", "24.05.2016", "2000");
            //Record contr2 = cmp.Create<Contract>("2", "25.05.2016", "4000");
            Record cus1 = cmp.FindByKey<Contract>("1");
            Console.WriteLine(cus1);
            //cmp.AddToRelationship<Customer_Contract>(cus1, contr1, contr2);
            cmp.Delete(cus1);
        }
    }
}
