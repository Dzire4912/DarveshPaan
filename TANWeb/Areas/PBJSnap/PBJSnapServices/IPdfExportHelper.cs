using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.PBJSnap.PBJSnapServices
{
    public interface IPdfExportHelper
    {
        Task<string> RenderToStringAsync(string viewName, object model);
        public class PdfExportHelper : IPdfExportHelper
        {
            private readonly IRazorViewEngine _razorViewEngine;
            private readonly ITempDataProvider _tempDataProvider;
            private readonly IServiceProvider _serviceProvider;
            private readonly IConfiguration _configuration;
            [ExcludeFromCodeCoverage]
            public PdfExportHelper(IRazorViewEngine razorViewEngine,
                ITempDataProvider tempDataProvider,
                IServiceProvider serviceProvider, IConfiguration configuration)
            {
                _razorViewEngine = razorViewEngine;
                _tempDataProvider = tempDataProvider;
                _serviceProvider = serviceProvider;
                _configuration = configuration;
            }
            [ExcludeFromCodeCoverage]
            public async Task<string> RenderToStringAsync(string viewName, object model)
            {
                try
                {
                    var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
                    var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

                    using (var sw = new StringWriter())
                    {
                        var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);
                        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                        {
                            Model = model
                        };

                        var viewContext = new ViewContext(
                            actionContext,
                            viewResult.View,
                            viewDictionary,
                            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                            sw,
                            new HtmlHelperOptions()
                        );

                        await viewResult.View.RenderAsync(viewContext);
                        return sw.ToString();
                    }
                }
                catch (Exception)
                { 
                    throw;
                }
                return "";
                
            }
        }
        [ExcludeFromCodeCoverage]
        class CustomPageEventHandler : PdfPageEventHelper
        {
            private readonly IConfiguration _configuration;
            public CustomPageEventHandler(IConfiguration configuration)
            {
                _configuration = configuration;
            }
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);
                string logo1 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images", "logo1.png"); ;
                string logo2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images", "logo2.png"); ;
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;

                Image img1 = Image.GetInstance(logo1);
                Image img2 = Image.GetInstance(logo2);
                img1.ScaleToFit(100, 100);
                img2.ScaleToFit(100, 100);
                float pageWidth = document.PageSize.Width;
                float pageHeight = document.PageSize.Height;
                float xCoordinateLeft = document.LeftMargin;
                float imageWidth = 100; // Adjust as needed
                float imageHeight = 100;
                float xCoordinateRight = pageWidth - document.RightMargin - imageWidth;

                float yCoordinate = pageHeight - document.TopMargin;

                PdfContentByte contentByte = writer.DirectContent;
                img1.SetAbsolutePosition(xCoordinateLeft + 4, yCoordinate);
                contentByte.AddImage(img1);
                img2.SetAbsolutePosition(xCoordinateRight - 4, yCoordinate);
                contentByte.AddImage(img2);
            }
        }
    }
}
