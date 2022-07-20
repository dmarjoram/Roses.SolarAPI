# Roses.SolarAPI

### Docker
docker pull drosedev/rosessolarapi

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

# Example calls 
### Using FoxCloud
Force charge (stop discharge) but don't charge from grid (from now until 23:59)

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/ForceChargeForTodayTimePeriod1?enableGridCharging=false`

Force charge now from grid (from now until 23:59)

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/ForceChargeForTodayTimePeriod1?enableGridCharging=true`

Stop force charging

`POST http://dockerhost-ip:mappedport/FoxESS/Cloud/DisableForceChargeTimePeriod1`

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

Set both battery minimum state of charge values at once

`POST http://dockerhost-ip:mappedport/FoxESS/Local/SetBothBatteryMinSoC?minSoc=10&minSocGrid=10`

# Home Assistant
### configuration.yml

    # Control of FOX ESS battery via Roses.SolarAPI (currently using FoxCloud)
        rest_command:
          fox_force_charge_now:
            url: http://dockerhost-ip:mappedport/FoxESS/Cloud/ForceChargeForTodayTimePeriod1?enableGridCharging=false
            method: POST
            headers:
              accept: "application/json"
              user-agent: 'Mozilla/5.0 {{ useragent }}'
            
          fox_force_charge_stop:
            url: http://dockerhost-ip:mappedport/FoxESS/Cloud/DisableForceChargeTimePeriod1
            method: POST
            headers:
              accept: "application/json"
              user-agent: 'Mozilla/5.0 {{ useragent }}'

          fox_workmode_selfuse:
            url: http://dockerhost-ip:mappedport/FoxESS/Cloud/WorkMode/SelfUse
            method: POST
            headers:
              accept: "application/json"
              user-agent: 'Mozilla/5.0 {{ useragent }}'

          fox_workmode_backup:
            url: http://dockerhost-ip:mappedport/FoxESS/Cloud/WorkMode/Backup
            method: POST
            headers:
              accept: "application/json"
              user-agent: 'Mozilla/5.0 {{ useragent }}'

          fox_workmode_feedin:
            url: http://dockerhost-ip:mappedport/FoxESS/Cloud/WorkMode/FeedIn
            method: POST
            headers:
              accept: "application/json"
              user-agent: 'Mozilla/5.0 {{ useragent }}'
