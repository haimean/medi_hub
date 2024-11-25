using DashboardApi.Application.DashboardDigicheck;
using Microsoft.AspNetCore.Mvc;

namespace DashboardApi.Apis
{
    public class DigicheckApi : IApis
    {
        public void RegisterApi(WebApplication app)
        {

            // 1. Casting Completion (Live)
            app.MapGet("/dashboard/digicheck/casting-completion",
                async (IDashboardDigicheckService service, [FromQuery] string request) =>
                    await service.CastingCompletion(request)).WithTags("Dashboard.Digicheck");

            // 2. In Progress Modules
            app.MapGet("/dashboard/digicheck/in-progress-modules",
                async (IDashboardDigicheckService service, [FromQuery] string request) =>
                    await service.InProgressModules(request)).WithTags("Dashboard.Digicheck");

            // 3. Monthly Report
            app.MapGet("/dashboard/digicheck/monthly",
                async (IDashboardDigicheckService service, [FromQuery] string request) =>
                    await service.DigicheckDashboardMonthly(request)).WithTags("Dashboard.Digicheck");

            // 4. Monthly Accumulate Report
            app.MapGet("/dashboard/digicheck/monthly-increase",
                async (IDashboardDigicheckService service, [FromQuery] string request) =>
                    await service.DigicheckDashboardMonthlyIncrease(request)).WithTags("Dashboard.Digicheck");

            // 5. Process Table Status
            app.MapGet("/dashboard/digicheck/progress-table",
                async (IDashboardDigicheckService service, [FromQuery] string request) =>
                    await service.ProgressTableBlock(request)).WithTags("Dashboard.Digicheck");

            // 6. Process Table Quantity
            app.MapGet("/dashboard/digicheck/progress-table/unit",
                async (IDashboardDigicheckService service, [FromQuery] string request) =>
                    await service.ProgressTableUnit(request)).WithTags("Dashboard.Digicheck");
        }
    }
}