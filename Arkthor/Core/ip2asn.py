import datetime

def create_ip2asn_data():

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
	import requests
	import gzip
	from io import BytesIO
	
	conn = sqlite3.connect("ipasn.sqlite3")
		
	cur = conn.cursor()
	#check if table exists
	#tequery = "SELECT name FROM sqlite_schema WHERE type ='table' AND name NOT LIKE 'sqlite_%';"
	#cur.execute(tequery)
	
	#row = cur.fetchone()
	
	#if row is not None:
	#	if type(row['name']) == list:
	#		print("Table exists already")
	#		return
			
	#download the tsv
	r = requests.get(url)
	if r.status_code != 200:
		print("Unable to get the tsv", r.status_code)
		
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
			print(query)
		
	conn.commit()
	gzf.close()
	conn.close()

if __name__ == "__main__":
    create_ip2asn_data()