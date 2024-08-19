using Microsoft.Extensions.Options;
using netProject.Models;
using System.Drawing;
using System.Text.Json;

namespace netProject.Service
{
    public class TimeEntryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _requestUrl;

        public TimeEntryService(HttpClient httpClient, IOptions<TimeEntryApiOptions> options)
        {
            _httpClient = httpClient;
            _requestUrl = options.Value.RequestUrl;
        }

        public async Task<List<TimeEntry>> GetTimeEntries()
        {
            HttpResponseMessage message = await _httpClient.GetAsync(_requestUrl);
            string content = await message.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TimeEntry>>(content)!;
        }

        public Dictionary<string, double> CalculateEmployeeTime(List<TimeEntry> timeEntries)
        {
            Dictionary<string, double> employeeTimeDictionary = new Dictionary<string, double>();

            foreach (var entry in timeEntries)
            {
                if (string.IsNullOrEmpty(entry.EmployeeName))
                {
                    continue;
                }

                double hoursWorked = (entry.EndTimeUtc - entry.StarTimeUtc).TotalHours;

                if (employeeTimeDictionary.ContainsKey(entry.EmployeeName))
                {
                    employeeTimeDictionary[entry.EmployeeName] += hoursWorked;
                }
                else
                {
                    employeeTimeDictionary[entry.EmployeeName] = hoursWorked;
                }
            }

            return employeeTimeDictionary;
        }
    }
}
