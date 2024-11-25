namespace DashboardApi.Dtos.QaQc.Requests;

public class ModuleStepCombineRequest
{
    public int count { get; set; }
    public DateTime WeekDateFrom { get; set; }
    public DateTime WeekDateTo { get; set; }
    public string NameOf { get; set; }
}