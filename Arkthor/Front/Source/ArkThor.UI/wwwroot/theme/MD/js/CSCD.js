//------------------------------------------//
// JavaScript File for ArkThor function
//Author: MD Jawed
// Email: Jawed.ace@gmail.com
// Application: ArkThor , Deployed During CSCD IITK Program
//Date: 2022-2023
//------------------------------------------//






//Get Current Week Date
function getCurrentWeekDates() {
    let curr = new Date
    let week = []

    for (let i = 1; i <= 7; i++) {
        let first = curr.getDate() - curr.getDay() + i
        let day = new Date(curr.setDate(first)).toISOString().slice(0, 10)
        week.push(day)
    }
    return week;
};
//Set value to an element
function setValue(id, info) {
    document.getElementById(id).textContent = info;    
};
//Get value from an Element
function getValue(id) {
    var value = document.getElementById(id).value;
    // console.log(id + "=" + value);
    return value;
};
//Set value to an element by Value
function setValueOfElement(id, info) {
    document.getElementById(id).value = info;
    //console.log(id + "=" + info);
    //return value;
};
//Get Value using innerHTML
function getValueinnerHTML(id) {
    return document.getElementById(id).innerHTML;
    //console.log(id + "=" + info);
    //return value;
};
//GeSett Value using innerHTML
function setValueinnerHTML(id, info) {
    document.getElementById(id).innerHTML = info;
    //console.log(id + "=" + info);
    //return value;
};
//Retrieve Parameter from URL 
function getParamFromUrl(baseURl) {
    //console.log("In:" + baseURl);
    var encodedUrl = encodeURI(baseURl); // Attempt to encode the URL
   // console.log("encodedUrl:" + encodedUrl);
    var params = {};
    var parser = document.createElement('a');
    parser.href = encodedUrl;
    var query = parser.search.substring(1);
    var vars = query.split('&');
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split('=');
        params[pair[0]] = decodeURIComponent(pair[1]);
    };
    return params;
};

//Testing ...............
function getTopThreatOperation() {

    let numberOfRecordsToFetch = 10;
    // console.log(numberOfRecordsToFetch);
    const recordsSummaryTableBody = document.getElementById("tblTbodyFileSummaryCurrent");
    while (recordsSummaryTableBody.hasChildNodes()) {
        recordsSummaryTableBody.removeChild(recordsSummaryTableBody.firstChild);
    }

    var requestTOPFileRecordURL = "ProxyToExternalEndpoint_GetTOPFileRecord?numberOfRecordsToFetch=" + numberOfRecordsToFetch;

    // Get the current URL
    var currentUrl = window.location.href;

    // Get the root URL
    var rootUrl = window.location.origin;

    // Check if the URL has "/Home" immediately after the root URL
    var hasHomeAfterRoot = currentUrl.startsWith(rootUrl + "/Home");

    if (!hasHomeAfterRoot) {
        requestTOPFileRecordURL = "Home/" + requestTOPFileRecordURL
    } 

    let xhr = new XMLHttpRequest();
    xhr.open("GET", requestTOPFileRecordURL); // Use the proxy controller's endpoint
    xhr.setRequestHeader("Content-Type", "application/json");
    xhr.send();

    xhr.onload = function () {
        if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {

            var apiResponse = JSON.parse(this.response);

            if (apiResponse.result) {

                // var openAnalysis = getInQAnalysisStatus(apiResponse);               
                // setValue("h3InQAnalysis", openAnalysis);
                //Disable No Request Found Div
                document.getElementById("divNoAnalyizeFound").style.display = "none";//divNorecordTeamFoud

                for (var record of apiResponse.result) {
                    displayFileRecords(record, "tblTbodyFileSummaryCurrent");
                }
                //Disable No record Found Text
                //var element = document.getElementById("trCurrentNoRecordFound");//.style.display = 'none';
                //element.setAttribute("hidden", "hidden");
            }
            else {
                //TODO
            }
        }
        else {
            // console.log("Status:" + this.status)          


        }
    };

   
    
};


//Get Current Week Record Count (New/Closed)
function getInQAnalysisStatus(response) {
    var newRequest = 0;
    for (var req of response) {
        //  console.log(response);
        if (!(req.isDeployed == '1' || req.finalStage == '9' || req.recordStatus != "New")) {
            //console.log(req.recordStatus);
            newRequest++;
        }
    }

    return newRequest;

};

//Current Week Analysis Report
function displayFileRecords(response, tblBodyID) {

   // console.log(response);
    const recordboardTableBodyCurrentWeek = document.getElementById(tblBodyID);

    let recordboardTableBodyRow = document.createElement('tr') // Create the current table row

    //var recordId = response.id;
    //var recordTime = response.recordTime;
   // Display ID
    let tdRecordID = document.createElement('td');
    tdRecordID.innerText = response.hashValue.slice(0, 4);


    let recordDate = document.createElement('td');
    if (response.uploadedDate == undefined || response.uploadedDate == null || response.uploadedDate == "") {
        recordDate.innerHTML = "</br>";
    }
    else { 
        var rDate = response.uploadedDate;
        var fileDate = rDate.split('T')[0];
        var rTime = (rDate.split('T')[1]).slice(0, 8);
        recordDate.innerHTML = fileDate + "</br>" + rTime + "";
        }
    // Day
    //recordDate.innerHTML = new Date(fileDate).toLocaleString('en-us', { weekday: 'long' }) + "</br>" + rTime;
   
    //Time
   // let recordTime = document.createElement('td');
    //recordTime.innerText = rTime;

    //Display Current Status
    let recordStatus = document.createElement('td')
    recordStatus.className = 'project-state'
    let recordStatusSpan = document.createElement('span')

    //For Inrogress Spinner
    let divInProgressStatus = document.createElement('div')
    divInProgressStatus.className = 'fa fa-cog fa-spin fa-2x fa-fw text-primary'//'spinner-border text-primary'
    divInProgressStatus.role ='status'
    let brAfterInProgressStatus = document.createElement('br')
   // divInProgressStatus.appendChild(brAfterInProgressStatus);

    if (response.status) {
        recordStatusSpan.innerText = response.status
        if ((response.status).toUpperCase() == 'DONE') { recordStatusSpan.className = 'badge bg-success' }
        else if ((response.status).toUpperCase() == 'CANCELED') { recordStatusSpan.className = 'badge bg-danger' }
        else if ((response.status).toUpperCase() == 'QUEUED') { recordStatusSpan.className = 'badge bg-info' }
        else if ((response.status).toUpperCase() == 'INPROGRESS') { recordStatusSpan.className = 'badge bg-primary'; recordStatus.append(divInProgressStatus, brAfterInProgressStatus); }
        else { recordStatusSpan.className = 'badge badge-warning' }

    }
    else {
        recordStatusSpan.innerText = "QUEUED"
        recordStatusSpan.className = 'badge bg-info'
    }
    recordStatus.append(recordStatusSpan);
    //File Name
    let recordFileName = document.createElement('td')
    if (response.fileName == undefined || response.fileName == null || response.fileName == "") {
        // recordUploadedBy.innerHTML = "</br>";
    }
    else { recordFileName.innerText = response.fileName }
   

    //Record SHA
    let recordSHA = document.createElement('td')
    recordSHA.innerText = response.hashValue
   

     //ThreatType
    let recordType = document.createElement('td')
    recordType.className = 'project-state'
   // recordType.style.whiteSpace = "nowrap";
    //recordType.style.overflow = "hidden";
    //recordType.style.textOverflow = "ellipsis";
    let recordTypeSpan = document.createElement('span')
   // recordTypeSpan.style.whiteSpace = "nowrap";
    //recordTypeSpan.style.overflow = "hidden";
    //recordTypeSpan.style.textOverflow = "ellipsis";
    //recordTypeSpan.style.display = "inline-block";
    //recordTypeSpan.style.maxWidth = "150px"; // Adjust the value as needed for the maximum width

    
    if (response.threatType) {
        let threatType = response.threatType.toUpperCase();
        if (threatType.includes(", ")) {


            threatType = getHighestConfidenceLevelThreatType(threatType);
            //console.log("Highest Threat Type: " + threatType);
            //  console.log(highestConfidenceVariable); // Output: COBALT STRIKE BOTNET C2 SERVER (CONFIDENCE LEVEL: 100%)

        }
        recordTypeSpan.innerText = threatType;// response.threatType.toUpperCase();    
        if (threatType == 'NO THREAT') { recordTypeSpan.className = 'badge bg-success' }
        else if ((threatType == 'AMBIGUOUS') || (threatType == 'UNCATEGORIZED')) { recordTypeSpan.className = 'badge badge-secondary ' } //Uncategorized
        else if (threatType == 'SUSPICIOUS') { recordTypeSpan.className = 'badge badge-warning' }
        else {
            recordTypeSpan.className = 'badge bg-danger wrap-text'
            //wrap - text' 
        }
    }
    else {
       // recordTypeSpan.innerText = "Waiting.."
        recordTypeSpan.className = 'badge badge-info'
    }
    recordType.append(recordTypeSpan);
    //END record Type//
    //Analysis Completed
    let dtAnalysisDate = document.createElement('td');
    if (!(response.analyzedDate == undefined || response.analyzedDate == null || response.analyzedDate == "")) {
        var analysisDate = response.analyzedDate;
        var fileAnalyzedDate = analysisDate.split('T')[0];

        var analyzedTime = (analysisDate.split('T')[1]).slice(0, 8);
        // Day
        //dtAnalysisDate.innerHTML = new Date(fileAnalyzedDate).toLocaleString('en-us', { weekday: 'long' }) + "</br>" + analyzedTime;
        dtAnalysisDate.innerHTML = fileAnalyzedDate + "</br>" + analyzedTime;
    }
   
 
   //Current Stage
  //  let tdCurrentStage = document.createElement('td');
   // if (response.currentStage == undefined || response.currentStage == null || response.currentStage == "") {
     //   tdCurrentStage.innerText = "";
    //}
    //else {
      //  tdCurrentStage.innerText = response.currentStage;
    //}
   

    //UploadedBy
    let recordUploadedBy = document.createElement('td')
    if (response.uploadedBy == undefined || response.uploadedBy == null || response.uploadedBy == "") {
       // recordUploadedBy.innerHTML = "</br>";
    }
    else { recordUploadedBy.innerText = response.uploadedBy }
    
    //Action
    
    let recordAction = document.createElement('td')
    recordAction.className = 'project-actions text-right';

    let recordActionViewLink = document.createElement('a');
    recordActionViewLink.className = 'btn btn-info';
    recordActionViewLink.href = '/Home/AnalysisInformation?SHA=' + response.hashValue;

    let recordActionViewLinkItalicText = document.createElement('i');
    recordActionViewLinkItalicText.className = 'fas fa-eye';
    //recordActionViewLinkItalicText.innerText = 'View';
    recordActionViewLink.append(recordActionViewLinkItalicText);
    recordAction.append(recordActionViewLink);

    //Append Table
    //recordboardTableBodyRow.append(tdRecordID, recordDate, recordStatus, recordFileName, recordSHA, tdCurrentStage, recordType, dtAnalysisDate, recordUploadedBy, recordAction);
    //recordboardTableBodyRow.append(recordSHA, recordFileName, recordDate, recordStatus, recordType, dtAnalysisDate, recordUploadedBy, tdCurrentStage, recordAction);
    recordboardTableBodyRow.append(tdRecordID,recordSHA, recordFileName, recordDate, recordStatus, recordType, dtAnalysisDate, recordAction);
   
    recordboardTableBodyCurrentWeek.append(recordboardTableBodyRow);

};
//Get Highest Confidence level Threat Type
function getHighestConfidenceLevelThreatType(threatType) {
    //threatType = "BOKBOT (CONFIDENCE LEVEL: 75%), COBALT STRIKE BOTNET C2 SERVER (CONFIDENCE LEVEL: 100%), NETWIRE RC BOTNET C2 SERVER (CONFIDENCE LEVEL: 100%)";
    //Split the threatType
    // Split the text into parts using the comma as the separator
    let parts = threatType.split(", ");
    // Initialize variables to track the highest confidence level and its corresponding part
    let highestConfidenceLevel = 0;
    let highestConfidenceVariable = "";
    // Iterate over each part and extract the confidence level
    for (let part of parts) {
        //if (part.match(/\s*\(CONFIDENCE LEVEL: [1-9][0-9]?(?:\d{0,1}|100)%\)/)) {
        let match = part.match(/\(CONFIDENCE LEVEL: (\d+)%\)/);
            if (match) {
                let confidenceLevel = parseInt(match[1]);

                if (confidenceLevel > highestConfidenceLevel) {
                    highestConfidenceLevel = confidenceLevel;
                    highestConfidenceVariable = part;
                } else if (confidenceLevel === highestConfidenceLevel && !highestConfidenceVariable.includes('(CONFIDENCE LEVEL:')) {
                    highestConfidenceVariable = part;
                }
            }
       // }
    }
    if ((highestConfidenceVariable == undefined || highestConfidenceVariable == null || highestConfidenceVariable == "")) {
        highestConfidenceVariable = parts[0];
    }
    return highestConfidenceVariable;

}
//C2 Communication Flow
function c2CommunicationFlowGraph() {
    var graph = new flowjs.DiGraph();
    graph.addPaths([
        ["Attacker", "C2 Server1", "C2 Proxy1", "Target"],
        ["C2 Server2", "C2 Proxy1", "Target"],
        ["C2 Server2", "C2 Proxy2", "C2 Proxy1"]
    ]);

    new flowjs.DiFlowChart("canvas_C2CommunicationFlow", graph).draw();

};

//Pad 2
function pad2(n) {
    return n < 10 ? '0' + n : n
}
//Generate PDF
function generatePDF(elementID, fileName) {

    // Choose the element id which you want to export.
    var element = document.getElementById(elementID);
    var date = new Date();
    var pdfFileDateAppend = date.getFullYear().toString() + pad2(date.getMonth() + 1) + pad2(date.getDate()) + pad2(date.getHours()) + pad2(date.getMinutes()) + pad2(date.getSeconds());
    let pdfFileName = fileName + pdfFileDateAppend + '.pdf';
    // element.style.width = '2000px';
    //element.style.height = '1900px';
    var opt = {
        margin: 0.0,
        filename: pdfFileName,
        image: { type: 'jpeg', quality: 1 },
        html2canvas: { scale: 1 },
        jsPDF: { unit: 'in', format: 'a3', orientation: 'Landscape', precision: '12' } //portrait Landscape
    };

    // choose the element and pass it to html2pdf() function and call the save() on it to save as pdf.
    html2pdf().set(opt).from(element).save();
};

//Get File Record Details
function GetFileRecordDetails() {
    const apiURL = "ProxyToExternalEndpoint_GetByHash";
    var clickedBaseURl = window.location.href;
    
    var getParams = getParamFromUrl(clickedBaseURl);
    var hashOfFile = getParams["SHA"];
   // var ID = getParams["ID"];


        var fileRecordAPIURL = apiURL + "?hash256=" + hashOfFile;
        let req = new XMLHttpRequest();
    req.open("GET", fileRecordAPIURL); // Use the proxy controller's endpoint
    req.setRequestHeader("Content-Type", "application/json");
    req.send();
          
                req.onload = function () {
                    if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {

                        var response = JSON.parse(this.response);
                        // console.log(response);
                        try {
                            //   console.log(response);
                            // console.log(response.result);
                            //  console.log(response.result[0]);
                            //console.log(response.result.length);
                            if (response.result.length > 0) {
                                
                                displayFileRecordInformation(response.result[0]);
                               
                                GetSupportFileRecords(hashOfFile);
                            }
                            else {
                                //   console.log("Status:" + this.status)
                                setValue("title", "No Record Found!");
                                document.getElementById("divCardBody").style.display = "none"; //hrefReleaseNote
                                document.getElementById("divCardTools").style.display = "none";
                                // document.getElementById("divCardBody").style.display = "none"; //hrefReleaseNote
                                // document.getElementById("divCardTools").style.display = "none";

                            }
                        }
                        catch (err) {
                            console.log("Error found while displaying File Information: " + err.message);
                        }

                    }
                    else {
                        //   console.log("Status:" + this.status)
                        setValue("title", "No Record Found!");
                        document.getElementById("divCardBody").style.display = "none"; //hrefReleaseNote
                        document.getElementById("divCardTools").style.display = "none";
                        // document.getElementById("divCardBody").style.display = "none"; //hrefReleaseNote
                        // document.getElementById("divCardTools").style.display = "none";

                    }
                };         
       


};
//Get Supprt Files
function GetSupportFileRecords(hashOfFile) {

    const ui_supportFiles = document.getElementById("ulSupportFiles");
    let req = new XMLHttpRequest();
    let apiUrlToGetSupportFiles = "ProxyToExternalEndpoint_GetSupportFiles?sha256=" + hashOfFile;
    if (!(hashOfFile == undefined || hashOfFile == null || hashOfFile == "")) {
        
        req.open("GET", apiUrlToGetSupportFiles);
        req.setRequestHeader("Content-Type", "application/json");
        req.send();
        req.onload = function () {
            if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                // var options = '';
                let files = JSON.parse(this.response);
               // console.log(files);

                if (!(files == undefined || files == null || files == "")) {
                                        
                    for (var file of files)
                    {
                       
                        let li_SupportFile = document.createElement("li");
                        let a_SupportFile = document.createElement("a");
                        a_SupportFile.className = "btn-link text-secondary";
                        a_SupportFile.target = "_blank";
                        a_SupportFile.href = "data:" + file.contentType + ";base64," + file.data;
                        a_SupportFile.download = file.fileName;
                        a_SupportFile.innerHTML = file.fileName;
                        // setValueinnerHTML("a_RelleaseFile", response.name);

                        let i_SupportFile = document.createElement("i");
                        // i_RelleaseFile.innerText = response.name;
                        let i_SupportFile_icon = '<i class="far fa-fw fa-file-word"></i>';
                        if (file.extension.includes("doc")) {
                            i_SupportFile_icon = '<i class="far fa-fw fa-file-word"></i>';

                        }
                        else if (file.extension.includes("pdf")) { i_SupportFile_icon = '<i class="far fa-fw fa-file-pdf"></i>'; }
                        else if (file.extension.includes("png") || file.extension.includes("jpg")) { i_SupportFile_icon = '<i class="far fa-fw fa-image"></i>'; }
                        else if (file.extension.includes("msg")) { i_SupportFile_icon = '<i class= "far fa-fw fa-envelope"></i>'; }
                        else if (file.extension.includes("word")) { i_SupportFile_icon = '<i class= "far fa-fw fa-file-word"></i>'; }
                        else { i_SupportFile_icon = '<i class="far fa-fw fa-file-code"></i>'; }// fa-file-code-o
                        //far fa-fw fa-file-word
                        // a_RelleaseFile.append(i_RelleaseFile);

                        a_SupportFile.innerHTML = i_SupportFile_icon + file.fileName;// response.name;

                        li_SupportFile.append(a_SupportFile);
                        ui_supportFiles.append(li_SupportFile);
                    }

                }
                else {
                    console.log("0 Support File Return from API..");
                }

            }
        };
    }
    else {
       
    }


}
//Display File Record on UI
function displayFileRecordInformation(resultResponse) {
   // console.log(resultResponse);
   // console.log("DUMMY Data, yet to bind with UI");
    //Display C2 Communication graph
    c2CommunicationFlowGraph();
    // console.log(resultResponse);
    setValue("title", "Malware's C2 Communication Summary for file : " + resultResponse.hashValue.slice(0,4));
    //Set Status
    //Set Threat Type
    if (!(resultResponse.status == undefined || resultResponse.status == null || resultResponse.status == "")) {
        setValue("spanStatus", resultResponse.status.toUpperCase());
    }
    
    var ispanStatus = document.getElementById("iStatus");
    if (resultResponse.status.toUpperCase() == "INPROGRESS") {
        ispanStatus.className = 'fa fa-cog fa-spin fa-2x fa-fw';
    }
    else if (resultResponse.status.toUpperCase() == "QUEUED") {
        ispanStatus.className = 'fa fa-hourglass';
    }
    else {
        ispanStatus.className = 'fa fa-cog fa-2x fa-fw';
    }   


    //Set Threat Type
    if (!(resultResponse.threatType == undefined || resultResponse.threatType == null || resultResponse.threatType == "")) {
        let threatType = resultResponse.threatType.toUpperCase();
       // setValueinnerHTML("ddThreatTypeIdentified", threatType.replace(/,/g, "<br>- "));
        let ddThreatTypeIdentifiedElement = document.getElementById("ddThreatTypeIdentified");
        ddThreatTypeIdentifiedElement.innerHTML = "- " + threatType.replace(/,/g, "<br>- ");

        if (threatType.includes(", ")) {
            threatType = getHighestConfidenceLevelThreatType(threatType);
           // console.log("Highest Threat Type: " + threatType);
            
        }
        var updatedthreatType = threatType.replace(/\s*\(CONFIDENCE LEVEL: [1-9][0-9]?(?:\d{0,1}|100)%\)/, "");
        setValue("spanType", updatedthreatType);
       // setValue("spanType", threatType);
    }

    
    //Set Severity
   //setValue("spanType", resultResponse.severity);
    if (resultResponse.severity) {
        setValue("spanSeverity", resultResponse.severity);
    }
    //set SHA-256
    if (resultResponse.hashValue) {
        setValue("ddSHA256", resultResponse.hashValue);
    }
    if (resultResponse.currentStage) {
       // setValue("spanStage", resultResponse.currentStage);
    }
   
    //Set Submitted By
    if (resultResponse.uploadedBy) {
        setValue("ddUploader", resultResponse.uploadedBy);
    }
    //Set Uploaded Date
    if (resultResponse.uploadedDate) {
        var rDate = resultResponse.uploadedDate;
        var fileDate = rDate.split('T')[0];
        var rTime = (rDate.split('T')[1]).slice(0, 8);
        setValue("ddUploadTime", fileDate + " " + rTime);
    }
    //Set Analysis date
    if (resultResponse.analyzedDate) {
        var aDate = resultResponse.analyzedDate;
        var fileADate = aDate.split('T')[0];
        var aTime = (aDate.split('T')[1]).slice(0, 8);
        setValue("ddAnalysisDateTime", fileADate + " " + aTime);
    }
    //Set File Download link
    if (resultResponse.data) {
        document.getElementById("hrefUploadedFile").href = "data:" + resultResponse.contentType + ";base64," + resultResponse.data;
        document.getElementById("hrefUploadedFile").download = resultResponse.fileName;
        setValueinnerHTML("hrefUploadedFile", resultResponse.fileName);
    }
    //console.log(resultResponse);
   // console.log("json data:" + resultResponse.jsonData);
    //Set jason File Download
    if (resultResponse.jsonData) {
       // console.log("json data:" + resultResponse.jsonData);
        document.getElementById("hrefJsonFile").href = "data:application/json;base64," + resultResponse.jsonData;
        document.getElementById("hrefJsonFile").download = resultResponse.hashValue+".json";
        setValueinnerHTML("hrefJsonFile", resultResponse.hashValue + ".json");
    }
    //Show similar threat records
    if (resultResponse.threatType) {
        document.getElementById("hrefsimilar").href = "/Home/ThreatRecords?threatType=" + resultResponse.threatType;
    }    
   //C2 Communication Countries
    var country = new CountryCode();
    if (!(resultResponse.c2Countries == undefined || resultResponse.c2Countries == null || resultResponse.c2Countries == ""))
    {
       
        // search by FIPS       
       // console.log(country.getName("us"));
        const tableRowC2Conrties = document.getElementById("tblTrCountriesFlag");
        //console.log(resultResponse.c2Countries);
       
        var countries = JSON.parse(resultResponse.c2Countries); 
       // console.log(typeof mulCountries);
        if (!(countries == undefined || countries == null || countries == "")) {
            for (var i = 0; i < countries.length; i++) {
                //console.log(countries[i]);
                let tdCountry = document.createElement('td');
                let flagicon = document.createElement('i');
                flagicon.className = 'flag flag-sm flag-country-' + countries[i].toLowerCase();
                tdCountry.innerHTML = "<p>" + country.getName(countries[i].toLowerCase());
                tdCountry.append(flagicon);
                tableRowC2Conrties.append(tdCountry);
            }
        }

    }

    if (!(resultResponse.infected_countries == undefined || resultResponse.infected_countries == null || resultResponse.infected_countries == "")) {

        // search by FIPS       
        // console.log(country.getName("us"));
        const tableRowInfC2Contries = document.getElementById("tblTrInfCountriesFlag");
        //console.log(resultResponse.infected_countries);

        var countries = JSON.parse(resultResponse.infected_countries);
        // console.log(typeof mulCountries);
        if (!(countries == undefined || countries == null || countries == "")) {
            for (var i = 0; i < countries.length; i++) {
                //console.log(countries[i]);
                let tdCountry = document.createElement('td');
                let flagicon = document.createElement('i');
                flagicon.className = 'flag flag-sm flag-country-' + countries[i].toLowerCase();
                tdCountry.innerHTML = "<p>" + country.getName(countries[i].toLowerCase());
                tdCountry.append(flagicon);
                tableRowInfC2Contries.append(tdCountry);
            }
        }

    }

    //Display MITRE
    if (!(resultResponse.mitre == undefined || resultResponse.mitre == null || resultResponse.mitre == "")) {

        // search by FIPS       
        // console.log(country.getName("us"));
        const ulMitreDisplay = document.getElementById("ulMitreDisplay");
       // console.log(resultResponse.mitre);

        var mitreTech = JSON.parse(resultResponse.mitre);
        if (!(mitreTech == undefined || mitreTech == null || mitreTech == "")) {
            // console.log(typeof mulCountries);
            for (var i = 0; i < mitreTech.length; i++) {
                //console.log(countries[i]);
                let liMitre = document.createElement('li');

                liMitre.innerHTML = mitreTech[i].toUpperCase();

                ulMitreDisplay.append(liMitre);
            }
        }

    }

};

//Show Analysis Records
//Release Request Summary
function GetAnalysisRecordsForChoosenDate() {
   

    var todayDateInDateTime4From = new Date();//.toISOString().slice(0, 10);
    var todayDateInDateTime4To = new Date();//new Date().toISOString().slice(0, 10);

   
    const recordsSummaryTableBody = document.getElementById("tblTbodyRecordSummary");
    while (recordsSummaryTableBody.hasChildNodes()) {
        recordsSummaryTableBody.removeChild(recordsSummaryTableBody.firstChild);
    }
    //Check for Fromdate
    let dropdownRecordSummaryFromDate = getValue("txtUploadDateFrom");

    let dropdownRecordSummaryToDate = getValue("txtUploadDateTo");

    if (dropdownRecordSummaryFromDate == undefined || dropdownRecordSummaryFromDate == null || dropdownRecordSummaryFromDate == "") {

        todayDateInDateTime4From.setMonth(todayDateInDateTime4From.getMonth() - 1);
        dropdownRecordSummaryFromDate = todayDateInDateTime4From.toISOString().slice(0, 10);     
        setValueOfElement("txtUploadDateFrom", dropdownRecordSummaryFromDate);
    }
    if (dropdownRecordSummaryToDate == undefined || dropdownRecordSummaryToDate == null || dropdownRecordSummaryToDate == "") {
       
        todayDateInDateTime4To.setMonth(todayDateInDateTime4To.getMonth() + 1);
        dropdownRecordSummaryToDate = todayDateInDateTime4To.toISOString().slice(0, 10);
        setValueOfElement("txtUploadDateTo", dropdownRecordSummaryToDate);

    }
    else {
        
    }

    if (dropdownRecordSummaryToDate >= dropdownRecordSummaryFromDate) {
        var getRecordRequestURL = "ProxyToExternalEndpoint_GetFileRecordByUploadedDate?FromUploadedDate=" + dropdownRecordSummaryFromDate + "&ToUploadedDate=" + dropdownRecordSummaryToDate;
       // console.log(getRecordRequestURL);
        let req = new XMLHttpRequest();
        req.open("GET", getRecordRequestURL); // Use the proxy controller's endpoint
        req.setRequestHeader("Content-Type", "application/json");
        req.send();

        req.onreadystatechange = function () {
                    if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                        var relResponse = JSON.parse(this.response);
                        // console.log(relResponse.result);
                        if (!(relResponse == undefined || relResponse == null || relResponse == "")) {
                            for (var resp of relResponse.result) {
                                // getReleaseRequest(release, 0);
                                displayFileRecords(resp, "tblTbodyRecordSummary");
                            }
                            DataTableInitialize("tblRecordSummary");
                        }

                    }
                };
    }
    else {
        alert("Please correct Date Range to fetch Analysis Records");
    }

};

//Initialize DataTable for release summary
function DataTableInitialize(id) {

    $('#' + id).DataTable({ // $("#tblReleaseSummary").DataTable({
        "responsive": true,
        "autoWidth": false,
        "pageLength": 10,
        "ordering": true,
        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
        "order": [[0, "desc"]]
    });
};

//On Records Fetch Button Click
function fetchRecords() {
    $("#tblRecordSummary").DataTable().destroy();
    GetAnalysisRecordsForChoosenDate();
};

//Show Analysis Records
//Release Request Summary
function GetSimilarThreats() {
   
    var clickedBaseURl = window.location.href;
   // var getParams = getParamFromUrl(clickedBaseURl);
   // var threatType = getParams["threatType"];    

    var encodedParam = (new URL(clickedBaseURl)).searchParams.get('threatType');

    //console.log(encodedParam);
    setValue("h2ThreatTraceTitle", encodedParam);
    var fileRecordAPIURL ="ProxyToExternalEndpoint_GetSimilarThreatRecords?threatType=" + encodedParam;
    let req = new XMLHttpRequest();

            req.open("GET", fileRecordAPIURL);
            req.setRequestHeader("Content-Type", "application/json");
            req.send();

    req.onload = function () {
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {

                    var responses = JSON.parse(this.response);
                    //console.log(responses.result);
                    if (!(responses == undefined || responses == null || responses == "")) {
                        try {
                            // console.log(responses.result.length);
                            if (responses.result.length > 0) {
                                for (var response of responses.result) {
                                    // console.log(response);
                                    displayFileRecords(response, "tblTbodyThreatSummary");
                                }
                                var element = document.getElementById("trCurrentNoRecordFound");//.style.display = 'none';
                                element.setAttribute("hidden", "hidden");
                                DataTableInitialize("tblThreatSummary");
                            }
                            else {
                                //   console.log("Status:" + this.status)
                                setValue("title", "No Record Found!");
                                document.getElementById("divCardBody").style.display = "none"; //hrefReleaseNote
                                document.getElementById("divCardTools").style.display = "none";

                            }
                        }
                        catch (err) {
                            console.log("Error found while displaying File Information: " + err.message);
                        }
                    }

                }
                else {
                    //   console.log("Status:" + this.status)
                    setValue("title", "No Record Found!");
                    document.getElementById("divCardBody").style.display = "none"; //hrefReleaseNote
                    document.getElementById("divCardTools").style.display = "none";


                }
            };      
       
    

};

//Get dashboard Count Values
function getDashboardCountValues() {
  //  console.log("getDashboardCountValues");
    let apiurl = "ProxyToExternalEndpoint_GetDashboardCount";
    // Get the current URL
    var currentUrl = window.location.href;

    // Get the root URL
    var rootUrl = window.location.origin;

    // Check if the URL has "/Home" immediately after the root URL
    var hasHomeAfterRoot = currentUrl.startsWith(rootUrl + "/Home");

    if (!hasHomeAfterRoot) {
        apiurl = "Home/" + apiurl
    } 
    let req = new XMLHttpRequest();

    req.open("GET", apiurl);
    req.setRequestHeader("Content-Type", "application/json");
            req.send();
            req.onload = function () {
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {

                    var responses = JSON.parse(this.response);
                    // console.log(responses.result);
                    // console.log(responses.result[0].totalCount);
                    try {
                        // console.log(responses.result.length);
                        if (responses.result.length > 0) {
                            setValue("h3FileSubmitted", responses.result[0].totalCount);
                            setValue("h3Queued", responses.result[0].queuedCount);
                            setValue("h3FileAnalyzed", responses.result[0].analyzedCount);
                            setValue("h3TC", responses.result[0].differentThreatType);

                        }
                        else {


                        }
                    }
                    catch (err) {
                        console.log("Error while fetching Dashboard Count: " + err.message);
                    }

                }
                else {


                }
            };
   

};
//Release Request for Single Screen
function getLiveTrackingInfo() {

    let TodaysDate = new Date()
    TodaysDate.toISOString().split('T')[0]
    htitleLivetRacking.innerHTML = "<i class='nav-icon fas fa-rocket'></i>  ArkThor Board - File Analyses Live Tracking - Date:  [ " + TodaysDate.toISOString().split('T')[0]+" ]"; 
   
    let numberOfRecordsToFetch = 16;
    var requestTOPFileRecordURL = "ProxyToExternalEndpoint_GetTOPFileRecord?numberOfRecordsToFetch=" + numberOfRecordsToFetch;

    let req = new XMLHttpRequest();

    req.open("GET", requestTOPFileRecordURL); // Use the proxy controller's endpoint
    req.setRequestHeader("Content-Type", "application/json");
    req.send();
            req.onload = function () {
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {

                    var apiResponse = JSON.parse(this.response);
                    if (!(apiResponse == undefined || apiResponse == null || apiResponse == "")) {
                        if (apiResponse.result) {
                            // console.log(apiResponse.result);
                            document.getElementById("divNoRequestFound").style.display = "none";
                            for (var record of apiResponse.result) {
                                displayLiveTrackingOnBoard(record);
                            }

                        }
                    }
                    else {
                        //TODO
                    }
                }
                else {
                    // console.log("Status:" + this.status)          


                }
            };

};
//Release Single Screen View 360
function displayLiveTrackingOnBoard(response) {
    // console.log(response);
    const release360DivRow = document.getElementById("divBoardRow"); //releaseRow360Closed

    //Display App name as link for release information
    let fileActionViewLink = document.createElement('a');
    fileActionViewLink.href = '/Home/AnalysisInformation?SHA=' + response.hashValue;
    fileActionViewLink.target = '_blank';

    let divcol = document.createElement('div');
    divcol.className = 'col-md-3 col-sm-5 col-12'; //

    //LiveAnalysisTracking

    let divinfo_box = document.createElement('div');
    // divinfo_box.className = 'info-box';
    divinfo_box.className = 'callout callout-info info-box';

    let span_info_box_content_Name = document.createElement('span');
    span_info_box_content_Name.className = 'info-box-number';

    span_info_box_content_Name.innerText =response.fileName;

    //Display first 8 char of sha
    let span_info_box_content_ID = document.createElement('span');
    span_info_box_content_ID.className = 'info-box-number';

    span_info_box_content_ID.innerText = response.hashValue.slice(0,4);

 
    //Div for Banner
    let div_ribbon_wrapper = document.createElement('div');
    div_ribbon_wrapper.className = 'ribbon-wrapper ribbon-lg ribbon.text-lg';

    let div_ribbon_bg_primary = document.createElement('div');
    div_ribbon_bg_primary.className = 'ribbon bg-primary';
    //End of Banner Div

    let divinfo_box_content = document.createElement('div');
    divinfo_box_content.className = 'info-box-content';
    let parentWidth = divinfo_box_content.clientWidth;
    //Display Request Status Icon at top

    let span_info_box_content_Status_Icon = document.createElement('span');


    let span_info_box_content_Status = document.createElement('span');
    span_info_box_content_Status.className = 'info-box-number';
    if ((response.status == undefined || response.status == null || response.status == ""))
    {
        span_info_box_content_Status.innerText = "Status: ";// + response.status;
    }
    else { span_info_box_content_Status.innerText = "Status: " + response.status.toUpperCase(); }
   // span_info_box_content_App.innerText = "Status: " + response.status.toUpperCase(); //

    let span_info_box_content_VR = document.createElement('span');
    span_info_box_content_VR.className = 'info-box-number';

    //span_info_box_content_VR.style.whiteSpace = "nowrap";
    span_info_box_content_VR.style.overflow = "hidden";
    span_info_box_content_VR.style.textOverflow = "ellipsis";
    span_info_box_content_VR.style.display = "inline-block";
   // console.log("parentWidth: " + parentWidth);

    // Get the screen width
   // let screenWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
    //console.log("parentWidth: " + parentWidth);
    // Set the maxWidth of the span based on the screen width
   //(parentWidth * 0.7) + "px"; // Adjust the factor (0.8) as needed

   // span_info_box_content_VR.style.maxWidth = (parentWidth * 0.8) + "px"; // Adjust the value as needed for the maximum width
   
    if ((response.threatType == undefined || response.threatType == null || response.threatType == "")) {
        span_info_box_content_VR.innerText = "Threat Type: ";// + response.threatType;
    }
    else {
        let threatType = response.threatType.toUpperCase();
        if (threatType.includes(", ")) {
            threatType = getHighestConfidenceLevelThreatType(threatType);
            // console.log("Highest Threat Type: " + threatType);

        }
        var updatedthreatType = threatType.replace(/\s*\(CONFIDENCE LEVEL: [1-9][0-9]?(?:\d{0,1}|100)%\)/, "");
       
        span_info_box_content_VR.innerText = "Threat Type: " + updatedthreatType;
    }
   
    span_info_box_content_VR.style.maxWidth = '100%';

    let span_info_box_content_Type = document.createElement('span');
    span_info_box_content_Type.className = 'badge bg-info';//'info-box-number';

    let span_info_box_content_Day = document.createElement('span');
    span_info_box_content_Day.className = 'info-box-number';
    if (response.uploadedDate == undefined || response.uploadedDate == null || response.uploadedDate == "") {
        span_info_box_content_Day.innerText = "</br>";
    }
    else {
        var rDate = response.uploadedDate;
        var fileDate = rDate.split('T')[0];
        var rTime = (rDate.split('T')[1]).slice(0, 8);
        span_info_box_content_Day.innerHTML = fileDate + "</br>" + rTime + "";
    }
    //let uploadedDay = new Date(response.uploadedDate).toLocaleString('en-us', { weekday: 'long' })
    //span_info_box_content_Day.innerText = "Uploaded Date: "+ uploadedDay;

    let span_ribbon_text = document.createElement('span');
    span_ribbon_text.innerText = "The Rise of ArkThor-MD"; //Needed to Increase Width of ribbon
    span_ribbon_text.style.visibility = "hidden";
    div_ribbon_bg_primary.append(span_ribbon_text);

    //For icon to show status 
    let iConStatus = document.createElement('i');
   // iConStatus.className = 'icon fas fa-shopping-cart';//'icon fas fa-check';

    //End icon display CANCELLED
    if (response.status) {
        // console.log(response.status);

        if ((response.status).toUpperCase() == 'DONE') { divinfo_box.className = 'callout callout-success info-box' }
        else if ((response.status).toUpperCase() == 'QUEUED') { divinfo_box.className = 'callout callout-warning info-box'; iConStatus.className = 'fa fa-hourglass'; }
        else if ((response.status).toUpperCase() == 'REMOVED' || (response.status).toUpperCase() == 'CANCELLED' || (response.status).toUpperCase() == 'FAILURE') { divinfo_box.className = 'callout callout-danger info-box'; iConStatus.className = 'icon fas fa-ban'; }
        else if ((response.status).toUpperCase() == 'INPROGRESS') { divinfo_box.className = 'callout callout-info info-box'; iConStatus.className = 'fa fa-cog fa-spin fa-2x fa-fw'; }
        else { divinfo_box.className = 'callout callout-danger info-box' }
    }
    else {
        divinfo_box.className = 'callout callout-info info-box'
    }
    var displayRibbon = '0';
    //New System
    //To Display Banner  
    if ((response.threatType)) { 
        if ((response.threatType).toUpperCase() == 'NO THREAT') {
        div_ribbon_bg_primary.className = 'ribbon bg-success';
        iConStatus.className = 'icon fas fa-check';
        displayRibbon = '1';
        div_ribbon_wrapper.append(div_ribbon_bg_primary);
    }

        else if ((response.threatType).toUpperCase() == 'AMBIGUOUS' || (response.threatType).toUpperCase() == 'SUSPICIOUS' || (response.threatType).toUpperCase() == 'UNCATEGORIZED' ) {
        div_ribbon_bg_primary.className = 'ribbon bg-secondary';
            iConStatus.className = 'icon far fa-question-circle';
        displayRibbon = '1';
        div_ribbon_wrapper.append(div_ribbon_bg_primary);
    }
    else {

        div_ribbon_bg_primary.className = 'ribbon bg-danger';
        iConStatus.className = 'icon fas fa-bug';//'icon fas fa-refresh';
        displayRibbon = '1';
        div_ribbon_wrapper.append(div_ribbon_bg_primary);
    }
}


    span_info_box_content_Status_Icon.append(iConStatus);
    fileActionViewLink.append(span_info_box_content_Name);
    divinfo_box_content.append(span_info_box_content_Status_Icon, span_info_box_content_ID, fileActionViewLink, span_info_box_content_Status, span_info_box_content_VR, span_info_box_content_Day);

    if (displayRibbon == '1') {
        divinfo_box.append( div_ribbon_wrapper, divinfo_box_content);
    }
    else {
        divinfo_box.append( divinfo_box_content);
    }

    divcol.append(divinfo_box);
    // releaseActionViewLink.append(divcol);
    release360DivRow.append(divcol);
};

//Refresh ip2asn
function refreship2asn() {

    let req = new XMLHttpRequest();
        req.open("POST", "ProxyToExternalEndpoint_Updateip2asn");
        req.setRequestHeader("Content-Type", "application/json");
            req.send();
            req.onload = function () {
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                    alert(this.response);
                }
                else {
                    alert(this.response);
                }
                
            };

  
    // console.log("Ext URL:"+ apiURLExt);
};

//Refresh refreshThreatFoxRule
function refreshThreatFoxRule() {

    let req = new XMLHttpRequest();
    req.open("POST", "ProxyToExternalEndpoint_refreshThreatFoxRule");
    req.setRequestHeader("Content-Type", "application/json");
    req.send();
    req.onload = function () {
        if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
            var apiResponse = JSON.parse(this.response);
            alert(this.response);
        }
        else {
            alert(this.response);
        }

    };
};

