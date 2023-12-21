# Roses.SolarAPI

### Docker

**AMD64**

docker pull drosedev/rosessolarapi:2.2

**Raspberry Pi**
NOTE: Fox Cloud calls currently not working on the Pi image. Use the x64 image instead for now.
docker pull drosedev/rosessolarapi:1.7-bullseye-slim-arm32v7

https://hub.docker.com/repository/docker/drosedev/rosessolarapi

| Environment Variable | Example | Description |
|--|--|--|
| FOXESSCONFIGURATION__IPADDRESS | addme | The IP address on the LAN for local USR device. Currently not used to set battery charging. |
| FOXESSCONFIGURATION__USERNAME| addme| Your FoxCloud username |
| FOXESSCONFIGURATION__PASSWORDMD5HASH | addme1234567890addme | Lower-case MD5 hash of your FoxCloud password |
| FOXESSCONFIGURATION__SERIALNUMBER | 66xxxxaddme| OPTIONAL - Your Fox inverter serial number (required for FoxCloud battery API calls). **Providing this speedsup API calls as we don't have to internally read the device list from the cloud**) |
| FOXESSCONFIGURATION__CLOUDDEVICEID | a234567a-dabc-4510-1234-e61ac52f4dv2 | OPTIONAL - Your Fox inverter device ID GUID from the address bar of Fox Cloud portal (required for FoxCloud battery API calls). **Providing this speedsup API calls as we don't have to internally read the device list from the cloud**) |
| TZ | Europe/London | Required so force charging starts at the correct local time |

Internal to the container the service runs on port 80. You can map this internal port to whatever you want with bridged networking on your Docker host.

Swagger UI for testing available at http://dockerhost-ip:mappedport/swagger/

# Example calls 
### Using FoxCloud
Force charge (stop discharge) but don't charge from grid (from now until 23:59)

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/ForceChargeForTodayTimePeriod1?enableGridCharging=false`

Force charge now from grid (from now until 23:59)

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/ForceChargeForTodayTimePeriod1?enableGridCharging=true`

Force charge all day from grid (from 00:01 until 23:59)

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/ForceChargeAllTodayTimePeriod1?enableGridCharging=true`

Stop force charging

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/DisableForceChargeTimePeriod1`

Force discharge now (from now until 23:59 at 5000W)

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/SetForceDischarge?dischargePower=5000`

Stop force discharge

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/DisableForceDischarge`

Work Mode

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/WorkMode/SelfUse`

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/WorkMode/Backup`

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/WorkMode/FeedIn`

Set both battery minimum state of charge values at once

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/SetBothBatteryMinSoC?minSoc=10&minSocGrid=10`

### Using modbus TCP
Query battery status using RS485 device

`GET http://dockerhost-ip:mappedport/FoxEss/Local/BatteryConfiguration`

Set battery minimum state of charge

`POST http://dockerhost-ip:mappedport/FoxESS/Local/SetBatteryMinSoC?percentage=10`

Set battery minimum state of charge when on-grid

`POST http://dockerhost-ip:mappedport/FoxESS/Local/SetBatteryMinGridSoC?percentage=10`

Hold current state of charge in battery (on-grid)

`POST http://dockerhost-ip:mappedport/FoxESS/Local/SetBatteryMinGridSoCToCurrentSoc`

Set both battery minimum state of charge values at once

`POST http://dockerhost-ip:mappedport/FoxESS/Local/SetBothBatteryMinSoC?minSoc=10&minSocGrid=10`

Work Mode

`POST http://dockerhost-ip:mappedport/FoxESS/Local/WorkMode/SelfUse`

`POST http://dockerhost-ip:mappedport/FoxESS/Local/WorkMode/Backup`

`POST http://dockerhost-ip:mappedport/FoxESS/Local/WorkMode/FeedIn`

Force Charge from Grid (now until 23:59)

`POST http://dockerhost-ip:mappedport/FoxESS/Local/ForceChargeForToday`

Force Charge from Grid (00:00 until 23:59)

`POST http://dockerhost-ip:mappedport/FoxESS/Local/ForceChargeAllToday`

Disable Force Charge

`POST http://dockerhost-ip:mappedport/FoxESS/Local/DisableForceCharge`

# Home Assistant
### configuration.yml
### Control of FOX ESS battery via Roses.SolarAPI
```
rest_command:
  fox_modbus_hold_charge:
    url: http://192.168.0.5:8200/FoxESS/Local/SetBatteryMinGridSoCToCurrentSoc
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'
      
  fox_modbus_slow_charge:
    url: http://192.168.0.5:8200/FoxESS/Local/SetBatteryMinGridSoC?percentage=99
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'
      
  fox_modbus_allow_discharge:
    url: http://192.168.0.5:8200/FoxESS/Local/SetBatteryMinGridSoC?percentage=10
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'
      
  fox_modbus_force_charge_now:
    url: http://192.168.0.5:8200/FoxESS/Local/ForceChargeAllToday
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'

  fox_modbus_force_charge_stop:
    url: http://192.168.0.5:8200/FoxESS/Local/DisableForceCharge
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'

  fox_force_charge_now_from_grid:
    url: http://192.168.0.5:8200/FoxESS/Cloud/ForceChargeAllTodayTimePeriod1?enableGridCharging=true
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'

  fox_force_charge_now:
    url: http://192.168.0.5:8200/FoxESS/Cloud/ForceChargeForTodayTimePeriod1?enableGridCharging=false
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'
    
  fox_force_charge_stop:
    url: http://192.168.0.5:8200/FoxESS/Cloud/DisableForceChargeTimePeriod1
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'
    
  fox_force_discharge_now:
    url: http://192.168.0.5:8200/FoxESS/Cloud/SetForceDischarge?dischargePower=5000
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}' 

  fox_force_discharge_stop:
    url: http://192.168.0.5:8200/FoxESS/Cloud/DisableForceDischarge
    method: POST
    headers:
      accept: "application/json"
      user-agent: 'Mozilla/5.0 {{ useragent }}'
```
