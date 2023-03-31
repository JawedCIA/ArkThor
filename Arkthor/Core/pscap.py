##############################################################
# Author: Sriram											 #
# Contact: star.sriram [att] gmail [dot] com				 #
# License: GPLV2											 #
# Reason: Capstone Project April 2023 - IIT Kanpur		   #
##############################################################

from scapy.all import *
import sys
import json
import rule_engine
import sqlite3
import hashlib
import requests
import re
import time


class packetprocessengine:
	def __init__(self):
		self.fname = None
		self.dns_list = {}
		self.packet_processed = False

	def loadpcap(self, filename=None):
		if self.fname == None and filename == None:
			raise Exception("No Parameters passed")

		elif filename != None and self.fname != None and filename != self.fname:
			raise Exception("Multiple filenames passed")

		elif filename != None and self.fname == None:
			self.fname = filename

		self.packets = rdpcap(self.fname)

	def process_packet(self, packettypes=[DNS, TCP]):
		for packet in self.packets:
			if DNS in packettypes and packet.haslayer(DNS):
				self.process_dns_packet(packet[DNS])

		self.packet_processed = True
		return

	def process_dns_packet(self, dns):
		pident = {1: "A", 2: "NS", 5: "CNAME", 6: "SOA", 12: "PTR", 15: "MX", 16: "TXT", 28: "AAAA", 33: "SRV",
				  255: "ANY"}
		if dns.qr == 0:  # DNS query
			dv = dns.qd.qname.decode()
			if dv not in self.dns_list:
				self.dns_list[dv] = {}
			self.dns_list[dv] = {"dns_query_num": dns.qd.qtype, "dns_query_type": pident[dns.qd.qtype]}
		# print("DNS Query Name:", dns.qd.qname.decode())
		# print("DNS Query Type:", dns.qd.qtype)

		elif dns.qr == 1:  # DNS response
			if dns.qd == None:
				return
			dv = dns.qd.qname.decode()
			if dv not in self.dns_list:
				raise Exception("DNS answer present, without query")
			self.dns_list[dv]["response_code"] = dns.rcode
			# print("DNS Response Code:", dns.rcode)
			if dns.an is not None:
				# print(dns.an)
				self.dns_list[dv]["answers"] = []
				for answer in dns.an:
					if hasattr(answer, "rdata") == True:
						if type(answer.rdata) == bytes:
							# print("Answer:", answer.rdata.decode())
							self.dns_list[dv]["answers"].append(answer.rdata.decode())
						else:
							# print("Answer:", answer.rdata)
							self.dns_list[dv]["answers"].append(answer.rdata)

				for ip in self.dns_list[dv]["answers"]:
					if re.match("\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}", ip) is not None:
						print("Val", ip)
						self.dns_list[dv]["CN"] = get_cn_from_ip(ip)

				self.dns_list[dv]["answers"] = ", ".join(self.dns_list[dv]["answers"])

	def get_processed_dns_packet(self):
		if self.packet_processed == True:
			return self.dns_list

		return {"success": False, "comments": "packet not processed"}

	def get_country_list(self):
		retlist = []
		for r in self.dns_list:
			if "CN" in self.dns_list[r]:
				if self.dns_list[r]['CN'] == "00":
					continue
				elif self.dns_list[r]['CN'] == "":
					continue
				elif self.dns_list[r]['CN'] in retlist:
					continue
				retlist.append(self.dns_list[r]['CN'])

		return retlist


class rulesengine:
	def __init__(self):
		self.rengine = {}
		self.ruleset = []

	def validaterules(self):

		return True

	def rundomainrules(self, content):
		for fn in os.listdir():
			if ".ark" not in fn: continue
			print("Processing rules from", fn)
			self.rengine = json.loads(open(fn).read())
			for r in self.rengine:
				context = rule_engine.Context(type_resolver=rule_engine.type_resolver_from_dict({
					'domain': rule_engine.DataType.STRING,
					'address': rule_engine.DataType.ARRAY(rule_engine.DataType.STRING)
				}))
				rule = rule_engine.Rule(r["rule"])
				for x in content:
					js = {"domain": x}
					for y in content[x]:
						js[y] = content[x][y]
					try:
						if rule.matches(js) == True:
							print("Matched", js)
							rs = r
							if "rule" in rs: del (rs['rule'])
							if "scope" in rs: del (rs['scope'])
							self.ruleset.append(rs)
					except rule_engine.errors.SymbolResolutionError as ex:
						# print("Unavailable param", ex)
						pass

	def get_detected_rules(self):
		return self.ruleset

class processing_history():
	def __int__(self):
		self.sqlite_fn = "processinglist.sqlite3"
		if not os.path.exists(self.sqlite_fn):
			conn = sqlite3.connect(self.sqlite_fn)
			cur = conn.cursor()
			cur.execute("CREATE TABLE processed (filehash char(64) not NULL, ts TIMESTAMP DEFAULT CURRENT_TIMESTAMP")
			conn.commit()
			conn.close()

def intimate_status(filehash, status):
	r = requests.put("http://localhost:33900/api/FileRecord/UpdateStatus",
					 params={"hash256": filehash.upper(), "status": status})
	print(r.status_code)


def intimate_completion(fjson):
	r = requests.post("http://localhost:33900/api/FileRecord/UploadFileOutPutJson", files={"file": open(fjson)},
					  headers={"Content-Type": "multipart/form-data"})
	print(r.status_code)


def get_cn_from_ip(ipaddr):
	import struct
	import socket

	if not os.path.exists("ipasn.sqlite3"):
		raise Exception("Please run ip2asn.py")

	conn = sqlite3.connect("ipasn.sqlite3")

	cur = conn.cursor()

	tequery = "SELECT name FROM sqlite_schema WHERE type ='table' "
	cur.execute(tequery)

	res = cur.fetchall()
	tablename = ""

	for tn in res:
		tablename = tn[0]

	ipnum = struct.unpack("!I", socket.inet_aton(ipaddr))[0]

	ipquery = "select * from %s where START < %s AND END > %s" % (tablename, ipnum, ipnum)

	cur.execute(ipquery)
	res = cur.fetchall()
	cn = ""

	for val in res:
		cn = val[3]

	conn.close()
	return cn


def aggregate_detections(ppe, s256):
	ren = rulesengine()
	ren.rundomainrules(ppe.get_processed_dns_packet())
	cl = ppe.get_country_list()

	if False:
		fd = json.load(open("%s_dnsproto.json" % (s256)))
		ren.rundomainrules(fd)

	res = ren.get_detected_rules()
	if res == []:
		print("No Detection seen")
		return

	# unify the whole jsons and remove them from list

	union_json = {}

	for val in res:
		for v in val:
			if v not in union_json:
				if type(val[v]) == int:
					union_json[v] = 0
				else:
					union_json[v] = []
			if type(val[v]) == list:
				for z in val[v]:
					if z not in union_json[v]:
						union_json[v].append(z)
			elif type(val[v]) == int:
				union_json[v] = union_json[v] + val[v]
			else:
				if val[v] not in union_json[v]:
					union_json[v].append(val[v])

	for val in union_json:
		if val == "MITRE": continue
		if type(union_json[val]) == list:
			union_json[val] = ", ".join(union_json[val])

	union_json["infected_countries"] = cl
	union_json["c2_countries"] = cl
	union_json['Status'] = "Done"
	union_json['SHA256'] = s256

	print(union_json)
	return union_json


def process_pcap(fname):
	sha256 = hashlib.sha256()
	sha256.update(open(fname, "rb").read())
	s256 = sha256.hexdigest()
	s256 = s256.upper()

	intimate_status(s256, "InProgress")

	ppe = packetprocessengine()
	try:
		ppe.loadpcap(fname)
	except:
		return "Error processing pcap file"
	ppe.process_packet()

	if not os.path.isdir(s256):
		os.mkdir(s256)

	with open("%s/dnsproto.json" % (s256), "w") as f:
		f.write(json.dumps(ppe.get_processed_dns_packet(), indent=4))

	# intimate_completion("%s.json"%(s256))
	res = []
	res.append(aggregate_detections(ppe, s256))

	with open("%s/detected.json" % (s256), "w") as f:
		f.write(json.dumps(res, indent=4))


def main():
	fold = ""
	# watch the folder UploadedFiles

	if len(sys.argv) >= 2:
		print("Unwanted commandlines passed, Quitting")
		exit(1)
	elif len(sys.argv) == 2:
		print("Usage: <pscap.py> <pcap folder to watch>")
		fold = sys.argv[1]

	# load config file
	if not os.path.exists("config.json"):
		print("config.json file not found in the folder")
		exit(1)

	jsn = json.load(open("config.json"))

	try:
		if fold == "":
			fold = jsn["watcher"]["watch-folder"]

		if not os.path.exists(fold):
			print("Folder Does not exist:", fold)
			exit(1)

	except KeyError:
		print("Error parsing the config file")
		return

	while True:
		for fn in os.listdir(fold):
			fp = os.path.join(fold, fn)
			process_pcap(fp)
			os.unlink(fp)
		print("Watching folder for file", fold)
		try:
			time.sleep(10)
		except KeyboardInterrupt:
			break


if __name__ == "__main__":
	main()
