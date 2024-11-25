using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using QAQCApi.AttributeCustom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DashboardApi.Common
{
    public enum ProjectKPR
    {
        [Color("#C00216")]
        [Text("Unacceptable")]
        UNA = 0,

        [Color("#FF5D5D")]
        [Text("Below Expectations")]
        BEX = 25,

        [Color("#EE9817")]
        [Text("Meets Expectations")]
        MEX = 50,

        [Color("#10B5A1")]
        [Text("Just Above Expectations")]
        JAE = 75,

        [Color("#06b056")]
        [Text("Exceeds Expectations ")]
        EEX = 100,
    }
}
