using iBMSApp.DataModels;
using iBMSApp.Shared;
using iBMSApp.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

#nullable enable
namespace iBMSApp.Services
{
    public enum EqptOrderType
    {
        Insp,
        Work,
        Repair
    }

    public class EqptService {
        private readonly ILocalStorageService _localStorageService;
        private readonly ILogger<EqptService> _logger;
        private readonly HttpClient _http;
        private readonly UnityDbAccessor _persistentDb;

        public EqptService(ILocalStorageService localStorageService, ILogger<EqptService> logger, HttpClient http, UnityDbAccessor persistentDb)
        {
            _localStorageService = localStorageService;
            _http = http;
            _persistentDb = persistentDb;
            _logger = logger;
        }

        #region LocalStorage getter/setter
        // Local Storage/LastOnlineTime
        private const string ONLINE_TIME_DATA = "LastOnlineTime";
        public async Task<string> GetLastOnlineTime()
            => await _localStorageService.GetItemAsync<string>(ONLINE_TIME_DATA) ?? "";

        public async Task SetLastOnlineTime(string lastOnlineTime)
            => await _localStorageService.SetItemAsync(ONLINE_TIME_DATA, lastOnlineTime);

        // Local Storage/Device
        private const string DEVICE_DATA = "Device";

        /// <summary>
        /// 將 Device 資訊暫存
        /// </summary>
        /// <param name="code"><seealso cref="DeviceStaticInfo.Code"/></param>
        /// <returns></returns>
        public async Task<bool> SetDevice(string code)
        {
            var matchedDevice = await GetDeviceData(x => x.Code == code);

            if (matchedDevice == null)
            {
                return false;
            }

            await _localStorageService.SetItemAsync(DEVICE_DATA, matchedDevice);
            return true;
        }

        public async Task<DeviceStaticInfo?> GetDevice()
            => await _localStorageService.GetItemAsync<DeviceStaticInfo>(DEVICE_DATA);
        #endregion

        #region IndexedDb getter/setter
        // IndexedDb >> devices
        private const string DEVICES_DATA = "devices";

        public async Task ClearDevicesData()
            => await _persistentDb.ClearDataAsync(DEVICES_DATA);

        public async Task SetAllDevicesData(List<DeviceStaticInfo> mappedDevices)
        {
            var storageFormat = new DeviceStaticStorage
            {
                Sn = DEVICES_DATA,
                Devices = mappedDevices
            };
            await _persistentDb.SetValueAsync<DeviceStaticStorage>(DEVICES_DATA, storageFormat);
        }

        /// <summary>
        /// 取得設備的同時會將設備暫存至記憶體
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<DeviceStaticInfo?> GetDeviceData(Func<DeviceStaticInfo, bool> predicate)
        {
            var storage = await _persistentDb.GetValueAsync<DeviceStaticStorage>(DEVICES_DATA, DEVICES_DATA);

            if (storage?.Devices == null) return null;

            var foundDevice = storage.Devices.FirstOrDefault(predicate);

            if (foundDevice != null)
                await _localStorageService.SetItemAsync(DEVICE_DATA, foundDevice);

            return foundDevice;
        }

        // IndexedDb >> tasks
        private const string TASKS_DATA = "tasks";

        public async Task<int> GetAllDataCountTasks()
            => await _persistentDb.GetAllDataCountAsync(TASKS_DATA);

        public async Task<Dictionary<string, string>> GetCursorTasks()
            //=> await _persistentDb.GetValueAsync<Dictionary<string, string>>(TASKS_DATA);
            => await _persistentDb.GetCursorValueAsync(TASKS_DATA);

        public async Task SetTask(Dictionary<string, string> taskQueryData)
            => await _persistentDb.SetCursorValueAsync(TASKS_DATA, taskQueryData);

        public async Task DeleteCursorTask(Dictionary<string, string> taskQueryData)
            => await _persistentDb.DeleteCursorTaskAsync(TASKS_DATA, taskQueryData);

        // IndexedDb >> inspOrders
        private const string INSP_ORDERS_DATA = "inspOrders";

        public async Task SaveAllInspOrders(List<EqptOrder> orders)
            => await _persistentDb.SetAllValueAsync(INSP_ORDERS_DATA, orders);

        public async Task<List<EqptOrder>> GetAllDataInspOrders()
            => await _persistentDb.GetAllDataAsync<EqptOrder>(INSP_ORDERS_DATA);

        public async Task<EqptOrder?> GetInspOrder(string buildingCode, string RecordSn)
            => await _persistentDb.GetValueAsync<EqptOrder>(INSP_ORDERS_DATA, $"{buildingCode},{RecordSn}");

        public async Task SetInspOrder(EqptOrder inspOrder)
            => await _persistentDb.SetValueAsync(INSP_ORDERS_DATA, inspOrder);

        // IndexDb >> workOrders
        private const string WORK_ORDERS_DATA = "workOrders";

        public async Task SaveAllWorkOrders(List<EqptOrder> orders)
            => await _persistentDb.SetAllValueAsync(WORK_ORDERS_DATA, orders);

        public async Task<List<EqptOrder>> GetAllDataWorkOrders()
            => await _persistentDb.GetAllDataAsync<EqptOrder>(WORK_ORDERS_DATA);

        public async Task<EqptOrder?> GetWorkOrder(string buildingCode, string RecordSn)
            => await _persistentDb.GetValueAsync<EqptOrder>(WORK_ORDERS_DATA, $"{buildingCode},{RecordSn}");

        public async Task SetWorkOrder(EqptOrder workOrder)
            => await _persistentDb.SetValueAsync(WORK_ORDERS_DATA, workOrder);

        // IndexedDb >> iPhotos
        private const string INSP_PHOTOS_DATA = "iPhotos";

        public async Task ClearInspPhotosData()
            => await _persistentDb.ClearDataAsync(INSP_PHOTOS_DATA);

        public async Task DeleteInspPhotoData(string sn)
            => await _persistentDb.DeleteDataAsync(INSP_PHOTOS_DATA, sn);

        public async Task<PhotoStorageDto?> GetInspPhoto(string sn)
            => await _persistentDb.GetValueAsync<PhotoStorageDto>(INSP_PHOTOS_DATA, sn);

        public async Task SetInspPhoto(string sn, string img, DateTime createTime)
            => await _persistentDb.SetValueAsync(INSP_PHOTOS_DATA, new PhotoStorageDto { Sn = sn, Img = img, CreateTime = createTime });

        // IndexedDb >> wPhotos
        private const string WORK_PHOTOS_DATA = "wPhotos";

        public async Task ClearWorkPhotosData()
            => await _persistentDb.ClearDataAsync(WORK_PHOTOS_DATA);

        public async Task DeleteWorkPhotoData(string sn)
            => await _persistentDb.DeleteDataAsync(WORK_PHOTOS_DATA, sn);

        public async Task<PhotoStorageDto?> GetWorkPhoto(string sn)
            => await _persistentDb.GetValueAsync<PhotoStorageDto>(WORK_PHOTOS_DATA, sn);

        public async Task SetWorkPhoto(string sn, string img, DateTime createTime)
            => await _persistentDb.SetValueAsync(WORK_PHOTOS_DATA, new PhotoStorageDto { Sn = sn, Img = img, CreateTime = createTime });
        #endregion

        #region Web APIs
        public bool IsSuccessStatusCode(HttpStatusCode statusCode)
            => (int)statusCode >= 200 && (int)statusCode <= 299;

        /// <summary>
        /// POST - Multipart/form-data 通用回傳方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiName">開頭為 api/Eqpt/，apiName 串接在之後</param>
        /// <param name="query">API 請求的內容</param>
        /// <returns></returns>
        private async Task<ApiResponse<T>> PostFormDataRequest<T>(string apiName, Dictionary<string, string> query)
        {
            ApiResponse<T> apiResponse = new ApiResponse<T>();
            var formData = ToFormData(query);

            _logger.LogInformation($"httpClient.PostAsync /api/Eqpt/{apiName} (multipart/form-data)");

            try
            {
                using var response = await _http.PostAsync($"/api/Eqpt/{apiName}", formData);

                apiResponse.IsSuccess = response.IsSuccessStatusCode;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.Data = default(T);

                if (!apiResponse.IsSuccess)
                {
                    apiResponse.ErrorMessage = await response.Content.ReadAsStringAsync();
                    LogFailedHttpStatus(apiResponse.StatusCode, apiResponse.ErrorMessage);
                    return apiResponse;
                }

                object? result;

                if (typeof(T) == typeof(string))
                {
                    var stringResult = await response.Content.ReadAsStringAsync();
                    result = stringResult;
                }
                else if (typeof(T) == typeof(Stream))
                {
                    result = await response.Content.ReadAsStreamAsync();
                }
                else
                {
                    result = await response.Content.ReadFromJsonAsync<T>();
                }

                apiResponse.Data = (T?)result;

                return apiResponse;
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            { // 請求逾時
                apiResponse.IsSuccess = false;
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                apiResponse.ErrorMessage = "Request Timeout";
                _logger.LogWarning("請求逾時: " + ex.Message);
                return apiResponse;
            }
            catch (HttpRequestException ex)
            { // 這是網路錯誤、無法連線等
                apiResponse.IsSuccess = false;
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                apiResponse.ErrorMessage = "Network Or Server response error";
                _logger.LogWarning("網路或伺服器錯誤: " + ex.Message);
                return apiResponse;
            }
            catch (Exception ex)
            { // 其他未知錯誤
                apiResponse.IsSuccess = false;
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                apiResponse.ErrorMessage = "Unknown Error: " + ex.Message;
                _logger.LogError("未知錯誤: " + ex.Message);
                return apiResponse;
            }
        }

        /// <summary>
        /// 將泛型物件轉換成 Multipart/Form-data 的 request body 格式
        /// </summary>
        /// <typeparam name="T">單層結構的class</typeparam>
        /// <param name="obj">僅能正確處理單層class</param>
        /// <returns></returns>
        public static MultipartFormDataContent ToFormData(Dictionary<string, string> data)
        {
            var formData = new MultipartFormDataContent();

            foreach (var kv in data)
            {
                if (kv.Value == null) continue;
                formData.Add(new StringContent(kv.Value ?? ""), kv.Key);
            }

            return formData;
        }

        private void LogFailedHttpStatus(HttpStatusCode statusCode, string? errorMessage)
        {
            errorMessage = errorMessage ?? string.Empty;
            if (statusCode == System.Net.HttpStatusCode.Unauthorized) // 401
                _logger.LogError("Unauthorized !!");
            else if (statusCode == System.Net.HttpStatusCode.NoContent) // 204
                _logger.LogError("NoContent !!");
            else if (statusCode == System.Net.HttpStatusCode.BadRequest) // 400
                _logger.LogError("BadRequest !!");
            else
                _logger.LogError($"There was an error! {errorMessage}, StatusCode: {statusCode}");
        }

        public string TaskMethod_DevicesAll { get; private set; } = "DevicesAll";

        /// <summary>
        /// 一次取得所有設備資訊，並記錄至 indexedDB 供離線使用
        /// </summary>
        /// <param name="buildingCode">依照案場的 BuildingCode 做查詢</param>
        /// <returns>回傳內容存入 indexDb 欄位: <seealso cref="DEVICES_DATA"/></returns>
        public async Task GetDevices(string buildingCode)
        {
            var query = new Dictionary<string, string>
            {
                { nameof(EqptRequestDeviceBasic.BuildingCode), buildingCode }
            };

            var response = await PostFormDataRequest<List<DeviceStaticInfo>>(TaskMethod_DevicesAll, query);

            if (!response.IsSuccess)
            {
                LogFailedHttpStatus(response.StatusCode, response.ErrorMessage);
                return;
            }

            List<DeviceStaticInfo>? devices = response.Data;
            //var devices = await JsonSerializer.DeserializeAsync<List<DeviceStaticInfo>>(stream, new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //});

            if (devices == null || devices.Count == 0)
            {
                _logger.LogInformation("No devices found.");
                return;
            }

            await SetAllDevicesData(devices); // _persistentDb.SetAllValueAsync("devices", mapped);
            _logger.LogInformation($"Device data added: {devices.Count}");
        }

        public string TaskMethod_InspOrdersAll { get; private set; } = "InspOrdersAll";

        /// <summary>
        /// 取得日期間隔內的巡檢單，並將結果暫存至 <seealso cref="INSP_ORDERS_DATA"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestOrders"/> 的資料結構</param>
        /// <returns>後端回傳的 HTTP 狀態，與 <seealso cref="List{T}"/>:<see cref="EqptOrder"/></returns>
        public async Task<ApiResponse<List<EqptOrder>>> PostInspOrders(Dictionary<string, string> query)
        {
            var response = await PostFormDataRequest<List<EqptOrder>>(TaskMethod_InspOrdersAll, query);

            if (response.IsSuccess)
            {
                string lastOnlineTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                await SaveAllInspOrders(response.Data ?? new List<EqptOrder>());
                await SetLastOnlineTime(lastOnlineTime);
            }

            return response;
        }

        public string TaskMethod_WorkOrdersAll { get; private set; } = "WorkOrdersAll";

        /// <summary>
        /// 取得日期間隔內的工單，並將結果暫存至 <seealso cref="WORK_ORDERS_DATA"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestOrders"/> 的資料結構</param>
        /// <returns>後端回傳的 HTTP 狀態，與 <seealso cref="List{T}"/>:<see cref="EqptOrder"/></returns>
        public async Task<ApiResponse<List<EqptOrder>>> PostWorkOrders(Dictionary<string, string> query)
        {
            var response = await PostFormDataRequest<List<EqptOrder>>(TaskMethod_WorkOrdersAll, query);

            if (!response.IsSuccess)
            {
                return response; // 401, 204, 400, ...
            }

            string lastOnlineTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            await SaveAllWorkOrders(response.Data ?? new List<EqptOrder>());
            await SetLastOnlineTime(lastOnlineTime);
            return response;
        }

        /// <summary>
        /// 執行紀錄於 CursorTask 中的任務 (離線時操作的任務)
        /// Blazor 呼叫時塞入的 Method 需要注意使用 <seealso cref="EqptService.TaskMethod_"/>
        /// 開頭的 class property，確保重連後的 task 呼叫是存在的 method
        /// </summary>
        /// <returns>後端回傳的 HTTP 狀態</returns>
        public async Task<HttpStatusCode> PostCursorTask()
        {
            Dictionary<string, string> task = await this.GetCursorTasks();

            _logger.LogInformation("Redo()...");
            if (task.ContainsKey("Method"))
            {
                // 準備 multipart/form-data 內容
                using var formData = new MultipartFormDataContent();
                foreach (var kv in task)
                {
                    formData.Add(new StringContent(kv.Value), kv.Key);
                }

                _logger.LogInformation($"Post to Method: {task["Method"]}");
                try
                {
                    using var response = await _http.PostAsync("api/Eqpt/" + task["Method"], formData);

                    if (!response.IsSuccessStatusCode)
                    {
                        LogFailedHttpStatus(response.StatusCode, await response.Content.ReadAsStringAsync());
                        return response.StatusCode;
                    }
                }
                catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested) 
                { // 請求逾時
                    _logger.LogWarning("請求逾時: " + ex.Message);
                    return HttpStatusCode.NotFound;
                }
                catch (HttpRequestException ex)
                { // 網路錯誤、無法連線等
                    _logger.LogWarning("網路或伺服器錯誤: " + ex.Message);
                    return HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                { // 其他未知錯誤
                    _logger.LogError("未知錯誤: " + ex.Message);
                    return HttpStatusCode.NotFound;
                }
            }

            await DeleteCursorTask(task);
            return HttpStatusCode.OK;
        }

        public string TaskMethod_GetPhoto { get; private set; } = "Photo";

        /// <summary>
        /// 取得工單/巡檢圖片，離線儲存Method為 <seealso cref="TaskMethod_GetPhoto"/>
        /// </summary>
        /// <param name="photoSn">圖片編號</param>
        /// <returns>後端回傳的 HTTP 狀態，以及圖片的 base64img</returns>
        public async Task<ApiResponse<EqptPhoto>> GetRequestPhoto(string photoSn)
        {
            var apiResponse = new ApiResponse<EqptPhoto>();

            try
            {
                using var response = await _http.GetAsync($"api/Eqpt/{TaskMethod_GetPhoto}?Sn={photoSn}");

                _logger.LogInformation($"Get api/Eqpt/{TaskMethod_GetPhoto}?Sn={photoSn}");
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.IsSuccess = response.IsSuccessStatusCode;
                apiResponse.Data = null;

                if (!response.IsSuccessStatusCode)
                {
                    apiResponse.ErrorMessage = await response.Content.ReadAsStringAsync();
                    LogFailedHttpStatus(response.StatusCode, apiResponse.ErrorMessage);
                    return apiResponse;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();

                apiResponse.Data = await Task.Run(async () =>
                {
                    // 僅解析需要欄位
                    using var doc = JsonDocument.Parse(stream);
                    var root = doc.RootElement;
                    string message = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<EqptPhoto>(message);
                    return new EqptPhoto
                    {
                        Sn = data?.Sn ?? -1,
                        Photo = data?.Photo,
                        CreateTime = data?.CreateTime ?? default
                        // 加入你需要的欄位
                    };
                });

                // 可能造成幀凍結 ?
                // apiResponse.Data = await response.Content.ReadFromJsonAsync<EqptPhoto>();

                return apiResponse;
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            { // 請求逾時
                apiResponse.IsSuccess = false;
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                apiResponse.ErrorMessage = "Request Timeout";
                _logger.LogWarning("請求逾時: " + ex.Message);
                return apiResponse;
            }
            catch (HttpRequestException ex)
            { // 這是網路錯誤、無法連線等
                apiResponse.IsSuccess = false;
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                apiResponse.ErrorMessage = "Network Or Server response error";
                _logger.LogWarning("網路或伺服器錯誤: " + ex.Message);
                return apiResponse;
            }
            catch (Exception ex)
            { // 其他未知錯誤
                apiResponse.IsSuccess = false;
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                apiResponse.ErrorMessage = "Unknown Error: " + ex.Message;
                _logger.LogError("未知錯誤: " + photoSn + ex.Message);
                return apiResponse;
            }
        }

        public string TaskMethod_GetPhotoVersion { get; private set; } = "PhotoVersion";

        /// <summary>
        /// 取得工單/巡檢圖片版本，離線儲存Method為 <seealso cref="TaskMethod_GetPhotoVersion"/>
        /// </summary>
        /// <param name="photoSn">圖片編號</param>
        /// <returns>後端回傳的 HTTP 狀態，以及圖片的 base64img</returns>
        public async Task<ApiResponse<bool>> PostPhotoVersion(EqptPhotoVersion query)
        {
            Dictionary<string, string> requestData = new Dictionary<string, string>
            {
                { nameof(EqptPhotoVersion.Sn), query.Sn.ToString() },
                { nameof(EqptPhotoVersion.CreateTime), query.CreateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") }
            };

            return await PostFormDataRequest<bool>(TaskMethod_GetPhotoVersion, requestData);
        }

        public string TaskMethod_SubmitInspDevice { get; private set; } = "SubmitInspDevice";

        /// <summary>
        /// 提交已完成巡檢的設備資料，離線儲存Method為 <seealso cref="TaskMethod_SubmitInspDevice"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSubmitInspDevice"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="EqptRespondSubmitDevice"/> 回應物件（若有）。
        /// </returns>
        public async Task<ApiResponse<EqptRespondSubmitDevice>> PostSubmitInspDevice(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRespondSubmitDevice>(TaskMethod_SubmitInspDevice, query);

        public string TaskMethod_SubmitInspOrder { get; private set; } = "SubmitInspOrder";

        /// <summary>
        /// 提交已完成的巡檢單，離線儲存Method為 <seealso cref="TaskMethod_SubmitInspOrder"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSubmitOrder"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="EqptRespondSubmitOrder"/> 回應物件（若有）。
        /// </returns>
        public async Task<ApiResponse<EqptRespondSubmitOrder>> PostSubmitInspOrder(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRespondSubmitOrder>(TaskMethod_SubmitInspOrder, query);

        public string TaskMethod_SaveNumericalData { get; private set; } = "SaveNumericalData";

        /// <summary>
        /// 提交設備讀值，離線儲存Method為 <seealso cref="TaskMethod_SaveNumericalData"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSaveNumericalData"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="string"/> (Success: "Done")
        /// </returns>
        public async Task<ApiResponse<string>> PostSaveNumericalData(Dictionary<string, string> query)
            => await PostFormDataRequest<string>("SaveNumericalData", query);

        public string TaskMethod_SaveInspConsumables { get; private set; } = "SaveInspConsumables";

        /// <summary>
        /// 提交設備耗材更換紀錄，離線儲存Method為 <seealso cref="TaskMethod_SaveInspConsumables"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSaveOrderConsumables"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="string"/> (Success: "Done")
        /// </returns>
        public async Task<ApiResponse<string>> PostSaveInspConsumables(Dictionary<string, string> query)
            => await PostFormDataRequest<string>(TaskMethod_SaveInspConsumables, query);

        public string TaskMethod_SaveWorkConsumables { get; private set; } = "SaveWorkConsumables";

        /// <summary>
        /// 提交設備耗材更換紀錄，離線儲存Method為 <seealso cref="TaskMethod_SaveWorkConsumables"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSaveOrderConsumables"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="string"/> (Success: "Done")
        /// </returns>
        public async Task<ApiResponse<string>> PostSaveWorkConsumables(Dictionary<string, string> query)
            => await PostFormDataRequest<string>(TaskMethod_SaveWorkConsumables, query);

        public string TaskMethod_UpdateInspDevice { get; private set; } = "UpdateInspDevice";

        /// <summary>
        /// 原本巡檢單是完工上傳的狀態，變更成再做一次，給新的開始時間，離線儲存Method為 <seealso cref="TaskMethod_UpdateInspDevice"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestUpdateOrderDevice"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="EqptRespondSubmitDevice"/> (Success: <see cref="EqptRespondSubmitDevice.Result"/> = "Done")
        /// </returns>
        public async Task<ApiResponse<EqptRespondSubmitDevice>> PostUpdateInspDevice(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRespondSubmitDevice>(TaskMethod_UpdateInspDevice, query);

        public string TaskMethod_StartInsp { get; private set; } = "StartInsp";

        /// <summary>
        /// 開始巡檢時做紀錄，離線儲存Method為 <seealso cref="TaskMethod_StartInsp"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestUpdateOrderDevice"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="string"/> (提交時間格式: yyyy-MM-dd HH:mm:ss)
        /// </returns>
        public async Task<ApiResponse<string>> PostStartInsp(Dictionary<string, string> query)
            => await PostFormDataRequest<string>(TaskMethod_StartInsp, query);


        public string TaskMethod_StartWorkOrder { get; private set; } = "StartWorkOrder";

        /// <summary>
        /// 開始工單時做紀錄，離線儲存Method為 <seealso cref="TaskMethod_StartWorkOrder"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestUpdateOrderDevice"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="string"/> (提交時間格式: yyyy-MM-dd HH:mm:ss)
        /// </returns>
        public async Task<ApiResponse<string>> PostStartWorkOrder(Dictionary<string, string> query)
            => await PostFormDataRequest<string>(TaskMethod_StartWorkOrder, query);

        public string TaskMethod_DeviceBasic { get; private set; } = "DeviceBasic";

        /// <summary>
        /// 開始巡檢設備時做的紀錄，離線儲存Method為 <seealso cref="TaskMethod_DeviceBasic"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestDeviceBasic"/> 的資料結構</param>
        /// <returns>
        /// 回傳 HTTP 狀態碼與 <see cref="EqptRespondDeviceBasic"/> 
        /// </returns>
        public async Task<ApiResponse<EqptRespondDeviceBasic>> PostDeviceBasic(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRespondDeviceBasic>(TaskMethod_DeviceBasic, query);

        public string TaskMethod_SaveWorkPrePhotos { get; private set; } = "SaveWorkPrePhotos";

        /// <summary>
        /// 儲存工單的處理前圖片，離線儲存Method為 <seealso cref="TaskMethod_SaveWorkPrePhotos"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSavePrePhoto"/> 的資料結構</param>
        /// <returns></returns>
        public async Task<ApiResponse<EqptRespondSavePrePhoto>> PostSaveWorkPrePhoto(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRespondSavePrePhoto>(TaskMethod_SaveWorkPrePhotos, query);

        public string TaskMethod_SubmitWorkDevice { get; private set; } = "SubmitWorkDevice";

        /// <summary>
        /// 提交工單的單個設備處理結果，離線儲存Method為 <seealso cref="TaskMethod_SubmitWorkDevice"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSubmitWorkDevice"/> 的資料結構</param>
        /// <returns></returns>
        public async Task<ApiResponse<EqptRespondSubmitWorkDevice>> PostSubmitWorkDevice(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRespondSubmitWorkDevice>(TaskMethod_SubmitWorkDevice, query);

        public string TaskMethod_SubmitWorkOrder { get; private set; } = "SubmitWorkOrder";

        /// <summary>
        /// 提交工單處理結果，離線儲存Method為 <seealso cref="TaskMethod_SubmitWorkOrder"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSubmitOrder"/> 的資料結構</param>
        /// <returns></returns>
        public async Task<ApiResponse<EqptRespondSubmitOrder>> PostSubmitWorkOrder(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRespondSubmitOrder>(TaskMethod_SubmitWorkOrder, query);

        public string TaskMethod_SetRepairOrder { get; private set; } = "SetRepairOrder";

        /// <summary>
        /// 提交工單處理結果，離線儲存Method為 <seealso cref="TaskMethod_SetRepairOrder"/>
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestSetRepairOrder"/> 的資料結構</param>
        /// <returns></returns>
        public async Task<ApiResponse<EqptRepairOrder>> PostSetRepairOrder(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRepairOrder>(TaskMethod_SetRepairOrder, query);

        public string TaskMethod_GetRepairOrder { get; private set; } = "GetRepairOrder";

        /// <summary>
        /// 取得報修單
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestOrder"/> 的資料結構</param>
        /// <returns>後端回傳的 HTTP 狀態，與 :<see cref="EqptRespondRepairOrder"/></returns>
        public async Task<ApiResponse<EqptRespondRepairOrder>> GetRepairOrder(Dictionary<string, string> query)
            => await PostFormDataRequest<EqptRespondRepairOrder>(TaskMethod_GetRepairOrder, query);

        public string TaskMethod_RepairOrdersAll { get; private set; } = "RepairOrdersAll";

        /// <summary>
        /// 取得報修單
        /// </summary>
        /// <param name="query">對應 <seealso cref="EqptRequestOrders"/> 的資料結構</param>
        /// <returns>後端回傳的 HTTP 狀態，與 <seealso cref="List{T}"/>:<see cref="EqptRespondRepairOrder"/></returns>
        public async Task<ApiResponse<List<EqptRespondRepairOrder>>> PostRepairOrders(Dictionary<string, string> query)
            => await PostFormDataRequest<List<EqptRespondRepairOrder>>(TaskMethod_RepairOrdersAll, query);

        #endregion
    }
}
