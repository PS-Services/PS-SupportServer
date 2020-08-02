using System;
using System.Collections;
using System.Management.Automation;
using System.Data;
using System.Collections.Generic;

namespace PSS.Data.Core
{
    public abstract class PSObjectFactory
    {
        public abstract IEnumerable<PSObject> FromDataReader(IDataReader dataReader);
    }
}
