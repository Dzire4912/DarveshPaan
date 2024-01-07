namespace TAN.DomainModels.ViewModels
{
    public class CostAnalysisViewModel
    {
        public string? AccountId { get; set; }
        public string? SubAccount { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Duration { get; set; }
        public string? CallingNumber { get; set; }
        public string? CalledNumber { get; set; }
        public string? CallDirection { get; set; }
        public string? CallType { get; set; }
        public string? Cost { get; set; }
        public string? CallResult { get; set; }
        public List<SelectListText> CallTypes { get; set; }
        public List<SelectListText> CallResults { get; set; }
        public List<SelectListText> CallDirections { get; set; }
        public List<Itemlist> Organizations { get; set; }
        public string? OrganizationName { get; set; }
        public int UserType { get; set; }
    }
}
