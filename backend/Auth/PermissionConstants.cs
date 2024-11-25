namespace DashboardApi.Auth
{
    public class PermissionConstants
    {
       public const string CREATE_PO = nameof(CREATE_PO);
       /// <summary>
       /// THEY CAN VIEW PO
       /// </summary>
       public const string VIEW_PO = nameof(VIEW_PO);

       /// <summary>
       /// USER CAN CREATE DO, UPDATE DO, DELETE DO
       /// </summary>
       public const string CREATE_DO = nameof(CREATE_DO);
       /// <summary>
       /// THEY CAN VIEW DO
       /// </summary>
       public const string VIEW_DO = nameof(VIEW_DO);
   }
}
