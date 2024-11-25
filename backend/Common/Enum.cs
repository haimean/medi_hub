using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DashboardApi.Common
{
    public enum LegalNCRegisterAuthority
    {
        [Display(Name = "MOM")]
        MOM = 1,
        [Display(Name = "NEA")]
        NEA = 2,
        [Display(Name = "PUB")]
        PUB = 3,
        [Display(Name = "RestrictionOfWorkHours")]
        RestrictionOfWorkHours = 4,
        [Display(Name = "Others")]
        Others = 5,
    }

    public enum LegalNCRegisterType
    {
        [Display(Name = "NNC")]
        NNC = 1,
        [Display(Name = "Fine")]
        Fine = 2,
        [Display(Name = "SWO")]
        SWO = 3,
        [Display(Name = "Demerit Points")]
        DemeritPoints = 4,
    }

    public enum LegalNCRegisterReason
    {
        [Display(Name = "WSH")]
        WSH = 1,
        [Display(Name = "Noise")]
        Noise = 2,
        [Display(Name = "Vector")]
        Vector = 3,
        [Display(Name = "Drainage")]
        Drainage = 4,
        [Display(Name = "SMM")]
        SMM = 5,
        [Display(Name = "Others")]
        Others = 6,
    }

    public enum IncidentClassificationType
    {
        [Display(Name = "Near Miss")]
        NearMiss = 1,
        [Display(Name = "Reportable Accident")]
        ReportableAccident = 2,
        [Display(Name = "Fatality")]
        Fatality = 3,
        [Display(Name = "Permanent Disability")]
        PermanentDisability = 4,
        [Display(Name = "Lost Time Injury")]
        LostTimeInjury = 5,
        [Display(Name = "Reportable Occupational Disease")]
        ReportableOccupationalDisease = 6,
        [Display(Name = "Light Duty Case")]
        LightDutyCase = 7,
        [Display(Name = "Medical Treatment Case")]
        MedicalTreatmentCase = 8,
        [Display(Name = "First Aid Injury")]
        FirstAidInjury = 9,
        [Display(Name = "Dangerous Occurrence")]
        DangerousOccurrence = 10,
        [Display(Name = "Environmental Incident")]
        EnvironmentalIncident = 11,
        [Display(Name = "Property")]
        Property = 12,
        [Display(Name = "Stop Work Order")]
        StopWorkOrder = 13
    }

}
