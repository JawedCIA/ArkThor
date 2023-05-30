################################################################
# Author: Sriram                                               #
# Contact: star.sriram [att] gmail [dot] com                   #
# License: GPLV2                                               #
# Reason: Capstone Project April 2023 - IIT Kanpur             #
# Converts JSONs in https://threatfox.abuse.ch/downloads/misp/ #
# to ARK rules format                                          #
################################################################

import json
import requests
import datetime
import sys

def main():
    js = []
    param = sys.argv[1]
    fname = sys.argv[2]
    if "http" in param:
        r = requests.get(param)
        s = r.json()
    else:
        s = json.load(open(param))
    for dom in s["Event"]["Attribute"]:
        if dom["type"] == "domain":
            js.append({
				"rule_name": dom["comment"],
				"authored_timestamp": int(datetime.datetime.utcnow().timestamp()),
				"rule": "domain == \"%s.\""%(dom["value"]),
				"scope": "DNS",
				"severity": 2,
				"MITRE": [ "T1071.001", "T1132.001", "T1568", "T1102.003" ]
			})

    f = open("ArkThorRule\\%s.ark"%(fname), "w")
    f.write(json.dumps(js, indent=4))
    f.close()
    return

if __name__ == "__main__":
    main()
