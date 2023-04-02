[![ArkThor Front Engine](https://github.com/JawedCIA/KCST/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JawedCIA/KCST/actions/workflows/dotnet.yml)  [![Docker Image CI](https://github.com/JawedCIA/ArkThor/actions/workflows/docker-image.yml/badge.svg)](https://github.com/JawedCIA/ArkThor/actions/workflows/docker-image.yml)

<h3 align="center">ArkThor</h3>

<p align="center">
  Provide Safetly and Strength to your organization using ArkThor  
    <br />
 <p/>

Threat categorization is one of the biggest challenges that the security community faces today. Malwares are hidden using multiple layers of packers, obfuscators thus hiding from revealing its true identity unless unpacked. 
	Of late, it is also observed that the same codebase / framework is reused by multiple RAT builders and Backdoors. Some of these packers and obfuscators are also reused across multiple malware families.

This project aims at looking into the networking concept of these C2 communicating malwares and tries to parse the network packets and classify the threats based on the unique communication pattern used by these malware families. The rules also involve fingerprinting the TLS certificates used in the communication. 



<img src="https://user-images.githubusercontent.com/16477789/229313617-2e20dda9-1ab0-47db-b738-33c35791e5bc.png" width="40%" height="40%">


<h3 align="center">ArkThor WorkFlow Architect with various components</h3>
<img src="https://user-images.githubusercontent.com/16477789/229313914-215f30c7-a085-4e5a-8258-e7e324e770f5.png" width="70%" height="90%">
