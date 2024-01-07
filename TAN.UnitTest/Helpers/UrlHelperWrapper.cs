using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.UnitTest.Helpers
{
    public class UrlHelperWrapper:IUrlHelperWrapper
    {
        private readonly IUrlHelper _urlHelper;

        public UrlHelperWrapper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        public string Page(string pageName, string pageHandler = null, object values = null, string protocol = null)
        {
            return _urlHelper.Page(pageName, pageHandler, values, protocol);
        }

        public string Content(string contentPath)
        {
            return _urlHelper.Content(contentPath);
        }

    }
}
