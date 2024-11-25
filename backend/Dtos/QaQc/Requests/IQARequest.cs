using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardApi.Dtos.QaQc.Requests;

public class IQARequest
{
    [Column("iqa_no")]
    public string IQANo { get; set; }

    [Column("auditor")]
    public string Auditor { get; set; }

    [Column("department")]
    public string Department { get; set; }

    [Column("site")]
    public string Site { get; set; }

    [Column("project_id")]
    public string ProjectId { get; set; }

    [Column("project_name")]
    public string ProjectName { get; set; }

    [Column("audit_schedule")]
    public DateTime AuditSchedule { get; set; }

    [Column("audit_notification")]
    public DateTime AuditNotification { get; set; }

    [Column("audit_date")]
    public DateTime AuditDate { get; set; }

    [Column("audit_report_date")]
    public DateTime AuditReportDate { get; set; }

    [Column("closed_out_date")]
    public DateTime ClosedOutDate { get; set; }

    [Column("nc")]
    public int NC { get; set; }

    [Column("obs")]
    public int Obs { get; set; }

    [Column("iqa_score")]
    public int IQAScore { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    public int TotalProject { get; set; }

    public double ScoreAvg { get; set; }

    public double SumOverallScore { get; set; }
}