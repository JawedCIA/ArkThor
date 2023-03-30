from scapy.all import *

# read the pcap file
#pcap_flow = rdpcap('2023-03-16-Emotet-E5-infection-with-spambot-traffic.pcap')
pcap_flow = rdpcap('2023-03-08-IcedID-with-BackConnect-and-VNC-traffic.pcap')


sessions = pcap_flow.sessions()
urls_output = open("urls_file", "wb")
for session in sessions:
    for packet in sessions[session]:
        try:
            if packet[TCP].dport == 80:
                payload = bytes(packet[TCP].payload)
                url = get_url_from_payload(payload)
                urls_output.write(url.encode())
        except Exception as e:
            pass
urls_output.close()