using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryTestUtilities
{
    public class TestDatabaseOptions
    {
        public string ConnectionString { get; }

        public TestDatabaseOptions(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
