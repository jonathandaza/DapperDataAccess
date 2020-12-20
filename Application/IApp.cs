using System.Collections.Generic;
using Models;
using Models.Output;

namespace app
{
	public interface IApp
	{
        ResponseMessage Add(Currencies employeeDto);

        ResponseMessage Update(Currencies employeeDto);

        IEnumerable<Currencies> Get();

        Currencies Get(int id);
    }
}

