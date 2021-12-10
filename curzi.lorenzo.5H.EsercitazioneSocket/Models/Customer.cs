using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace curzi.lorenzo._5H.EsercitazioneSocket.Models
{
    class Customer
    {
        public string Customer_id { get; set; }

        public string  Customer_name { get; set; }

        public List<Employee> Employees { get; set; }

        public Customer() 
        {
            Employees = new List<Employee>();
        }

        public Customer (string customer_id, string customer_name, List<Employee> employees)
        {
            Customer_id = customer_id;
            Customer_name = customer_name;
            Employees = employees;
        }
    }
}
