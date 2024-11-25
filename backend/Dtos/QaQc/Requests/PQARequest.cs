using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DashboardApi.Dtos.QaQc.Requests;

public class PQARequest
{
    [Column("id_report")]
    public int? IdReport { get; set; }

    [Column("template_name")]
    public string TemplateName { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("location")]
    public string Location { get; set; }

    [Column("template_type")]
    public string TemplateType { get; set; }

    [Column("submitted_by")]
    public string SubmittedBy { get; set; }

    [Column("status")]
    public string Status { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("notes")]
    public string Notes { get; set; }

    [Column("last_update")]
    public DateTime LastUpdate { get; set; }

    [Column("last_update_by")]
    public string LastUpdateBy { get; set; }

    [Column("one_trade")]
    public string OneTrade { get; set; }

    [Column("one_trade_location")]
    public string OneTradeLocation { get; set; }

    [Column("two_trade")]
    public string TwoTrade { get; set; }

    [Column("two_trade_location")]
    public string TwoTradeLocation { get; set; }

    [Column("foureone_na")]
    public double? FoureoneNA { get; set; }

    [Column("foureone_no")]
    public double? FoureoneNO { get; set; }

    [Column("foureone_partial")]
    public double? FoureonePartial { get; set; }

    [Column("foureone_total")]
    public double? FoureoneTotal { get; set; }

    [Column("foureone_yes")]
    public double? FoureoneYes { get; set; }

    [Column("audit_date")]
    public DateTime AuditDate { get; set; }

    [Column("auditor")]
    public string Auditor { get; set; }

    [Column("close")]
    public JsonDocument Close { get; set; }

    [Column("conform")]
    public JsonDocument Conform { get; set; }

    [Column("findings_score")]
    public double? FindingsScore { get; set; }

    [Column("findings")]
    public JsonDocument Findings { get; set; }

    [Column("frequency")]
    public JsonDocument Frequency { get; set; }

    [Column("image_af_image")]
    public JsonDocument ImageAfImage { get; set; }

    [Column("matrix")]
    public JsonDocument Matrix { get; set; }

    [Column("observation_followup_score")]
    public double? ObservationFollowupScore { get; set; }

    [Column("observation_three")]
    public JsonDocument ObservationThree { get; set; }

    [Column("overall_score")]
    public double? OverallScore { get; set; }

    [Column("pqa_followup_score")]
    public double? PqaFollowupScore { get; set; }

    [Column("points")]
    public JsonDocument Points { get; set; }

    [Column("project")]
    public string Project { get; set; }

    [Column("remarks_three")]
    public JsonDocument RemarksThree { get; set; }

    [Column("severity")]
    public JsonDocument Severity { get; set; }

    [Column("site_findings_approved_materials")]
    public string SiteFindingsApprovedMaterials { get; set; }

    [Column("site_findings_approved_two")]
    public string SiteFindingsApprovedMaterialsTwo { get; set; }

    [Column("site_findings_approved_statement")]
    public string SiteFindingsApprovedMaterialsStatement { get; set; }

    [Column("site_findings_approved_method_statement_two")]
    public string SiteFindingsApprovedMethodStatementTwo { get; set; }

    [Column("site_findings_common_check_implemented")]
    public string SiteFindingsCommonCheckImplemented { get; set; }

    [Column("site_findings_common_check_implemented_two")]
    public string SiteFindingsCommonCheckImplementedTwo { get; set; }

    [Column("site_findings_critical_check_implemented")]
    public string SiteFindingsCriticalCheckImplemented { get; set; }

    [Column("site_findings_critical_check_implemented_two")]
    public string SiteFindingsCriticalCheckImplementedTwo { get; set; }

    [Column("site_findings_latest_approved_drawing")]
    public string SiteFindingsLatestApprovedDrawing { get; set; }

    [Column("site_findings_latest_approved_drawing_two")]
    public string SiteFindingsLatestApprovedDrawingTwo { get; set; }

    [Column("site_findings_test_report_submission_pass")]
    public string SiteFindingsTestReportSubmissionPass { get; set; }

    [Column("site_findings_test_report_submission_pass_two")]
    public string SiteFindingsTestReportSubmissionPassTwo { get; set; }

    [Column("site_findings_trade_book")]
    public string SiteFindingsTradeBook { get; set; }

    [Column("site_findings_trade_book_two")]
    public string SiteFindingsTradeBookTwo { get; set; }

    [Column("site_findings_trade_demo_gpt_conducted")]
    public string SiteFindingsTradeDemoGPTConducted { get; set; }

    [Column("site_findings_trade_demo_gpt_conducted_two")]
    public string SiteFindingsTradeDemoGPTConductedTwo { get; set; }

    [Column("trade_one_score")]
    public double? TradeOneScore { get; set; }

    [Column("trade_two_score")]
    public double? TradeTwoScore { get; set; }

    [Column("weightage")]
    public JsonDocument Weightage { get; set; }

    [Column("link")]
    public string Link { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    public int TotalProject { get; set; }

    public double ScoreAvg { get; set; }

    public double SumOverallScore { get; set; }
}