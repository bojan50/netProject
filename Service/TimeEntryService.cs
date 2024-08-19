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

        public void GeneratePieChart(Dictionary<string, double> employeeTimeDictionary, string outputPath)
        {
            double totalHours = employeeTimeDictionary.Values.Sum();

            int width = 900;
            int height = 600;

            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                Rectangle rect = new Rectangle(50, 100, 500, 500);

                Color[] colors = new Color[]
                {
                    Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple,
                    Color.Cyan, Color.Magenta, Color.YellowGreen, Color.Brown,
                    Color.Pink, Color.Gray, Color.Olive, Color.Maroon,
                    Color.Teal, Color.Lime
                };

                float startAngle = 0f;
                int colorIndex = 0;

                foreach (var entry in employeeTimeDictionary)
                {
                    float sweepAngle = (float)(entry.Value / totalHours * 360);

                    using (var brush = new SolidBrush(colors[colorIndex % colors.Length]))
                    {
                        graphics.FillPie(brush, rect, startAngle, sweepAngle);
                    }

                    float midAngle = startAngle + sweepAngle / 2;
                    float labelX = (float)(rect.X + rect.Width / 2 + Math.Cos(midAngle * Math.PI / 180) * rect.Width / 3);
                    float labelY = (float)(rect.Y + rect.Height / 2 + Math.Sin(midAngle * Math.PI / 180) * rect.Height / 3);

                    string percentage = $"{entry.Value / totalHours:P0}";

                    using (var font = new Font("Arial", 12, FontStyle.Bold))
                    using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        graphics.DrawString(percentage, font, Brushes.White, new PointF(labelX, labelY), format);
                    }

                    startAngle += sweepAngle;
                    colorIndex++;
                }

                int legendX = 600;
                int legendY = 100;

                colorIndex = 0;
                foreach (var entry in employeeTimeDictionary)
                {
                    using (var brush = new SolidBrush(colors[colorIndex % colors.Length]))
                    {
                        graphics.FillRectangle(brush, legendX, legendY, 20, 20);
                    }

                    graphics.DrawString(entry.Key, new Font("Arial", 12), Brushes.Black, new PointF(legendX + 30, legendY));

                    legendY += 30;
                    colorIndex++;
                }

                bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
