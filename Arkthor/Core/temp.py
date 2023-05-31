from scapy.all import *


def get_url_from_payload(payload):
	print(payload)
	# read the pcap file
	#pcap_flow = rdpcap('2023-03-16-Emotet-E5-infection-with-spambot-traffic.pcap')
	pcap_flow = rdpcap('C:\\iitk\\UploadedFiles\\2022-02-23-traffic-analysis-exercise.pcap')


	sessions = pcap_flow.sessions()
	urls_output = open("urls_file", "wb")
	for session in sessions:
		print(session)
		for packet in sessions[session]:
			try:
				if packet[TCP].dport == 80:
					payload = bytes(packet[TCP].payload)
					url = get_url_from_payload(payload)
					urls_output.write(url.encode())
			except Exception as e:
				pass
	urls_output.close()

def main():
	pcapfile = rdpcap('C:\\iitk\\UploadedFiles\\2022-02-23-traffic-analysis-exercise.pcap')

	for packets in pcapfile:
		if packets.haslayer(DNSRR):
			print("DNSRR")
		elif packets.haslayer(TCP):
			#print("TCP")
			pass
		else:
			#print("Unknown")
			pass
	return

if __name__ == "__main__":
	main()
