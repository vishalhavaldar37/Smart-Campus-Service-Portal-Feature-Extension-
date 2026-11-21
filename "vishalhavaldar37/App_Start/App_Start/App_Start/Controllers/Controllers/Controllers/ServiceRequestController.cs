using SmartCampusPortal.DAL;
using SmartCampusPortal.Models;
using System.Net;
using System.Web.Mvc;

namespace SmartCampusPortal.Controllers
{
    public class ServiceRequestController : Controller
    {
        ServiceRequestDAL dal = new ServiceRequestDAL();

        // GET: ServiceRequest?search=wifi&status=Pending
        public ActionResult Index(ServiceRequestFilter filter)
        {
            if (filter == null) filter = new ServiceRequestFilter();

            ServiceRequestIndexViewModel vm = new ServiceRequestIndexViewModel
            {
                Filter = filter,
                Requests = dal.GetAll(filter),
                Summary = dal.GetSummary()
            };
            return View(vm);
        }

        // GET: ServiceRequest/Details/1
        public ActionResult Details(int id)
        {
            ServiceRequest request = dal.GetById(id);
            if (request == null) return HttpNotFound();
            return View(request);
        }

        // GET: ServiceRequest/Create
        public ActionResult Create()
        {
            return View(new ServiceRequest { Priority = "Medium" });
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Exclude = "RequestId, Status, RequestDate")] ServiceRequest request)
        {
            if (ModelState.IsValid)
            {
                dal.Insert(request);
                TempData["Message"] = "Service request created successfully.";
                return RedirectToAction("Index");
            }
            return View(request);
        }

        // GET: ServiceRequest/Edit/1
        public ActionResult Edit(int id)
        {
            ServiceRequest request = dal.GetById(id);
            if (request == null) return HttpNotFound();
            return View(request);
        }

        // POST: ServiceRequest/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "RequestId, StudentName, Department, ServiceType, Description, Priority")]
            ServiceRequest request)
        {
            if (ModelState.IsValid)
            {
                if (!dal.Update(request)) return HttpNotFound();
                TempData["Message"] = "Service request updated successfully.";
                return RedirectToAction("Index");
            }
            return View(request);
        }

        // GET: ServiceRequest/UpdateStatus/1
        public ActionResult UpdateStatus(int id)
        {
            ServiceRequest request = dal.GetById(id);
            if (request == null) return HttpNotFound();
            return View(request);
        }

        // POST: ServiceRequest/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            string valid = Lookups.Match(Lookups.Statuses, status);
            if (valid == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest, "Invalid status value.");
            }

            if (!dal.UpdateStatus(id, valid)) return HttpNotFound();
            TempData["Message"] = "Status updated to '" + valid + "'.";
            return RedirectToAction("Index");
        }

        // GET: ServiceRequest/Delete/1 (confirmation page)
        public ActionResult Delete(int id)
        {
            ServiceRequest request = dal.GetById(id);
            if (request == null) return HttpNotFound();
            return View(request);
        }

        // POST: ServiceRequest/Delete/1
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!dal.Delete(id)) return HttpNotFound();
            TempData["Message"] = "Service request deleted.";
            return RedirectToAction("Index");
        }

        // GET: ServiceRequest/Dashboard (page that consumes the Web API with AJAX)
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}
