var dataTable;

$(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tbldatai').DataTable({
        ajax: {
            url: '/Admin/GetAllItems', 
            dataSrc: 'data'
        },
        columns: [
            { data: "name", width: "20%" },      
            { data: "SubCategory", width: "20%" },
            { data: "price", width: "10%" },
            { data: "stock", width: "10%" },
        ]
    });
}
