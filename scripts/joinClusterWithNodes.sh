#!/bin/bash
MATSER_NAME="${1:-kmaster}"

COMMAND=$(vagrant ssh $MATSER_NAME -c 'cat /home/kube/joincluster.sh' 2>/dev/null | tr -d '\r')
MACHINES=$( vagrant status --no-color --machine-readable | cut -d, -f2 | uniq -u | sed -E "/^$|$MATSER_NAME|device/ d" )

for machine in $MACHINES; do
   echo "Joining $machine to cluster"
   vagrant ssh -c "sudo $COMMAND" $machine >/dev/null 2>&1
done