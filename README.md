[![ArkThor Front Engine](https://github.com/JawedCIA/KCST/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JawedCIA/KCST/actions/workflows/dotnet.yml)

<h3 align="center">ArkThor</h3>

<p align="center">
  Provide Safetly and Strength to your organization using ArkThor  
    <br />
 <p/>

Threat categorization is one of the biggest challenges that the security community faces today. Malwares are hidden using multiple layers of packers, obfuscators thus hiding from revealing its true identity unless unpacked. 
	Of late, it is also observed that the same codebase / framework is reused by multiple RAT builders and Backdoors. Some of these packers and obfuscators are also reused across multiple malware families.

This project aims at looking into the networking concept of these C2 communicating malwares and tries to parse the network packets and classify the threats based on the unique communication pattern used by these malware families. The rules also involve fingerprinting the TLS certificates used in the communication. 


