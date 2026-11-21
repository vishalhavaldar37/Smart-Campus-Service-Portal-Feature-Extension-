using SmartCampusPortal.DAL;
using SmartCampusPortal.Models;
using System.Web.Http;

namespace SmartCampusPortal.Controllers
{
    [RoutePrefix("api/ServiceApi")]
    public class ServiceApiController : ApiController
    {
        ServiceRequestDAL dal = new ServiceRequestDAL();

        // GET: api/ServiceApi
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetRequests([FromUri] ServiceRequestFilter filter)
        {
            return Ok(dal.GetAll(filter));
        }

        // GET: api/ServiceApi/5
        [HttpGet]
        [Route("{id:int}", Name = "GetServiceRequestById")]
        public IHttpActionResult GetRequest(int id)
        {
            ServiceRequest request = dal.GetById(id);
            if (request == null) return NotFound();
            return Ok(request);
        }

        // GET: api/ServiceApi/Summary
        [HttpGet]
        [Route("Summary")]
        public IHttpActionResult GetSummary()
        {
            return Ok(dal.GetSummary());
        }

        // POST: api/ServiceApi
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateRequest(ServiceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is missing.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Only allow known values
            request.ServiceType = Lookups.Match(Lookups.ServiceTypes, request.ServiceType);
            request.Priority = request.Priority == null
                ? "Medium"
                : Lookups.Match(Lookups.Priorities, request.Priority);

            if (request.ServiceType == null)
            {
                return BadRequest("ServiceType must be one of: " +
                    string.Join(", ", Lookups.ServiceTypes));
            }
            if (request.Priority == null)
            {
                return BadRequest("Priority must be one of: " +
                    string.Join(", ", Lookups.Priorities));
            }

            int newId = dal.Insert(request);

            // 201 Created + Location header + the saved record
            return CreatedAtRoute("GetServiceRequestById",
                new { id = newId }, dal.GetById(newId));
        }

        // PUT: api/ServiceApi/5
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdateRequest(int id, ServiceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is missing.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.RequestId = id;
            request.ServiceType = Lookups.Match(Lookups.ServiceTypes, request.ServiceType);
            request.Priority = Lookups.Match(Lookups.Priorities, request.Priority);

            if (request.ServiceType == null || request.Priority == null)
            {
                return BadRequest("Invalid ServiceType or Priority.");
            }

            if (!dal.Update(request)) return NotFound();
            return Ok(dal.GetById(id));
        }

        // PUT: api/ServiceApi/UpdateStatus/1
        [HttpPut]
        [Route("UpdateStatus/{id:int}")]
        public IHttpActionResult UpdateStatus(int id, [FromBody] string status)
        {
            string valid = Lookups.Match(Lookups.Statuses, status);
            if (valid == null)
            {
                return BadRequest("Status must be one of: " +
                    string.Join(", ", Lookups.Statuses));
            }

            if (!dal.UpdateStatus(id, valid)) return NotFound();
            return Ok("Status updated successfully");
        }

        // DELETE: api/ServiceApi/5
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteRequest(int id)
        {
            if (!dal.Delete(id)) return NotFound();
            return Ok("Service request deleted successfully");
        }
    }
}
