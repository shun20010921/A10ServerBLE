using System;
using System.Collections.Generic;
using Windows.Devices.Bluetooth;
using A10ServerBLE.TargetDevice;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Diagnostics;

namespace A10ServerBLE
{
    public class TargetDeviceFactory
    {
        public static async Task<ITargetDevice> factory(BluetoothLEDevice device)
        {
            return await buildVorzeDevice(device);
        }
        private static async Task<ITargetDevice> buildVorzeDevice(BluetoothLEDevice device)
        {
            string vorzeServiceUuidStr = "40ee1111-63ec-4b7f-8ce7-712efd55b90e";
            string vorzeCharactersticUuidStr = "40ee2222-63ec-4b7f-8ce7-712efd55b90e";

            IDictionary<string, Type> deviceMap = new Dictionary<string, Type>() {
        { "VorzePiston", typeof(VorzeA10Piston)},
        { "CycSA", typeof(VorzeA10Cyclone)},
        { "UFOSA", typeof(VorzeUFOSA)},
    };

            if (device == null) return null;

            var servicesResult = await device.GetGattServicesForUuidAsync(new Guid(vorzeServiceUuidStr));
            var service = servicesResult?.Services.Count > 0 ? servicesResult.Services[0] : null;
            if (service == null) return null;

            Logger.log($"DeviceName: {service.Device.Name}");

            var charResult = await service.GetCharacteristicsForUuidAsync(new Guid(vorzeCharactersticUuidStr));
            if (charResult == null || charResult.Characteristics.Count < 1) return null;

            var characteristic = charResult.Characteristics[0];

            foreach (var key in deviceMap.Keys)
            {
                if (service.Device.Name.Contains(key))
                {
                    var deviceInstance = Activator.CreateInstance(deviceMap[key]) as ITargetDevice;
                    deviceInstance?.init(characteristic);
                    return deviceInstance;
                }
            }

            return null;
        }
    }
}