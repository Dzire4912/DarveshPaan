
function createChart(labels, data) {
    let ctx = document.getElementById('myChart').getContext('2d');
    let myChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                label: 'Test Chart',
                data: data,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(255, 159, 64, 0.2)'
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            plugins: {
                labels: {
                    render: function (args) {
                        return args.label + ': ' + args.value;
                    },
                    fontColor: '#fff',
                    precision: 0
                }
            }
        }
    });
}



function createBarChart(labels, data) {
    let ctx = document.getElementById('myBarChart').getContext('2d');
    let myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Test Chart',
                data: data,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(255, 159, 64, 0.2)'
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            //plugins: {
            //    labels: {
            //        render: function (args) {
            //            return args.label + ': ' + args.value;
            //        },
            //        fontColor: '#fff',
            //        precision: 0
            //    }
            //}
        }
    });
}



function createLabourBarChart(labels, data) {
    let ctx = document.getElementById('myLabourChart').getContext('2d');
    let myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Test Chart',
                data: data,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(255, 159, 64, 0.2)'
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            plugins: {
                labels: {
                    render: function (args) {
                        return args.label + ': ' + args.value;
                    },
                    fontColor: '#fff',
                    precision: 0
                }
            }
        }
    });
}

function createnewLabourBarChart() {

}

function generatePieChart(data, labels, containerId) {

    let chartOptions = {
        chart: {
            type: 'pie'
        },
        legend: {
            position: 'bottom',
            show: true
        },
        series: data,
        labels: labels,
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: 250
                },
                legend: {
                    position: 'bottom'
                }
            }
        }]
    };

    let chart = new ApexCharts(document.getElementById(containerId), chartOptions);
    chart.render();
}


function generateBarChart(data, labels, containerId) {
    let chartOptions = {
        chart: {
            height: 350,
            type: 'bar'
        },
        plotOptions: {
            bar: {
                barHeight: '100%',
                distributed: true,
                width: '2%',
                /*endingShape: 'rounded'*/
            }
        },
        colors: ['#fd7f6f', '#7eb0d5', '#b2e061', '#bd7ebe', '#ffb55a', '#ffee65', '#beb9db', '#fdcce5', '#8bd3c7'],      //colors: ['#2962ff', '#33b2df', '#546E7A', '#7460ee'],
        stroke: {
            width: 5,
            colors: ['#fff']
        },
        legend: {
            position: 'top',
            show: true
        },
        series: [{
            name: 'Employee',
            data: data // Your array of data values
        }],
        xaxis: {
            categories: labels
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: '10%'
                },
                legend: {
                    position: 'bottom'
                }
            }
        }]
    };

    let chart = new ApexCharts(document.getElementById(containerId), chartOptions);
    chart.render();
}

function generateSubmissionBarChart(data, labels, containerId) {
    let chartOptions = {
        chart: {
            height: 350,
            type: 'bar'
        },
        plotOptions: {
            bar: {
                barHeight: '50%',
                distributed: true,
                horizontal: true,
                width: '5%',
                /*endingShape: 'rounded'*/
            }
        },
        colors: ["#fd7f6f", "#7eb0d5", "#b2e061", "#bd7ebe", "#ffb55a", "#ffee65", "#beb9db", "#fdcce5", "#8bd3c7"],    //colors: ['#2962ff', '#33b2df', '#546E7A', '#7460ee'],
        stroke: {
            width: 5,
            colors: ['#fff']
        },
        legend: {
            position: 'top',
            show: true
        },
        series: [{
            name: 'Facilities',
            data: data // Your array of data values
        }],
        xaxis: {
            categories: labels
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: '10%'
                },
                legend: {
                    position: 'bottom'
                }
            }
        }]
    };

    let chart = new ApexCharts(document.getElementById(containerId), chartOptions);
    chart.render();
}


function generateEChartBar(data, labels, containerId) { const option = { xAxis: { type: 'category', data: labels, }, yAxis: { type: 'value', }, series: [{ type: 'bar', data: data, }] }; const chart = echarts.init(document.getElementById(containerId)); chart.setOption(option); } 


function GenerateSingleradialChart(count, divId) {
    let chartData = count;

    let options = {
        chart: {
            type: 'radialBar',
            height: 250,
        },
        plotOptions: {
            radialBar: {
                hollow: {
                    margin: 0,
                    size: '70%',
                },
                track: {
                    dropShadow: {
                        enabled: true,
                        top: 0,
                        left: 0,
                        blur: 4,
                        opacity: 0.15
                    },
                    strokeWidth: "50%",



                },
                dataLabels: {
                    value: {
                        color: undefined,
                       fontSize: "30px",
                       show: true,
                       formatter: function (val) {
                           return val + '%';
                       }
                    },
                total: {
                    //offsetY: -10,
                        show: true,
                        fontSize: "13px",
                        label: 'Total Percentage',
                        color: '#888',
                        formatter: function () {
                            return chartData + '%';
                        }
                    }
                }
            }
        },
        fill: {
            type: 'solid',
            solid: {
                shade: 'dark',
                type: 'horizontal',
                shadeIntensity: 0.5,
                color: ['#2962ff'],
                inverseColors: false,
                opacityFrom: 1,
                opacityTo: 1,
                stops: [0, 100]
            }
        },
        stroke: {
            lineCap: "round",

        },
        series: [chartData],
        labels: ['Series 1'],
    };

    let chart = new ApexCharts(document.getElementById(divId), options);
    chart.render();
};

function GenerateMultiradialChart(count, labels, divId) {
    let chartData = count;

    let options = {
        chart: {
            type: 'radialBar',
            height: 280,
        },
        colors: ["#2D6BFF", "#56D6FF", "#FF3159"],
        plotOptions: {
            radialBar: {
                startAngle: -90,
                endAngle: 90,
                hollow: {
                    margin: 0,
                    size: '60%',
                },
                track: {  
                    //dropShadow: {
                    //    enabled: true,
                    //    top: 0,
                    //    left: 0,
                    //    blur: 4,
                    //    opacity: 0.15
                    //},
                    startAngle: -90,
                    endAngle: 90,
                    strokeWidth: "100%",
                },
                dataLabels: {
                    value: {
                        color: undefined,
                        fontSize: "30px",
                        show: true,
                        formatter: function (val) {
                            return val + '%';
                        }
                    },
                    total: {
                        //offsetY: -10,
                        show: true,
                        fontSize: "13px",
                        label: 'All Percentage',
                        color: '#888',
                      
                        formatter: function (e) {
                         //   var formattedData = chartData.map(function (value) {
                          //      return value + '%';
                          //  });                         

                            // Join the formatted values with a comma and space
                            //return formattedData.join(', ');

                            // Calculate the sum of all values in chartData
                            let sum = chartData.reduce(function (a, b) {
                                return a + b;
                            }, 0);
                            return sum + '%';
                        }
                    }
                }
            }
        },
        fill: {
            type: 'solid',
            solid: {
                shade: 'dark',
                type: 'horizontal',
                shadeIntensity: 0.5,
                color: ['#2962ff'],
                inverseColors: false,
                opacityFrom: 1,
                opacityTo: 1,
                stops: [0, 100]
            }
        },
        stroke: {
            lineCap: "round",

        },
        series: chartData,
        labels: labels,
    };

    let chart = new ApexCharts(document.getElementById(divId), options);
    chart.render();
};

function generateEchartBarChart(data, labels, containerId) {
    let option = {
        xAxis:
            { type: 'category', data: labels, },
        yAxis:
            { type: 'value', },
        series:
            [{
                type: 'bar',
                data: data,
                itemStyle: {
                    color: 'blur'
                },
                barWidth: '50%',
            }]
    };
    let chart = echarts.init(document.getElementById(containerId));
    chart.setOption(option);
}

function generateMorisBarChart(data, labels, containerId) {

    // Prepare the data in Morris.js format
    let morrisData = [];
    for (let i = 0; i < data.length; i++) {
        morrisData.push({
            y: labels[i],
            a: data[i]
        });
    }

    // Create the bar chart
    new Morris.Bar({
        element: containerId,
        data: morrisData,
        xkey: 'y',
        ykeys: ['a'],
        labels: ['Value'],
        barColors: ['#36b9cc'], // Change color if needed
        resize: true, // Make the chart responsive
        hideHover: 'auto', // Hide hover effect on small screens
        xLabelAngle: 0 // Rotate x-axis labels to prevent overlap
    });
}


function GeneratedonutApex(data, labels, containerId) {
    
    let chartOptions = {
        chart: {
            type: 'donut'
        },
        colors: ["#20E647", "#7BD2F9", "#DBDFE4"],
        legend: {
            position: 'bottom',
            show: true
        },
        labels: labels,
        series: data,
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: 280
                },
                legend: {
                    position: 'bottom',
                    show: true
                }
            }
        }]
    };

    // Create the chart
    let chart = new ApexCharts(document.getElementById(containerId), chartOptions);

    // Render the chart
    chart.render();
}

function GeneratesemidonutApex(data, labels, containerId) {
    
    console.log(containerId);
    let chartOptions = {
        chart: {
            type: 'donut'
        },
        legend: {
            position: 'bottom',
            show: true
        },
        track: {
            background: '#333',
            startAngle: -135,
            endAngle: 135,
        },
        labels: labels,
        series: data,
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: 250
                },
                legend: {
                    position: 'bottom',
                    show: true
                }
            }
        }]
    };

    // Create the chart
    let chart = new ApexCharts(document.getElementById(containerId), chartOptions);

    // Render the chart
    chart.render();
}
function generateLineApex(data, labels, containerId) {
    let chartData = {
        series: [
            {
                name: 'Exempt',
                data: [120, 130, 125, 140, 155, 150, 160] // Data points for Exempt employees
            },
            {
                name: 'Non-Exempt',
                data: [80, 85, 90, 100, 95, 105, 110] // Data points for Non-Exempt employees
            },
            {
                name: 'Contract',
                data: [60, 65, 70, 75, 80, 85, 90] // Data points for Contract employees
            }
        ],
        xaxis: {
            categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul']
        }
    };
    let chartOptions = {
        chart: {
            type: 'line'
        },
        series: chartData.series,
        xaxis: chartData.xaxis
    };

    // Create the chart
    let chart = new ApexCharts(document.getElementById(containerId), chartOptions);

    // Render the chart
    chart.render();
}

function generateLineApex(data, labels, containerId) {
    let chartData = {
        series: [
            {
                name: 'series1',
                data: data // Data points for Exempt employees
            },

        ],
        xaxis: {
            categories: labels
        }
    };
    let chartOptions = {
        chart: {
            type: 'line'
        },
        stroke: {
            curve: 'curve',
        },
        series: chartData.series,
        xaxis: chartData.xaxis
    };

    // Create the chart
    let chart = new ApexCharts(document.getElementById(containerId), chartOptions);

    // Render the chart
    chart.render();
}


function GenerateAreaApex(data, labels, containerId) {

    let options = {
        chart: {
            type: 'area',
            height: 300,
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                },
                dynamicAnimation: {
                    enabled: true,
                    speed: 350
                }
            }
        },
        series: [{
            name: 'Employee',
            data: data // Your array of data values
        }],
        xaxis: {
            categories: labels // Your array of category labels
        },
        yaxis: {
            labels: {
                formatter: function (val) {
                    return "#" + val;
                }
            }
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth'
        },
        fill: {
            opacity: 0.3
        },
        tooltip: {
            theme: 'dark'
        }
    };

    let chart = new ApexCharts(document.getElementById(containerId), options);
    chart.render();
}

function generateRateChart() {
    let bouncerate = {
        series: [
            {
                name: "Rates : ",
                labels: ["2012", "2013", "2014", "2015", "2016", "2017"],
                data: [12, 19, 3, 5, 2, 3],
            },
        ],
        chart: {
            width: 150,
            height: 60,
            type: "line",
            toolbar: {
                show: false,
            },
            sparkline: {
                enabled: true,
            },
        },
        fill: {
            type: "solid",
            opacity: 1,
            colors: ["#2962ff"],
        },
        grid: {
            show: false,
        },
        stroke: {
            curve: "smooth",
            lineCap: "square",
            colors: ["#2962ff"],
            width: 3,
        },
        markers: {
            size: 3,
            colors: ["#2962ff"],
            strokeColors: "transparent",
            shape: "square",
            hover: {
                size: 7,
            },
        },
        xaxis: {
            axisBorder: {
                show: false,
            },
            axisTicks: {
                show: false,
            },
            labels: {
                show: false,
            },
        },
        fill: {
            type: "solid",
            colors: ["#FDD835"],
        },
        yaxis: {
            labels: {
                show: false,
            },
        },
        tooltip: {
            theme: "dark",
            style: {
                fontFamily: '"Nunito Sans", sans- serif',
            },
            x: {
                show: false,
            },
            y: {
                formatter: undefined,
            },
            marker: {
                show: false,
            },
            followCursor: true,
        },
    };
    let chart_line_basic = new ApexCharts(
        document.querySelector("#rate-chart"),
        bouncerate
    );
    chart_line_basic.render();
}

function GenerateSinglefilledradialChart(count, divId) {
    let chartData = count;
    let options = {
        chart: {
            height: 200,
            type: "radialBar",
        },



        series: [count],

        colors: ["#20E647"],
        plotOptions: {
            radialBar: {
                startAngle: -90,
                endAngle: 90,
                hollow: {
                    margin: 0,
                    size: "70%",
                   /* background: "#293450"*/
                },
                track: {
                    dropShadow: {
                        enabled: true,
                        top: 0,
                        left: 0,
                        blur: 4,
                        opacity: 0.15
                    },
                    startAngle: -90,
                    endAngle: 90,
                    strokeWidth: "50%",



                },
                dataLabels: {
                    name: {
                        offsetY: -10,
                        color: '#888',
                        fontSize: "13px"
                    },
                    value: {
                        color: undefined,
                        fontSize: "30px",
                        show: true
                    }
                }
            }
        },
        fill: {
            type: 'solid',
            solid: {
                shade: 'dark',
                type: 'horizontal',
                shadeIntensity: 0.5,
                color: ['#3699ff'],
                inverseColors: false,
                opacityFrom: 1,
                opacityTo: 1,
                stops: [0, 100]
            }
        },
        stroke: {
            lineCap: "round",

        },
        labels: ["Progress"]
    };



    let chart = new ApexCharts(document.getElementById(divId), options);
    chart.render();
}