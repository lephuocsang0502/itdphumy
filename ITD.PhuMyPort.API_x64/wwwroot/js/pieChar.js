// Set new default font family and font color to mimic Bootstrap's default styling
Chart.defaults.global.defaultFontFamily = 'Nunito', '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#858796';


$(document).ready(function () {

   
        $.getJSON("/Transection/Chart", function (data) {
            $.each(data, function (i, item) {
                if (item.name == 1) {
                    data[i].name = "Hình giống"
                    return true;
                }
                if (item.name == 3) {
                    data[i].name = "Hình không có"
                    return true;
                }
                if (item.name == 2) {
                    data[i].name = "Hình không giống"
                    return true;
                }
                if (item.name == 0) {
                    data[i].name = "Hình chưa được duyệt"
                    return true;
                }
            });

            var Names = [];
            var Qts = [];
            for (var i = 0; i < data.length; i++) {
                Names.push(data[i].name);
                Qts.push(data[i].count);

            }
            // Pie Chart Example
            var ctx = document.getElementById("myPieChart");
            var myPieChart = new Chart(ctx, {
                type: 'doughnut',
                //dataPoints: [
                //    { indexLabel: "Hình chưa duyệt" },
                //    { indexLabel: "Hình giống " },
                //    { indexLabel: "Hình không giống " },
                //    { indexLabel: "Hình không có" },
                //],
                data: {
                    labels: Names,
                   
                    datasets: [{
                        data: Qts,
                        showInLegend: true,
                        backgroundColor: ['#E60004', '#1E8449', '#FF9D00','#E04B00'],
                        hoverBackgroundColor: ['#7A0000', '#007000', '#F5A700', '#C23D00'],
                        hoverBorderColor: "rgba(234, 236, 244, 1)",
                    }],
                  
                },
                options: {
                    maintainAspectRatio: false,
                    tooltips: {
                        backgroundColor: "rgb(8,71,198)",
                        bodyFontColor: "rgb(255,255,255)",
                        bodyFontSize: 15,
                        borderColor: '#dddfeb',
                        borderWidth: 1,
                        xPadding: 15,
                        yPadding: 15,
                        displayColors: false,
                        caretPadding: 10
                    },
                    legend: {
                        verticalAlign: "bottom",
                        horizontalAlign: "center"
                    },
                    cutoutPercentage: 80,
                },
            })
        })

   

   
});
