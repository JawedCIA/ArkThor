//------------------------------------------//
// JavaScript File for ArkThor function
//Author: MD Jawed
// Email: Jawed.ace@gmail.com
// Application: ArkThor , Deployed During CSCD IITK Program
//Date: 2022-2023
//------------------------------------------//

var CICDChart;
//var drawChart;
//Show all Measurement Graph on button Click
async function ShowAllMeasurementGraph() {

    //fetch base API URL
  //  const BaseAPIURL =await fetchBaseAPIUrl();


    
    var todayDateForPast6Month = new Date();//.toISOString().slice(0, 10);
    var todayDate = new Date().toISOString().slice(0, 10);
    let dropdownFromDate = getValue("txtStatisticsdateFrom");
    let dropdownToDate = getValue("txtStatisticsdateTo");

   // console.log(dropdownFromDate);
    //console.log(dropdownToDate);
    //Call Function to draw chart
    if (dropdownFromDate == undefined || dropdownFromDate == null || dropdownFromDate == "") {
        //Consider Taking past 6 months date
        todayDateForPast6Month.setMonth(todayDateForPast6Month.getMonth() - 6);
        dropdownFromDate = todayDateForPast6Month.toISOString().slice(0, 10);
       //  console.log(dropdownFromDate);
        setValueOfElement("txtStatisticsdateFrom", dropdownFromDate);
    }

    if (dropdownToDate == undefined || dropdownToDate == null || dropdownToDate == "") {
        //Consider Taking Todays Date
        dropdownToDate = todayDate;
        //console.log(dropdownToDate);
        setValueOfElement("txtStatisticsdateTo", dropdownToDate);
    }
   // convertToDateWeekPairs(dropdownFromDate, dropdownToDate, BaseAPIURL);
  document.getElementById("chartBody_StatusWiseDistributionChart").innerHTML = '&nbsp;';
    document.getElementById("chartBody_typeWiseDistributionChart").innerHTML = '&nbsp;';
    document.getElementById("chartBody_UploadedChart").innerHTML = '&nbsp;';
    document.getElementById("chartBody_AnalysesChart").innerHTML = '&nbsp;';
    document.getElementById("chartBody_AnalyzedRate").innerHTML = '&nbsp;';

    document.getElementById("chartBody_CountriesWiseDistributionChart").innerHTML = '&nbsp;';
    document.getElementById("chartBody_InfCountriesWiseDistributionChart").innerHTML = '&nbsp;';


    document.getElementById("chartBody_UploadedChart").innerHTML = '<canvas id="canvasUploadedChart" style="height: 300px;"></canvas>';
    const fileUploaddata = await fetchFileUploadedData(dropdownFromDate, dropdownToDate);

    DistributionBarChart(fileUploaddata, "canvasUploadedChart", "#Total File Uploaded (","rgb(54, 162, 235)");

    
    document.getElementById("chartBody_AnalysesChart").innerHTML = '<canvas id="canvasAnalysesChart" style="height: 300px;"></canvas>';
    const fileAnalyzeddata = await fetchFileAnalyzedData(dropdownFromDate, dropdownToDate);

    DistributionBarChart(fileAnalyzeddata, "canvasAnalysesChart", "#Total File Analyzed (", "rgb(0,128,0)");


    document.getElementById("chartBody_StatusWiseDistributionChart").innerHTML = '<div id="donut-chart-Status" style="height: 350px;"></div>';
    DistributionStatusChart(dropdownFromDate, dropdownToDate, "donut-chart-Status");

    document.getElementById("chartBody_typeWiseDistributionChart").innerHTML = '<div id="donut-chart-Type" style="height: 350px;"></div>';
   // document.getElementById("chartBody_typeWiseDistributionChart").innerHTML = '<canvas id="donut-chart" style="height: 300px;"></canvas>';
    DistributionTypeChart(dropdownFromDate, dropdownToDate, "donut-chart-Type");
  
   //Get Value for Knob
    document.getElementById("chartBody_AnalyzedRate").innerHTML = '<input type="text" class="knob" value="0" data-width="250" data-height="250" data-fgColor="rgb(0,128,0)" id="inputTxtKnobRate" readonly><div class="knob-label">Analyzed Rate</div>';
    const getKnobData = await darwKnobRateForAnalyses(fileAnalyzeddata, fileUploaddata);

    document.getElementById("chartBody_CountriesWiseDistributionChart").innerHTML = '<canvas id="canvasCountriesWiseDistributionChart" style="height: 300px;"></canvas>';
    const C2ContriesData = await fetchC2ContriesData(dropdownFromDate, dropdownToDate);

    document.getElementById("chartBody_InfCountriesWiseDistributionChart").innerHTML = '<canvas id="canvasInfCountriesWiseDistributionChart" style="height: 300px;"></canvas>';
    const C2InfContriesData = await fetchC2InfecContriesData(dropdownFromDate, dropdownToDate);
    
   // DistributionBarChart(fileUploaddata, "canvasCountriesWiseDistributionChart", "#Total File Uploaded (", "rgb(54, 162, 235)");
  //  DistributionC2ContriesChart(dropdownFromDate, dropdownToDate, "donut-chart-Status");

};
//For File Bar Chart Draw
function DistributionBarChart(filedata, elementID,titleMessage,backgroundColor) {
    
    const labels = [];
    const Data = [];
   // const backgroundColor = 'rgb(54, 162, 235)';
    const hoverBackgroundColor = "rgba(54, 162, 235)";
    if (filedata.length > 0) {

        for (var index = 0; index < filedata.length; index++) {

            labels.push(
                (filedata[index].date).split('T')[0]
            );
            Data.push(
                Number(filedata[index].totalCount)
            );

        }
        let sum_request = 0;
        for (let i = 0; i < Data.length; i++) {
            sum_request += Data[i];
        }
       
        drawBarChart(labels, Data, backgroundColor, elementID, "bar", hoverBackgroundColor, sum_request, titleMessage)
    }

};

//Type Chart
//GetReleaseRequestDistributionType
function DistributionTypeChart(FromDate, ToDate, elementID) {


    const labels = [];
    const Data = [];
    const backgroundColor = ["#5AD3D1", "#46BFBD", "#F7464A", "#FDB45C"];
    const hoverBackgroundColor = ["#A8B3C5", "#5AD3D1", "#FF5A5E", "#FFC870"];

    let urlToGetData = "ProxyToExternalEndpoint_GetThreatDistributionBasedOnUploadedDate?FromUploadedDate=" + FromDate + "&ToUploadedDate=" + ToDate;
    

    let req = new XMLHttpRequest();  

    req.open("GET", urlToGetData);
    req.setRequestHeader("Content-Type", "application/json");
            req.send();
            req.onload = function () {
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                     //console.log(this.response);
                    var responseResult = JSON.parse(this.response);
                    // console.log(responseResult);
                    if (responseResult.result.length > 0) {
                       // console.log(responseResult.result);
                        for (var index = 0; index < responseResult.result.length; index++) {
                            let label = (responseResult.result[index].type).toUpperCase().replace(/\s*\(CONFIDENCE LEVEL: [1-9][0-9]?(?:\d{0,1}|100)%\)/g, "");
                            let dat = Number(responseResult.result[index].totalCount);
                           // console.log(label);
                            if (label.includes(", ")) {
                                let parts = label.split(", ");
                               // console.log(parts);
                                for(let part of parts)
                                {
                                    labels.push(part);
                                    Data.push(dat);

                                }
                            }
                            else {
                                labels.push(label);
                                Data.push(dat);
                            }
                          
                            
                        }
                      //  console.log(labels);
                        //console.log(Data);
                        var combinedList1 = labels.map(function (key, index) {
                            return { key: key, value: Data[index] };
                        });
                       // console.log(combinedList1);

                        var combinedList = {};

                        for (var i = 0; i < labels.length; i++) {
                            var key = labels[i];
                            var value = parseInt(Data[i]);

                            if (combinedList[key]) {
                                combinedList[key] += value;
                            } else {
                                combinedList[key] = value;
                            }
                        }
                        var keys = Object.keys(combinedList);
                        var values = Object.values(combinedList);

                       // console.log(combinedList);
                        drawDoughNutChart(keys, values, backgroundColor, elementID, hoverBackgroundColor);
                    }
                }
            };

      
   
};

//Status Chart
function DistributionStatusChart(FromDate, ToDate, elementID) {


    const labels = [];
    const data = [];
    const backgroundColor = ["#5AD3D1", "#46BFBD", "#F7464A", "#FDB45C", "#00FF00"];
    const hoverBackgroundColor = ["#A8B3C5", "#5AD3D1", "#FF5A5E", "#FFC870"];

    let urlToGetData = "ProxyToExternalEndpoint_GetStatusDistributionBasedOnUploadedDate?FromUploadedDate=" + FromDate + "&ToUploadedDate=" + ToDate;


    let req = new XMLHttpRequest();
    req.open("GET", urlToGetData);
    req.setRequestHeader("Content-Type", "application/json");
            req.send();
            req.onload = function () {
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                   // console.log(this.response);
                    var responseResult = JSON.parse(this.response);
                   // console.log(responseResult);
                    if (responseResult.result.length > 0) {
                       // console.log(responseResult.result);
                        for (var index = 0; index < responseResult.result.length; index++) {

                            labels.push(
                                responseResult.result[index].status
                            );
                            data.push(
                                Number(responseResult.result[index].totalCount)
                            );


                        }
                        
                        drawDoughNutChart(labels, data, backgroundColor, elementID, hoverBackgroundColor);
                    }
                }
            };

       

};
//Draw Pie Chart
function drawPieChart(fileRecord, fileRecordData, backgroundcolor, elementID, chartType, hoverBackgroundColor) {

    var ctxP = document.getElementById(elementID).getContext('2d');

    var PieChart = new Chart(ctxP, {
        // plugins: [ChartDataLabels],
        type: chartType,
        data: {
            labels: fileRecord,// ["Red", "Green", "Yellow", "Grey", "Dark Grey"],
            datasets: [{
                data: fileRecordData,//[210, 130, 120, 160, 120],
                backgroundColor: backgroundcolor,//["#F7464A", "#46BFBD", "#FDB45C", "#949FB1", "#4D5360"],
                hoverBackgroundColor: hoverBackgroundColor,//["#FF5A5E", "#5AD3D1", "#FFC870", "#A8B3C5", "#616774"]
            }]
        },
        options: {
            tooltips: {
                enabled: true
            },
            responsive: true,
            legend: {
                position: 'right',
                labels: {
                    padding: 20,
                    boxWidth: 10
                }
            },
            plugins: {
                datalabels: {
                    formatter: (value, ctx) => {
                        let sum = 0;
                        let dataArr = ctx.chart.data.datasets[0].data;
                        dataArr.map(data => {
                            sum += data;
                        });
                        let percentage = (value * 100 / sum).toFixed(2) + "%";
                        return percentage;
                    },
                    color: 'white',
                    labels: {
                        title: {
                            font: {
                                size: '16'
                            }
                        }
                    }
                }
            }
        }
    });
};

//Draw DoughNut Chart
function drawDoughNutChart(labels, data, colors, elementID, hoverBackgroundColor) {
   
      var donutData = [];

    for (var i = 0; i < labels.length; i++) {
        if ((labels[i].toUpperCase() == "NO THREAT") || (labels[i].toUpperCase() == "DONE")) {
            colors[i] = "#008000"
        }
        if (labels[i].toUpperCase() == "INPROGRESS") {
            colors[i] = "#00FF00"
        }
        if (labels[i].toUpperCase() == "UNCATEGORIZED") {
            colors[i] = "#949FB1"
        }
        if (labels[i].toUpperCase() == "FAILURE") {
            colors[i] = "red"
        }
        donutData.push({
            label: labels[i].toUpperCase(),
            data: data[i],
            color: colors[i]
        });
    }
   // console.log(donutData);
    $.plot('#'+elementID, donutData, {
        series: {
            pie: {
                show: true,
                radius: 1.0,
                innerRadius: 0.2,
                label: {
                    show: true,
                    radius: 2 / 3,
                    formatter: labelFormatter,
                    threshold: 0.0
                }

            }
        },
        legend: {
            show: false
        }
    });
};
//Get Color based on number
function getColor(index) {
    let colorvalue = "rgba(0, 137, 132, .2)";
    if (index == '0') {
        colorvalue = 'rgba(0, 250, 220, .2)';
    }
    else if (index == '1') { colorvalue = 'rgba(255, 99, 132, 0.2)'; }
    else if (index == '2') { colorvalue = '#ff4000'; }
    else if (index == '3') { colorvalue = '#ff8000'; }
    else if (index == '4') { colorvalue = '#ffbf00'; }
    else if (index == '5') { colorvalue = '#ffff00'; }
    else if (index == '6') { colorvalue = '#bfff00'; }
    else if (index == '7') { colorvalue = '#80ff00'; }
    else if (index == '8') { colorvalue = '#40ff00'; }
    else if (index == '9') { colorvalue = '#00ff00'; }
    else if (index == '10') { colorvalue = 'rgba(219, 0, 0, 0.1)'; }
    else if (index == '11') { colorvalue = 'rgba(0, 165, 2, 0.1)'; }
    else if (index == '12') { colorvalue = 'rgba(255, 195, 15, 0.2)'; }
    else if (index == '13') { colorvalue = 'rgba(55, 59, 66, 0.1)'; }
    else if (index == '14') { colorvalue = 'rgba(0, 0, 0, 0.3)'; }
    else if (index == '15') { colorvalue = 'rgba(55, 59, 66, 0.1)'; }
    else if (index == '16') { colorvalue = 'rgba(0, 0, 0, 0.4)'; }
    else { colorvalue = "rgba(255, 99, 132, 0.2)"; }

    return colorvalue;
}


//Draw Pie Chart
function drawLineChart(x_Axis, y_Axis_1, y_Axis_2, backgroundcolor, elementID, chartType, hoverBackgroundColor, sum_release, sum_deployment) {

    //line
    var ctxL = document.getElementById(elementID).getContext('2d');
      CICDChart = new Chart(ctxL, {
        type: chartType,
        data: {
            labels: x_Axis,//["January", "February", "March", "April", "May", "June", "July"],
            datasets: [{
                label: "#Deployments (Total: " + sum_deployment+ ")",
                data: y_Axis_2,//[65, 59, 80, 81, 56, 55, 40],
                backgroundColor: [
                    'rgba(105, 0, 132, .2)',
                ],
                borderColor: [
                    'rgba(200, 99, 132, .7)',
                ],
                borderWidth: 2
            },
            {
                label: "#Releases (Total: " + sum_release + ")",
                data: y_Axis_1,//[28, 48, 40, 19, 86, 27, 90],
                backgroundColor: [
                    'rgba(0, 137, 132, .2)',
                ],
                borderColor: [
                    'rgba(0, 10, 130, .7)',
                ],
                borderWidth: 2
            }
            ]
        },
        options: {
            responsive: true
        }
    });
};

//Generate Random Color
function generateRandomColor() {
    var randomColor = '#' + Math.floor(Math.random() * 16777215).toString(16);
   // console.log(randomColor);
    return randomColor;
    //random color will be freshly served
}

/*
* Custom Label formatter
* ----------------------
*/
function labelFormatter_old(label, series) {
    return "<div style='font-size:12px; text-align:center; padding:2px; color:#ffffff;'>"
        + label
        + '<br>'
        + Math.round(series.percent) + '%</div>'
}
function labelFormatter(label, series) {
    var formattedLabel = label;
    var percent = Math.round(series.percent);
    if (formattedLabel.length > 10) {
        formattedLabel = formattedLabel.substring(0, 14) + '...';
    }
    return "<div style='font-size:12px; text-align:center; padding:2px; color:#ffffff;' title='" + label + "'>" +
        formattedLabel + "<br/>" +
        percent + "%</div>";
}
//fetch API URL
async function fetchBaseAPIUrl() {
    const response = await fetch('/GetBaseAPIUrl');
    const apibaseurl = await response.text();
    return apibaseurl;
};

//Fetch File Uploaded Count Data
// Fetch the date-value pairs dynamically
async function fetchFileUploadedData(FromDate,ToDate) {
    let urlToGetData ="ProxyToExternalEndpoint_GetFilesDistributionBasedOnUploadedDate?FromUploadedDate=" + FromDate + "&ToUploadedDate=" + ToDate;
    const response = await fetch(urlToGetData);
    const data = await response.json();
   // console.log(data.result);
    return data.result;
};

async function fetchFileAnalyzedData(FromDate, ToDate) {
    let urlToGetData = "ProxyToExternalEndpoint_GetFilesDistributionBasedOnAnalyzedDate?FromUploadedDate=" + FromDate + "&ToUploadedDate=" + ToDate;
    const response = await fetch(urlToGetData);
    const data = await response.json();
   // console.log(data.result);
    return data.result;
};


async function fetchC2ContriesData(FromDate, ToDate) {
    let urlToGetData ="ProxyToExternalEndpoint_GetC2CountriesDistributionBasedOnUploadedDate?FromUploadedDate=" + FromDate + "&ToUploadedDate=" + ToDate;
    const response = await fetch(urlToGetData);
    const data = await response.json();  
    const backgroundColor = [];// "#46BFBD";//, "#46BFBD", "#F7464A", "#FDB45C"];
    const hoverBackgroundColor = ["#A8B3C5"];//, "#5AD3D1", "#FF5A5E", "#FFC870"];
    var country = new CountryCode();

   // console.log(data.result);
    const combinedArrays = [];
   
    for (var count of data.result) {
        //console.log(count);
        if (!(count.countries == undefined || count.countries == null || count.countries == 'null' || count.countries == "")) {

            // console.log((JSON.parse(count.countries)).length);
            combinedArrays.push(JSON.parse(count.countries));
        }

    }

    const counts = combinedArrays.reduce((accumulator, currentArray) => {
        currentArray.forEach(item => {
            if (accumulator.hasOwnProperty(item)) {
                accumulator[item]++;
            } else {
                accumulator[item] = 1;
            }
        });
        return accumulator;
    }, {});

    const uniqueItems = Object.keys(counts);
    const uniqueValue = Object.values(counts);

  
    const CountriesFullName = [];
  //  console.log(uniqueValue);
   // console.log(counts);
   // console.log(uniqueItems.length);
    //console.log(uniqueValue);
    for (var index = 0; index < uniqueItems.length; index++) {

        CountriesFullName.push(country.getName(uniqueItems[index].toLowerCase()));
        backgroundColor.push(
            //getColor(index)
            generateRandomColor()
        );
    }
   // console.log(CountriesFullName);
    drawContriesChart(CountriesFullName, uniqueValue, backgroundColor, "canvasCountriesWiseDistributionChart", "horizontalBar", hoverBackgroundColor, uniqueItems.length);

    return data.result;
};

async function fetchC2InfecContriesData(FromDate, ToDate, baseAPIURL) {
    let urlToGetData = "ProxyToExternalEndpoint_GetC2InfectedCountriesDistributionBasedOnUploadedDate?FromUploadedDate=" + FromDate + "&ToUploadedDate=" + ToDate;
    const response = await fetch(urlToGetData);
    const data = await response.json();
    const backgroundColor = [];// "#46BFBD";//, "#46BFBD", "#F7464A", "#FDB45C"];
    const hoverBackgroundColor = ["#A8B3C5"];//, "#5AD3D1", "#FF5A5E", "#FFC870"];
    // console.log(data.result);
    const combinedArrays = [];
    for (var count of data.result) {
        // console.log(count);
        if (!(count.countries == undefined || count.countries == null || count.countries == 'null' || count.countries == "")) {
           // console.log(JSON.parse(count.countries));
            combinedArrays.push(JSON.parse(count.countries));
        }

    }
   // console.log(combinedArrays);

    const counts = combinedArrays.reduce((accumulator, currentArray) => {
        currentArray.forEach(item => {
            if (accumulator.hasOwnProperty(item)) {
                accumulator[item]++;
            } else {
                accumulator[item] = 1;
            }
        });
        return accumulator;
    }, {});

    const uniqueItems = Object.keys(counts);
    const uniqueValue = Object.values(counts);
   
    const CountriesFullName = [];
    var country = new CountryCode();

    for (var index = 0; index < uniqueItems.length; index++) {

        CountriesFullName.push(country.getName(uniqueItems[index].toLowerCase()));
        backgroundColor.push(
            //getColor(index)
            generateRandomColor()
        );
    }
    drawContriesChart(CountriesFullName, uniqueValue, backgroundColor, "canvasInfCountriesWiseDistributionChart", "horizontalBar", hoverBackgroundColor, uniqueItems.length);

    return data.result;
};
//Draw Bar Chart
function drawContriesChart(contries, values, backgroundcolor, elementID, chartType, hoverBackgroundColor, sum_request) {



    var contriesData = {
        labels: contries,
        datasets: [{
            label: '# Unique Contries (Total:' + sum_request + ')',
            // fill: true,
            backgroundColor: backgroundcolor,//'rgba(255, 99, 132, 0.2)',
            borderColor: hoverBackgroundColor,// 'rgba(54, 162, 235, 1)',//'rgba(54, 162, 235, 1)',//'black',// 'rgba(255,99,132,1)',
            data: values,
            borderWidth: 0
        }]
    };

    // Options define for display value on top of bars
    var releaseOption = {
        tooltips: {
            enabled: true
        },
        hover: {
            animationDuration: 1
        },
        
        animation: {
            duration: 1,
            onComplete: function () {
                var chartInstance = this.chart,
                    ctx = chartInstance.ctx;
                ctx.textAlign = 'center';
                ctx.fillStyle = "#684abe";
                ctx.textBaseline = 'bottom';
                // Loop through each data in the datasets
                this.data.datasets.forEach(function (dataset, i) {
                    var meta = chartInstance.controller.getDatasetMeta(i);
                    meta.data.forEach(function (bar, index) {
                        var data = dataset.data[index];
                        ctx.fillText(data, bar._model.x, bar._model.y - 5);
                    });
                });
            }
        }
    };
    //  console.log(elementID);
    var ctxB = document.getElementById(elementID).getContext('2d');
    // let drawChart = null;
    let drawChart = new Chart(ctxB, {
        type: chartType,
        data: contriesData,
        options: releaseOption

    });
};


    //const grouped = combinedArrays.reduce((obj, val) => {
    //    if (obj[val]) {
    //        obj[val]++;
    //    } else {
    //        obj[val] = 1;
    //    }
    //    return obj;
    //}, {});
    //const countValues = Object.keys(grouped).map((key) => ({
    //    item: key,
    //    count: grouped[key],
    //}));
   // console.log(countValues);
  //  console.log(grouped);

// Fetch the date-value pairs dynamically
async function darwKnobRateForAnalyses(analyzeddata, uploadeddata) {
    let sum_AnalyzedCount = 0;
    let sum_UploadedCount = 0;
   
       
    if (analyzeddata.length > 0) {        
        for (let i = 0; i < analyzeddata.length; i++) {
            sum_AnalyzedCount += Number(analyzeddata[i].totalCount);
        }       
    }
    if (uploadeddata.length > 0) {
        for (let i = 0; i < uploadeddata.length; i++) {
            sum_UploadedCount += Number(uploadeddata[i].totalCount);
        }
    }
   // console.log("Sum:" + sum_AnalyzedCount);
    //console.log("Sum:" + sum_UploadedCount);
    if (sum_AnalyzedCount > 0 && sum_UploadedCount > 0) {
        let rate = (sum_AnalyzedCount / sum_UploadedCount) * 100;
        let fixedDecimanlRate = rate.toFixed(1);
      //  console.log("Sum:" + fixedDecimanlRate);
        document.getElementById("inputTxtKnobRate").value = fixedDecimanlRate;
        drawKnob();
    }  

};
//Convert to date Week Pair
async function convertToDateWeekPairs(FromDate, ToDate, baseAPIURL) {
    const dateValuePairs = await fetchFileUploadedData(FromDate, ToDate, baseAPIURL);

  //  console.log(dateValuePairs);     

    const weekValuePairs = dateValuePairs.map(({ date, totalCount }) => ({
        week: getWeek(new Date(date)),
        totalCount
    }));

    //console.log(weekValuePairs);
    const groupedByWeek = weekValuePairs.reduce((acc, curr) => {
        const week = curr.week;
        const value = parseInt(curr.totalCount);
        if (!acc[week]) {
            acc[week] = 0;
        }
        acc[week] += value;
        return acc;
    }, {});

   // console.log(groupedByWeek);

    //const groupedByWeek = {};
    //weekValuePairs.forEach(pair => {
    //    if (!groupedByWeek[pair.week]) {
    //        groupedByWeek[pair.week] = parseInt(pair.totalCount);
    //    } else {
    //        groupedByWeek[pair.week] += parseInt(pair.totalCount);// pair.totalCount;
    //    }
    //});

    //console.log(groupedByWeek);
};

//get Week based on provided Date
function getWeek(date) {
    const onejan = new Date(date.getFullYear(), 0, 1);
    return Math.ceil(((date - onejan) / 86400000 + onejan.getDay() + 1) / 7);
};


//Draw Bar Chart
function drawBarChart(labels, values, backgroundcolor, elementID, chartType, hoverBackgroundColor, sum_request,msg) {
    var cahrtData = {
        labels: labels,
        datasets: [{
            label: msg + sum_request + ')',
            // fill: true,
            backgroundColor: backgroundcolor,//'rgba(255, 99, 132, 0.2)',
            borderColor: hoverBackgroundColor,// 'rgba(54, 162, 235, 1)',//'rgba(54, 162, 235, 1)',//'black',// 'rgba(255,99,132,1)',
            data: values,
            borderWidth: 0
        }]
    };

    // Options define for display value on top of bars
    var chartOption = {
        tooltips: {
            enabled: true
        },
        hover: {
            animationDuration: 1
        },
        animation: {
            duration: 1,
            onComplete: function () {
                var chartInstance = this.chart,
                    ctx = chartInstance.ctx;
                ctx.textAlign = 'center';
                ctx.fillStyle = "#684abe";
                ctx.textBaseline = 'bottom';
                // Loop through each data in the datasets
                this.data.datasets.forEach(function (dataset, i) {
                    var meta = chartInstance.controller.getDatasetMeta(i);
                    meta.data.forEach(function (bar, index) {
                        var data = dataset.data[index];
                        ctx.fillText(data, bar._model.x, bar._model.y - 5);
                    });
                });
            }
        }
    };
    //  console.log(elementID);
    var ctxB = document.getElementById(elementID).getContext('2d');
    // let drawChart = null;
    let drawChart = new Chart(ctxB, {
        type: chartType,
        data: cahrtData,
        options: chartOption

    });
};

 //Draw Knob
function drawKnob() {
    /* jQueryKnob */

    $('.knob').knob({
        /*change : function (value) {
         //console.log("change : " + value);
         },
         release : function (value) {
         console.log("release : " + value);
         },
         cancel : function () {
         console.log("cancel : " + this.value);
         },*/
        draw: function () {

            // "tron" case
            if (this.$.data('skin') == 'tron') {

                var a = this.angle(this.cv)  // Angle
                    ,
                    sa = this.startAngle          // Previous start angle
                    ,
                    sat = this.startAngle         // Start angle
                    ,
                    ea                            // Previous end angle
                    ,
                    eat = sat + a                 // End angle
                    ,
                    r = true

                this.g.lineWidth = this.lineWidth

                this.o.cursor
                    && (sat = eat - 0.3)
                    && (eat = eat + 0.3)

                if (this.o.displayPrevious) {
                    ea = this.startAngle + this.angle(this.value)
                    this.o.cursor
                        && (sa = ea - 0.3)
                        && (ea = ea + 0.3)
                    this.g.beginPath()
                    this.g.strokeStyle = this.previousColor
                    this.g.arc(this.xy, this.xy, this.radius - this.lineWidth, sa, ea, false)
                    this.g.stroke()
                }

                this.g.beginPath()
                this.g.strokeStyle = r ? this.o.fgColor : this.fgColor
                this.g.arc(this.xy, this.xy, this.radius - this.lineWidth, sat, eat, false)
                this.g.stroke()

                this.g.lineWidth = 2
                this.g.beginPath()
                this.g.strokeStyle = this.o.fgColor
                this.g.arc(this.xy, this.xy, this.radius - this.lineWidth + 1 + this.lineWidth * 2 / 3, 0, 2 * Math.PI, false)
                this.g.stroke()

                return false
            }
        }
    })
    /* END JQUERY KNOB */
};