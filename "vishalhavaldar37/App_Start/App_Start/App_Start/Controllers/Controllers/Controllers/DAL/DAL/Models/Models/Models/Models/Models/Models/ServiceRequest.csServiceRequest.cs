using System;
using System.ComponentModel.DataAnnotations;

namespace SmartCampusPortal.Models
{
    public class ServiceRequest
    {
        public int RequestId { get; set; }

        [Required(ErrorMessage = "Student name is required.")]
        [StringLength(100, ErrorMessage = "Student name can be at most 100 characters.")]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [StringLength(100, ErrorMessage = "Department can be at most 100 characters.")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Service type is required.")]
        [StringLength(50)]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 500 characters.")]
        public string Description { get; set; }

        [StringLength(20)]
        public string Priority { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; }
    }
}
