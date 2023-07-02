[![ArkThor Front Engine](https://github.com/JawedCIA/KCST/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JawedCIA/KCST/actions/workflows/dotnet.yml)  [![Docker ArkThor UI Image CI](https://github.com/JawedCIA/ArkThor/actions/workflows/docker-image-UI.yml/badge.svg)](https://github.com/JawedCIA/ArkThor/actions/workflows/docker-image-UI.yml) [![Docker ArkThor API Image](https://github.com/JawedCIA/ArkThor/actions/workflows/docker-image-API.yml/badge.svg)](https://github.com/JawedCIA/ArkThor/actions/workflows/docker-image-API.yml)[![Docker Image CORE](https://github.com/JawedCIA/ArkThor/actions/workflows/docker-image-CORE.yml/badge.svg)](https://github.com/JawedCIA/ArkThor/actions/workflows/docker-image-CORE.yml)

## We have created a Wiki page with a wealth of information on ArkThor. Please take a moment to visit the Wiki page!!
https://github.com/JawedCIA/ArkThor/wiki


# ArkThor
## ![image](https://user-images.githubusercontent.com/16477789/231995799-ed466aa4-538b-4504-b738-38ea0e853488.png)

<p align="center">
 <strong> Safety and Strength to an organization in Cyber Security Cyber Defence  </strong>
    <br />
 <p/>

The threat landscape facing modern organizations is constantly evolving and becoming increasingly complex. With policies like BYOD and social data of individuals available on the social media, Passwords and 2 FA are not going to stop the cyber attacks. Understanding the threat discovered in a corporate environment will help the infosec team to assess the impact of the attack and take measures accordingly Cyber attacks are becoming more sophisticated and complex, and
it is becoming increasingly difficult to detect and block /prevent them.

One of the key challenges in effectively defending against cyber threats is the ability to accurately categorize and analyze potential threats.

In particular, understanding the Command and Control (C2) communications used by attackers is critical in identifying and responding to cyber attacks. Command and Control (C2) communication is a common technique used by attackers to control the infected hosts and steal sensitive information. It is crucial to identify C 2 communication and categorize the network threats accurately to prevent and mitigate cyber attacks.

This project aims at looking into the networking concept of these C2 communicating malwares and tries to parse the network packets and classify the threats based on the unique communication pattern used by these malware families. 

The rules also involve fingerprinting the TLS certificates used in the communication.

### During our CSCD (Advanced Certification Program in Cyber Security and Cyber Defense) program for 2022-2023 at IIT Kanpur, in collaboration with TalentSprint, we began this as a Capstone project. 
### As we continued to develop the project, we discovered that it had evolved into a product, and thus, ArkThor was born..


<h4> The ArkThor team is committed to providing a comprehensive view of analyzed files, including threat categorization and rich details. </h4>


<img src="https://user-images.githubusercontent.com/16477789/229313617-2e20dda9-1ab0-47db-b738-33c35791e5bc.png" width="40%" height="40%">
 <b>"Ark"</b> imply safety or protection <br/>
 <strong>"Thor"</strong> is associated with strength or power

### Team
* [Contributor] Mohammed Jawed
* [Contributor] SriRam P
* [Mentor] Prof. Anand handa, IIT-K
* [Mentor] Nitesh Kumar, IIT-K

### A Sample ArkThor site is available for public view 
https://arkthor.azurewebsites.net/

## Project Core Architect Idea
The entire project is comprised of three distinct layers: a platform-independent layer that is scalable and built using microservices. It is also designed to be easy to deploy and relies entirely on an opensource technology stack,
As illustrated in the diagram below.
![image](https://user-images.githubusercontent.com/16477789/231136431-6ea546b1-1cf4-4cb6-ad9d-f97ac0abc5f7.png)



### The fundamental concept behind the ArkThor project is to ensure that it is:,
1. Platform independent by leveraging containerization
2. Scalable
3. Composed of microservices
4. Easy to plug in and analyze, with the option to run the Core layer using a simple Python command
5. Easy to deploy
6. Built exclusively on open-source technology
7. Capable of presenting the analyzed results on the UI in a manner that is easily comprehensible even to non-security professionals.. 

#### "The most important principle we follow is that organizations do not buy products, but solutions to their problems. This means that we strive to provide solutions that address the specific needs and challenges of an organization, rather than just providing a generic product."


## ArkThor WorkFlow Architect with various components
With common direct workflow path.
<img src="https://user-images.githubusercontent.com/16477789/231137888-1106539b-efa9-47b3-803e-7b446c6fa9ae.png" width="110%" height="110%">
### End -to-End Working based on above workflow diagram
![image](https://user-images.githubusercontent.com/16477789/231138530-d90fa17b-2759-4ff8-a814-7ae4285b556a.png)
![image](https://user-images.githubusercontent.com/16477789/231138600-db5d41c8-0947-4bff-a98d-ef9239da2113.png)
![image](https://user-images.githubusercontent.com/16477789/231138668-c87ffdcf-5ce0-40fa-8a57-a21555a05ea9.png)



## Refrences used in ArkThor
***[AdminLTE] https://github.com/ColorlibHQ/AdminLTE***</br>
***[Bootstarp] https://getbootstrap.com/***</br>
***[Flags] https://tabler.io/docs/plugins/flags***</br>
***[SQLite] https://sqlite.org/index.html***</br>
***[RabbitMQ] https://www.rabbitmq.com/***</br>
***[SCAPY] https://github.com/secdev/scapy***</br>
***[Rule Engine] https://pypi.org/project/rule-engine/***</br>
***[ThreatFox] https://threatfox.abuse.ch/*** </br>
***[ASPNet Core] https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-7.0***</br>
***[Pyhton] https://www.python.org/***</br>
***[Docker] https://www.docker.com/***</br>
