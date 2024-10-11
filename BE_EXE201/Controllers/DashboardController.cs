/*using BE_EXE201.Services;
using Microsoft.AspNetCore.Mvc;

namespace BE_EXE201.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly DashboardServices _dashboardServices;
        public DashboardController(DashboardServices dashboardServices) { _dashboardServices = dashboardServices; }

        [HttpGet("userCount")]
        public async Task<IActionResult> GetTotalUsersCount()
        {
            var count = await _dashboardServices.GetTotalUsersCount();
            return Ok(new { TotalUsers = count });
        }
        [HttpGet("postCount")]
        public async Task<IActionResult> GetTotalHomeCount()
        {
            var count = await _dashboardServices.GetTotalHomePosts();
            return Ok(new { TotalPosts = count });
        }

        [HttpGet("total-earnings")]
        public async Task<IActionResult> GetTotalEarnings()
        {
            var totalEarnings = await _dashboardServices.GetTotalEarningsFromActiveTransactions();
            return Ok(new { TotalEarnings = totalEarnings });
        }

        [HttpGet("total-earnings-by-day")]
        public async Task<IActionResult> GetTotalEarningsByDay()
        {
            var totalEarnings = await _dashboardServices.GetTotalEarningsByDayForLastSevenDays();
            return Ok(totalEarnings);
        }

        [HttpGet("recent-users")]
        public async Task<IActionResult> GetRecentUsers([FromQuery] int count = 10)
        {
            var recentUsers = await _dashboardServices.GetRecentUsers(count);
            return Ok(recentUsers);
        }
    }
}
*/