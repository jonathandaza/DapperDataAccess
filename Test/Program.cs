using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infraestructure.Api.SPXDapperDataAccess;
using Models;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SPXDPGenericRepository<Currencies> repo = new SPXDPGenericRepository<Currencies>("ArgosMain");

            Currencies currency = new Currencies() {Code = "BIT", Id = 0, Name= "BITCOIN"};

            var entidad = repo.GetAll();
            
            foreach (var registro in entidad)
            {
                Console.Write(registro.Name);
                Console.Write(Environment.NewLine);
            }

            Console.ReadLine();

        }
    }
}
