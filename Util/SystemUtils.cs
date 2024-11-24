using System;
using System.IO;
using Windows.Storage;

namespace AudioReplacer2.Util
{
    public static class SystemUtils
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void CreateSetting(string key, object value)
        {
            try
            {
                if (localSettings.Values.ContainsKey(key))
                {
                    // Optionally update value if the key already exists
                    localSettings.Values[key] = value;
                }
                else
                {
                    // Otherwise, create the new key-value pair
                    localSettings.Values.Add(key, value);
                }

                System.Diagnostics.Debug.WriteLine($"Setting saved: {key} = {value}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving setting: {key}. Exception: {ex.Message}");
            }
        }

        public static object GetSetting(string keyName)
        {
            try
            {
                if (DoesSettingExist(keyName))
                {
                    return localSettings.Values[keyName];
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Setting not found: {keyName}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving setting: {keyName}. Exception: {ex.Message}");
                return null;
            }
        }

        public static bool DoesSettingExist(string keyName)
        {
            try
            {
                return localSettings.Values.ContainsKey(keyName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking setting existence: {keyName}. Exception: {ex.Message}");
                return false;
            }
        }
    }
}
