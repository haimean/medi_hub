namespace DashboardApi.Dtos.DashboardWorker.Response;

public class UnitDetail
{
    public string? activity { get; set; }

    public double? duration { get; set; }


    public DateTime date { get; set; }

    public string worker { get; set; }

    public string supervisor { get; set; }

    public UnitDetail(string activity, double duration, DateTime date, string worker, string supervisor)
    {
        this.activity = activity;
        this.duration = duration;
        this.date = date;
        this.worker = worker;
        this.supervisor = supervisor;
    }
}