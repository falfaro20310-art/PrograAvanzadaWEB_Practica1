using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VitalApp_API.Models;

namespace VitalApp_API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(IConfiguration _config) : ControllerBase
    {
        [HttpGet("GetDashboard/{userId}")]
        public IActionResult GetDashboard(int userId)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var response = context.Query<DashboardResponseModel>(
                "sp_GetHomeDashboard",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(response);
        }

        [HttpGet("GetTrend")]
        public IActionResult GetTrend(int userId, int indicatorTypeId, string range)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@IndicatorTypeId", indicatorTypeId);
            parameters.Add("@Range", range);

            var response = context.Query<TrendResponseModel>(
                "sp_GetIndicatorTrend",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(response);
        }
    }
}