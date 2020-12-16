﻿using System.Collections.Generic;
using Models;

namespace app
{
	public interface IApp
	{
        Currencies Add(Currencies employeeDto);

        IEnumerable<Currencies> Get();
    }
}

