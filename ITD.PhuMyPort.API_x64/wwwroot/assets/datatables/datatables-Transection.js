// Call the dataTables jQuery plugin

$(document).ready(function () {
    $('#dataTableCard').DataTable({
        "scrollY": "380px",
        "scrollX": false,
        "info": false,
        "lengthChange": false,
        "pageLength": 10,
        paging: false,
        "searching": false
      
        
    });
   
    
});



