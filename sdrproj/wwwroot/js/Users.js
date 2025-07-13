var dataTable;

$(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tbldata').DataTable({
        ajax: {
            url: '/Admin/GetAllUsers', 
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
