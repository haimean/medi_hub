using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Linq;

namespace System
{
   public static class StringExtensions
   {
      public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

      public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

      public static string EnsureEndsWithDot(this string value) => value.EndsWith(".") ? value : $"{value}.";

      public static string PathCombines(this string f1, List<string> f2s)
      {
         var path = f1;
         foreach (var f2 in f2s)
         {
            path = Path.Combine(path, f2);
         }

         return path;
      }

      public static string PathCombinesParams(this string f1, params string[] f2s)
      {
         var path = f1;
         foreach (var f2 in f2s)
         {
            path = Path.Combine(path, f2);
         }

         return path;

      }

      public static string ToUrl(this string path)
      {
         return "" + path;
      }

      public static long UnixTimeNow()
      {
         var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
         return (long)timeSpan.TotalSeconds;
      }

      public static bool IsEqual(this string s1, string s2)
      {
         if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
         {
            return true;
         }

         if (s1.IsNullOrEmpty())
         {
            s1 = string.Empty;
         }


         if (s2.IsNullOrEmpty())
         {
            s2 = string.Empty;
         }

         return s1.Equals(s2, StringComparison.OrdinalIgnoreCase);
      }


      public static bool IsEqual(this double? s1, double? s2)
      {
         if (s1 != null && s2 != null)
         {
            return Math.Abs(s1.GetValueOrDefault() - s2.GetValueOrDefault()) < 0.001;
         }

         return false;

      }


      public static bool IsEqual(this double s1, double s2)
      {
         return Math.Abs(s1 - s2) < 0.001;
      }


      public static string GetDateStringFromCellEpplus(this object obj)
      {
         if (obj == null)
         {
            return string.Empty;
         }
         try
         {
            long dateNum = long.Parse(obj.ToString());
            DateTime result = DateTime.FromOADate(dateNum);

            return result.ToString("dd/MM/yyyy");
         }
         catch (Exception e)
         {
            return string.Empty;
         }
      }

      public static string TryToString(this object obj)
      {
         if (obj != null)
         {
            return obj.ToString();
         }
         else
         {
            return string.Empty;
         }
      }

      public static int TryToInt(this object obj)
      {
         var s = TryToString(obj);

         int.TryParse(s, out var i);


         return i;
      }


      public static double TryToDouble(this object obj)
      {
         var s = TryToString(obj);

         double.TryParse(s, out var i);

         return i;
      }


      public static string ToSnakeCase(this string input)
      {
         if (string.IsNullOrEmpty(input)) { return input; }

         var startUnderscores = Regex.Match(input, @"^_+");
         return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
      }


      public static string SqlJoinColumns(this string tableName, List<string> pas)
      {
         if (string.IsNullOrEmpty(tableName))
         {
            return string.Join(",", pas.Select(x => $"{tableName}.{x}"));
         }

         return string.Join(",", pas.Select(x => $"{x}"));
      }
   }
}
