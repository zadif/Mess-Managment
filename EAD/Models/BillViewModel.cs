using Microsoft.AspNetCore.Mvc;

namespace EAD.Models
{
    public class BillViewModel
    {
        public List<Bill> Bil { get; set; }
        public List<BillRecheckRequest> Recheck { get; set; }

        // This will bind perfectly from checkboxes
        public List<bool> BillInRecheck { get; set; } = new List<bool>();

        public BillViewModel() { } // Needed for model binding

        public BillViewModel(List<Bill> bil, List<BillRecheckRequest> recheck)
        {
            Bil = bil ?? new List<Bill>();
            Recheck = recheck ?? new List<BillRecheckRequest>();

            var recheckIds = recheck.Where(r => r.Status == "Pending").Select(r => r.BillId).ToHashSet();

            BillInRecheck = Bil.Select(b => recheckIds.Contains(b.Id)).ToList();
        }
    }

}


