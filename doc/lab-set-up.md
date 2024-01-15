# Lab setup
At the end of this guide you should have up end running kubernetes cluster for demo purposes.

This guide helps you with setting up lab for demo.
Guide was tested using: `virtualbox-6.1`, `vagrant-2.3.4`, `debian-11`.

## Requirements
- [virtualbox](https://www.virtualbox.org/) (or other compatible VMM)
- [ansible](https://www.ansible.com/)
- [vagrant](https://www.vagrantup.com/)
- kubectl

## Guide

### 0. Setting up host
In case you are using virtualbox, make sure you have enabled the creation of the necessary ip ranges

```bash
# allow full range of IPv4 for VMs
echo '* 0.0.0.0/0 ::/0' | sudo tee -a /etc/vbox/networks.conf
```
Install required ansible packages
```bash
# install ansible requirements
ansible-galaxy install -r ansible/requirements.yaml
```
Add all public SSH keys, that you want to import to VMs,  to `vagrant/.ssh_public_keys`. If you don't want to add any, leave the file empty.
```bash
# set list of public keys to import
cat ~/.ssh/id_rsa.pub >> ./vagrant/.ssh_public_keys
```

### 1. Create VMs
Use vagrant to create all VMs.
```bash
cd vagrant
# create and run VMs
vagrant up
```
If you want to add or remove any machine, fell fre to customize `vagrant/Vagrantfile`

### 2. Setup VMs
Install and configure VMs. You can use provided playbooks from `ansible/playbook`.
```bash
cd ansible
# setup cluster nodes
ansible-playbook playbook/infra.yaml -i inventory/inv
# setup demo on devices
ansible-playbook playbook/device.yaml -i inventory/inv
```

### 3. Set up k8s
To set up k8s cluster, log into `kmaster` machine. Use *kube* user for interacting with k8s. When using `kubeadm` for setting up cluster, make sure you use correct parameter (in case you are not using default settings).

Feel free to add custom IP and DNS names to `apiserver-cert-extra-sans` parameter so you can access k8s cluster from remote locations. Example uses bt (bachelor thesis) DNS name.

Make sure that `/etc/kubernetes/admin.conf` file located on `kmaster` is readable by user *kube*.
```bash
cd vagrant
# log into kmaster
vagrant ssh kmaster
sudo su - kube

# pull container images for k8s
sudo kubeadm config images pull
# create kubernetes cluster
# set kubernetes API endpoint
# add records about other IPs to certificate (add IPs and DNS names you will be using for connecting to cluster)
# set IP range for PODs, depends on used CNI and its configuration
sudo kubeadm init \
    --apiserver-advertise-address=172.16.16.100 \
    --apiserver-cert-extra-sans=bt,10.38.6.86 \
    --pod-network-cidr=10.244.0.0/16 \
    | tee -a ~/kubeinit.log
# store connection string !DON'T USE FOR PRODUCTION!
sudo kubeadm token create --print-join-command | tee ~/joincluster.sh
# set up credentials for kubelet
ln -s /etc/kubernetes/admin.conf ~/.kube/config
exit

sudo chmod +r /etc/kubernetes/admin.conf
exit
```

```bash
cd vagrant
# add other nodes to cluster
../scripts/joinClusterWithNodes.sh
```

### 4. Set up local env
Flowing script will add credentials for kubectl.
```bash
cd vagrant
# update your kubectl config for newly created kubernetes cluster
../scripts/locaKubectl.sh bt_trojaj12
# switch context to newly created kubernetes cluster
kubectl config use-context bt_trojaj12
```
Feel free to edit cluster name. You can pass new name as an first (`$1`) argument to script. Example uses *bt_trojaj12* as a name for the cluster.

### 5. Set up k8s
```bash
# setup cluster
kubectl apply -f manifests/lab/
```

## Test
Now you should have running kubernetes cluster, you can test that by running following command.

**Setting up lab environment may take some time.** Please wait at least 5 minutes until you start looking for a problem.
```bash
kubectl get nodes
```
In case you see list of nodes which are in state `Ready`, you have successfully created lab environment.

---
## Links
1. ~~**BACK**~~
1. [**NEXT** - Operator Install](operator-install.md)
1. [**HOME**](README.md)