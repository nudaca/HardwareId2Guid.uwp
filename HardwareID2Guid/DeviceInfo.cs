//------------------------------------------------------------------------------
//  This code was refactored from Ptc.Ovx.WindowsPhone.Client.App.Utilities.DeviceInfo
//
//  Changes to this file may create inconsistencies with other platforms.
//------------------------------------------------------------------------------

using System;
using Windows.Networking.Connectivity;
using System.Net.NetworkInformation;
using System.Text;
using Windows.Devices.Enumeration.Pnp;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using System.Runtime.InteropServices.WindowsRuntime;

namespace HardwareID2Guid
{
    public sealed class DeviceInfo
    {
        private const string COMPUTER_CONTAINER_ID = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";
        private static DeviceInfo _Instance;
        public static DeviceInfo Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DeviceInfo();
                return _Instance;
            }

        }

        public string Id { get; private set; }
        public string GUID { get; private set; }
        public string Model { get; private set; }
        public string Manufracturer { get; private set; }
        public string Name { get; private set; }
        public static string OSName { get; set; }

        private DeviceInfo()
        {
            Id = GetId();
            GUID = GetMachineId();
            var deviceInformation = new EasClientDeviceInformation();
            Model = deviceInformation.SystemProductName;
            Manufracturer = deviceInformation.SystemManufacturer;
            Name = deviceInformation.FriendlyName;
            OSName = deviceInformation.OperatingSystem;
        }

        private static string GetId()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
            {
                var token = HardwareIdentification.GetPackageSpecificToken(null);
                var hardwareId = token.Id;
                var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

                byte[] bytes = new byte[hardwareId.Length];
                dataReader.ReadBytes(bytes);

                return BitConverter.ToString(bytes).Replace("-", "");
            }

            throw new Exception("NO API FOR DEVICE ID PRESENT!");
        }

        public static string GetMachineId()
        {
            var hardwareToken =
                HardwareIdentification.GetPackageSpecificToken(null).Id.ToArray();
            var count = hardwareToken.Length / 4;
            string id1 = "0000";
            string id2 = "0000";
            string id3 = "0000";
            string id4 = "0000";
            string id5 = "0000";
            string id9 = "0000";
            for (int i = 0; i < count; i++)
            {
                switch (BitConverter.ToUInt16(hardwareToken, i * 4))
                {
                    case 1:
                        // processor
                        if (id1 == "0000")
                            id1 = BitConverter.ToString(hardwareToken, i * 4 + 2, 2).Replace("-", "");
                        break;
                    case 2:
                        // memory
                        if (id2 == "0000")
                            id2 = BitConverter.ToString(hardwareToken, i * 4 + 2, 2).Replace("-", "");
                        break;
                    case 3:
                        // disk
                        if (id3 == "0000")
                            id3 = BitConverter.ToString(hardwareToken, i * 4 + 2, 2).Replace("-", "");
                        break;
                    case 4:
                        // network adapter
                        if (id4 == "0000")
                            id4 = BitConverter.ToString(hardwareToken, i * 4 + 2, 2).Replace("-", "");
                        break;
                    case 5:
                        // audio adapter
                        if (id5 == "0000")
                            id5 = BitConverter.ToString(hardwareToken, i * 4 + 2, 2).Replace("-", "");
                        break;
                    case 6:
                        // docking station
                        break;
                    case 7:
                        // mobile broadband
                        break;
                    case 8:
                        // bluetooth
                        break;
                    case 9:
                        // system BIOS
                        if (id9 == "0000")
                            id9 = BitConverter.ToString(hardwareToken, i * 4 + 2, 2).Replace("-", "");
                        break;
                }
            }

            return id1 + id2 + "-" + id3 + "-" + id4 + "-" + id5 + "-" + id9 + "FFFFFFF";
        }


        /// <summary>
        /// Gets the unique identifier for the device.
        /// </summary>
        /// <returns>A string representation of the unique device identifier.
        internal static string GetDeviceId()
        {
            //Keep in mind that the user could delete the local storage and cause your app to forget and regenerate 
            //that value (or potentially even modify/spoof the value), so don't depend on it for any critical security 
            //purposes.

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Guid id;
            //string id2;
            //if (!localSettings.Values.ContainsKey("UniqueDeviceId"))
            //{
            //var id2 = GetId();
            var id2 = GetMachineId();
            //id = new Guid(GetId());
            id = Guid.NewGuid();
            localSettings.Values["UniqueDeviceId"] = id;
            //}
            //else
            id = (Guid)localSettings.Values["UniqueDeviceId"];
            return id.ToString();
        }

        /// <summary>
        /// Gets the unique identifier for the device RMS channel for Live TV. (deviceID = c649f5b3-82b7-47c7-9079-b748c4bc56a6 and return deviceIDRMS = "b7:48:c4:bc:56:a6")
        /// </summary>
        /// <returns>A string representation of the unique device identifier.
        internal static string GetDeviceIdRMS(string deviceID)
        {
            string idRMS = string.Empty;
            string lastPart = string.Empty;

            if (deviceID.Contains("-"))
            {
                string[] guidParts = deviceID.Split('-');
                lastPart = guidParts[guidParts.Length - 1];
            }

            if (lastPart != string.Empty)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < lastPart.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        if (i != 0)
                        {
                            sb.Append('-');
                        }
                    }
                    sb.Append(lastPart[i]);
                }
                idRMS = sb.ToString();
            }

            return idRMS;
        }

        /// <summary>
        /// Gets the vendor of the device.
        /// </summary>
        /// <returns>A string representation of the vendor device.
        internal static string GetVendor()
        {
            return GetComputerPropertyAsync("System.Devices.Manufacturer").Result;
        }

        /// <summary>
        /// Gets the model of the device.
        /// </summary>
        /// <returns>A string representation of the model device.
        internal static string GetModelDevice()
        {
            return GetComputerPropertyAsync("System.Devices.ModelName").Result;
        }

        /// <summary>
        /// Gets a computer property async.
        /// </summary>
        /// <param name="property">The property to get.</param>
        /// <returns></returns>
        private static async Task<string> GetComputerPropertyAsync(string property)
        {
            string result = null;

            string[] properties = { property };

            //Other properties:
            //"System.Devices.Manufacturer",
            //"System.ItemNameDisplay", 
            //"System.Devices.ModelName", 
            //"System.Devices.Connected",
            //"System.Devices.InterfaceClassGuid",
            //"System.Devices.ContainerId", 
            //"{78c34fc8-104a-4aca-9ea4-524d52996e57} 90"

            var containers = await PnpObject.FindAllAsync(PnpObjectType.DeviceContainer, properties);
            var computer = containers.FirstOrDefault(o => o.Id == COMPUTER_CONTAINER_ID);

            if (computer.Properties.ContainsKey(property))
            {
                result = computer.Properties[property].ToString();
                if (!string.IsNullOrEmpty(result)) result = result.Replace("_", "-");
            }
            return result;
        }

        /// <summary>
        /// Gets the OS version.
        /// </summary>
        /// <returns>A string representation of the OS version.
        internal static string GetOSVersion()
        {
            //There is intentionally no way of getting the OS version. Historically applications have mis-used the OS version instead of relying on various forms of feature detection which have caused significant appcompat issues for the development team. For Windows 8 the dev team decided to avoid the issue entirely by not providing a GetVersion API.
            return "10.0";
        }

        /// <summary>
        /// Determines whether device is using Wifi or not.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if is using Wifi otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsWifiEnabled()
        {
            return (GetNetworkType() == NetworkType.Wifi || GetNetworkType() == NetworkType.Ethernet);
        }

        internal static bool IsEthernetEnabled()
        {
            return (GetNetworkType() == NetworkType.Ethernet);
        }

        internal static bool IsAnyInternetAvailable()
        {
            //if (Is3GEnabled() || IsEthernetEnabled() || IsWifiEnabled())
            if (IsInternetEnabled())
                return true;
            else
                return false;
        }

        public static bool IsInternetEnabled()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            return connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
        }

        /// <summary>
        /// Determines whether device is using 3G or not.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if is using 3G connection otherwise, <c>false</c>.
        /// </returns>
        internal static bool Is3GEnabled()
        {
            return GetNetworkType() == NetworkType.Mobile;
        }


        // http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh452991.aspx

        /// <summary>
        /// Represents a network type
        /// </summary>
        public enum NetworkType { Ethernet, Wifi, Mobile, Unknown };


        /// <summary>
        /// Gets the connection type in use by the device
        /// </summary>
        /// <returns>An enum representing the connection type.</returns>
        public static NetworkType GetNetworkType()
        {
            ConnectionProfile internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (NetworkInterface.GetIsNetworkAvailable() && internetConnectionProfile != null)
            {
                /*
                ConnectionCost cost=internetConnectionProfile.GetConnectionCost();

                DataPlanStatus planstatus=internetConnectionProfile.GetDataPlanStatus();
                NetworkConnectivityLevel connectivitylevel=internetConnectionProfile.GetNetworkConnectivityLevel();
                IReadOnlyList<String> names=internetConnectionProfile.GetNetworkNames();
                foreach(String s in names)
                {
                    //System.Diagnostics.Debug.WriteLine(s);

                    // Ethernet
                 //   if (s.Equals("Ethernet")) return NetworkType.Ethernet;
             //       if (s.Equals("Wifi")) return NetworkType.wifi;

                }
                */
                NetworkTypes types = internetConnectionProfile.NetworkAdapter.NetworkItem.GetNetworkTypes();

                //http://msdn.microsoft.com/en-us/library/windows/apps/xaml/windows.networking.connectivity.networkadapter.ianainterfacetype.aspx
                uint ianatype = internetConnectionProfile.NetworkAdapter.IanaInterfaceType;


                if (string.IsNullOrEmpty(GetConnectedNetwork()))
                {
                    return NetworkType.Unknown;
                }
                else if (GetConnectedNetwork() == "Ethernet")
                {
                    return NetworkType.Ethernet;
                }
                else
                {
                    if (ianatype == 23) return NetworkType.Mobile;
                    if (ianatype == 71 || ianatype == 6) return NetworkType.Wifi;
                }
            }

            return NetworkType.Unknown;
        }



        /// <summary>
        /// Check if current Device is Mobile
        /// </summary>
        /// <returns></returns>
        public static bool IsMobile()
        {
            return Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ? true : false;

        }



        /// <summary>
        /// Verify if exist hardware back button mobile
        /// </summary>
        /// <returns>true if exist back hardware button in mobile, otherwise, false</returns>
        public static bool CheckHardwareBackButtonIsAvailable()
        {


            string nspace = "Windows.Phone.UI.Input.HardwareButtons";
            bool isHardwareButtonsAPIPresent = Windows.Foundation.Metadata.ApiInformation.IsTypePresent(nspace);
            return isHardwareButtonsAPIPresent;
        }

        private static string GetConnectedNetwork()
        {
            string connectedNetwork = string.Empty;

            try
            {

                List<ConnectionProfile> ConnectionProfiles = Windows.Networking.Connectivity.NetworkInformation.GetConnectionProfiles().ToList();

                if (ConnectionProfiles.Count > 0)
                {
                    for (var i = 0; i < ConnectionProfiles.Count; i++)
                    {
                        string str = ConnectionProfiles[i].GetNetworkConnectivityLevel().ToString();
                        if (str != "None")
                        {
                            connectedNetwork = ConnectionProfiles[i].ProfileName;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return connectedNetwork;
        }
    }

}