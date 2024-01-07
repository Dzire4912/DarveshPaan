using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.PBJSnap.Data.PBJSnapServices
{
    [ExcludeFromCodeCoverage]
    public static class ConvertRazorViewToPDF
    {
        [ExcludeFromCodeCoverage]
        public static string RenderViewToString(this Controller controller, string viewName, object model, ITempDataProvider tempDataProvider)
        {
            var httpContext = controller.HttpContext;
            var actionContext = new ActionContext(httpContext, new RouteData(), controller.ControllerContext.ActionDescriptor);

            var viewEngine = controller.HttpContext.RequestServices.GetService<ICompositeViewEngine>();

            using (var writer = new StringWriter())
            {
                var viewResult = viewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), controller.ModelState);
                viewDictionary.Model = model;

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    writer,
                    new HtmlHelperOptions()
                );

                var t = viewResult.View.RenderAsync(viewContext);

                t.Wait();

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
