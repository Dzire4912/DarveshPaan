using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TAN.DomainModels.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Helpers
{
    public interface IMailContentHelper
    {
        Task<string> CreateMailContent(EmailTemplateViewModel emailTemplateVM);
    }

    [ExcludeFromCodeCoverage]
    public class MailContentHelper:IMailContentHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;
        public MailContentHelper(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;

        }

        public async Task<string> CreateMailContent(EmailTemplateViewModel emailTemplateVM)
        {
            try
            {
                var actionContext = GetActionContext();
                var partial = FindView(actionContext, emailTemplateVM.TemplateFilePath);
                using (var output = new StringWriter())
                {
                    var viewContext = new ViewContext(
                        actionContext,
                        partial,
                        new ViewDataDictionary(
                            metadataProvider: new EmptyModelMetadataProvider(),
                            modelState: new ModelStateDictionary())
                        {
                            Model = emailTemplateVM
                        },
                        new TempDataDictionary(
                            actionContext.HttpContext,
                            _tempDataProvider),
                        output,
                        new HtmlHelperOptions()
                    );
                    await partial.RenderAsync(viewContext);
                    return output.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("", ex.Message);
                return string.Empty;
            }
        }
        private IView FindView(ActionContext actionContext, string partialName)
        {
            try
            {
                var getPartialResult = _viewEngine.GetView(null, partialName, false);
                if (getPartialResult.Success)
                {
                    return getPartialResult.View;
                }
                var findPartialResult = _viewEngine.FindView(actionContext, partialName, false);
                if (findPartialResult.Success)
                {
                    return findPartialResult.View;
                }
                var searchedLocations = getPartialResult.SearchedLocations.Concat(findPartialResult.SearchedLocations);
                var errorMessage = string.Join(
                    Environment.NewLine,
                    new[] { $"Unable to find partial '{partialName}'. The following locations were searched:" }.Concat(searchedLocations)); ;
                throw new InvalidOperationException(errorMessage);
            }
            catch (Exception ex)
            {
                Log.Error("", ex.Message);
                return null;
            }
        }
        private ActionContext GetActionContext()
        {
            try
            {
                var httpContext = new DefaultHttpContext
                {
                    RequestServices = _serviceProvider
                };
                return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            }
            catch (Exception ex)
            {
                Log.Error("", ex.Message);
                return null;
            }
        }
    }
}
