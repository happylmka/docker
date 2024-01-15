# Device kind
Edge operator works with two *CRD*, one of the is `Device`, this resource is part of `edge-operator.k8s.dvojak.cz` group.
```yaml
apiVersion: edge-operator.k8s.lms.ru/v1
kind: Device
```
`Device` represents a device connected to the cluster. This device should be available over one of the nodes in the cluster. Usually those devices are directly connected to one of nodes. Devices are often placed in private network that is not accessible from anywhere, but node(s) in the cluster, that serve as a gateway to the device.

Device represents a entity that can communicate over *TCP* / *UDP*. This entity can be a physical device or any other service.

This information holds information about the device that are referred in resource [`Connection`](connection.md)

For more information, see [`Connection`](connection.md).

## API
> You can documentation of `CRD` you can you
> ```bash
> # Get documentation for CRD device
> kubectl explain device
> # Get documentation for CRD's device.spec
> kubectl explain device.spec
> ```

> You can get YAML schema of CRD by using following command
> ```bash
> # Get openAPIV3Schema of device resource
> kubectl get crd devices.edge-operator.k8s.dvojak.cz -o yaml \
>     | yq '.spec.versions.[] | select( .name == "v1").schema'
> ```

### Sample `device` definition
```yaml
apiVersion: edge-operator.k8s.lms.ru/v1
kind: Device
metadata:
  name: device01-01
spec:
  nodeName: kedge1
  up: true
  ipAddress: 172.17.16.120
  components:
    - name: backend
      up: true
      handlers:
        - name: api server
          protocol: HTTP
          port: 443
          endPoints:
            - https://<path>/api/*
            - https://<path>/version
```

### Meaning of attributes
- `.spec.nodeName` \<string\> --required--
    - name of the node, the device is connected to
- `.spec.up` \<boolean\> --Default: `true`--
    - determines if the device is up or down (ready and running or not)
- `.spec.ipAddress` \<string\> --required--
    - IP address of the device
- `.spec.components` \<list[Component]\> --required--
    - list of components on the device
- `.spec.components.name` \<string\> --required--
    - name of the component
- `.spec.components.up` \<boolean\> --Default: `true`--
    - determines if the component is up or down (ready and running or not)
- `.spec.components.handlers` \<list[Handler]\>
    - list of handlers of the component (ports)
- `.spec.components.handlers.name` \<string\> --Default: `null`--
    - name of the handler for the component
- `.spec.components.handlers.protocol` \<enum\> --Default: `TCP`--
    - type of the handler protocol ([`TCP`, `UDP`, `HTTP`])
- `.spec.components.handlers.port` \<uint\> --required--
    - port of the handler
- `.spec.components.handlers.endpoints` \<list[string]\> --Default: `null`--
    - list of endpoints of the handler

### Validator
API is checking for folowing validations.

1. Check IP address is valid
1. Check node exists
    1. Check if node has required labels
1. Check if all components, that are listed, are up
2. Check if device has unique names for *TCP* and *UDP*

**With strict mode enabled (default)**\
If all checks pass, device will be created. If any of checks fail. API will return error.

**With strict mode disabled**\
API will accept device definition and report validation only as a waring.

> To setup validator see [Operator Configuration](operator-configuration.md)

---
## Links
1. ~~**BACK**~~
1. ~~**NEXT**~~
1. [**HOME**](README.md)