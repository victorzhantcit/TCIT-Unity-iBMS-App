using iBMSApp.Shared;
using iBMSApp.Utility;
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
    public class AuthService : IAuthService
    {
        private readonly ILocalStorageService localStorageService;
        private readonly ILogger<AuthService> _logger = null!;
        private readonly HttpClient httpClient;
        //private readonly AuthenticationStateProvider authenticationStateProvider;

        // 身分與 API response query 對照表
        private readonly Dictionary<UserRole, string> _roleMap = new Dictionary<UserRole, string>
            {
                { UserRole.Staff, "AppStaff" },
                { UserRole.QC, "AppQC" },
                { UserRole.Insp, "AppInsp" },
                { UserRole.Maint, "AppMaint" },
                { UserRole.DeviceMaint, "AppDeviceMaint" }
            };

        public AuthService(ILocalStorageService localStorageService, ILogger<AuthService> logger, HttpClient httpClient)
        {
            this.localStorageService = DIContainer.Resolve<ILocalStorageService>();
            this._logger = DIContainer.Resolve<ILogger<AuthService>>();
            this.httpClient = httpClient;
            //this.httpClient.Timeout = new TimeSpan(0, 0, 3); //3 sec
            //this.authenticationStateProvider = authenticationStateProvider;
        }

        /// <summary>
        /// 向後端 /api/Auth/Login 發送登入請求
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns>登入成功: OK 失敗:Error message</returns>
        public async Task<string> LoginAsync(UserInfo userInfo)
        {
            _logger.LogInformation("LoginAsync Started");
            string result = "";

            var loginQuery = new MultipartFormDataContent // 準備 API 的 Request body
            {
                { new StringContent(userInfo.Id), "account" },
                { new StringContent(userInfo.Password), "pw" }
            };


            _logger.LogInformation("Post Login");
            var response = await PostFormDataRequest<string>("Login", loginQuery); // 呼叫 API

            if (!response.IsSuccess) // 會由 PostFormDataRequest<T> 負責 Log 錯誤訊息
            {
                return response.ErrorMessage ?? "";
            }


            _logger.LogInformation("Response success");
            string? resContent = response.Data; // 取得 API 回傳的資料，這裡回傳後端 AuthController 生成的 Token (string)

            if (resContent == null) // 回傳資料異常處理
            {
                _logger.LogError("UserToken is null");
                result = "UserToken is null";
                return result;
            }

            string userToken = resContent;

            _logger.LogInformation("Set AuthToken");
            await localStorageService.SetItemAsync<string>("authToken", userToken);

            //((JwtAuthenticationStateProvider)authenticationStateProvider).NotifyUserAuthentication(userToken);
            //await ((JwtAuthenticationStateProvider)authenticationStateProvider).NotifyUserAuthenticationAsync();

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            result = "OK";


            _logger.LogInformation("Get Roles");
            await GetRolesAsync();

            _logger.LogInformation("LoginAsync Ended");
            return result;
        }

        public async Task AddAuthenticationHeader()
        {
            var Token = await localStorageService.GetItemAsync<string>("authToken");
            // ((JwtAuthenticationStateProvider)authenticationStateProvider).NotifyUserAuthentication(Token);

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            //await ((JwtAuthenticationStateProvider)authenticationStateProvider).NotifyUserAuthenticationAsync();
        }

        public async Task LogoutAsync()
        {
            await localStorageService.RemoveItemAsync("authToken");
            await localStorageService.RemoveItemAsync("roles");
            await localStorageService.RemoveItemAsync("user");
            await localStorageService.RemoveItemAsync("AutoLogin"); // 設定登出以判斷不再快速登入
            await localStorageService.SetItemAsStringAsync(ServiceManager.Instance.LanguageKeyWord, ""); // 清空選擇的語系讓使用者重選語系
            //((JwtAuthenticationStateProvider)authenticationStateProvider).NotifyUserLogOut();
            httpClient.DefaultRequestHeaders.Authorization = null;
        }

        //public async Task<string> getUserName()
        //{
        //    return await ((JwtAuthenticationStateProvider)authenticationStateProvider).getUserName();
        //}
        //public async Task<string> getUserID()
        //{
        //    return await ((JwtAuthenticationStateProvider)authenticationStateProvider).getUserID();
        //}
        //public async Task<List<string>> getRoles()
        //{
        //    return await ((JwtAuthenticationStateProvider)authenticationStateProvider).getRoles();
        //}
        public async Task<UserInfo> GetUserData()
        {
            return await localStorageService.GetItemAsync<UserInfo>("user") ?? new UserInfo
            {
                Name = "localUser",
                Department = "",
                Tel = "",
                Email = ""
            };
            //return await ((JwtAuthenticationStateProvider)authenticationStateProvider).getUserData();
        }

        /// <summary>
        /// 取得 UserRole 並暫存至 localStorage
        /// </summary>
        /// <returns></returns>
        public async Task GetRolesAsync()
        {
            var roles = new List<UserRole>();

            try
            {
                var response = await httpClient.GetAsync("/api/Auth/Login/GetPermission");

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    //_logger.LogError("Error getting permission: {Error}, StatusCode: {StatusCode}", errorMessage, response.StatusCode);
                    return;
                }

                await using var resContent = await response.Content.ReadAsStreamAsync();
                var loginData = await JsonSerializer.DeserializeAsync<LoginData>(resContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginData == null)
                {
                    //_logger.LogError("loginData is null after deserialization.");
                    return;
                }

                foreach (var (role, pageId) in _roleMap)
                {
                    bool? editable = loginData.Permissions?
                        .PagePermission?
                        .FirstOrDefault(p => p.Id == pageId)?
                        .Permissions?.Editable;

                    if (editable == true)
                        roles.Add(role);
                }

                Personal userData = loginData?.Personal ?? new Personal();
                UserInfo userInfo = new UserInfo
                {
                    Name = userData.DisplayName ?? "localUser",
                    Department = userData.Department ?? "",
                    Tel = userData.Tel ?? "",
                    Email = userData.Email ?? "",
                };

                await localStorageService.SetItemAsync("roles", roles);
                await localStorageService.SetItemAsync("user", userInfo);
                //await ((JwtAuthenticationStateProvider)authenticationStateProvider).NotifyUserAuthenticationAsync();
                //_logger.LogInformation("Roles successfully stored in localStorage: [{Roles}]", string.Join(", ", roles));
            }
            catch
            {
                throw new Exception("Exception occurred while retrieving user roles.");
                //_logger.LogError(ex, "Exception occurred while retrieving user roles.");
            }
        }

        public async Task<bool> isInRole(UserRole role)
        {
            var roles = await localStorageService.GetItemAsync<List<UserRole>>("roles");
            return roles?.Contains(role) == true;
        }

        public async Task<ApiResponse<string>> ChangePassword(string oldPW, string newPassword, string confirmPassword)
        {
            var query = new MultipartFormDataContent 
            {
                { new StringContent(oldPW), "oldPW" },
                { new StringContent(newPassword),"newPW" },
                { new StringContent(confirmPassword), "confirmPW" }
            };

            return await PostFormDataRequest<string>("ChangePassword", query);
        }

        /// <summary>
        /// POST - Multipart/form-data 通用回傳方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiName">開頭為 api/Eqpt/，apiName 串接在之後</param>
        /// <param name="query">API 請求的內容</param>
        /// <returns></returns>
        private async Task<ApiResponse<T>> PostFormDataRequest<T>(string apiName, MultipartFormDataContent query)
        {
            ApiResponse<T> apiResponse = new ApiResponse<T>();

            _logger.LogInformation($"httpClient.PostAsync /api/Auth/{apiName} (multipart/form-data)");

            try
            {
                using var response = await httpClient.PostAsync($"/api/Auth/{apiName}", query);

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
            //if (statusCode == System.Net.HttpStatusCode.Unauthorized) // 401
            //    _logger.LogError("Unauthorized !!");
            //else if (statusCode == System.Net.HttpStatusCode.NoContent) // 204
            //    _logger.LogError("NoContent !!");
            //else if (statusCode == System.Net.HttpStatusCode.BadRequest) // 400
            //    _logger.LogError("BadRequest !!");
            //else
            //    _logger.LogError($"There was an error! {errorMessage}, StatusCode: {statusCode}");

            Console.WriteLine($"There was an error! {errorMessage}, StatusCode: {statusCode}");
        }

    }
}
