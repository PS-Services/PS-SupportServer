using System.Collections;
using System.Collections.Generic;

namespace PSS.Data.Core.Tests
{
    public class PsObjectConverterTestsDataSet1 : IEnumerable<object[]>
    {
        readonly object[] _data = {
            new {
                A=1, B=2, C=3
            },
            new
            {
                AA="11", BB="22", CC="33"
            },
            };

        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var item in _data) yield return new object[] { item };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}


/*
 * (C) 2019 Your Legal Entity's Name
 */
