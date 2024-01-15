# Edge Operator
This page contains basic information how to use **Edge Operator**.

> If you want to try Edge operator in virtual lab environment, please see [Lab Set Up](lab-set-up.md) which will guide you with setting up the environment.

> For installing the operator please see and [Operator Install](operator-install.md) pages.

**Edge Operator** is a kubernetes operator that allows to create proxy between internal kubernetes network and adjacent private network, that is routable from one of worker nodes. For illustration purposes see schema bellow.

![network-schema](http://www.plantuml.com/plantuml/svg/TP7TQiGW48NlVOeXzxMEXDsBKjZtMCfYwn0akoQe-O6ITszihAIi6H0KltEE1oy-A1U6nvtPIHlLm3S3i1IcphLWR21drVtM3f_hXWBvt_iuIcj7tiCxL2Yuh6i-y_aLJnLBGEEweSyMMHUdVIWVoC7rsKIi9hAkFjxsU7nC6SSIJheXW18k4vxeXMthhs_lpAW2tTAvsFKiEPuvdZQdb9ipuroBE2BkepMDvei4lWWy4aYHg_rNo4C5PiN25dOXgzUV_G40)


Edge operator allow to store data about devices in private network and creating proxy connection between private network and kubernetes internal network.

There is CRD [`Device`](device.md) for storing data about devices. **Please see page [`Device`](device.md), before using operator.**

For creating proxy connection between private network and kubernetes internal network [`Connection`](connection.md) CRD is used. **Please see page [`Connection`](connection.md), before using operator.**

---

## Usage

### Register new device
To register new device to cluster, create new CRD [`Device`](device.md). As a sample you can use prepared definition.

```bash
kubectl apply -f doc/sample/device.yaml
```

### Creating proxy
To creating proxy you need to define [NetworkAttachmentDefinition](https://github.com/k8snetworkplumbingwg/multus-cni/blob/master/docs/how-to-use.md)
(NAD). Using NAD you can define how to connect proxy (running in pod) to private network. You can use prepared definition by executing following command.
```bash
kubectl apply -f doc/sample/nad.yaml
```
Once you have your NAD and [`Device`](device.md) ready, you can create proxy. To deploy proxy to server, create [`Connection`](connection.md). For sample purposes yuo can use following command.
```bash
kubectl apply -f doc/sample/sample-connection.yaml
```

> In case you are using sample demo lab, use following command to run required services on sample device
> ```bash
> cd vagrant
> vagrant ssh device01-01
> docker compose up
> ```

At this point new connection to device should be established. You can access device from internal kubernetes network by sending requests to service named after `sample-connection` (`dop-<name_of_connection>`) in example its `dop-sample-connection`.

If you are using provided examples you can test this. See sample below
```bash
kubectl apply -f doc/sample/dbg.yaml
kubectl exec -it debug -- bash
nc dop-sample-connection 8080
nc -u dop-sample-connection 9090
curl dop-sample-connection:8000
```

### Deleting proxy
To delete proxy, just simply delete [`Connection`](connection.md). For sample purposes yuo can use following command.
```bash
kubectl delete -f doc/sample/sample-connection.yaml
```

## Links
1. [**BACK** - Operator Install](operator-install.md)
1. ~~**NEXT**~~
1. [**HOME**](README.md)


