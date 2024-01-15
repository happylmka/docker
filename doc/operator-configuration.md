# Configuration of edge-operator
Edge-operator is configurable by environment variables, command line arguments, configuration file. All 3 methods provide same set of configuration parameters. In case of colision, method with highes priprity will used. Priorities are `command_line > environment_variables > conf_file`.

## Usefull configuration Envirnment variable
- `Serilog__MinimumLevel__Default`: ['Verbose', 'Debug', 'Information', 'Warning', 'Error', 'Fatal']
- `Validator__DeviceStrict`: ['false', 'true']
- `Validator__ConnectionStrict`: ['false', 'true']

For more information you can see default configuration file [appsetings.json](https://github.com/dvojak-cz/Bachelor-Thesis/blob/master/code/EdgeOperator/EdgeOperator/appsettings.json)
---
## Links
1. ~~**BACK**~~
1. ~~**NEXT**~~
1. [**HOME**](README.md)
