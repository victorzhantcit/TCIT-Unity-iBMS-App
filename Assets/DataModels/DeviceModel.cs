//using Newtonsoft.Json.Linq;
//using System.Text.Json;
//using Blazored.LocalStorage;

//namespace DTRGApp.Client.Model
//{
//    public class DeviceModel
//    {
//        /// <summary>
//        /// 設備清單(含各語言)儲存於 IndexedDb 的 devices 中，以設備 Code 對應到的設備主要資訊儲存於
//        /// 名為 Device 的 LocalStorage 中，供各前端讀取
//        /// </summary>
//        private readonly ISyncLocalStorageService LocalStorage;
//        private readonly IndexedDbAccessor IndexedDbAccessor;
//        public DeviceModel(ISyncLocalStorageService LocalStorage, IndexedDbAccessor IndexedDbAccessor)
//        {
//            this.LocalStorage = LocalStorage;
//            this.IndexedDbAccessor = IndexedDbAccessor;
//        }
//        public async Task<bool> GetDeviceIntoLocalStorageAsync(string Code)
//        {
//            JsonDocument device = await IndexedDbAccessor.GetValueAsync<JsonDocument>("devices", Code);
//            if (device != null)
//            {
//                JObject ls_device = new JObject();
//                ls_device.Add(new JProperty("BuildingCode", device.RootElement.GetProperty("bldg").GetString() ?? ""));
//                ls_device.Add(new JProperty("BuildingName", device.RootElement.GetProperty("bn").GetString() ?? ""));
//                ls_device.Add(new JProperty("DeviceType", device.RootElement.GetProperty("type").GetString() ?? ""));
//                ls_device.Add(new JProperty("DeviceCode", device.RootElement.GetProperty("dc").GetString() ?? ""));
//                ls_device.Add(new JProperty("Code", device.RootElement.GetProperty("code").GetString() ?? ""));
//                ls_device.Add(new JProperty("SiteCode", device.RootElement.GetProperty("s").GetString() ?? ""));
//                ls_device.Add(new JProperty("DailyMaint", device.RootElement.GetProperty("day").GetBoolean()));
//                var desc = "";
//                try
//                {
//                    var CurrentCulture = LocalStorage.GetItem<string>("i18nextLng");
//                    if (CurrentCulture != null)
//                    {
//                        if (CurrentCulture.Equals("en_WW"))
//                            desc = device.RootElement.GetProperty("en_WW").GetString() ?? "";
//                        else if (CurrentCulture.Equals("zh_CN"))
//                            desc = device.RootElement.GetProperty("zh_CN").GetString() ?? "";
//                        else if (CurrentCulture.Equals("th_TH"))
//                            desc = device.RootElement.GetProperty("th_TH").GetString() ?? "";
//                        else
//                            desc = device.RootElement.GetProperty("zh_TW").GetString() ?? "";
//                    }
//                    else
//                        desc = device.RootElement.GetProperty("zh_TW").GetString() ?? "";
//                    if (desc.Equals("")) desc = device.RootElement.GetProperty("desc").GetString() ?? "";
//                }
//                catch (Exception ex)
//                {
//                    string exception = ex.Message.ToString();
//                    Console.WriteLine(DateTime.Now + " DeviceModel Exception:" + exception);
//                    System.Diagnostics.Debug.WriteLine(DateTime.Now + ":DeviceModel " + exception);
//                }
//                ls_device.Add(new JProperty("Description", desc));

//                LocalStorage.SetItem("Device", ls_device.ToString(Newtonsoft.Json.Formatting.None));

//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }
//        public async Task<string> GetDescriptionOfCodeAsync(string Code)
//        {
//            JsonDocument device = await IndexedDbAccessor.GetValueAsync<JsonDocument>("devices", Code);
//            if (device != null)
//            {
//                var desc = "";
//                try
//                {
//                    var CurrentCulture = LocalStorage.GetItem<string>("i18nextLng");
//                    if (CurrentCulture != null)
//                    {
//                        if (CurrentCulture.Equals("en_WW"))
//                            desc = device.RootElement.GetProperty("en_WW").GetString() ?? "";
//                        else if (CurrentCulture.Equals("zh_CN"))
//                            desc = device.RootElement.GetProperty("zh_CN").GetString() ?? "";
//                        else if (CurrentCulture.Equals("th_TH"))
//                            desc = device.RootElement.GetProperty("th_TH").GetString() ?? "";
//                        else
//                            desc = device.RootElement.GetProperty("zh_TW").GetString() ?? "";
//                    }
//                    else
//                        desc = device.RootElement.GetProperty("zh_TW").GetString() ?? "";
//                    if (desc.Equals("")) desc = device.RootElement.GetProperty("desc").GetString() ?? "";

//                    return desc;
//                }
//                catch(Exception ex)
//                {
//                    string exception = ex.Message.ToString();
//                    Console.WriteLine(DateTime.Now + " DeviceModel Exception:" + exception);
//                    System.Diagnostics.Debug.WriteLine(DateTime.Now + ":DeviceModel " + exception);
//                    return "";
//                }
//            }
//            else
//            {
//                return "";
//            }
//        }
//        public async Task<string> GetTWDescriptionOfCodeAsync(string Code)
//        {
//            JsonDocument device = await IndexedDbAccessor.GetValueAsync<JsonDocument>("devices", Code);
//            if (device != null)
//            {
//                var desc = "";
//                try
//                {
//                    desc = device.RootElement.GetProperty("zh_TW").GetString() ?? "";
//                    if (desc.Equals("")) desc = device.RootElement.GetProperty("desc").GetString() ?? "";
//                }
//                catch(Exception ex)
//                {
//                    string exception = ex.Message.ToString();
//                    Console.WriteLine(DateTime.Now + " DeviceModel Exception:" + exception);
//                    System.Diagnostics.Debug.WriteLine(DateTime.Now + ":DeviceModel " + exception);
//                }
//                return desc;
//            }
//            else
//            {
//                return "";
//            }
//        }
//        public async Task<string> GetDeviceCodeOfCodeAsync(string Code)
//        {
//            JsonDocument device = await IndexedDbAccessor.GetValueAsync<JsonDocument>("devices", Code);
//            if (device != null)
//            {
//                var dc = "";
//                dc = device.RootElement.GetProperty("dc").GetString() ?? "";

//                return dc;
//            }
//            else
//            {
//                return "";
//            }
//        }
//        public async Task<string> GetBuildingCodeOfCodeAsync(string Code)
//        {
//            JsonDocument device = await IndexedDbAccessor.GetValueAsync<JsonDocument>("devices", Code);
//            if (device != null)
//            {
//                var dc = "";
//                dc = device.RootElement.GetProperty("bldg").GetString() ?? "";

//                return dc;
//            }
//            else
//            {
//                return "";
//            }
//        }
//        public string GetValue(string name)
//        {
//            if (LocalStorage.ContainKey("Device"))
//            {
//                var device = JObject.Parse(LocalStorage.GetItem<string>("Device"));
//                if (device.TryGetValue(name, out var Value))
//                {
//                    return Value.ToString();
//                }
//                else return "";
//            }
//            else return "";
//        }
//        public string GetCode()
//        {
//            if (LocalStorage.ContainKey("Device"))
//            {
//                var device = JObject.Parse(LocalStorage.GetItem<string>("Device"));
//                if (device.TryGetValue("Code", out var Code))
//                {
//                    return Code.ToString();
//                }
//                else return "";
//            }
//            else return "";
//        }
//        public string GetBuildingCode()
//        {
//            if (LocalStorage.ContainKey("Device"))
//            {
//                var device = JObject.Parse(LocalStorage.GetItem<string>("Device"));
//                if (device.TryGetValue("BuildingCode", out var BuildingCode))
//                {
//                    return BuildingCode.ToString();
//                }
//                else return "";
//            }
//            else return "";
//        }
//        public string GetDeviceCode()
//        {
//            if (LocalStorage.ContainKey("Device"))
//            {
//                var device = JObject.Parse(LocalStorage.GetItem<string>("Device"));
//                if (device.TryGetValue("DeviceCode", out var DeviceCode))
//                {
//                    return DeviceCode.ToString();
//                }
//                else return "";
//            }
//            else return "";
//        }
//        public string GetSiteCode()
//        {
//            if (LocalStorage.ContainKey("Device"))
//            {
//                var device = JObject.Parse(LocalStorage.GetItem<string>("Device"));
//                if (device.TryGetValue("SiteCode", out var SiteCode))
//                {
//                    return SiteCode.ToString();
//                }
//                else return "";
//            }
//            else return "";
//        }

//        public string GetDescription()
//        {
//            if (LocalStorage.ContainKey("Device"))
//            {
//                var device = JObject.Parse(LocalStorage.GetItem<string>("Device"));
//                if (device.TryGetValue("Description", out var Description))
//                {
//                    return Description.ToString();
//                }
//                else return "";
//            }
//            else return "";
//        }
//        public bool GetIsDailyMaint()
//        {
//            if (LocalStorage.ContainKey("Device"))
//            {
//                var device = JObject.Parse(LocalStorage.GetItem<string>("Device"));
//                if (device.TryGetValue("DailyMaint", out var DailyMaint))
//                {
//                    if(bool.Parse(DailyMaint.ToString())) return true;
//                }
//            }
//            return false;
//        }

//        public JObject? GetDevice()
//        {
//            if (LocalStorage.ContainKey("Device"))
//            {
//                JObject device = JObject.Parse(LocalStorage.GetItem<string>("Device"));
//                return device;
//            }
//            else return null;
//        }

//        public void LocalStorageClear()
//        {
//            LocalStorage.RemoveItem("Device");
//        }
//        public async ValueTask IndexedDbDisposeAsync()
//        {
//            if (IndexedDbAccessor != null) {
//                await IndexedDbAccessor.DisposeAsync();
//            }
//        }
//    }
//}
