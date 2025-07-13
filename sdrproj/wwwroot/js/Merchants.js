var dataTable;

$(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tbldatam').DataTable({
        ajax: {
            url: '/Admin/GetAllMerchants', 
            dataSrc: 'data'
        },
        columns: [
            { data: "name", width: "20%" },      
            { data: "email", width: "20%" },
            { data: "phone", width: "10%" },
            { data: "status", width: "10%" },
            { data: "registeredOn", width: "10%" }, 
            
        ]
    });
}
