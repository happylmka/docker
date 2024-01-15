#!/bin/bash
MATSER_NAME="${1:-bt_trojaj12}"
vagrant ssh kmaster -c 'sudo cat /etc/kubernetes/admin.conf' >/tmp/k8s.konf.txt 2>/dev/null
SERVER=$(cat /tmp/k8s.konf.txt | yq '.clusters[0].cluster.server')
CERTIFICATE_CRT=$(cat /tmp/k8s.konf.txt | yq '.clusters[0].cluster.certificate-authority-data')
USER=$(cat /tmp/k8s.konf.txt | yq '.users[0].name' ) 
USER_CRT=$(cat /tmp/k8s.konf.txt | yq '.users[0].user.client-certificate-data' )
USER_KEY=$(cat /tmp/k8s.konf.txt | yq '.users[0].user.client-key-data' )

kubectl config set-cluster $MATSER_NAME --server=$SERVER
kubectl config set clusters.${MATSER_NAME}.certificate-authority-data $CERTIFICATE_CRT

kubectl config set-credentials $MATSER_NAME
kubectl config set users.${MATSER_NAME}.client-certificate-data $USER_CRT
kubectl config set users.${MATSER_NAME}.client-key-data $USER_KEY

kubectl config set-context $MATSER_NAME --cluster=$MATSER_NAME --user=$MATSER_NAME

rm /tmp/k8s.konf.txt