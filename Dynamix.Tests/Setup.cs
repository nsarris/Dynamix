using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading;
using System.Globalization;

namespace Dynamix.Tests
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
        }

    }
}
