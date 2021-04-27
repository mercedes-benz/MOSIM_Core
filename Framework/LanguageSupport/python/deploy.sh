#!/bin/bash

MOSIMPIP=pip

echo ${1}

if [ "${1}" ] ; then MOSIMPIP=${1} ;  fi

echo ${MOSIMPIP}

if [ ! -d MMIStandard/src ] ; then
	mkdir MMIStandard/src ;
fi

if [ ! -d MMIStandard/src/MMIStandard ] ; then
	cp -r ../thrift/gen-py/MMIStandard MMIStandard/src/ ;
fi

${MOSIMPIP} install -e MMIStandard
${MOSIMPIP} install -e MMIPython
