using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace curzi.lorenzo._5H.EsercitazioneSocket.Models
{
    class Employee
    {
        public string First_name { get; set; }

        public string Last_name { get; set; }

        public string Phone { get; set; }

        public Employee() { }

        public Employee(string name, string surname, string phone)
        {
            First_name = name;
            Last_name = surname;
            Phone = phone;
        }
    }
}
