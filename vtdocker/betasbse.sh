#!/usr/bin/env bash
fsl5.0-bet $4 $6 -f 0.5 -R
gzip -d $6.gz
