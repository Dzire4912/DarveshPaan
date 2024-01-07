using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.UnitTest.Helpers
{ 
    
    public interface IUrlHelperWrapper
    {
        string Page(string pageName, string pageHandler = null, object values = null, string protocol = null);
        string Content(string contentPath);

    }
}
