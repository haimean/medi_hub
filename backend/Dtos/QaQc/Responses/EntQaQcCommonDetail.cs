using Newtonsoft.Json;

namespace DashboardApi.Dtos.QaQc.Responses
{
    public class EntQaQcCommonDetail
    {
        public string project { get; set; }
        public string project_id { get; set; }
        public string name { get; set; }
        public string per_month { get; set; }
        public string key { get; set; }
        public string trade_full_name { get; set; }
        public int? stage { get; set; }
        public string color { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string trade { get; set; }
        public string sub_trade { get; set; }
        public string check_list { get; set; }
        public string discipline { get; set; }
        public string short_name { get; set; }
        public string shortform { get; set; }
        public string created_at { get; set; }

        public int no_of_no { get; set; }
        public int no_of_yes { get; set; }
        public int total_no { get; set; }
        public int total_na { get; set; }
        public int total_yes { get; set; }

        public int total
        {
            get
            {
                return total_no + total_yes;
            }
        }

        public double percent
        {
            get
            {
                double res = 0;
                if (total != 0)
                {
                    res = 10000 * total_no / total;
                }

                return Math.Round(res / 100, 1);
            }
        }
    }

    public class EntQaQcCriticalDetail
    {
        public int month { get; set; }
        public int year { get; set; }
        public string project { get; set; }
        public string trade { get; set; }
        public string shortform { get; set; }
        public string discipline { get; set; }

        public int no_of_no { get; set; }
        public int no_of_1st { get; set; }
        public int no_of_1plus { get; set; }

        public int total { get; set; }

        public double percent
        {
            get
            {
                double res = 0;
                if (no_of_no + no_of_1st + no_of_1plus != 0)
                {
                    res = 10000 * no_of_1st / (no_of_no + no_of_1st + no_of_1plus);
                }

                return Math.Round(res / 100, 1);
            }
        }
    }

    public class EntQaQcCriticalDetailV2
    {
        public string project { get; set; }
        public string project_id { get; set; }
        public string name { get; set; }
        public string per_month { get; set; }
        public string key { get; set; }
        public string trade_full_name { get; set; }
        public string color { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string trade { get; set; }
        public string sub_trade { get; set; }
        public string check_list { get; set; }
        public string discipline { get; set; }
        public string short_name { get; set; }

        public int total_no { get; set; }
        public int total_1st { get; set; }
        public int total_1plus { get; set; }

        public int total { get; set; }

        public double percent
        {
            get
            {
                double res = 0;
                if (total_no + total_1st + total_1plus != 0)
                {
                    res = 10000 * total_1st / (total_no + total_1st + total_1plus);
                }

                return Math.Round(res / 100, 1);
            }
        }
    }


    public class EntQaQcRework
    {
        public string subcontractor { get; set; }
        public int total { get; set; }
        public string yyyymm { get; set; }
        public string mmyy { get; set; }
    }

    public class EntQaQcViolation
    {
        public string subcontractor { get; set; }
        public int total { get; set; }
        public int demeritPoint { get; set; }
        public int withholdingAmount { get; set; }
        public string yyyymm { get; set; }
        public string mmyy { get; set; }
    }

    public class EntQaQcObservation
    {
        public string sxf { get; set; }
        public int total { get; set; }
        public string project { get; set; }
        public string yyyymm { get; set; }
        public string mmyy { get; set; }
    }
    public class EntQaQcObservationStaticReport
    {
        public int reports { get; set; }
        public int totalDraft { get; set; }
        public int totalQa { get; set; }
        public int totalPm { get; set; }
    }

    public class EntQaQcCommonSum
    {
        public DateTime created_at { get; set; }
        public string project { get; set; }
        public int no_of_no { get; set; }
        public int no_of_yes { get; set; }
        public double total
        {
            get
            {
                return no_of_no + no_of_yes;
            }
        }
        public int month { get; set; }
        public int year { get; set; }

        public double percent
        {
            get
            {
                double res = 0;
                if (total != 0)
                {
                    res = 1000 * no_of_no / (total);
                }

                return Math.Round(res / 100, 1);
            }
        }
    }

    public class EntQaQcCritical
    {
        public string project { get; set; }
        public DateTime created_at { get; set; }
        public string trade { get; set; }
        public string sub_trade { get; set; }
        public string check_list { get; set; }
        public string discipline { get; set; }
        public string short_name { get; set; }
        public string shortform { get; set; }
        public int no_of_no { get; set; }
        public int no_of_1st { get; set; }
        public int no_of_1plus { get; set; }

        public double total
        {
            get
            {
                return no_of_no + no_of_1st + no_of_1plus;
            }
        }

        public int month { get; set; }
        public int year { get; set; }

        public double percent
        {
            get
            {
                double percent = total != 0 ? 10000 * no_of_1st / total : 0;
                return Math.Round(percent / 100, 1);
            }
        }
    }

    public class EntQaQcCommonCheck
    {
        //public DateTime created_at { get; set; }
        public string project { get; set; }
        public string project_id { get; set; }
        public int total_no { get; set; }
        public int total_yes { get; set; }
        public int total { get; set; }

        public double percent
        {
            get
            {
                double percent = 10000 * total_no / total;
                return Math.Round(percent / 100, 1);
            }
        }
    }

    // class combine critical and common
    public class SummaryCriticalCommon
    {
        public string project { get; set; }
        public string project_id { get; set; }
        public int total_no { get; set; }
        public int total_1st { get; set; }
        public int total_yes { get; set; }
        public int total_1plus { get; set; }
        public int total { get; set; }
        public string ProjectType { get; set; }

        public double percentCommonCheck
        {
            get
            {
                double percent = total > 0 ? 10000 * total_no / total : 0;
                return Math.Round(percent / 100, 1);
            }
        }

        public double percentCriticalCheck
        {
            get
            {
                double percent = total > 0 ? 10000 * total_1st / total : 0;
                return Math.Round(percent / 100, 1);
            }
        }

        public string color { get; set; }
        public string kprText { get; set; }
        public int kprValue { get; set; }

        /// <summary>
        /// data follow month or summary
        /// </summary>
        public string month_name { get; set; }
    }

    public class SummaryQAQCTab
    {
        public string project { get; set; }
        public string projectId { get; set; }
        public double total_project { get; set; }
        public double sum_overall_score { get; set; }
        public double score_avg { get; set; }
        public string ProjectType { get; set; }
        public string color { get; set; }
        public string kprText { get; set; }
        public int kprValue { get; set; }

        /// <summary>
        /// data follow month or summary
        /// </summary>
        public string month_name { get; set; }
    }

    public class EntQaQcCriticalCheck
    {
        //public DateTime created_at { get; set; }
        public string project { get; set; }
        public string project_id { get; set; }
        public int total_no { get; set; }
        public int total_1st { get; set; }
        public int total_1plus { get; set; }
        public int total { get; set; }

        public double percent
        {
            get
            {
                double percent = 10000 * total_1st / total;
                return Math.Round(percent / 100, 1);
            }
        }
    }

    public class EntQaQcBcaInspection
    {
        public string id { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        public string[] histories { get; set; }
        public DateTime? WeekDateFrom { get; set; }
        public DateTime? WeekDateTo { get; set; }
        public DateTime? CreatedAt { get; set; }

        public List<History> HistoriesJson
        {
            get
            {
                List<History> listHis = new List<History>();
                object a = "";
                foreach (var item in histories)
                {
                    History his = JsonConvert.DeserializeObject<History>(item);
                    listHis.Add(his);
                }

                return listHis;
            }
        }

        public double? Score
        {
            get
            {
                double result;

                List<History> listHis = new List<History>();
                foreach (var item in histories)
                {
                    History his = JsonConvert.DeserializeObject<History>(item);
                    listHis.Add(his);
                }

                bool success = double.TryParse(listHis[2].Value, out result);
                return success ? result : 0;
            }
        }
    }

    public class EntQaQcQmJot
    {
        public DateTime? createdAt { get; set; }
        public string Unit { get; set; }

        public int Hours { get; set; }
        public int no_of_sample { get; set; }
        public string Foreman { get; set; }
        public string WorkType { get; set; }
        public string Activity { get; set; }
        public string Worker { get; set; }

    }

    public class EntQaQcQmJotGroupByUnit
    {
        public string Unit { get; set; }

        public int Hours { get; set; }
        public int no_of_sample { get; set; }
        public int qmwork { get; set; }
        public int reconwork { get; set; }
        public int bcwork { get; set; }
        public int otherwork { get; set; }
        public int total
        {
            get
            {
                int t = qmwork + reconwork + bcwork + otherwork;
                return t;
            }
        }

        public double Noofday { get; set; }

        public double ms
        {
            get
            {
                double m = 0;
                if (no_of_sample > 0)
                {
                    m = Math.Round((double)((double)total / 10) / no_of_sample, 2);
                }
                return m;
            }
        }
        public double msQR
        {
            get
            {
                double msqr = 0;
                if (no_of_sample > 0)
                {
                    double mq = (double)((qmwork + reconwork) / no_of_sample / 11 * 100 / 100);
                    msqr = Math.Round(mq, 2);
                }
                return msqr;
            }
        }
    }


    public class History
    {
        public string Id { get; set; }
        public string Comment { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class FormCheckItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string? excelId { get; set; }
        public string? sub1 { get; set; }
        public string? options { get; set; }
        public string? enableComment { get; set; }
        public string? isRequired { get; set; }
    }

    public class EntQaQcHandedOver
    {
        public DateTime? date { get; set; }
        public string ArchiRepresentative { get; set; }
        public string QMRepresentative { get; set; }
        public string Block { get; set; }
        public string Level { get; set; }
        public string Unit { get; set; }
        public int Status { get; set; }
        public Boolean BCAInspected { get; set; }
        public string? Comment { get; set; }
        public List<string> listItemsNotAcceptted { get; set; }
        public Decimal score { get; set; }
        public DateTime? PlanStart { get; set; }
        public DateTime? PlanEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public string Status_Unit { get; set; }

        // merge block, level, unit
        public string Key { get; set; }
    }

    public class EntQaQcCommonCheckByProject
    {
        public string project { get; set; }
        public string trade { get; set; }
        public int no_of_no { get; set; }
        public int no_of_yes { get; set; }

        public string shortform { get; set; }

        public string discipline { get; set; }

        public double percent
        {
            get
            {
                double res = 0;
                if (no_of_no + no_of_yes != 0)
                {
                    res = 1000 * no_of_no / (no_of_no + no_of_yes);
                }

                return Math.Round(res / 100, 1);
            }
        }
    }
    public class Score
    {
        public double reworkScore { get; set; }
        public double defectScore { get; set; }
        public string project { get; set; }
    }
    public class Comment
    {
        public Guid id { get; set; }
        public string project { get; set; }
        public DateTime month_year { get; set; }
        public int key_project { get; set; }
        public string comment { get; set; }
        public DateTime create_at { get; set; }
        public DateTime update_at { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class EntQaQcCriticalByProject
    {
        public string project { get; set; }
        public int no_of_no { get; set; }
        public int no_of_1st { get; set; }
        public int no_of_1plus { get; set; }

        public string shortform { get; set; }

        public string discipline { get; set; }

        public double percent
        {
            get
            {
                double res = 0;
                if (no_of_no + no_of_1st + no_of_1plus != 0)
                {
                    res = 1000 * no_of_1st / (no_of_no + no_of_1st + no_of_1plus);
                }

                return Math.Round(res / 100, 1);
            }
        }
    }

    public class EntQaQcCommonCheckByShortform
    {
        public string shortform { get; set; }
        public int no_of_no { get; set; }
        public int no_of_yes { get; set; }

        public int total { get; set; }

        public double percent
        {
            get
            {
                double res = 0;
                if (no_of_no + no_of_yes != 0)
                {
                    res = 1000 * no_of_no / (no_of_no + no_of_yes);
                }

                return Math.Round(res / 100, 1);
            }
        }
    }

    public class EntQaQcReworkBySubcontractor
    {
        public string subcontractor { get; set; }

        public int total { get; set; }
    }

    public class EntQaQcViolationBySubcontractor
    {
        public string subcontractor { get; set; }
        public int demerit_point { get; set; }
        public int withholding_amount { get; set; }

        public int total { get; set; }
    }

    public class EntQaQcObservationBySxf
    {
        public string sxf { get; set; }

        public int total { get; set; }
    }

    public class EntQaQcCriticalByTrade
    {
        public string trade { get; set; }
        public int no_of_no { get; set; }
        public int no_of_1st { get; set; }
        public int no_of_1plus { get; set; }


        public int total { get; set; }

        public double percent
        {
            get
            {
                double res = 0;
                if (no_of_no + no_of_1st + no_of_1plus != 0)
                {
                    res = 1000 * no_of_1st / (no_of_no + no_of_1st + no_of_1plus);
                }

                return Math.Round(res / 100, 1);
            }
        }
    }

    public class EntQaQcReworkByProject
    {
        public string project { get; set; }
        public int total { get; set; }
    }

    public class EntQaQcViolationByProject
    {
        public string project { get; set; }
        public int demerit_point { get; set; }
        public int withholding_amount { get; set; }
        public int total { get; set; }
    }

    public class EntQaQcCommonCheckGroupDate
    {
        public string yyyymm { get; set; }
        public string mmyy { get; set; }
    }

    public class EntQaQcReworkDate
    {
        public string yyyymm { get; set; }
        public string mmyy { get; set; }

        public float total { get; set; }
    }

    public class EntQaQcViolationDate
    {
        public string yyyymm { get; set; }
        public string mmyy { get; set; }

        public float total { get; set; }
        public int demerit_point { get; set; }
        public int withholding_amount { get; set; }
    }

    public class EntQaQcObservationDate
    {
        /// <summary>
        /// create at with year month
        /// </summary>
        public string ym { get; set; }

        /// <summary>
        /// create at with month year
        /// </summary>
        public string my { get; set; }

        /// <summary>
        /// sxf
        /// </summary>
        public string sxf { get; set; }

        /// <summary>
        /// total sxf
        /// </summary>
        public float total_sxf { get; set; }

        /// <summary>
        /// total by month
        /// </summary>
        public float total { get; set; }
    }
    public class EntQaQcObservationContractor
    {
        public string name { get; set; }
        public int total { get; set; }
        public string pjtBreakdown { get; set; }
    }
    public class EntQaQcContractor
    {
        public string contractor { get; set; }
        public string sxf { get; set; }
        public string project { get; set; }
        public float total_project { get; set; }
    }
    public class EntQaQcContractorByProject
    {
        public string project { get; set; }
        public string sxf { get; set; }
        public int count { get; set; }
    }
}