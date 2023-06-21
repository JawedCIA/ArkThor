##############################################################
# Author: Sriram											 #
# Contact: star.sriram [att] gmail [dot] com				 #
# License: GPLV2						 					 #
# Reason: Capstone Project April 2023 - IIT Kanpur	   		 #
##############################################################

from asyncio.log import logger
from scapy.all import *
from scapy.layers.http import *

import os, sys
import json
import rule_engine
import sqlite3
import hashlib
import requests
import re
import time
import mimetypes
import datetime
import pika
import logging
import threading as th
from urllib.parse import urlparse
from io import BytesIO
import zipfile

# Global variable declaration
global_var_foldertowatch = None
global_var_arkthorapiUrl = None
global_var_threatfox_rule_update_from_Date = None
global_var_threatfox_rule_update_to_Date = None
class config_loader:
	def __init__(self):
		file_path = os.path.join("arkthorcoreconfig", "config.json")
		with open(file_path) as file:
			jsn = json.load(file)
		#jsn = json.load(open("config.json"))
		try:
			self.watchfolder = jsn["watcher"]["watch-folder"]
			self.delaytime = int(jsn["watcher"]["watch-delay"])
			if jsn['debugmode'].lower() == "false":
				self.debugmode = False
			elif jsn['debugmode'].lower() == "true":
				self.debugmode = True
			else:
				raise Exception("Unknown value in debugmode of config file")

			if jsn['arkthor']['usearkthorapi'].lower() == "false":
				self.usearkthorapi = False
			elif jsn['arkthor']['usearkthorapi'].lower() == "true":
				self.usearkthorapi = True
			else:
				raise Exception("Unknown value in usearkthorapi of config file")

			if jsn['deleteprocessed'].lower() == "false":
				self.deleteprocessed = False
			elif jsn['deleteprocessed'].lower() == "true":
				self.deleteprocessed = True
			else:
				raise Exception("Unknown value in deleteprocessed of config file")

			if jsn['arkthor']['userabbitmq'].lower() == "false":
				self.userabbitmq = False
			elif jsn['arkthor']['userabbitmq'].lower() == "true":
				self.userabbitmq = True
			else:
				raise Exception("Unknown value in userabbitmq of config file")
			
			if jsn['update_ip2asn'].lower() == "false":
				self.update_ip2asn = False
			elif jsn['update_ip2asn'].lower() == "true":
				self.update_ip2asn = True
			else:
				raise Exception("Unknown value in update_ip2asn of config file")

			if jsn['multithreaded_rules_processing'].lower() == "false":
				self.multithreaded_rules_processing = False
			elif jsn['multithreaded_rules_processing'].lower() == "true":
				self.multithreaded_rules_processing = True
			else:
				raise Exception("Unknown value in multithreaded_rules_processing of config file")
			
			if jsn['run_rules_on_processed_pcap'].lower() == "false":
				self.run_rules_on_processed_pcap = False
			elif jsn['run_rules_on_processed_pcap'].lower() == "true":
				self.run_rules_on_processed_pcap = True
			else:
				raise Exception("Unknown value in run_rules_on_processed_pcap of config file")

			self.baseurl = jsn['arkthor']["apibaseurl"]
			self.rabbitmqhost = jsn['arkthor']["rabbitmqhost"]

			self.threatfox_rule_update_from_Date = jsn['threatfox_rule_update_from_Date']
		
			self.threatfox_rule_update_to_Date = jsn['threatfox_rule_update_to_Date']

		except KeyError as e:
			raise Exception("Error parsing the config file" + e)

class packetprocessengine:
	def __init__(self):
		self.fname = None
		self.dns_list = {}
		self.http_list = []
		self.ip_list = []
		self.is_zip = False
		self.packet_processed = False

	def __del__(self):
		pass
	
	def close(self):
		if self.is_zip == False:
			self.packets.close()

	def loadpcap(self, filename=None):
		if self.fname == None and filename == None:
			raise Exception("No Parameters passed")

		elif filename != None and self.fname != None and filename != self.fname:
			raise Exception("Multiple filenames passed")

		elif filename != None and self.fname == None:
			self.fname = filename

		#self.packets = rdpcap(self.fname)
		if ".zip" in self.fname:
			bio = BytesIO()
			zf = zipfile.ZipFile(self.fname)
			zf.pwd = b'infected'
			bio = zf.extract(zf.filelist[0])		#Extract first file assuming only 1 file
			zf.close()
			self.packets = PcapReader(bio)
			bio = None
			self.is_zip = True
		else:
			self.packets = PcapReader(self.fname)
		return True

	def process_packet(self):
		for pkt in self.packets:
			if pkt.haslayer(DNS):
				self.process_dns_packet(pkt[DNS])
			if pkt.haslayer(IP):
				self.process_ip_packet(pkt)
			# Processing only HTTPRequest, We can still develop HTTPResponse
			if pkt.haslayer(HTTPRequest):
				self.process_http_packet(pkt)

		self.packet_processed = True
		return
	
	def process_ip_packet(self, hpkt):
		for xpkt in hpkt[IP]:
			tjs = {	}
			if hasattr(xpkt, "version"):
				tjs["version"] = xpkt.version
			else:
				tjs["version"] = None

			if hasattr(xpkt, "src"):
				tjs["src"] = xpkt.src
			else:
				tjs["src"] = None

			if hasattr(xpkt, "dst"):
				tjs["dst"] = xpkt.dst
			else:
				tjs["dst"] = None

			if hasattr(xpkt, "sport"):
				tjs["sport"] = xpkt.sport
			else:
				tjs["sport"] = None

			if hasattr(xpkt, "dport"):
				tjs["dport"] = xpkt.dport
			else:
				tjs["dport"] = None

			# convert bytes to string
			for x in tjs:
				if tjs[x] is not None and isinstance(tjs[x], bytes):
					try:
						tjs[x] = tjs[x].decode()
					except UnicodeDecodeError:
						pass
			self.ip_list.append(tjs)

	def process_http_packet(self, hpkt):
		for xpkt in hpkt[HTTPRequest]:
			print(xpkt)
			tjs = {}
			if hasattr(xpkt, "Accept"):
				tjs["accept"] = xpkt.Accept
			else:
				tjs["accept"] = None
			if hasattr(xpkt, "Accept_Encoding"):
				tjs["accept_encoding"] = xpkt.Accept_Encoding
			else:
				tjs["accept_encoding"] = None
			if hasattr(xpkt, "Accept_Language"):
				tjs["accept_language"] = xpkt.Accept_Language
			else:
				tjs["accept_language"] = None
			if hasattr(xpkt, "Method"):
				tjs["method"] = xpkt.Method
			else:
				tjs["method"] = None
			if hasattr(xpkt, "Http_Version"):
				tjs["http_version"] = xpkt.Http_Version
			else:
				tjs["http_version"] = None
			if hasattr(xpkt, "Accept_Language"):
				tjs["accept_language"] = xpkt.Accept_Language
			else:
				tjs["accept_language"] = None
			if hasattr(xpkt, "Authorization"):
				tjs["authorization"] = xpkt.Authorization
			else:
				tjs["authorization"] = None
			if hasattr(xpkt, "User_Agent"):
				tjs["user_agent"] = xpkt.User_Agent
			else:
				tjs["user_agent"] = None
			if hasattr(xpkt, "HTTP2_Settings"):
				tjs["http2_settings"] = xpkt.HTTP2_Settings
			else:
				tjs["http2_settings"] = None
			if hasattr(xpkt, "Permanent"):
				tjs["permanent"] = xpkt.Permanent
			else:
				tjs["permanent"] = None
			if hasattr(xpkt, "Path"):
				tjs["path"] = xpkt.Path
			else:
				tjs["path"] = None
			if hasattr(xpkt, "Host"):
				tjs["host"] = xpkt.Host
			else:
				tjs["host"] = None
			# convert bytes to string
			for x in tjs:
				if tjs[x] is not None and isinstance(tjs[x], bytes):
					try:
						tjs[x] = tjs[x].decode()
					except UnicodeDecodeError:
						pass
			self.http_list.append(tjs)

	def process_dns_packet(self, dns):
		pident = {1: "A", 2: "NS", 5: "CNAME", 6: "SOA", 12: "PTR", 15: "MX", 16: "TXT", 28: "AAAA", 33: "SRV",
				  255: "ANY"}
		if dns.qr == 0:  # DNS query
			dv = dns.qd.qname.decode()
			if dv not in self.dns_list:
				self.dns_list[dv] = {}
			self.dns_list[dv] = {"dns_query_num": dns.qd.qtype, "dns_query_type": pident[dns.qd.qtype]}

		elif dns.qr == 1:  # DNS response
			if dns.qd == None:
				return
			dv = dns.qd.qname.decode()
			if dv not in self.dns_list:
				#raise Exception("DNS answer present, without query")
				logging.info("DNS answer present, without query")
				return
			self.dns_list[dv]["response_code"] = dns.rcode
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
					if not isinstance(ip, str): continue
					if re.match("\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}", ip) is not None:
						# print("Val", ip)
						self.dns_list[dv]["CN"] = get_cn_from_ip(ip)

				self.dns_list[dv]["answers"] = ", ".join(self.dns_list[dv]["answers"])

	def get_processed_dns_packet(self):
		if self.packet_processed == True:
			return self.dns_list

		return {"success": False, "comments": "packet not processed"}
	
	def load_processed_dns_packet(self, jsn):
		self.packet_processed = True
		self.dns_list = jsn

	def get_processed_http_packet(self):
		if self.packet_processed == True:
			return self.http_list

		return {"success": False, "comments": "packet not processed"}
	
	def load_processed_http_packet(self, jsn):
		self.packet_processed = True
		self.http_list = jsn

	def get_processed_ip_packet(self):
		if self.packet_processed == True:
			return self.ip_list

		return {"success": False, "comments": "packet not processed"}
	
	def load_processed_ip_packet(self, jsn):
		self.packet_processed = True
		self.ip_list = jsn

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
	
	def domainrulesinthread(self, fn, content):
		self.rengine = json.loads(open(os.path.join("ArkThorRule",fn)).read())
		st = time.perf_counter()
		for r in self.rengine:
			if r["scope"] == "DNS":
				context = rule_engine.Context(type_resolver=rule_engine.type_resolver_from_dict({
					'domain': rule_engine.DataType.STRING,
					'address': rule_engine.DataType.ARRAY(rule_engine.DataType.STRING)
				}))
				for ru in r["rule"]:
					rule = rule_engine.Rule(ru)
					for x in content:
						js = {"domain": x}
						for y in content[x]:
							js[y] = content[x][y]
						try:
							if rule.matches(js) == True:
								rs = r
								if "rule" in rs: del (rs['rule'])
								if "scope" in rs: del (rs['scope'])
								self.ruleset.append(rs)
						except rule_engine.errors.SymbolResolutionError as ex:
							# print("Unavailable param", ex)
							pass

		logging.info("Domain [-] %s took %s to complete"%(fn, str(time.perf_counter() - st)))

	def rundomainrules(self, content):
		count = 0
		for fn in os.listdir("ArkThorRule"):
			if ".ark" not in fn: continue
			# print("Processing rules from", fn)
			th.Thread(target=self.domainrulesinthread, args=(fn, content)).start()
			count = count + 1
			if (count % 10) == 0:
				print("Wait for threads to spawn")
				time.sleep(10)
			
		while th.active_count() != 1:
			logging.info(th.active_count())
			time.sleep(10)

		logging.info("Domain Rules Processing COmplete")

	def iprulesinthread(self, fn, content):
		self.rengine = json.loads(open(os.path.join("ArkThorRule",fn)).read())
		st = time.perf_counter()
		for r in self.rengine:
			if r["scope"] == "IPPORT":
				# to check rules with IP only
				context = rule_engine.Context(type_resolver=rule_engine.type_resolver_from_dict({
					'ip': rule_engine.DataType.STRING,
					'port': rule_engine.DataType.STRING,
				}))
				for ru in r["rule"]:
					rule = rule_engine.Rule(ru)
					for x in content:
						if 'src' in x:
							js = {"ip": x['src'], "port" : x['sport']}
							try:
								if rule.matches(js) == True:
									rs = r
									if "rule" in rs: del (rs['rule'])
									if "scope" in rs: del (rs['scope'])
									self.ruleset.append(rs)
							except rule_engine.errors.SymbolResolutionError as ex:
								# print("Unavailable param", ex)
								pass

		logging.info("IP [-] %s took %s to complete"%(fn, str(time.perf_counter() - st)))

	def runiprules(self, content):
		count = 0
		for fn in os.listdir("ArkThorRule"):
			if ".ark" not in fn: continue
			# print("Processing rules from", fn)
			th.Thread(target=self.iprulesinthread, args=(fn, content)).start()
			count = count + 1
			if (count % 10) == 0:
				print("Wait for threads to spawn")
				time.sleep(10)
			
		while th.active_count() != 1:
			logging.info(th.active_count())
			time.sleep(10)

		logging.info("IP Rules Processing COmplete")

	def httprulesinthread(self, fn, content):
		self.rengine = json.loads(open(os.path.join("ArkThorRule",fn)).read())
		st = time.perf_counter()
		for r in self.rengine:
			if r["scope"] == "URL":
				context = rule_engine.Context(type_resolver=rule_engine.type_resolver_from_dict({
					'user_agent': rule_engine.DataType.STRING,
					'path': rule_engine.DataType.STRING,
					'host': rule_engine.DataType.STRING,
					'url': rule_engine.DataType.STRING,
				}))
				for ru in r["rule"]:
					rule = rule_engine.Rule(ru)
					for x in content:
						js = {"user_agent": x['user_agent'], "path" : x['path'],
							"host" : x['host'], 'url' : x['host'] + x['path'] }
						try:
							if rule.matches(js) == True:
								rs = r
								if "rule" in rs: del (rs['rule'])
								if "scope" in rs: del (rs['scope'])
								self.ruleset.append(rs)
						except rule_engine.errors.SymbolResolutionError as ex:
							# print("Unavailable param", ex)
							pass
		logging.info("IP [-] %s took %s to complete"%(fn, str(time.perf_counter() - st)))

	def runhttprules(self, content):
		count = 0
		for fn in os.listdir("ArkThorRule"):
			if ".ark" not in fn: continue
			# print("Processing rules from", fn)
			#th.Thread(target=self.httprulesinthread, args=(fn, content)).start()
			self.httprulesinthread(fn, content)
			count = count + 1
			if (count % 10) == 0:
				print("Wait for threads to spawn")
				time.sleep(10)
			
		while th.active_count() != 1:
			logging.info(th.active_count())
			time.sleep(10)

		logging.info("IP Rules Processing COmplete")


	def get_detected_rules(self):
		return self.ruleset

class processing_history:
	def __init__(self):
		self.sqlite_fn = "processinglist.sqlite3"
		# delete if 0 kb sqlite file
		if os.path.exists(self.sqlite_fn):
			s = os.stat(self.sqlite_fn)
			if s.st_size == 0: os.unlink(self.sqlite_fn)
		if not os.path.exists(self.sqlite_fn):
			conn = sqlite3.connect(self.sqlite_fn)
			cur = conn.cursor()
			cur.execute("CREATE TABLE processed (filehash char(64) not NULL, fdtime INTEGER, ts TIMESTAMP DEFAULT CURRENT_TIMESTAMP)")
			conn.commit()
			conn.close()

	def exists_in_processing(self, sha256, mtime):
		conn = sqlite3.connect(self.sqlite_fn)
		cur = conn.cursor()
		cur.execute("select * from processed where filehash = '%s' and fdtime = '%s'"%(sha256.upper(), int(mtime) ))
		retval = False
		for v in cur.fetchall():
			logging.info(v)
			retval = True
		conn.close()
		return retval

	def insert_into_processing(self, sha256, mtime):
		conn = sqlite3.connect(self.sqlite_fn)
		cur = conn.cursor()
		cur.execute("insert into processed ( `filehash`, `fdtime` ) values ('%s', '%s')" % (sha256.upper(), int(mtime)))
		conn.commit()
		conn.close()

def set_global_variable(value):
	# Declare global_var as global within the method
	global global_var_foldertowatch
	global_var_foldertowatch = value

def intimate_status(filehash, status, url_prefix):
    try:
        r = requests.put(f"{url_prefix}/api/FileRecord/UpdateStatus",
                         params={"hash256": filehash.upper(), "status": status})
    except requests.exceptions.ConnectionError as e:
        logging.error("Cannot connect to Arkthor API server to post data")
    else:
        logging.info(f"Updated Processing Status {status} to ArkThor API with StatusCode: {r.status_code}")
        
        # Check if the request was successful (status code 200)
        if r.status_code == 200:
            # Parse the JSON response
            data = r.json()
            
            # Check if the 'result' key exists in the response
            if 'result' in data:
                # Retrieve the value of the 'result' key
                result = data['result']
                logging.info(f"Result: {result}")
            else:
                logging.error("No 'result' key found in the response")
        else:
            logging.error(f"Request failed with status code: {r.status_code}")

def check_create_ip2asn_data(should_create):
	######################################################################
	# For scalability in containers, this database file has to be stored #
	# in a common shared location like s3 bucket or gcs bucket. The file #
	# creation has to be locked with a lockfile and other process has to #
	# # wait until this lockfile is released                             #
	######################################################################

	table_date = datetime.datetime.utcnow().strftime("as%Y%m%d")
	
	url = "https://iptoasn.com/data/ip2asn-v4-u32.tsv.gz"
	ctquery = """
		CREATE TABLE IF NOT EXISTS `%s` (
		`start` BIGINT NULL DEFAULT NULL,
		`end` BIGINT NULL DEFAULT NULL,
		`as_number` BIGINT NULL DEFAULT NULL,
		`country_name` CHAR(2) NULL
	)
	;"""%(table_date)
	
	import sqlite3
	import gzip
	from io import BytesIO

	if not os.path.exists("ipasn.sqlite3") and should_create == False:
		print("ip2asn does not exist and creation option in config is set to false.")
		logging.info("ip2asn does not exist and creation option in config is set to false.")
		return False

	if os.path.exists("ipasn.sqlite3"):
		st = os.stat("ipasn.sqlite3")
		if st.st_mtime < datetime.datetime.now().timestamp() - 14400: 	# 4 hrs in seconds
			logging.info("IP2asn databse is more than 4 hrs old.")
			os.unlink("ipasn.sqlite3")

		if st.st_size < 10240:
			logging.info("IP2asn databse may be corrupted.")
			os.unlink("ipasn.sqlite3")

	if os.path.exists("ipasn.sqlite3"):
		logging.info("IP2ASN is updated one")
		return True
	logging.info("Updating IP2ASN Database")
	conn = sqlite3.connect("ipasn.sqlite3")

	cur = conn.cursor()
			
	#download the tsv=
	try:
		r = requests.get(url)
	except requests.exceptions.SSLError:
		r = requests.get(url, verify = False)
	if r.status_code != 200:
		print("Unable to get the tsv", r.status_code)
		logging.error(f"Unable to get the tsv : {r.status_code} ")
		return False
		
	#Extract the TSV in memory
	bio = BytesIO(r.content)
	gzf = gzip.open(bio)
	
	#create the table
	cur.execute(ctquery)
	conn.commit()
	
	count = 0
		
	#for ln in open("ip2asn-v4.tsv", encoding="ISO-8859-1").readlines():
	for ln in gzf.readlines():
		(start, end, as_number, country_code, dont_want) = ln.decode().strip().split("\t")
		if country_code == "None" or country_code == "Unknown":
			country_code = "00"
		query = "INSERT INTO `%s`(`start`, `end`, `as_number`, `country_name`) VALUES(%s, %s, %s, \"%s\")"%(table_date, start, end, as_number, country_code)
		cur.execute(query)
			
		count = count + 1
		if count > 10000:
			conn.commit()
			count = 0
		
	conn.commit()
	gzf.close()
	conn.close()
	logging.info("IP2ASN Database update complete..")
	return True

def intimate_completion(fjson, url_prefix):
	retries = 3
	delay = 1  # seconds
	from io import BytesIO	
	while retries > 0:
		if os.path.exists(fjson):
			try:
				d = json.load(open(fjson))

				try:
					#event = BytesIO(json.dumps(max(d, key=lambda ev: ev['severity'])).encode())
					event = BytesIO(json.dumps(d).encode())
				except KeyError:
					#event = BytesIO(json.dumps(d).encode())
					event = BytesIO()
				event.seek(0)

				print(fjson)
				headers = {
					'accept': '*/*'
				}
				files = {
					'file': (fjson, event, mimetypes.guess_type(fjson)[0])
				}
				r = requests.post("%s/api/FileUpload/UploadFileOutPutJson" % (url_prefix), files=files, headers=headers)

				# Check the response status code
				if r.status_code == 200:
					print('File upload successful.')
					logging.info(f"File {fjson} upload successful.")
					# os.unlink(fjson)
					break  # Exit the loop if the upload is successful
				else:
					print(f'File {fjson} upload failed with status code {r.status_code}.')
					logging.error(f"File {fjson} upload failed with status code {r.status_code}.")
			except requests.exceptions.ConnectionError as e:
				print("Cannot connect to the server to post data")
				logging.error("Cannot connect to the server to post data")
		else:
			print("File does not exist.")
			logging.error(f"File {fjson} does not exist.")

		retries -= 1
		if retries > 0:
			print(f"Retrying after {delay} seconds...")
			logging.error(f"Retrying after {delay} seconds...")
			time.sleep(delay)
	
	if retries == 0:
		print("Maximum retries exceeded. File upload failed.")
		logging.error(f"Maximum retries exceeded. File {fjson}  upload failed..")
		  

def submit_artifacts_of_pcaprun(filehash,foldername, url_prefix):
	for fn in os.listdir(os.path.join("results", foldername)):
		#get the mimetype of submiiting file
		#content_type, encoding = mimetypes.guess_type(os.path.join(foldername, fn))
		submit_successful = True
		if fn == "detected.json": continue
		try:
			r = requests.post("%s/api/FileUpload/UploadSupportingFile"%(url_prefix),
						params={"sha256": filehash.upper()},
					files={"file":(fn,open(os.path.join("results", foldername, fn),'rb'), mimetypes.guess_type(os.path.join("results", foldername, fn))[0])},
					headers={ 'accept': '*/*'})
		except requests.exceptions.ConnectionError as e:
			print("Cannot connect to server to post data")
			logging.error("Cannot connect to server to post data")
			submit_successful = False
		else:
			print("Submitting ", fn, "with return code", r.status_code)
			logging.info(f"Submitting  {fn}, with return code: {r.status_code}")
			if r.status_code == 200:
				os.unlink(os.path.join("results", foldername, fn))
				
	if submit_successful == True:
		import shutil
		shutil.rmtree(os.path.join("results", foldername))
	
##############################################################
# Author: Sriram											 #
# Contact: star.sriram [att] gmail [dot] com				 #
# License: GPLV2						 					 #
# Reason: Capstone Project April 2023 - IIT Kanpur	   		 #
##############################################################

def get_cn_from_ip(ipaddr, fname="ipasn.sqlite3"):
	import struct
	import socket

	if not os.path.exists(fname):
		raise Exception("Please run ip2asn.py")

	conn = sqlite3.connect(fname)

	cur = conn.cursor()

	tequery = "SELECT name FROM sqlite_master WHERE type ='table' "
	cur.execute(tequery)

	res = cur.fetchall()
	if res == []:
		raise Exception("Error in last run of ip2asn. Please rerun ip2asn")
	
	tablename = ""

	for tn in res:
		tablename = tn[-1]

	ipnum = struct.unpack("!I", socket.inet_aton(ipaddr))[0]

	ipquery = "select * from %s where START < %s AND END > %s" % (tablename, ipnum, ipnum)

	cur.execute(ipquery)
	res = cur.fetchall()
	cn = ""

	for val in res:
		cn = val[3]

	conn.close()
	return cn

def process_threatfox_to_arkthor(overwrite_rules=True):
	import bs4
	import re
	import datetime

	mispurl = "https://threatfox.abuse.ch/downloads/misp/"
	# manifest.json can be read to understand more, Unfortunately, I saw this in the last moment
	from_date = datetime.datetime.strptime(global_var_threatfox_rule_update_from_Date, "%Y-%m-%d")
	to_date = datetime.datetime.strptime(global_var_threatfox_rule_update_to_Date, "%Y-%m-%d")
	logging.info(f"from date to convert threatfox : {from_date}")
	logging.info(f"To date to convert threatfox : {to_date}")

	if True:
		try:
			r = requests.get(mispurl)
		except requests.exceptions.SSLError:
			r = requests.get(mispurl, verify = False)
		if r.status_code != 200:
			raise Exception("Error accessing Threatfox MISP page")
		# I use this for testing
		#with open("threatfox.html", "w") as f:
		#	f.write(r.text)
		data = r.text
	else:
		# I use this for testing
		data = open("threatfox.html", "r").read()
	soup = bs4.BeautifulSoup(data, "html.parser")

	for tr in soup.find('table').find_all('tr'):
		# extract the UUID and date for processing
		td = tr.find_all('td')
		if len(td) != 5: continue
		a = re.search("^<td align=\"right\">(\d{4}\-\d{2}\-\d{2}).*<\/td>", str(td[2]))
		if a == None: continue
		dt = datetime.datetime.strptime(a[1], "%Y-%m-%d")
		#print(dt)
		if not (from_date <= dt <= to_date):
			continue
		print(td[2], a[1], dt)

		href = td[1].find_all("a")
		if "manifest.json" in  str(href[0]).lower(): continue
		b = re.search("^<a href=\".*\">([0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[0-9a-f]{4}-[0-9a-f]{12})\.json</a>$", str(href[0]))
		if b is None: raise Exception("Error in understanding of threatfox page")
		fn = os.path.join("ArkThorRule", "arkthor-%s.ark"%(b[1]))
		if os.path.exists(fn): continue

		jsonurl = mispurl + b[1] + ".json"

		try:
			r = requests.get(jsonurl)
		except requests.exceptions.SSLError:
			r = requests.get(jsonurl, verify = False)
		if r.status_code != 200:
			raise Exception("Error accessing Threatfox Individual page", jsonurl)
		s = r.json()
		js = []
		for dom in s["Event"]["Attribute"]:
			if "(confidence level" in dom["comment"]:
				name = dom["comment"][ : dom["comment"].find("(confidence level")]
			else:
				name = dom["comment"]
			name = name.strip()
			sev = re.search("\(confidence level: (\d{1,3})%\)", dom["comment"])
			if sev == None:
				raise Exception("Error understanding threatfox page")
			if dom["type"] == "domain":
				js.append({
					"rule_name": name,
					"authored_timestamp": int(datetime.datetime.utcnow().timestamp()),
					"rule": ["domain == \"%s.\""%(dom["value"])],
					"scope": "DNS",
					"severity": sev[1],
					"automated" : "true",
					"MITRE": [ "T1071.001", "T1132.001", "T1568", "T1102.003" ]
				})
			elif dom["type"] == "url":
				path = urlparse(dom["value"])
				netloc = path.netloc
				if ":" in netloc:
					netloc = netloc[ : netloc.find(":")]
				js.append({
					"rule_name": name,
					"authored_timestamp": int(datetime.datetime.utcnow().timestamp()),
					"rule": ["url == \"%s\""%(dom["value"]), "path == \"%s\""%(path.path), "domain == \"%s\""%(netloc)],
					"scope": "URL",
					"automated" : "true",
					"severity": sev[1]
				})
			elif dom["type"] == "ip-dst|port":
				ip, port = dom["value"].split("|")
				if port == "1": port = "*"
				js.append({
					"rule_name": name,
					"authored_timestamp": int(datetime.datetime.utcnow().timestamp()),
					"rule": ["ip == \"%s\""%(ip), "ip == \"%s\" and port == \"%s\""%(ip, port)],
					"scope": "IPPORT",
					"automated" : "true",
					"severity": sev[1]
				})

		f = open(fn, "w")
		f.write(json.dumps(js, indent=4))
		f.close()
	return

def aggregate_detections(ppe, s256):
	ren = rulesengine()
	ren.rundomainrules(ppe.get_processed_dns_packet())
	ren.runiprules(ppe.get_processed_ip_packet())
	ren.runhttprules(ppe.get_processed_http_packet())
	cl = ppe.get_country_list()

	union_json = {}
	res = ren.get_detected_rules()
	if res == []:
		print("No Detection seen")
		logging.info("No Detection seen")
		union_json = {}
		union_json['Status'] = "Done"
		union_json['SHA256'] = s256
		union_json["c2_countries"] = []
		union_json["rule_name"] = "Uncategorized"
		union_json['analyzed_time'] = int(datetime.datetime.utcnow().timestamp())
		return union_json

	# unify the whole jsons and remove them from list

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
	
	#Check for Severity in case multiple rule detected with different severity 
	severity_str = union_json['severity']

	if severity_str:
		severity_list = []
		split_values = severity_str.split(',')
		if len(split_values) > 0:
			severity_list = [int(num) for num in split_values]

		if len(severity_list) > 0:
			highest_number = max(severity_list)
			union_json["severity"]=str(highest_number)
			#print(highest_number)
		else:
			union_json["severity"]=0
	else:
		union_json["severity"]=0

	union_json["infected_countries"] = cl
	union_json["c2_countries"] = cl
	union_json['Status'] = "Done"
	union_json['SHA256'] = s256
	union_json['analyzed_time'] = int(datetime.datetime.utcnow().timestamp())
	return union_json


def process_pcap(fname):
	logging.info(f"Method: process_pcap")
	logging.info(f"loading config_loader: Start")
	cnf = config_loader()
	logging.info(f"loading config_loader: DONE")
	logging.info(f"loading processing_history:Start")
	ph = processing_history()
	logging.info(f"loading processing_history:Done")

	# if file is zip password protected, extract the file and then calculate sha256
	sha256 = hashlib.sha256()
	if ".zip" in fname:
		bio = BytesIO()
		zf = zipfile.ZipFile(fname)
		zf.pwd = b'infected'
		sha256.update(zf.extract(zf.filelist[0]).encode())		#Extract first file assuming only 1 file
		zf.close()
		logging.info(f"The pcap is Zipped password protected, successfully processed")
	else:
		sha256.update(open(fname, "rb").read())
	s256 = sha256.hexdigest()
	s256 = s256.upper()
	logging.info(f"SHA256 for the pcap is calculated as %s"%(s256))

	#Check if IP2ASN is updated one
	if check_create_ip2asn_data(cnf.update_ip2asn) == False:
		return

	stat = os.stat(fname)

	if ph.exists_in_processing(s256, stat.st_mtime) == True:
		logging.info(f"Already Processed :{fname}")
		if cnf.run_rules_on_processed_pcap == False: return
		ppe = packetprocessengine()
		js = json.load(open(os.path.join("results", s256, "dnsproto.json"), "r"))
		ppe.load_processed_dns_packet(js)
		v = aggregate_detections(ppe, s256)
		return
	
	if cnf.usearkthorapi == True:
		intimate_status(s256, "InProgress", cnf.baseurl)

	logging.info(f"Method:packetprocessengine : Start")
	ppe = packetprocessengine()
	
	logging.info(f"Method:packetprocessengine : DONE")
	if ppe.loadpcap(fname) != True:
		if cnf.usearkthorapi == True:
			intimate_status(s256, "Failure", cnf.baseurl)
		return "Error processing pcap file"
	
	ppe.process_packet()

	if not os.path.isdir("results"):
		os.mkdir("results")

	if not os.path.isdir(os.path.join("results", s256)):
		os.mkdir(os.path.join("results", s256))

	with open("results/%s/dnsproto.json" % (s256), "w") as f:
		f.write(json.dumps(ppe.get_processed_dns_packet(), indent=4))

	with open("results/%s/httpproto.json" % (s256), "w") as f:
		f.write(json.dumps(ppe.get_processed_http_packet(), indent=4))

	with open("results/%s/ipproto.json" % (s256), "w") as f:
		f.write(json.dumps(ppe.get_processed_ip_packet(), indent=4))

	v = aggregate_detections(ppe, s256)
	if v is not None:
		res = []
		res.append(v)
		with open("results/%s/detected.json" % (s256), "w") as f:
			f.write(json.dumps(res, indent=4))
		if cnf.usearkthorapi == True:
			intimate_completion("results/%s/detected.json" % (s256), cnf.baseurl)
			os.unlink("results/%s/detected.json" % (s256))

	if cnf.usearkthorapi == True:
		submit_artifacts_of_pcaprun(s256,s256, cnf.baseurl)

	ph.insert_into_processing(s256, stat.st_mtime)
	ppe.close()

	if cnf.deleteprocessed == True: 
		try:
			os.unlink(fname)
			logging.info(f"File {fname} successfully deleted.")
		except OSError as e:
			logging.error(f"Failed to delete file {fname}. Error: {str(e)}")
	return

# Define message callback function
#Added by jawed for RabbitMQ message processing
def process_message(ch, method, properties, body):
	try:
		# Perform your operations on the received message here
		#logging.info("Received message:", body.decode())  # Example: Print the message
		# Assuming `body` contains the decoded message body
		decoded_body = body.decode()
		# Parse the JSON string
		message_data = json.loads(decoded_body)
		# Access the hash value from the parsed JSON
		hash_value = message_data["message"]
		# Use the hash value for further processing
		logging.info(f"++++++++++++++++++++++++++++++ {hash_value} ++++++++++++++++++++++++++++++")
		logging.info(f"Analysing File with hash Value: {hash_value}")
		#Analysis Operation
		fp=find_file_by_filename(global_var_foldertowatch, hash_value)
		if fp is None:
			logging.error("Unable to find file at given location")
		else:
			try:
				process_pcap(fp)
			except Exception as e:
				logging.error(f"Error cought in process_pcap module: {str(e)}")
				intimate_status(hash_value, "Failure", global_var_arkthorapiUrl)
		# Acknowledge the message
		ch.basic_ack(delivery_tag=method.delivery_tag)
		logging.info("Acknowledge the message and waiting for Message..")
		# Use the hash value for further processing
	except Exception as e:
		logging.error(f"Error occurred while processing the message: {str(e)}")
		# Handle the error and decide whether to retry or take other actions

		# Retry after a delay
		time.sleep(5)  # Wait for 5 seconds before retrying
		consume_messages()
	logging.info(f"+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
# Define message callback function for ip2asn
#Added by Jawed for UI trigger ip2asn rule refresh
def process_message_ip2asn(ch, method, properties, body):
	try:
		# Perform your operations on the received message here
		logging.info("="*50)
	   # logging.info("Received message to update ip2asn:", body.decode())  # Example: Print the message
		# Assuming `body` contains the decoded message body
		decoded_body = body.decode()
		# Parse the JSON string
		message_data = json.loads(decoded_body)
		# Access the hash value from the parsed JSON
		message_value = message_data["message"]
		# Use the hash value for further processing
		logging.info(f"Updating ip2asn at : {message_value}")
		#trigger update Operation		
		check_create_ip2asn_data(True)
		
		# Acknowledge the message
		ch.basic_ack(delivery_tag=method.delivery_tag)
		logging.info("Acknowledge the message of ip2ans and wiating for Message..")
		# Use the hash value for further processing
		logging.info("="*50)
	except Exception as e:
		logging.error(f"Error occurred while processing the ip2ans message: {str(e)}")
		# Handle the error and decide whether to retry or take other actions

		# Retry after a delay
		time.sleep(30)  # Wait for 30 seconds before retrying
		consume_messages()

##############################################################
# Author: Sriram											 #
# Contact: star.sriram [att] gmail [dot] com				 #
# License: GPLV2						 					 #
# Reason: Capstone Project April 2023 - IIT Kanpur	   		 #
##############################################################

# Define message callback function for ip2asn
#Added by Jawed for UI trigger Threatfox rule refresh
def process_message_threatfoxRule(ch, method, properties, body):
	logging.info("="*50)
	try:
		# Perform your operations on the received message here
	   # logging.info("Received message to update ThreatFoxRule:", body.decode())  # Example: Print the message
		# Assuming `body` contains the decoded message body
		decoded_body = body.decode()
		# Parse the JSON string
		message_data = json.loads(decoded_body)
		# Access the hash value from the parsed JSON
		message_value = message_data["message"]
		# Use the hash value for further processing
		logging.info(f"Updating ThreatFoxRule at :{message_value}")
		#trigger update Operation
		
		process_threatfox_to_arkthor()
		# Acknowledge the message
		ch.basic_ack(delivery_tag=method.delivery_tag)
		logging.info("Acknowledge the message of ThreatFoxRule and wiating for Message..")
		# Use the hash value for further processing
		logging.info("="*50)
		# Use the hash value for further processing
	except Exception as e:
		logging.error(f"Error occurred while processing the ThreatFoxRule message: {str(e)}")
		# Handle the error and decide whether to retry or take other actions

		# Retry after a delay
		time.sleep(30)  # Wait for 30 seconds before retrying
		consume_messages()
	logging.info("="*50)

#Added by Jawed for RabbitMQ connection for Queues
def consume_messages(connection_params):
	
	try:
		# Establish connection
		connection = pika.BlockingConnection(connection_params)
		channel = connection.channel()

		# Declare queue
		channel.queue_declare(queue='Analysis')
		channel.queue_declare(queue='ip2asn')
		channel.queue_declare(queue='ThreatFoxRule')
		logging.info("RabbitMQ Connection Succesful, Now start consuming Message..")
		# Start consuming messages
		channel.basic_consume(queue='Analysis', on_message_callback=process_message)
		channel.basic_consume(queue='ip2asn', on_message_callback=process_message_ip2asn)
		channel.basic_consume(queue='ThreatFoxRule', on_message_callback=process_message_threatfoxRule)

		# Start the event loop
		channel.start_consuming()
	except Exception as e:
		
		logging.error(f"Error occurred while consuming messages: {str(e)}")
		# Handle the error and decide whether to retry or take other actions

		# Retry after a delay
		time.sleep(5)  # Wait for 5 seconds before retrying
		consume_messages(connection_params)

#Added by Jawed to get the file based on hash value from RabbitMQ Queue
def find_file_by_filename(folder_path, filename):
	for root, dirs, files in os.walk(folder_path):
		for file in files:
			if os.path.splitext(file)[0] == filename:
				logging.info(f"Uploaded File Available at location: {os.path.join(root, file)}");
				return os.path.join(root, file)
	logging.error(f"Uploaded File {filename} not-available at location: {folder_path}")
	return None

def main():
	fold = ""
	# Enable verbose logging
	logging.basicConfig(level=logging.INFO)
	# watch the folder UploadedFiles
	param = ""

	cnf = config_loader()
	global global_var_arkthorapiUrl
	global_var_arkthorapiUrl = cnf.baseurl

	global global_var_threatfox_rule_update_from_Date
	global_var_threatfox_rule_update_from_Date = cnf.threatfox_rule_update_from_Date

	global global_var_threatfox_rule_update_to_Date
	global_var_threatfox_rule_update_to_Date = cnf.threatfox_rule_update_to_Date

	if len(sys.argv) > 3:
		logging.info("Unwanted commandlines passed, Exiting...")
		return
	
	elif len(sys.argv) == 2:
		param = sys.argv[1]

		if param == "updateark":
			process_threatfox_to_arkthor()
			return
		if param == "updateip2asn":
			check_create_ip2asn_data(True)
			return
		else:
			print("Unknown parameter:", param, "Exiting...")
			return
	elif len(sys.argv) == 3:
		param = sys.argv[1]
		if param == "testpcap":
			fn = sys.argv[2]
			process_pcap(fn)
			return


	if fold == "":
			fold = cnf.watchfolder
			set_global_variable(fold)
	else:
		set_global_variable(fold)
	if not os.path.exists(global_var_foldertowatch):
			logging.info(f"Watcher folder not found {global_var_foldertowatch}")
			exit(1)
	#Added by Jawed for RabbitMQ Integration
	if cnf.userabbitmq == True:
		# Connection parameters
		logging.info("Subscribing to RabbitMQ for messages..")
		logging.info(global_var_foldertowatch)
		logging.info(cnf.rabbitmqhost)		
		#credentials = pika.credentials.PlainCredentials('guest', 'guest')
		connection_params = pika.ConnectionParameters(host=cnf.rabbitmqhost, port=5672, virtual_host='/')#, credentials=credentials)
		time.sleep(5) #Wait for connection to established
		#Start consuming messages
		consume_messages(connection_params)
	else:
		
		while True:
			for fn in os.listdir(global_var_foldertowatch):
				fp = os.path.join(global_var_foldertowatch, fn)
				process_pcap(fp)
				#if cnf.deleteprocessed == True: 
					#os.unlink(fp)
				#	delete_file(fp)
			logging.info(f"Watching folder for file {global_var_foldertowatch}")
			try:
				time.sleep(cnf.delaytime)
			except KeyboardInterrupt:
				break

if __name__ == "__main__":
	main()
	

##############################################################
# Author: Sriram											 #
# Contact: star.sriram [att] gmail [dot] com				 #
# License: GPLV2						 					 #
# Reason: Capstone Project April 2023 - IIT Kanpur	   		 #
##############################################################