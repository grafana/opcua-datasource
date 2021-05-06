# Change Log

## [1.0.0] - 2020-06-15

- Initial official release that will be submitted to grafana.com
- OPC DA/HDA reads working
- Certificate and "no security" authentication working

## [1.0.1] - 2021-05-6
- Exchanged the UA Client to Prediktor.UA.Client (https://www.nuget.org/packages/Prediktor.UA.Client/)
- Added Alarms and Events; historic and subscription
- Config UI and persistence have been changed, not backwards compatible
- Added support for dashboard mapping from instance/type. This is to support complementary Grafana panel plugins at https://github.com/PrediktorAS/grafana
