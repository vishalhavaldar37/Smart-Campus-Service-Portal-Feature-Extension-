using System.Collections.Generic;

namespace SmartCampusPortal.Models
{
    public class ServiceRequestIndexViewModel
    {
        public ServiceRequestFilter Filter { get; set; }
        public List<ServiceRequest> Requests { get; set; }
        public DashboardSummary Summary { get; set; }
    }
}
