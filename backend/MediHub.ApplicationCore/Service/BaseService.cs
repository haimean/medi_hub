using Microsoft.AspNetCore.Http;
using MISA.ApplicationCore.Entities;
using MISA.ApplicationCore.Enums;
using MISA.ApplicationCore.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;
using MemoryCache = System.Runtime.Caching.MemoryCache;

namespace MISA.ApplicationCore.Service
{
    /// <summary>
    /// Baser service 
    /// </summary>
    /// <typeparam name="Generic"></typeparam>
    /// CreatedBy: PQ Huy (05.07.2021)
    public class BaseService<Generic> : IBaseService<Generic> where Generic:BaseEntity
    {
        #region DECLARE
        IBaseRepository<Generic> _baseRepository;
        ServiceResult _serviceResult;
        public string _tableName = string.Empty;
        List<string> _checkedResult = new List<string>();
        ObjectCache cache = MemoryCache.Default;
        #endregion

        #region Contructor
        public BaseService(IBaseRepository<Generic> baseRepository)
        {
            _baseRepository = baseRepository;
            _serviceResult = new ServiceResult() { MISACode = MISAEnum.Success };
            _tableName = typeof(Generic).Name;
        }
        #endregion

        #region Method
        /// <summary>
        /// Lấy toàn bộ dữ liệu bản ghi
        /// </summary>
        /// <returns>Lấy ra toàn bộ bản ghi</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public IEnumerable<Generic> Get()
        {
            try
            {
                return _baseRepository.Get();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Lấy dữ liệu bản ghi theo id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Trả về bản ghi theo id</returns>
        public IEnumerable<Generic> GetById(Guid id)
        {
            try
            {
                return _baseRepository.GetById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Thêm mới bản ghi
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Trạng thái thêm</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public virtual ServiceResult Insert(Generic data)
        {
            try
            {
                // set state action
                data.EntityState = EntityState.AddNew;

                // validate require
                var isValid = Validate(data);

                if (isValid)
                {
                    _serviceResult.Data = _baseRepository.Insert(data);
                }

                //return service result
                return _serviceResult;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Cấp nhật bản ghi theo id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns>Trạng thái update</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public ServiceResult Update(Guid id, Generic data)
        {
            try
            {
                // set state action
                data.EntityState = EntityState.Update;

                // validate require
                var isValid = Validate(data);

                if (isValid)
                {
                    _serviceResult.Data = _baseRepository.Update(id, data);
                }
                else
                {
                    _serviceResult.MISACode = MISAEnum.NotValid;
                }

                //return service result
                return _serviceResult;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Xóa bản ghi theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Trạng thái xóa</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public ServiceResult DeleteById(Guid id)
        {
            try
            {
                _serviceResult = _baseRepository.DeleteById(id);

                if (Convert.ToInt32(_serviceResult.Data) > 0)
                {
                    _serviceResult.MISACode = MISAEnum.Success;
                    _serviceResult.Messenger = Properties.Resources.EditSuccess;
                }
                else
                {
                    _serviceResult.MISACode = MISAEnum.NotValid;
                    _serviceResult.Messenger = Properties.Resources.ExitId + id;
                }

                //return service result
                return _serviceResult;
            }
            catch (Exception)
            {
                throw;
            }
            
        } 

        /// <summary>
        /// Validate data
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True/false validate</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        private bool Validate(Generic data)
        {
            var messArr = new List<string>();
            bool isValid = true;

            try
            {
                // Get all property:
                var properties = data.GetType().GetProperties();

                foreach (var property in properties)
                {
                    // get property name
                    var propertyName = "";
                    if (property.GetCustomAttributesData().Count() != 0)
                    {
                        try
                        {
                            propertyName = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single().DisplayName;
                        }
                        catch (Exception ce)
                        {
                            propertyName = "";
                            Console.Write(ce);
                        }
                    }

                    // check attribute need validate
                    if (property.IsDefined(typeof(Required), false))
                    {
                        // check required
                        var propertyValue = property.GetValue(data);
                        if (propertyValue == null)
                        {
                            isValid = false;
                            messArr.Add($"{Properties.Resources.Notnull} {propertyName}");
                            _serviceResult.MISACode = MISAEnum.NotValid;
                            _serviceResult.Data = messArr;
                            _serviceResult.Messenger = Properties.Resources.DataFormatErr;

                            // trả về kết quả khi kiểm tra
                            return isValid;
                        }
                    }

                    if (property.IsDefined(typeof(CheckDuplicate), false))
                    {
                        // check duplicate data
                        var valueDuplicate = _baseRepository.GetEntityByProperty(data, property);

                        if (valueDuplicate.Count() != 0)
                        {
                            isValid = false;

                            messArr.Add($"{propertyName} {property.GetValue(data)} {Properties.Resources.ExitDuplicate}");
                            _serviceResult.MISACode = MISAEnum.NotValid;
                            _serviceResult.Data = messArr;
                            _serviceResult.Messenger = Properties.Resources.DataFormatErr;

                            // trả về kết quả khi kiểm tra
                            return isValid;
                        }
                    }
                }

                // tạo function cho các class con muốn validate thêm
                isValid = CustomValidate(data, _checkedResult) == true ? isValid : false;

                // lưu lại message thông báo và trạng thái sau khi validate
                _serviceResult.MISACode = MISAEnum.IsValid;
                _serviceResult.Data = messArr;
                _serviceResult.Messenger = Properties.Resources.ValidSuccess;

                return isValid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Hàm cho phép custom thêm validate
        /// </summary>
        /// <param name="generic">data</param>
        /// <param name="checkedResult">biến ghi log lỗi</param>
        /// <returns>Trả về true/false và log lỗi</returns>
        /// CreatedBy: PQ Huy (12.07.2021)
        protected virtual bool CustomValidate(Generic generic, List<string> checkedResult)
        {
            return true;
        }

        /// <summary>
        /// Xử lý request import dữ liệu
        /// </summary>
        /// <param name="formFile">Đường dẫn files</param>
        /// <param name="cancellationToken">Token file</param>
        /// <returns>Trả về một key cache lưu dữ liệu sau khi xử lý</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public string ProcessDataImport(IFormFile formFile, CancellationToken cancellationToken)
        {
            List<Generic> listGenerics = new List<Generic>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    formFile.CopyToAsync(stream, cancellationToken);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        // import resource file
                        // Create a resource manager to retrieve resources.
                        ResourceManager resourceManager = new ResourceManager($"MISA.ApplicationCore.Resource.{_tableName}", Assembly.GetExecutingAssembly());
                        ResourceSet resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

                        // get number of rows and columns in the sheet
                        int rows = worksheet.Dimension.Rows;
                        int columns = worksheet.Dimension.Columns;

                        /*
                            Convert data table to object with key and value
                         */
                        List<string> listKey = new List<string>();

                        // get title table convert to resource
                        for (int i = 1; i <= columns; i++)
                        {
                            string title = worksheet.Cells[2, i].Value.ToString();

                            // format title
                            string formatTitle = title.Trim(new Char[] { ' ', '(', '*', ')', '.' }).Trim();

                            // covert title to lowercase string
                            string titleLowerCase = formatTitle.ToLower();

                            // compare with resouce and save to list key
                            foreach (DictionaryEntry entry in resourceSet)
                            {
                                string resourceKey = entry.Key.ToString();
                                string resourceValue = entry.Value.ToString();

                                if (titleLowerCase == resourceValue.ToLower())
                                {
                                    listKey.Add(resourceKey);
                                    break;
                                }
                            }
                        }

                        // get data to list generics
                        for (int i = 3; i <= rows; i++)
                        {
                            List<object> temp = new List<object>();
                            var generic = (Generic)Activator.CreateInstance(typeof(Generic), new object[] { });

                            for (int j = 0; j < listKey.Count(); j++)
                            {
                                string key = listKey[j];
                                string value = "";

                                // kiểm tra từng cell một
                                if (worksheet.Cells[i, j + 1].Value != null)
                                {
                                    value = worksheet.Cells[i, j + 1].Value != null ? worksheet.Cells[i, j + 1].Value.ToString().Trim() : "";

                                    // format data
                                    dynamic convertData = FormatData(generic.GetType().GetProperty(key).PropertyType, value);

                                    // set value data
                                    generic.GetType().GetProperty(key).SetValue(generic, convertData);
                                }
                            }
                            // add component to list
                            listGenerics.Add(generic);
                        }
                    }
                }

                // validate data
                var dataGetAll = _baseRepository.Get();
                IDictionary<object, List<string>> checkedProp = new Dictionary<object, List<string>>();

                foreach (var generic in listGenerics)
                {
                    generic.MsgImport = new List<string>();

                    var isValid = this.ValidateImport(generic, dataGetAll, checkedProp);

                    // kiểm tra trạng thái sau khi validate từng generic
                    if (generic.MsgImport.Count == 0)
                    {
                        ServiceResult temp = new ServiceResult();
                        temp.MISACode = MISAEnum.IsValid;
                        temp.Messenger = Properties.Resources.IsValid;
                        generic.ImportResult = temp;
                        generic.MsgImport.Add(Properties.Resources.IsValid);
                    }
                    else
                    {
                        ServiceResult temp = new ServiceResult();
                        temp.MISACode = MISAEnum.NotValid;
                        temp.Messenger = Properties.Resources.DataFormatErr;
                        generic.ImportResult = temp;
                        generic.MsgImport.Add(Properties.Resources.DataFormatErr);
                    }
                }

                string CacheKey = $"{_tableName}{Guid.NewGuid()}";
                // Store data in the cache    
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddHours(60.0);
                cache.Add(CacheKey, listGenerics, cacheItemPolicy);
                GetDataByKeyCache(CacheKey);

                // return result
                return CacheKey;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// function test get data cache by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Trả về bản ghi theo key cache</returns>
        /// PQ Huy (05.07.2021)
        private IEnumerable<Generic> GetDataByKeyCache(string key)
        {
            try
            {
                if (cache.Contains(key))
                    return (IEnumerable<Generic>)cache.Get(key);
                else
                    return null;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// function format data import
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>Trả về dữ liệu chuẩn sau khi format</returns>
        /// PQ Huy (05.07.2021)
        private dynamic FormatData(Type type, string value, bool statusValid = true)
        {
            try
            {
                dynamic res = null;
                if (value.ToString().Trim() == "")
                    return res;

                // check type is guid
                if (type == typeof(Guid))
                {
                    Guid newGuid = Guid.Parse(value);
                    return newGuid;
                }

                if (type.Name == "Nullable`1")
                {
                    type = Nullable.GetUnderlyingType(type);
                    //Đổi format ngày tháng
                    if (statusValid)
                    {
                        if (type.Name == "DateTime")
                        {
                            var temp = Regex.Split(value, @"/").ToList();
                            while (temp.Count < 3)
                            {
                                temp.Insert(0, "01");
                            }
                            temp.Reverse();
                            value = String.Join('-', temp);
                        }
                    }
                }
                try
                {
                    res = Convert.ChangeType(value, type);
                }
                catch (Exception e)
                {
                    res = value;
                    Console.Write(e);
                }

                return res;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Validate import file
        /// </summary>
        /// <param name="generic"></param>
        /// <param name="dataGetAll">data</param>
        /// <param name="checkedProp">property</param>
        /// <returns>Trả về kết quả kiểm tra </returns>
        /// PQ Huy (05.07.2021)
        private bool ValidateImport(Generic generic, IEnumerable<Generic> dataGetAll = null, IDictionary<object, List<string>> checkedProp = null)
        {
            try
            {
                var isValid1 = true;
                var isValid2 = true;
                if (generic == null && checkedProp == null)
                {
                    generic.MsgImport = new List<string>();
                }

                // validate từng trường trong generic
                // Get all property:
                var properties = generic.GetType().GetProperties();

                foreach (var property in properties)
                {
                    // get property name
                    var propertyName = "";
                    if (property.GetCustomAttributesData().Count() != 0)
                    {
                        try
                        {
                            propertyName = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single().DisplayName;
                        }
                        catch (Exception ce)
                        {
                            propertyName = "";
                            Console.Write(ce);
                        }
                    }

                    // check attribute need validate -  validate require
                    if (property.IsDefined(typeof(Required), false))
                    {
                        isValid1 = validateRequired(property.GetValue(generic), propertyName);
                    }

                    // validate check duplicate
                    if (property.IsDefined(typeof(CheckDuplicate), false))
                    {
                        // check duplicate data
                        isValid2 = this.ValidateDuplicate(generic, property.Name, propertyName, dataGetAll, checkedProp);
                    }
                }

                // add service result
                ServiceResult temp = new ServiceResult();
                temp.ImportMsg = _checkedResult;

                // check valid
                if (isValid1 && isValid2)
                {
                    temp.MISACode = MISAEnum.IsValid;
                    temp.Data = isValid1;
                }
                else
                {
                    temp.MISACode = MISAEnum.NotValid;
                    temp.Data = isValid1;
                }

                // add result
                generic.ImportResult = temp;
                generic.MsgImport.AddRange(_checkedResult);
                _checkedResult.Clear();

                if (isValid1 && isValid2)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra duplicate
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyName">property</param>
        /// <param name="displayName">name</param>
        /// <param name="dataGetAll">data</param>
        /// <param name="checkedProp">property</param>
        /// <returns>Trả về true/false</returns>
        /// PQ Huy (05.07.2021)
        private bool ValidateDuplicate(Generic entity, string propertyName, string displayName, IEnumerable<Generic> dataGetAll, IDictionary<object, List<string>> checkedProp)
        {
            try
            {
                var isValid = true;
                var value = entity.GetType().GetProperty(propertyName).GetValue(entity);

                if (value != null)
                {
                    // validate dữ liệu trong file excel
                    isValid = this.ValidateDuplicateFile(value, propertyName, displayName, checkedProp);
                    // validate với dữ liệu trên db
                    isValid = this.ValidateDuplicateDb(dataGetAll, propertyName, value, displayName);

                }

                if (!isValid)
                {
                    _serviceResult.MISACode = MISAEnum.IsValid;
                    _serviceResult.Messenger = Properties.Resources.DataFormatErr;
                }

                return isValid;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Validate dữ liệu trên hệ thống
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="propertyName">property</param>
        /// <param name="displayName">name</param>
        /// <param name="checkedProp">property</param>
        /// <returns>Trả về true/false</returns>
        /// PQ Huy (05.07.2021)
        private bool ValidateDuplicateFile(object value, string propertyName, string displayName, IDictionary<object, List<string>> checkedProp)
        {
            try
            {
                if (!checkedProp.ContainsKey(value))
                {
                    var list = new List<string>();
                    list.Add(propertyName);

                    checkedProp.Add(value, list);
                }
                else
                {
                    if (checkedProp[value].Contains(propertyName))
                    {
                        _checkedResult.Add($"{displayName} {Properties.Resources.DuplicateWith} {displayName} {Properties.Resources.InFile}");

                        return false;
                    }
                    else
                    {
                        checkedProp[value].Add(propertyName);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// validate duplicate in database
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        /// <param name="displayName"></param>
        /// <returns>Trả về true/false</returns>
        /// PQ Huy (05.07.2021)
        private bool ValidateDuplicateDb(IEnumerable<Generic> dataGetAll, string propertyName, object value, string displayName)
        {
            try
            {
                // get first item result
                var filterResult = dataGetAll.Where(item => item.GetType().GetProperty(propertyName).GetValue(item).ToString() == value.ToString()).FirstOrDefault();

                if (filterResult != null)
                {
                    string message = $"{displayName} {Properties.Resources.Duplicate}";
                    _checkedResult.Add(message);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        ///  funtion validate require
        /// </summary>
        /// <param name="val"></param>
        /// <param name="propertyName"></param>
        /// <returns>Trả về true/false</returns>
        /// PQ Huy (05.07.2021)
        private bool validateRequired(object val, object propertyName)
        {
            try
            {
                if (val == null || val.ToString().Length == 0)
                {
                    _checkedResult.Add($"{propertyName} {Properties.Resources.Notnull}");
                    _serviceResult.MISACode = MISAEnum.IsValid;
                    _serviceResult.Messenger = Properties.Resources.DataFormatErr;

                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        ///  Get value in item sorted list
        /// </summary>
        /// <param name="item"></param>
        /// <param name="prop"></param>
        /// <returns>Trả về giá trị sau khi sort</returns>
        /// PQ Huy (05.07.2021)
        private string getValueSortedList(SortedList item, string prop)
        {
            try
            {
                string value = "";

                // sorted list
                foreach (var obj in item)
                {
                    if (obj.GetType().GetProperty("Key").GetValue(obj).ToString() == $"{_tableName}Code")
                    {
                        #pragma warning disable CS8794 // The input always matches the provided pattern.
                        if (obj.GetType().GetProperty("Value").GetValue(obj) is not null or not "")
                           #pragma warning restore CS8794 // The input always matches the provided pattern.
                        {
                            value = obj.GetType().GetProperty("Value").GetValue(obj).ToString();
                        }
                        break;
                    }
                }

                return value;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Lấy value từ resource
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Lấy ra giá trị theo resource</returns>
        /// PQ Huy (05.07.2021)
        private string GetValueResource(string key)
        {
            try
            {
                string value = "";
                // import resource file
                // Create a resource manager to retrieve resources.
                ResourceManager resourceManager = new ResourceManager($"MISA.ApplicationCore.Resource.{_tableName}", Assembly.GetExecutingAssembly());
                ResourceSet resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

                foreach (DictionaryEntry entry in resourceSet)
                {
                    if (key == entry.Key.ToString())
                    {
                        value = entry.Value.ToString();
                        break;
                    }
                }

                return value;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// function covert data sao cho đúng kiểu
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>Trả về dữ liệu format</returns>
        /// PQ Huy (05.07.2021)
        private dynamic ConvertFormatData(Type type, string value)
        {
            try
            {
                dynamic res = null;

                if (value == "")
                    return res;

                // format with nullable, datetime
                if (type.Name == "Nullable`1")
                {
                    type = Nullable.GetUnderlyingType(type);

                    if (type.Name == "DateTime")
                    {
                        var temp = Regex.Split(value, @"/").ToList();
                        while (temp.Count < 3)
                        {
                            temp.Insert(0, "01");
                        }
                        temp.Reverse();
                        value = String.Join('-', temp);
                    }
                }

                res = Convert.ChangeType(value, type);

                // return value
                return res;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Thêm nhiều bản ghi
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <returns>Trả về số bản ghi được ghi</returns>
        /// PQ Huy (05.07.2021)
        public ServiceResult MutilpleInsert(string CacheKey)
        {
            try
            {
                ServiceResult serviceResult = new ServiceResult();
                // get data by key cache
                List<Generic> resValue = (List<Generic>)GetDataByKeyCache(CacheKey);
                int count = 0;

                // kiểm tra dữ liệu có tồn tại
                if (resValue != null)
                {
                    // lấy ra từng bản ghi
                    foreach (Generic generic in resValue)
                    {
                        if (generic.ImportResult.MISACode == MISAEnum.IsValid)
                        {
                            // convert status data import 
                            var tempGeneric = (Generic)Activator.CreateInstance(typeof(Generic), new object[] { });

                            var properties = generic.GetType().GetProperties();

                            // kiểm tra lại dữ liệu của bản ghi 
                            foreach (var property in properties)
                            {
                                var propertyName = property.Name;
                                if (propertyName != "ImportResult" && propertyName != "EntityState" && propertyName != "MsgImport")
                                {
                                    var valueAdd = " ";
                                    var propertyValue = property.GetValue(generic);
                                    var propertyType = generic.GetType().GetProperty(propertyName).PropertyType;
                                    if (propertyValue != null)
                                    {
                                        valueAdd = propertyValue.ToString();
                                    }

                                    // convert lại cho đúng kiểu dữ liệu
                                    dynamic convertData = FormatData(propertyType, valueAdd, false);
                                    tempGeneric.GetType().GetProperty(propertyName).SetValue(tempGeneric, convertData);
                                }
                            }

                            // tiến hành thêm bản ghi
                            serviceResult = Insert(tempGeneric);

                            // đếm số bản ghi thêm thành công
                            if (serviceResult.MISACode == MISAEnum.Success || serviceResult.MISACode == MISAEnum.IsValid)
                            {
                                count++;
                            }
                        }
                    }
                }
                else
                {
                    // trả về kết quả khi dữ liệu trên cache không tồn tại
                    serviceResult.MISACode = MISAEnum.NotValid;
                    serviceResult.Messenger = Properties.Resources.NullDataCache;
                }

                // trả về số bản ghi được thêm thành công
                serviceResult.Data = count;
                serviceResult.Messenger = $"{Properties.Resources.AddTotalSuccess} {count}";
                return serviceResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
