# Roses.SolarAPI

### Docker
docker pull drosedev/rosessolarapi

| Environment Variable | Example | Description |
|--|--|--|
| FOXESSCONFIGURATION__IPADDRESS | addme | The IP address on the LAN for local USR device. Currently not used to set battery charging. |
| FOXESSCONFIGURATION__USERNAME| addme| Your FoxCloud username |
| FOXESSCONFIGURATION__PASSWORDMD5HASH | addme1234567890addme | Lower-case MD5 hash of your FoxCloud password |
| FOXESSCONFIGURATION__SERIALNUMBER | 66xxxxaddme| Your Fox inverter serial number (required for FoxCloud battery API calls) |

# Example calls 
### Using FoxCloud
Force charge (stop discharge) but don't charge from grid (from now until 23:59)
`POST http://192.168.0.5:8200/FoxESS/Cloud/ForceChargeForTodayTimePeriod1?enableGridCharging=false`

Force charge now from grid (from now until 23:59)
`POST http://192.168.0.5:8200/FoxESS/Cloud/ForceChargeForTodayTimePeriod1?enableGridCharging=true`

Stop force charging
`POST http://192.168.0.5:8200/FoxESS/Cloud/DisableForceChargeTimePeriod1`

### Using modbus
Query battery status using RS485 device
`GET http://192.168.0.5:8200/FoxEss/Local/BatteryConfiguration`

# Home Assistant
### configuration.yml

    # Control of FOX ESS battery via Roses.SolarAPI (currently using FoxCloud)
        rest_command:
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
