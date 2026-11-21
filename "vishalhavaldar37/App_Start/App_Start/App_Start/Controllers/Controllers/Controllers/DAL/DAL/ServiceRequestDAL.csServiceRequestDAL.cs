using SmartCampusPortal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SmartCampusPortal.DAL
{
    public class ServiceRequestDAL
    {
        private readonly string cs =
            ConfigurationManager.ConnectionStrings["CampusConnection"].ConnectionString;

        private const string SelectColumns =
            "SELECT RequestId, StudentName, Department, ServiceType, " +
            "Description, Priority, Status, RequestDate FROM ServiceRequests";

        // Get all requests (optionally filtered)
        public List<ServiceRequest> GetAll(ServiceRequestFilter filter)
        {
            List<ServiceRequest> list = new List<ServiceRequest>();
            StringBuilder sql = new StringBuilder(SelectColumns + " WHERE 1 = 1");

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Search))
                    {
                        string term = filter.Search.Trim();
                        if (term.Length > 100) term = term.Substring(0, 100);
                        sql.Append(" AND (StudentName LIKE @Search" +
                                   " OR Department LIKE @Search" +
                                   " OR Description LIKE @Search)");
                        cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 120).Value =
                            "%" + term + "%";
                    }
                    if (!string.IsNullOrWhiteSpace(filter.ServiceType))
                    {
                        sql.Append(" AND ServiceType = @ServiceType");
                        cmd.Parameters.Add("@ServiceType", SqlDbType.NVarChar, 50).Value =
                            filter.ServiceType;
                    }
                    if (!string.IsNullOrWhiteSpace(filter.Priority))
                    {
                        sql.Append(" AND Priority = @Priority");
                        cmd.Parameters.Add("@Priority", SqlDbType.NVarChar, 20).Value =
                            filter.Priority;
                    }
                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        sql.Append(" AND Status = @Status");
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value =
                            filter.Status;
                    }
                }

                sql.Append(" ORDER BY RequestDate DESC, RequestId DESC");
                cmd.CommandText = sql.ToString();
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(Map(dr));
                    }
                }
            }
            return list;
        }

        public List<ServiceRequest> GetAll()
        {
            return GetAll(null);
        }

        // Get a single request (null when it does not exist)
        public ServiceRequest GetById(int id)
        {
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(
                SelectColumns + " WHERE RequestId = @RequestId", con))
            {
                cmd.Parameters.Add("@RequestId", SqlDbType.Int).Value = id;
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    return dr.Read() ? Map(dr) : null;
                }
            }
        }

        // Counts for the dashboard cards
        public DashboardSummary GetSummary()
        {
            const string query = @"
                SELECT
                    COUNT(*) AS Total,
                    ISNULL(SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END), 0) AS Pending,
                    ISNULL(SUM(CASE WHEN Status = 'In Progress' THEN 1 ELSE 0 END), 0) AS InProgress,
                    ISNULL(SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END), 0) AS Completed,
                    ISNULL(SUM(CASE WHEN Priority = 'High' AND Status <> 'Completed' THEN 1 ELSE 0 END), 0) AS HighPriorityOpen
                FROM ServiceRequests";

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    DashboardSummary s = new DashboardSummary();
                    if (dr.Read())
                    {
                        s.Total = Convert.ToInt32(dr["Total"]);
                        s.Pending = Convert.ToInt32(dr["Pending"]);
                        s.InProgress = Convert.ToInt32(dr["InProgress"]);
                        s.Completed = Convert.ToInt32(dr["Completed"]);
                        s.HighPriorityOpen = Convert.ToInt32(dr["HighPriorityOpen"]);
                    }
                    return s;
                }
            }
        }

        // Insert new request and return the generated RequestId
        public int Insert(ServiceRequest request)
        {
            const string query = @"
                INSERT INTO ServiceRequests
                    (StudentName, Department, ServiceType, Description, Priority, Status)
                VALUES
                    (@StudentName, @Department, @ServiceType, @Description, @Priority, @Status);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                AddEditableParameters(cmd, request);
                // New requests always start as Pending
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = "Pending";
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Update the editable fields (not Status / date). Returns false if the Id does not exist.
        public bool Update(ServiceRequest request)
        {
            const string query = @"
                UPDATE ServiceRequests
                SET StudentName = @StudentName,
                    Department = @Department,
                    ServiceType = @ServiceType,
                    Description = @Description,
                    Priority = @Priority
                WHERE RequestId = @RequestId";

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                AddEditableParameters(cmd, request);
                cmd.Parameters.Add("@RequestId", SqlDbType.Int).Value = request.RequestId;
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Update request status. Returns false if the Id does not exist.
        public bool UpdateStatus(int id, string status)
        {
            const string query = @"
                UPDATE ServiceRequests
                SET Status = @Status
                WHERE RequestId = @RequestId";

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
                cmd.Parameters.Add("@RequestId", SqlDbType.Int).Value = id;
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Delete a request. Returns false if the Id does not exist.
        public bool Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(
                "DELETE FROM ServiceRequests WHERE RequestId = @RequestId", con))
            {
                cmd.Parameters.Add("@RequestId", SqlDbType.Int).Value = id;
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private static void AddEditableParameters(SqlCommand cmd, ServiceRequest r)
        {
            cmd.Parameters.Add("@StudentName", SqlDbType.NVarChar, 100).Value = r.StudentName;
            cmd.Parameters.Add("@Department", SqlDbType.NVarChar, 100).Value = r.Department;
            cmd.Parameters.Add("@ServiceType", SqlDbType.NVarChar, 50).Value = r.ServiceType;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = r.Description;
            cmd.Parameters.Add("@Priority", SqlDbType.NVarChar, 20).Value =
                string.IsNullOrWhiteSpace(r.Priority) ? "Medium" : r.Priority;
        }

        private static ServiceRequest Map(SqlDataReader dr)
        {
            return new ServiceRequest
            {
                RequestId = Convert.ToInt32(dr["RequestId"]),
                StudentName = dr["StudentName"].ToString(),
                Department = dr["Department"].ToString(),
                ServiceType = dr["ServiceType"].ToString(),
                Description = dr["Description"].ToString(),
                Priority = dr["Priority"].ToString(),
                Status = dr["Status"].ToString(),
                RequestDate = Convert.ToDateTime(dr["RequestDate"])
            };
        }
    }
}
