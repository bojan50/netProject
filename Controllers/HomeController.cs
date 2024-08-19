using Microsoft.AspNetCore.Mvc;
using netProject.Models;
using netProject.Service;

namespace netProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly TimeEntryService _timeEntryService;

        public HomeController(TimeEntryService timeEntryService)
        {
            _timeEntryService = timeEntryService;
        }

        public async Task<IActionResult> Index()
        {
            var timeEntries = await _timeEntryService.GetTimeEntries();
            var employeeTimeDictionary = _timeEntryService.CalculateEmployeeTime(timeEntries);

            List<EmployeeMonthlyTime> employeeMonthlyTimes = employeeTimeDictionary
                .Select(e => new EmployeeMonthlyTime
                {
                    EmployeeName = e.Key,
                    TotalTimeInHours = e.Value
                })
                .OrderByDescending(e => e.TotalTimeInHours)
                .ToList();

            

            return View(employeeMonthlyTimes);
        }
    }
}
