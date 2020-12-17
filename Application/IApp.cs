using System.Collections.Generic;
using Models;

namespace app
{
	public interface IApp
	{
        Currencies Add(Currencies employeeDto);

        Currencies Update(Currencies employeeDto);

        IEnumerable<Currencies> Get();

        Currencies Get(int id);
    }
}

