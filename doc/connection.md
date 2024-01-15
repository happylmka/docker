# Connection kind
Edge operator works with two *CRD*, one of the is `Connection`, this resource is part of `edge-operator.k8s.dvojak.cz` group.
```yaml
apiVersion: edge-operator.k8s.lms.ru/v1
kind: Connection
```
Connection represents a connection between a device and kubernetes cluster. By creating a connection, service will be created for abstracting device connection. By sending data to that service, data will be sent to device.

For more information, see [`Device`](device.md).

## API
> You can documentation of `CRD` you can you
> ```bash
> # Get documentation for CRD connection
> kubectl explain connection
> # Get documentation for CRD's connection.spec
> kubectl explain connection.spec
> ```

> You can get YAML schema of CRD by using following command
> ```bash
> # Get openAPIV3Schema of connection resource
> kubectl get crd connection.edge-operator.k8s.dvojak.cz -o yaml \
>     | yq '.spec.versions.[] | select( .name == "v1").schema'
> ```

### Sample `connection` definition
```yaml
apiVersion: edge-operator.k8s.lms.ru/v1
kind: Connection
metadata:
  name: sample-connection
  namespace: default
spec:
  deviceName: device01-01
  networkName: bridge-conf
  componentNames:
    - backend
```

### Meaning of attributes
- `.spec.deviceName` \<string\> --required--
    - name of the Device to connect to
- `.spec.networkName` \<string\> --required--
    - name of the [`NetworkAttachmentDefinition`](https://github.com/k8snetworkplumbingwg/multus-cni/blob/master/docs/how-to-use.md) to use for the connection
- `.spec.componentNames` \<list[string]\> --required--
    - names of the Components (of device) to connect to


### Validator
API is checking for folowing validations.

1. Check if device exists
    1. Check if device is up
    1. Check if device contains all components that are listed in `.spec.componentNames`
    1. Check if all components, that are listed, are up
2. Check if `NAD` (`.spec.networkName`) exists

**With strict mode enabled (default)**\
If all checks pass, connection will be created. If any of checks fail. API will return error.

**With strict mode disabled**\
API will accept connection definition and report validation only as a waring.

> To setup validator see [Operator Configuration](operator-configuration.md)

---
## Links
1. ~~**BACK**~~
1. ~~**NEXT**~~
1. [**HOME**](README.md)