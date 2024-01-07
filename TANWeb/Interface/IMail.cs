using TANWeb.Models;

namespace TANWeb.Interface
{
    public interface IMail
    {
        Task<bool> SendMail(SendMail sendMail);
    }
}
