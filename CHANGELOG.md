# Change Log

## [1.1.6] - 2022-04-05
- Removed all references to Dashboard Mapping (dashboard.db)
- Exchanged BinaryFormatter for BinaryWriter due to security issues with BinaryFormatter

## [1.1.5] - 2022-01-24
- Fixed issue with dynamic files in the MANIFEST.txt
- Removed log4net; all logging goes through grafana logs
- The dashboard mapping database, dashboardmapping.db, is moved out of the plugins directory. If the file already exists in the old location, it will be moved

## [1.1.4] - 2021-11-14
- Plugin published on grafana.com

## [1.1.2] - 2021-06-10
- Mostly fixes needed for Successfully registering in the Grafana Plugin Registry

## [1.0.1] - 2021-05-6
- Exchanged the UA Client to Prediktor.UA.Client (https://www.nuget.org/packages/Prediktor.UA.Client/)
- Added Alarms and Events; historic and subscription
- Config UI and persistence have been changed, not backwards compatible
- Added support for dashboard mapping from instance/type. This is to support complementary Grafana panel plugins at https://github.com/PrediktorAS/grafana

## [1.0.0] - 2020-06-15

- Initial official release that will be submitted to grafana.com
- OPC DA/HDA reads working
- Certificate and "no security" authentication working


