using System.Collections.Generic;
using Models;
using Models.Output;

namespace app
{
	public interface IApp
	{
        ResponseMessage Add(Currencies currency);

        ResponseMessage Add(IEnumerable<Currencies> currencies);

        ResponseMessage Update(Currencies currency);

        IEnumerable<Currencies> Get();

        Currencies Get(int id);
    }
}

