namespace ContractClaimSystem.Models
{
    public class HRSummaryViewModel
    {
        public List<HRClaimSummaryViewModel> LecturerSummaries { get; set; } = new();
        public decimal GrandTotalHours { get; set; }
        public decimal GrandTotalPayment { get; set; }

    }
}
